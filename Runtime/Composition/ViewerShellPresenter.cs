using System;
using Deucarian.Theming;
using Deucarian.UI;
using Deucarian.ViewerRendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Runtime owner of the shared viewer shell. Product composition roots
    /// adapt lifecycle state and commands, while this component owns all
    /// reusable shell presentation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class ViewerShellPresenter :
        MonoBehaviour,
        IDisposable
    {
        private ViewerShellConfiguration configuration;
        private ViewerShellReferenceProfile profile;
        private ViewerShellStatusView statusView;
        private ViewerShellStatusPresenter statusPresenter;
        private ViewerShellDiagnosticsView diagnosticsView;
        private ViewerDisplaySettingsView displaySettingsView;
        private DeucarianViewerMenuCluster menuCluster;
        private DeucarianMorphingMenu settingsMenu;
        private UIDocument statusDocument;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;
        private bool initialized;
        private bool disposed;

        public event Action<bool> DiagnosticsExpandedChanged;
        public event Action<bool> DisplaySettingsExpandedChanged;

        public DeucarianThemeProvider ThemeProvider { get; private set; }
        public IViewerRenderingController RenderingController
        {
            get;
            private set;
        }
        public ViewerShellReferenceProfile Profile => profile;
        public UIDocument StatusDocument => statusDocument;
        public ViewerShellDiagnosticsView DiagnosticsView => diagnosticsView;
        public ViewerDisplaySettingsView DisplaySettingsView =>
            displaySettingsView;
        public DeucarianViewerMenuCluster MenuCluster => menuCluster;
        public DeucarianMorphingMenu DiagnosticsMenu =>
            menuCluster?.InformationMenu;
        public DeucarianMorphingMenu DisplaySettingsMenu =>
            menuCluster != null ? menuCluster.SettingsMenu : settingsMenu;
        public VisualElement StatusCard => statusPresenter?.Card;
        public bool IsDiagnosticsExpanded =>
            menuCluster?.ExpandedMenu ==
            DeucarianViewerMenuKind.Information;
        public bool IsDisplaySettingsExpanded =>
            menuCluster != null
                ? menuCluster.ExpandedMenu ==
                  DeucarianViewerMenuKind.Settings
                : settingsMenu?.IsExpanded == true;

        public void Initialize(
            IViewerRenderingController renderingController,
            ViewerShellConfiguration value = null)
        {
            if (renderingController == null)
            {
                throw new ArgumentNullException(
                    nameof(renderingController));
            }

            TearDownPresentation();
            disposed = false;
            configuration = value ??
                ViewerShellReferenceComposition.CreateConfiguration();
            profile = configuration.ResolveProfile();
            RenderingController = renderingController;
            ThemeProvider =
                ViewerShellReferenceComposition.EnsureThemeProvider(
                    ResolveThemeHost(),
                    configuration.ThemeProvider);
            EnsureStatusDocument();
            statusView = ViewerShellStatusViewFactory.Build(
                statusDocument,
                configuration.BindInputGuard);
            statusView.Root.RegisterCallback<GeometryChangedEvent>(
                OnStatusRootGeometryChanged);
            statusPresenter = new ViewerShellStatusPresenter(
                this,
                statusView,
                profile,
                configuration.ResolveShouldAnimate,
                ApplyResponsiveLayout);
            diagnosticsView = configuration.ShowDiagnostics
                ? new ViewerShellDiagnosticsView(
                    profile.DefaultDiagnosticsText)
                : null;
            displaySettingsView = new ViewerDisplaySettingsView(
                renderingController);
            displaySettingsView.PresentationChanged +=
                RefreshMenuPresentation;
            BuildMenus();
            BindThemeProvider();
            ApplyTheme();
            ApplyResponsiveLayout();
            initialized = true;
            ApplyStatus(ViewerShellStatusSnapshot.Uninitialized());
        }

        public void ApplyStatus(ViewerShellStatusSnapshot snapshot)
        {
            ThrowIfUnavailable();
            EnsureStatusViewAttached();
            statusPresenter.Apply(snapshot);
            diagnosticsView?.SetText(snapshot.DiagnosticsText);
            ApplyResponsiveLayout();
            ApplyTheme();
        }

        public void SetStatusPresentationSuppressed(bool suppressed)
        {
            ThrowIfUnavailable();
            EnsureStatusViewAttached();
            statusPresenter.SetPresentationSuppressed(suppressed);
        }

        public void SetDiagnosticsExpanded(
            bool expanded,
            bool animate = true)
        {
            ThrowIfUnavailable();
            if (menuCluster != null)
            {
                menuCluster.SetExpanded(
                    DeucarianViewerMenuKind.Information,
                    expanded,
                    true,
                    animate);
            }
        }

        public void SetDisplaySettingsExpanded(
            bool expanded,
            bool animate = true)
        {
            ThrowIfUnavailable();
            if (menuCluster != null)
            {
                menuCluster.SetExpanded(
                    DeucarianViewerMenuKind.Settings,
                    expanded,
                    true,
                    animate);
                return;
            }

            settingsMenu?.SetExpanded(expanded, true, animate);
        }

        public void CollapseMenus(bool animate = true)
        {
            ThrowIfUnavailable();
            if (menuCluster != null)
            {
                menuCluster.CollapseAll(true, animate);
                return;
            }

            settingsMenu?.SetExpanded(false, true, animate);
        }

        public void SetMenusVisible(bool visible)
        {
            ThrowIfUnavailable();
            if (menuCluster != null)
            {
                menuCluster.SetVisible(visible);
            }
            else
            {
                settingsMenu?.SetVisible(visible);
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            TearDownPresentation();
            DiagnosticsExpandedChanged = null;
            DisplaySettingsExpandedChanged = null;
            disposed = true;
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            statusPresenter?.UpdateSpinner(Time.unscaledDeltaTime);
            if (lastScreenWidth != Screen.width ||
                lastScreenHeight != Screen.height)
            {
                ApplyResponsiveLayout();
            }
        }

        private void OnDisable()
        {
            if (!initialized)
            {
                return;
            }

            statusPresenter?.OnDisable();
            menuCluster?.OnDisable();
            settingsMenu?.OnDisable();
            if (statusDocument != null)
            {
                statusDocument.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!initialized)
            {
                return;
            }

            if (statusDocument != null)
            {
                statusDocument.enabled = true;
                EnsureStatusViewAttached();
            }

            menuCluster?.OnEnable();
            settingsMenu?.OnEnable();
            statusPresenter?.OnEnable();
            ApplyResponsiveLayout();
            ApplyTheme();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private GameObject ResolveThemeHost()
        {
            Transform parent = transform.parent;
            return parent != null ? parent.gameObject : gameObject;
        }

        private void EnsureStatusDocument()
        {
            statusDocument = GetComponent<UIDocument>();
            if (statusDocument == null)
            {
                statusDocument = gameObject.AddComponent<UIDocument>();
            }

            DeucarianUIRuntime.Configure(
                statusDocument,
                DeucarianUISurfaceRole.Status);
            statusDocument.enabled = isActiveAndEnabled;
        }

        private void EnsureStatusViewAttached()
        {
            if (statusDocument == null || statusView?.Root == null)
            {
                return;
            }

            VisualElement documentRoot =
                statusDocument.rootVisualElement;
            if (statusView.Root.parent == documentRoot)
            {
                return;
            }

            ViewerShellStatusViewFactory.Attach(
                statusDocument,
                statusView,
                configuration.BindInputGuard);
        }

        private void TearDownPresentation()
        {
            initialized = false;
            UnbindThemeProvider();
            if (menuCluster != null)
            {
                menuCluster.ExpandedChanged -= OnMenuExpandedChanged;
                menuCluster.Dispose();
                menuCluster = null;
            }

            if (settingsMenu != null)
            {
                settingsMenu.ExpandedChanged -=
                    OnStandaloneSettingsExpandedChanged;
                settingsMenu.Dispose();
                settingsMenu = null;
            }

            if (displaySettingsView != null)
            {
                displaySettingsView.PresentationChanged -=
                    RefreshMenuPresentation;
                displaySettingsView.Dispose();
                displaySettingsView = null;
            }

            diagnosticsView = null;
            statusPresenter?.Dispose();
            statusPresenter = null;
            if (statusView != null)
            {
                statusView.Root?.UnregisterCallback<GeometryChangedEvent>(
                    OnStatusRootGeometryChanged);
                statusView.Dispose();
                statusView = null;
            }

            ThemeProvider = null;
            RenderingController = null;
        }

        private void ThrowIfUnavailable()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(ViewerShellPresenter));
            }

            if (!initialized)
            {
                throw new InvalidOperationException(
                    "Viewer Shell has not been initialized.");
            }
        }

    }
}
