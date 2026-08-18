using System;
using System.Collections;
using Deucarian.Common;
using Deucarian.Theming;
using Deucarian.Theming.UIToolkit;
using Deucarian.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    internal sealed class ViewerShellStatusPresenter : IDisposable
    {
        private readonly MonoBehaviour host;
        private readonly ViewerShellStatusView view;
        private readonly ViewerShellReferenceProfile profile;
        private readonly Func<bool> shouldAnimate;
        private readonly Action layoutChanged;
        private ViewerShellStatusSnapshot snapshot;
        private Coroutine readyToastRoutine;
        private bool presentationSuppressed;
        private bool hasSnapshot;
        private bool loading;
        private float spinnerAngle;

        public ViewerShellStatusPresenter(
            MonoBehaviour host,
            ViewerShellStatusView view,
            ViewerShellReferenceProfile profile,
            Func<bool> shouldAnimate,
            Action layoutChanged)
        {
            this.host = host ??
                throw new ArgumentNullException(nameof(host));
            this.view = view ??
                throw new ArgumentNullException(nameof(view));
            this.profile = profile ??
                throw new ArgumentNullException(nameof(profile));
            this.shouldAnimate = shouldAnimate ?? (() => true);
            this.layoutChanged = layoutChanged ??
                throw new ArgumentNullException(nameof(layoutChanged));
        }

        public VisualElement Card => view.Card;
        public ViewerShellStatusSnapshot Snapshot => snapshot;
        public bool HasSnapshot => hasSnapshot;
        public bool IsLoading => loading;

        public void Apply(ViewerShellStatusSnapshot value)
        {
            snapshot = value;
            hasSnapshot = true;
            bool ready = value.State == ViewerShellStatusState.Ready;
            bool error = value.State == ViewerShellStatusState.Error;
            loading = !ready && !error;
            view.StateLabel.text = error
                ? profile.ErrorTitle
                : ready
                    ? profile.ReadyTitle
                    : profile.LoadingTitle;
            view.MessageLabel.text = ResolveMessage(value, profile);
            view.Spinner.style.display = loading
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            view.Card.style.display = presentationSuppressed
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            view.Card.style.opacity = 1f;

            StopReadyToast();
            if (!presentationSuppressed &&
                ready &&
                Application.isPlaying &&
                host.isActiveAndEnabled)
            {
                readyToastRoutine = host.StartCoroutine(
                    HideReadyToastAfterDelay(value));
            }
        }

        public void UpdateSpinner(float unscaledDeltaTime)
        {
            if (!loading || view.Spinner == null)
            {
                return;
            }

            spinnerAngle = Mathf.Repeat(
                spinnerAngle +
                (unscaledDeltaTime * profile.SpinnerDegreesPerSecond),
                360f);
            view.Spinner.style.rotate = new Rotate(
                new Angle(spinnerAngle, AngleUnit.Degree));
        }

        public void SetPresentationSuppressed(bool suppressed)
        {
            presentationSuppressed = suppressed;
            if (!suppressed)
            {
                return;
            }

            StopReadyToast();
            view.Card.style.display = DisplayStyle.None;
            view.Card.style.opacity = 1f;
            layoutChanged();
        }

        public void ApplyTheme(
            DeucarianTheme theme,
            DeucarianThemeStyle style,
            Component context)
        {
            DeucarianTheme resolvedTheme = theme ??
                DeucarianViewerReferenceThemePreset.Resolve().DefaultTheme;
            DeucarianGlassPanelStyle.ApplyPanel(
                view.Card,
                resolvedTheme,
                style,
                context);
            DeucarianUIToolkitThemeTypography.ApplyStyle(
                view.Root,
                style ?? resolvedTheme.VisualStyle);
            Color primary = ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.TextPrimary,
                Color.white);
            Color secondary = ResolveColor(
                resolvedTheme,
                DeucarianBuiltinColorRoleIds.TextSecondary,
                new Color(0.82f, 0.9f, 0.86f, 1f));
            Color state = ResolveStateColor(resolvedTheme);
            view.StateLabel.style.color = primary;
            view.MessageLabel.style.color = secondary;
            view.Indicator.style.backgroundColor = state;

            Color quiet = state;
            quiet.a *= 0.22f;
            view.Spinner.style.borderLeftColor = quiet;
            view.Spinner.style.borderRightColor = quiet;
            view.Spinner.style.borderBottomColor = quiet;
            view.Spinner.style.borderTopColor = state;
        }

        public void Dispose()
        {
            StopReadyToast();
        }

        public void OnDisable()
        {
            StopReadyToast();
        }

        public void OnEnable()
        {
            if (view.Card != null)
            {
                view.Card.style.opacity = 1f;
            }

            if (hasSnapshot)
            {
                Apply(snapshot);
            }
        }

        internal static string ResolveMessage(
            ViewerShellStatusSnapshot value,
            ViewerShellReferenceProfile profile)
        {
            if (!string.IsNullOrWhiteSpace(value.Message))
            {
                return value.Message;
            }

            return value.State == ViewerShellStatusState.Loading ||
                   value.State == ViewerShellStatusState.Uninitialized
                ? profile.DefaultLoadingMessage
                : string.Empty;
        }

        private IEnumerator HideReadyToastAfterDelay(
            ViewerShellStatusSnapshot readySnapshot)
        {
            float fadeDuration = shouldAnimate()
                ? profile.ReadyFadeDurationSeconds
                : 0f;
            float visibleDuration = Mathf.Max(
                0f,
                profile.ReadyToastDurationSeconds - fadeDuration);
            if (visibleDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(visibleDuration);
            }

            if (!hasSnapshot || snapshot != readySnapshot)
            {
                readyToastRoutine = null;
                yield break;
            }

            if (fadeDuration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration &&
                       snapshot == readySnapshot &&
                       shouldAnimate())
                {
                    float progress = Mathf.Clamp01(
                        elapsed / fadeDuration);
                    view.Card.style.opacity =
                        1f - DeucarianEasingUtility.Evaluate(
                            DeucarianEasing.EaseOutCubic,
                            progress);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            if (snapshot == readySnapshot)
            {
                view.Card.style.display = DisplayStyle.None;
                view.Card.style.opacity = 1f;
                layoutChanged();
            }

            readyToastRoutine = null;
        }

        private Color ResolveStateColor(DeucarianTheme theme)
        {
            string role = hasSnapshot &&
                          snapshot.State == ViewerShellStatusState.Error
                ? DeucarianBuiltinColorRoleIds.Error
                : hasSnapshot &&
                  snapshot.State == ViewerShellStatusState.Ready
                    ? DeucarianBuiltinColorRoleIds.Success
                    : DeucarianBuiltinColorRoleIds.Info;
            return ResolveColor(
                theme,
                role,
                role == DeucarianBuiltinColorRoleIds.Error
                    ? new Color(0.87f, 0.51f, 0.47f, 1f)
                    : role == DeucarianBuiltinColorRoleIds.Success
                        ? new Color(0.48f, 0.81f, 0.65f, 1f)
                        : new Color(0.46f, 0.65f, 0.72f, 1f));
        }

        private void StopReadyToast()
        {
            if (readyToastRoutine == null)
            {
                return;
            }

            host.StopCoroutine(readyToastRoutine);
            readyToastRoutine = null;
        }

        private static Color ResolveColor(
            DeucarianTheme theme,
            string role,
            Color fallback)
        {
            return theme != null && theme.TryGetColorById(role, out Color color)
                ? color
                : fallback;
        }
    }
}
