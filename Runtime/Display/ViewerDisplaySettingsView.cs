using System;
using Deucarian.Theming;
using Deucarian.Theming.UIToolkit;
using Deucarian.UI;
using Deucarian.ViewerRendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Standard display-settings body shared by Deucarian viewers. It owns UI
    /// presentation and delegates all rendering behavior through the injected
    /// controller contract.
    /// </summary>
    public sealed class ViewerDisplaySettingsView : IDisposable
    {
        private readonly IViewerRenderingController controller;
        private readonly Label heading;
        private readonly Label renderingLabel;
        private readonly Label cameraRelativeLightState;
        private readonly DeucarianControlFeedback colorFeedback;
        private readonly DeucarianControlFeedback realisticFeedback;
        private readonly DeucarianControlFeedback lightFeedback;
        private bool suppressControlEvents;
        private bool disposed;

        public ViewerDisplaySettingsView(
            IViewerRenderingController controller,
            MonoBehaviour animationHost = null,
            Func<bool> shouldAnimate = null)
        {
            this.controller = controller ??
                throw new ArgumentNullException(nameof(controller));
            Root = new VisualElement
            {
                name = ViewerShellElementNames.DisplaySettingsBody,
                pickingMode = PickingMode.Position
            };
            Root.style.flexDirection = FlexDirection.Column;

            heading = new Label("Display");
            heading.style.fontSize = 16f;
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            heading.style.marginBottom = 12f;

            renderingLabel = new Label("Rendering");
            renderingLabel.style.fontSize = 12f;
            renderingLabel.style.marginBottom = 6f;

            VisualElement modeRow = new VisualElement
            {
                name = "ViewerShellRenderingModeRow"
            };
            modeRow.style.flexDirection = FlexDirection.Row;
            modeRow.style.marginBottom = 12f;
            ColorFaithfulButton = CreateModeButton(
                ViewerShellElementNames.ColorFaithfulButton,
                "Color faithful");
            RealisticButton = CreateModeButton(
                ViewerShellElementNames.RealisticButton,
                "Realistic");
            ColorFaithfulButton.style.marginRight = 4f;
            RealisticButton.style.marginLeft = 4f;
            modeRow.Add(ColorFaithfulButton);
            modeRow.Add(RealisticButton);

            CameraRelativeLightToggle = new Toggle(
                "Light follows camera")
            {
                name = ViewerShellElementNames.CameraRelativeLightToggle,
                tooltip =
                    "Keep the key light aligned to the camera with a small local offset"
            };
            CameraRelativeLightToggle.style.marginBottom = 8f;
            cameraRelativeLightState = new Label("Off")
            {
                name = ViewerShellElementNames.CameraRelativeLightState,
                pickingMode = PickingMode.Ignore
            };
            DeucarianTextControlStyle.ConfigureToggle(
                CameraRelativeLightToggle, cameraRelativeLightState);
            colorFeedback = new DeucarianControlFeedback(animationHost, ColorFaithfulButton, shouldAnimate);
            realisticFeedback = new DeucarianControlFeedback(animationHost, RealisticButton, shouldAnimate);
            lightFeedback = new DeucarianControlFeedback(animationHost, CameraRelativeLightToggle, shouldAnimate);

            QualityNotice = new Label(
                "Full viewer effects are disabled for the active viewer quality tier.")
            {
                name = ViewerShellElementNames.ReducedQualityNotice,
                pickingMode = PickingMode.Ignore
            };
            QualityNotice.style.fontSize = 11f;
            QualityNotice.style.whiteSpace = WhiteSpace.Normal;

            Root.Add(heading);
            Root.Add(renderingLabel);
            Root.Add(modeRow);
            Root.Add(CameraRelativeLightToggle);
            Root.Add(QualityNotice);

            ColorFaithfulButton.clicked += OnColorFaithfulClicked;
            RealisticButton.clicked += OnRealisticClicked;
            CameraRelativeLightToggle.RegisterValueChangedCallback(
                OnCameraRelativeLightChanged);
            controller.SettingsChanged += OnSettingsChanged;
            ApplySnapshot(controller.CurrentSettings);
        }

        public VisualElement Root { get; }
        public Button ColorFaithfulButton { get; }
        public Button RealisticButton { get; }
        public Toggle CameraRelativeLightToggle { get; }
        public Label QualityNotice { get; }
        public ViewerDisplaySettingsSnapshot CurrentSnapshot { get; private set; }

        public event Action PresentationChanged;

        public void Refresh()
        {
            ThrowIfDisposed();
            ApplySnapshot(controller.CurrentSettings);
        }

        public void ApplyTheme(
            DeucarianTheme theme,
            Component context)
        {
            ThrowIfDisposed();
            DeucarianUIToolkitThemeTypography.Apply(Root, theme, context);
            Color primary = DeucarianControlIslandVisualStyle
                .ResolveTextColor(theme, context);
            Color secondary = DeucarianControlIslandVisualStyle
                .ResolveMutedTextColor(theme, context);
            heading.style.color = primary;
            renderingLabel.style.color = secondary;
            QualityNotice.style.color = secondary;
            colorFeedback.ApplyTheme(theme, context);
            realisticFeedback.ApplyTheme(theme, context);
            lightFeedback.ApplyTheme(theme, context);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            ColorFaithfulButton.clicked -= OnColorFaithfulClicked;
            RealisticButton.clicked -= OnRealisticClicked;
            CameraRelativeLightToggle.UnregisterValueChangedCallback(
                OnCameraRelativeLightChanged);
            controller.SettingsChanged -= OnSettingsChanged;
            colorFeedback.Dispose();
            realisticFeedback.Dispose();
            lightFeedback.Dispose();
            PresentationChanged = null;
            disposed = true;
        }

        internal void RequestRenderingMode(
            ViewerRenderingMode mode)
        {
            ThrowIfDisposed();
            controller.ApplyDisplaySettings(
                new ViewerDisplaySettingsRequest(mode, null),
                ViewerDisplaySettingsChangeSource.ViewerUi);
        }

        internal void RequestCameraRelativeLight(bool enabled)
        {
            ThrowIfDisposed();
            controller.ApplyDisplaySettings(
                new ViewerDisplaySettingsRequest(null, enabled),
                ViewerDisplaySettingsChangeSource.ViewerUi);
        }

        private void OnSettingsChanged(
            ViewerDisplaySettingsSnapshot snapshot,
            ViewerDisplaySettingsChangeSource source)
        {
            ApplySnapshot(snapshot);
        }

        private void ApplySnapshot(ViewerDisplaySettingsSnapshot snapshot)
        {
            CurrentSnapshot = snapshot;
            suppressControlEvents = true;
            CameraRelativeLightToggle.SetValueWithoutNotify(
                snapshot.CameraRelativeLight);
            cameraRelativeLightState.text =
                snapshot.CameraRelativeLight ? "On" : "Off";
            suppressControlEvents = false;
            QualityNotice.style.display = snapshot.EffectsActive
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            colorFeedback.SetSelected(snapshot.RenderingMode == ViewerRenderingMode.ColorFaithful);
            realisticFeedback.SetSelected(snapshot.RenderingMode == ViewerRenderingMode.Realistic);
            lightFeedback.SetSelected(snapshot.CameraRelativeLight);
            PresentationChanged?.Invoke();
        }

        private void OnColorFaithfulClicked()
        {
            RequestRenderingMode(ViewerRenderingMode.ColorFaithful);
        }

        private void OnRealisticClicked()
        {
            RequestRenderingMode(ViewerRenderingMode.Realistic);
        }

        private void OnCameraRelativeLightChanged(ChangeEvent<bool> evt)
        {
            if (!suppressControlEvents)
            {
                RequestCameraRelativeLight(evt.newValue);
            }
        }

        private static Button CreateModeButton(string name, string text)
        {
            Button button = new Button { name = name, text = text };
            button.style.flexGrow = 1f;
            button.style.minWidth = 0f;
            return button;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(ViewerDisplaySettingsView));
            }
        }
    }
}
