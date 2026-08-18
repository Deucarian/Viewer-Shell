using Deucarian.Theming;
using Deucarian.Theming.UIToolkit;
using Deucarian.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Consumer-neutral body hosted by the package-owned information menu.
    /// Products provide only already-formatted diagnostics text.
    /// </summary>
    public sealed class ViewerShellDiagnosticsView
    {
        public ViewerShellDiagnosticsView(string initialText = null)
        {
            Root = new VisualElement
            {
                name = ViewerShellElementNames.DiagnosticsBody,
                pickingMode = PickingMode.Ignore
            };
            Root.style.flexDirection = FlexDirection.Column;

            ContextLabel = new Label
            {
                name = ViewerShellElementNames.DiagnosticsContext,
                pickingMode = PickingMode.Ignore
            };
            ContextLabel.style.fontSize = 12f;
            ContextLabel.style.whiteSpace = WhiteSpace.Normal;
            ContextLabel.style.marginBottom = 0f;
            Root.Add(ContextLabel);
            SetText(initialText);
        }

        public VisualElement Root { get; }
        public Label ContextLabel { get; }

        public void SetText(string value)
        {
            ContextLabel.text = string.IsNullOrWhiteSpace(value)
                ? ViewerShellReferencePreset.Profile.DefaultDiagnosticsText
                : value;
        }

        public void ApplyTheme(
            DeucarianTheme theme,
            Component context)
        {
            DeucarianUIToolkitThemeTypography.Apply(
                Root,
                theme,
                context);
            ContextLabel.style.color =
                DeucarianControlIslandVisualStyle.ResolveMutedTextColor(
                    theme,
                    context);
        }
    }
}
