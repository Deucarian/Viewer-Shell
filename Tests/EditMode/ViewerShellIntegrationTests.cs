using System;
using Deucarian.Theming;
using Deucarian.UI;
using Deucarian.ViewerRendering;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Deucarian.ViewerShell.Tests
{
    public sealed class ViewerShellIntegrationTests
    {
        private GameObject root;
        private ViewerShellPresenter presenter;
        private GuardCounter guards;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("ViewerShellIntegrationRoot");
            guards = new GuardCounter();
            var controller =
                new ViewerShellContractTests.FakeRenderingController(
                    new ViewerDisplaySettingsSnapshot(
                        ViewerRenderingMode.ColorFaithful,
                        false,
                        true));
            ViewerShellConfiguration configuration =
                ViewerShellReferenceComposition.CreateConfiguration(
                    shouldAnimate: () => false,
                    bindInputGuard: guards.Bind,
                    showDiagnostics: true);
            presenter = ViewerShellReferenceComposition.Install(
                root.transform,
                controller,
                configuration);
        }

        [TearDown]
        public void TearDown()
        {
            presenter?.Dispose();
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void InstallUsesOneReferenceThemeAndCanonicalDepths()
        {
            Assert.AreSame(
                ViewerShellReferencePreset.Profile,
                presenter.Profile);
            Assert.NotNull(presenter.ThemeProvider);
            Assert.AreEqual(
                DeucarianViewerReferenceThemePreset.FamilyId,
                presenter.ThemeProvider.CurrentThemeFamily.FamilyId);
            Assert.AreEqual(
                DeucarianViewerReferenceThemePreset.DefaultMode,
                presenter.ThemeProvider.ThemeMode);
            Assert.IsTrue(
                DeucarianUIRuntime.IsConfigured(
                    presenter.StatusDocument,
                    DeucarianUISurfaceRole.Status));
            Assert.IsTrue(
                DeucarianUIRuntime.IsConfigured(
                    presenter.DiagnosticsMenu.Document,
                    DeucarianUISurfaceRole.Menu));
            Assert.IsTrue(
                DeucarianUIRuntime.IsConfigured(
                    presenter.DisplaySettingsMenu.Document,
                    DeucarianUISurfaceRole.Menu));
            Assert.IsTrue(
                DeucarianUIRuntime.IsConfigured(
                    presenter.DiagnosticsMenu.RuntimeTooltip.OverlayDocument,
                    DeucarianUISurfaceRole.Tooltip));
            Assert.AreSame(
                presenter.DiagnosticsMenu.RuntimeTooltip.OverlayDocument,
                presenter.DisplaySettingsMenu.RuntimeTooltip.OverlayDocument);
            Assert.AreEqual(3, guards.BindCount);
        }

        [Test]
        public void TopRightMenusUseDonorSlotsAndRemainMutuallyExclusive()
        {
            Assert.AreEqual(
                DeucarianMorphingMenuIcon.Information,
                presenter.DiagnosticsMenu.CollapsedIcon);
            Assert.AreEqual(
                DeucarianMorphingMenuIcon.Settings,
                presenter.DisplaySettingsMenu.CollapsedIcon);
            Assert.AreEqual(76f, presenter.DiagnosticsMenu.RightInset);
            Assert.AreEqual(24f, presenter.DisplaySettingsMenu.RightInset);
            Assert.AreEqual(
                "Show viewer diagnostics",
                presenter.DiagnosticsMenu.Button.tooltip);
            Assert.AreEqual(
                "Display settings",
                presenter.DisplaySettingsMenu.Button.tooltip);

            presenter.SetDiagnosticsExpanded(true, false);

            Assert.IsTrue(presenter.IsDiagnosticsExpanded);
            Assert.IsFalse(presenter.IsDisplaySettingsExpanded);
            Assert.AreEqual(24f, presenter.DiagnosticsMenu.RightInset);
            Assert.IsTrue(presenter.DiagnosticsMenu.IsVisible);
            Assert.IsFalse(presenter.DisplaySettingsMenu.IsVisible);

            presenter.SetDisplaySettingsExpanded(true, false);

            Assert.IsFalse(presenter.IsDiagnosticsExpanded);
            Assert.IsTrue(presenter.IsDisplaySettingsExpanded);
            Assert.IsFalse(presenter.DiagnosticsMenu.IsVisible);
            Assert.IsTrue(presenter.DisplaySettingsMenu.IsVisible);

            presenter.CollapseMenus(false);
            Assert.AreEqual(76f, presenter.DiagnosticsMenu.RightInset);
            Assert.IsTrue(presenter.DiagnosticsMenu.IsVisible);
            Assert.IsTrue(presenter.DisplaySettingsMenu.IsVisible);
        }

        [Test]
        public void StatusMapsLoadingReadyAndErrorWithoutConsumerUi()
        {
            VisualElement documentRoot =
                presenter.StatusDocument.rootVisualElement;
            Label state = documentRoot.Q<Label>(
                ViewerShellElementNames.StatusState);
            Label message = documentRoot.Q<Label>(
                ViewerShellElementNames.StatusMessage);
            VisualElement spinner = documentRoot.Q(
                ViewerShellElementNames.LoadingSpinner);

            presenter.ApplyStatus(
                ViewerShellStatusSnapshot.Loading(
                    diagnosticsText: "asset=example"));
            Assert.AreEqual("Loading viewer", state.text);
            Assert.AreEqual(
                presenter.Profile.DefaultLoadingMessage,
                message.text);
            Assert.AreEqual(DisplayStyle.Flex, spinner.style.display.value);
            Assert.AreEqual(
                "asset=example",
                presenter.DiagnosticsView.ContextLabel.text);

            presenter.ApplyStatus(ViewerShellStatusSnapshot.Ready());
            Assert.AreEqual("Ready", state.text);
            Assert.AreEqual(string.Empty, message.text);
            Assert.AreEqual(DisplayStyle.None, spinner.style.display.value);
            Assert.IsTrue(
                presenter.ThemeProvider.CurrentTheme.TryGetColorById(
                    DeucarianBuiltinColorRoleIds.Success,
                    out Color readyColor));
            Assert.AreEqual(
                readyColor,
                documentRoot.Q(
                        ViewerShellElementNames.StatusIndicator)
                    .style.backgroundColor.value);

            presenter.ApplyStatus(
                ViewerShellStatusSnapshot.Error("Network unavailable"));
            Assert.AreEqual("Something went wrong", state.text);
            Assert.AreEqual("Network unavailable", message.text);
        }

        [Test]
        public void StatusLayoutUsesExactDesktopAndNarrowRules()
        {
            presenter.ApplyResponsiveLayout(519f);
            Assert.AreEqual(
                StyleKeyword.Auto,
                presenter.StatusCard.style.width.keyword);
            Assert.AreEqual(
                24f,
                presenter.StatusCard.style.right.value.value);

            presenter.ApplyResponsiveLayout(520f);
            Assert.AreEqual(
                390f,
                presenter.StatusCard.style.width.value.value);
            Assert.AreEqual(
                StyleKeyword.Auto,
                presenter.StatusCard.style.right.keyword);
            Assert.AreEqual(
                PickingMode.Position,
                presenter.StatusCard.pickingMode);
        }

        [Test]
        public void StatusSuppressionWaitsForTheNextSnapshotToRestore()
        {
            presenter.ApplyStatus(ViewerShellStatusSnapshot.Loading("One"));
            presenter.SetStatusPresentationSuppressed(true);
            Assert.AreEqual(
                DisplayStyle.None,
                presenter.StatusCard.style.display.value);

            presenter.SetStatusPresentationSuppressed(false);
            Assert.AreEqual(
                DisplayStyle.None,
                presenter.StatusCard.style.display.value);

            presenter.ApplyStatus(ViewerShellStatusSnapshot.Loading("Two"));
            Assert.AreEqual(
                DisplayStyle.Flex,
                presenter.StatusCard.style.display.value);
        }

        [Test]
        public void DisableAndEnableKeepsStatusPresenterReusable()
        {
            root.SetActive(false);
            root.SetActive(true);

            Assert.DoesNotThrow(() => presenter.ApplyStatus(
                ViewerShellStatusSnapshot.Error("Recovered")));
            Label message = presenter.StatusDocument.rootVisualElement.Q<Label>(
                ViewerShellElementNames.StatusMessage);
            Assert.AreEqual("Recovered", message.text);
            Assert.IsTrue(presenter.StatusDocument.enabled);
            Assert.AreEqual(4, guards.BindCount);
            Assert.AreEqual(1, guards.DisposeCount);
        }

        [Test]
        public void DisposeReleasesEveryInjectedInputGuard()
        {
            presenter.Dispose();

            Assert.AreEqual(3, guards.DisposeCount);
        }

        [Test]
        public void DiagnosticsCanBeOmittedWithoutChangingSettingsChrome()
        {
            var alternateRoot = new GameObject(
                "ViewerShellWithoutDiagnostics");
            var alternateGuards = new GuardCounter();
            ViewerShellPresenter alternate = null;
            try
            {
                var controller =
                    new ViewerShellContractTests.FakeRenderingController(
                        new ViewerDisplaySettingsSnapshot(
                            ViewerRenderingMode.ColorFaithful,
                            false,
                            true));
                alternate = ViewerShellReferenceComposition.Install(
                    alternateRoot.transform,
                    controller,
                    ViewerShellReferenceComposition.CreateConfiguration(
                        shouldAnimate: () => false,
                        bindInputGuard: alternateGuards.Bind,
                        showDiagnostics: false));

                Assert.IsNull(alternate.MenuCluster);
                Assert.IsNull(alternate.DiagnosticsMenu);
                Assert.IsNull(alternate.DiagnosticsView);
                Assert.NotNull(alternate.DisplaySettingsMenu);
                Assert.AreEqual(
                    DeucarianMorphingMenuIcon.Settings,
                    alternate.DisplaySettingsMenu.CollapsedIcon);
                Assert.AreEqual(24f, alternate.DisplaySettingsMenu.RightInset);
                Assert.AreEqual(2, alternateGuards.BindCount);
            }
            finally
            {
                alternate?.Dispose();
                Object.DestroyImmediate(alternateRoot);
            }

            Assert.AreEqual(2, alternateGuards.DisposeCount);
        }

        private sealed class GuardCounter
        {
            public int BindCount { get; private set; }
            public int DisposeCount { get; private set; }

            public IDisposable Bind(VisualElement rootElement)
            {
                Assert.NotNull(rootElement);
                BindCount++;
                return new CallbackDisposable(
                    () => DisposeCount++);
            }
        }

        private sealed class CallbackDisposable : IDisposable
        {
            private Action callback;

            public CallbackDisposable(Action callback)
            {
                this.callback = callback;
            }

            public void Dispose()
            {
                Action value = callback;
                callback = null;
                value?.Invoke();
            }
        }
    }
}
