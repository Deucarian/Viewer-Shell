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
        private bool suppressControlEvents;
        private bool disposed;

        public ViewerDisplaySettingsView(
            IViewerRenderingController controller)
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
            CameraRelativeLightToggle.style.minHeight = 28f;
            CameraRelativeLightToggle.style.marginBottom = 8f;
            CameraRelativeLightToggle.style.paddingLeft = 10f;
            CameraRelativeLightToggle.style.paddingRight = 10f;
            CameraRelativeLightToggle.style.paddingTop = 4f;
            CameraRelativeLightToggle.style.paddingBottom = 4f;
            ApplyRadius(CameraRelativeLightToggle, 8f);

            cameraRelativeLightState = new Label("Off")
            {
                name = ViewerShellElementNames.CameraRelativeLightState,
                pickingMode = PickingMode.Ignore
            };
            cameraRelativeLightState.style.position = Position.Absolute;
            cameraRelativeLightState.style.right = 10f;
            cameraRelativeLightState.style.top = 6f;
            cameraRelativeLightState.style.unityFontStyleAndWeight =
                FontStyle.Bold;
            CameraRelativeLightToggle.Add(cameraRelativeLightState);

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
            Color normal = DeucarianControlIslandTheme.ResolveColor(
                theme,
                DeucarianBuiltinColorRoleIds.UiNormal,
                Color.clear);
            Color selected = DeucarianControlIslandTheme.ResolveColor(
                theme,
                DeucarianBuiltinColorRoleIds.UiSelected,
                normal);
            Color border = DeucarianGlassPanelStyle.ResolveBorder(
                theme,
                context);

            heading.style.color = primary;
            renderingLabel.style.color = secondary;
            CameraRelativeLightToggle.style.color = primary;
            cameraRelativeLightState.style.color = primary;
            CameraRelativeLightToggle.style.backgroundColor =
                CurrentSnapshot.CameraRelativeLight ? selected : normal;
            SetBorder(CameraRelativeLightToggle, border, 1f);
            QualityNotice.style.color = secondary;
            ApplyModeButton(
                ColorFaithfulButton,
                CurrentSnapshot.RenderingMode ==
                    ViewerRenderingMode.ColorFaithful,
                primary,
                normal,
                selected,
                border);
            ApplyModeButton(
                RealisticButton,
                CurrentSnapshot.RenderingMode ==
                    ViewerRenderingMode.Realistic,
                primary,
                normal,
                selected,
                border);
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
            button.style.height = 36f;
            button.style.minHeight = 36f;
            button.style.paddingLeft = 8f;
            button.style.paddingRight = 8f;
            button.style.fontSize = 12f;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            ApplyRadius(button, 8f);
            return button;
        }

        private static void ApplyModeButton(
            Button button,
            bool selected,
            Color text,
            Color normal,
            Color selectedColor,
            Color border)
        {
            button.style.color = text;
            button.style.backgroundColor = selected
                ? selectedColor
                : normal;
            SetBorder(button, border, 1f);
        }

        private static void ApplyRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private static void SetBorder(
            VisualElement element,
            Color color,
            float width)
        {
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
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
