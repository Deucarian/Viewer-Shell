using System;
using Deucarian.Theming;
using Deucarian.ViewerRendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Canonical installation entry point. Every viewer uses this same profile,
    /// theme family, shell presenter, and integration boundary.
    /// </summary>
    public static class ViewerShellReferenceComposition
    {
        public static ViewerShellConfiguration CreateConfiguration(
            DeucarianThemeProvider themeProvider = null,
            Func<bool> shouldAnimate = null,
            Func<VisualElement, IDisposable> bindInputGuard = null,
            bool showDiagnostics = true)
        {
            return new ViewerShellConfiguration
            {
                Profile = ViewerShellReferencePreset.Profile,
                ThemeProvider = themeProvider,
                ShowDiagnostics = showDiagnostics,
                ShouldAnimate = shouldAnimate,
                BindInputGuard = bindInputGuard
            };
        }

        public static DeucarianThemeProvider EnsureThemeProvider(
            GameObject host,
            DeucarianThemeProvider preferred = null)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            DeucarianThemeProvider provider = preferred ??
                host.GetComponent<DeucarianThemeProvider>();
            if (provider == null)
            {
                provider = host.AddComponent<DeucarianThemeProvider>();
            }

            if (provider.CurrentTheme == null)
            {
                DeucarianViewerReferenceThemeProfile reference =
                    DeucarianViewerReferenceThemePreset.Resolve();
                provider.SetThemeFamily(
                    reference.ThemeFamily,
                    DeucarianViewerReferenceThemePreset.DefaultMode);
            }

            return provider;
        }

        public static ViewerShellPresenter Install(
            Transform parent,
            IViewerRenderingController renderingController,
            ViewerShellConfiguration configuration = null)
        {
            if (renderingController == null)
            {
                throw new ArgumentNullException(
                    nameof(renderingController));
            }

            ViewerShellPresenter presenter = parent != null
                ? parent.GetComponentInChildren<ViewerShellPresenter>(true)
                : null;
            if (presenter == null)
            {
                GameObject shellObject = new GameObject(
                    "DeucarianViewerShell");
                if (parent != null)
                {
                    shellObject.transform.SetParent(parent, false);
                }

                presenter = shellObject.AddComponent<ViewerShellPresenter>();
            }

            presenter.Initialize(renderingController, configuration);
            return presenter;
        }
    }
}
