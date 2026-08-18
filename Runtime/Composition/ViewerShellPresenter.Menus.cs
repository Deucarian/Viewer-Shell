using Deucarian.Theming;
using Deucarian.UI;

namespace Deucarian.ViewerShell
{
    public sealed partial class ViewerShellPresenter
    {
        private void BuildMenus()
        {
            DeucarianViewerMenuClusterLayout layout = CreateClusterLayout();
            if (diagnosticsView != null)
            {
                menuCluster = new DeucarianViewerMenuCluster(
                    this,
                    diagnosticsView.Root,
                    displaySettingsView.Root,
                    layout);
                menuCluster.ExpandedChanged += OnMenuExpandedChanged;
                return;
            }

            settingsMenu = new DeucarianMorphingMenu(
                this,
                displaySettingsView.Root,
                new DeucarianMorphingMenuLayout
                {
                    EdgeMargin = profile.EdgeMargin,
                    RightInset = profile.EdgeMargin,
                    CollapsedIcon = DeucarianMorphingMenuIcon.Settings,
                    MaximumWidth = profile.DisplayMenuMaximumWidth,
                    ExpandedFallbackHeight =
                        profile.DisplayExpandedFallbackHeight,
                    OpenTooltip = "Display settings",
                    CloseTooltip = "Close display settings",
                    ShouldAnimate = configuration.ResolveShouldAnimate,
                    BindInputGuard = configuration.BindInputGuard,
                    ThemeProvider = ThemeProvider,
                    ApplyBodyTheme = ApplyDisplaySettingsTheme,
                    ThemeContext = this
                });
            settingsMenu.ExpandedChanged +=
                OnStandaloneSettingsExpandedChanged;
        }

        private DeucarianViewerMenuClusterLayout CreateClusterLayout()
        {
            return new DeucarianViewerMenuClusterLayout
            {
                EdgeMargin = profile.EdgeMargin,
                HorizontalGap = profile.MenuHorizontalGap,
                InformationMaximumWidth =
                    profile.DiagnosticsMenuMaximumWidth,
                SettingsMaximumWidth = profile.DisplayMenuMaximumWidth,
                InformationExpandedFallbackHeight =
                    profile.DiagnosticsExpandedFallbackHeight,
                SettingsExpandedFallbackHeight =
                    profile.DisplayExpandedFallbackHeight,
                OpenInformationTooltip = "Show viewer diagnostics",
                CloseInformationTooltip = "Close viewer diagnostics",
                OpenSettingsTooltip = "Display settings",
                CloseSettingsTooltip = "Close display settings",
                ShouldAnimate = configuration.ResolveShouldAnimate,
                BindInputGuard = configuration.BindInputGuard,
                ThemeProvider = ThemeProvider,
                ApplyInformationBodyTheme = ApplyDiagnosticsTheme,
                ApplySettingsBodyTheme = ApplyDisplaySettingsTheme,
                ThemeContext = this
            };
        }

        private void RefreshMenuPresentation()
        {
            menuCluster?.RefreshPresentation();
            settingsMenu?.RefreshPresentation();
        }

        private void OnMenuExpandedChanged(
            DeucarianViewerMenuKind kind,
            bool expanded)
        {
            if (kind == DeucarianViewerMenuKind.Information)
            {
                DiagnosticsExpandedChanged?.Invoke(expanded);
            }
            else
            {
                DisplaySettingsExpandedChanged?.Invoke(expanded);
            }
        }

        private void OnStandaloneSettingsExpandedChanged(bool expanded)
        {
            DisplaySettingsExpandedChanged?.Invoke(expanded);
        }
    }
}
