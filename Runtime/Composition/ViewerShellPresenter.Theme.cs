using Deucarian.Theming;

namespace Deucarian.ViewerShell
{
    public sealed partial class ViewerShellPresenter
    {
        private void BindThemeProvider()
        {
            if (ThemeProvider == null)
            {
                return;
            }

            ThemeProvider.ThemeChanged += OnThemeChanged;
            ThemeProvider.StyleChanged += OnStyleChanged;
        }

        private void UnbindThemeProvider()
        {
            if (ThemeProvider == null)
            {
                return;
            }

            ThemeProvider.ThemeChanged -= OnThemeChanged;
            ThemeProvider.StyleChanged -= OnStyleChanged;
        }

        private void OnThemeChanged(DeucarianTheme theme)
        {
            ApplyTheme();
        }

        private void OnStyleChanged(DeucarianThemeStyle style)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (!initialized && statusPresenter == null)
            {
                return;
            }

            DeucarianTheme theme = ThemeProvider != null
                ? ThemeProvider.CurrentTheme
                : DeucarianViewerReferenceThemePreset.Resolve().DefaultTheme;
            DeucarianThemeStyle style = ThemeProvider != null
                ? ThemeProvider.CurrentStyle
                : theme != null ? theme.VisualStyle : null;
            statusPresenter?.ApplyTheme(theme, style, this);
            diagnosticsView?.ApplyTheme(theme, this);
            displaySettingsView?.ApplyTheme(theme, this);
            RefreshMenuPresentation();
        }

        private void ApplyDiagnosticsTheme(DeucarianTheme theme)
        {
            diagnosticsView?.ApplyTheme(theme, this);
        }

        private void ApplyDisplaySettingsTheme(DeucarianTheme theme)
        {
            displaySettingsView?.ApplyTheme(theme, this);
        }
    }
}
