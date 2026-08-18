using System;
using Deucarian.ViewerRendering;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell.Tests
{
    public sealed class ViewerShellContractTests
    {
        [Test]
        public void ReferenceProfilePreservesExactDonorValues()
        {
            ViewerShellReferenceProfile profile =
                ViewerShellReferencePreset.Profile;

            Assert.AreSame(profile, ViewerShellReferencePreset.Profile);
            Assert.AreEqual("deucarian.viewer-shell.reference", profile.Id);
            Assert.AreEqual(24f, profile.EdgeMargin);
            Assert.AreEqual(390f, profile.StatusCardWidth);
            Assert.AreEqual(520f, profile.NarrowBreakpoint);
            Assert.AreEqual(12f, profile.MenuHorizontalGap);
            Assert.AreEqual(340f, profile.DiagnosticsMenuMaximumWidth);
            Assert.AreEqual(80f, profile.DiagnosticsExpandedFallbackHeight);
            Assert.AreEqual(300f, profile.DisplayMenuMaximumWidth);
            Assert.AreEqual(196f, profile.DisplayExpandedFallbackHeight);
            Assert.AreEqual(3f, profile.ReadyToastDurationSeconds);
            Assert.AreEqual(0.22f, profile.ReadyFadeDurationSeconds);
            Assert.AreEqual(210f, profile.SpinnerDegreesPerSecond);
        }

        [Test]
        public void ReferenceConfigurationRetainsSharedProfileAndHooks()
        {
            Func<bool> motion = () => false;
            Func<VisualElement, IDisposable> input = root => null;

            ViewerShellConfiguration configuration =
                ViewerShellReferenceComposition.CreateConfiguration(
                    shouldAnimate: motion,
                    bindInputGuard: input,
                    showDiagnostics: false);

            Assert.AreSame(
                ViewerShellReferencePreset.Profile,
                configuration.Profile);
            Assert.AreSame(motion, configuration.ShouldAnimate);
            Assert.AreSame(input, configuration.BindInputGuard);
            Assert.IsFalse(configuration.ShowDiagnostics);
            Assert.IsFalse(configuration.ResolveShouldAnimate());
        }

        [Test]
        public void StatusSnapshotIsImmutableAndValueComparable()
        {
            ViewerShellStatusSnapshot first =
                ViewerShellStatusSnapshot.Loading("Loading", "context=one");
            ViewerShellStatusSnapshot same =
                ViewerShellStatusSnapshot.Loading("Loading", "context=one");
            ViewerShellStatusSnapshot changed =
                ViewerShellStatusSnapshot.Ready("Loading", "context=one");

            Assert.AreEqual(first, same);
            Assert.IsTrue(first == same);
            Assert.IsTrue(first != changed);
            Assert.AreEqual(ViewerShellStatusState.Loading, first.State);
            Assert.AreEqual("Loading", first.Message);
            Assert.AreEqual("context=one", first.DiagnosticsText);
        }

        [Test]
        public void EmptyLoadingMessageUsesReferenceCopyOnlyForLoadingStates()
        {
            ViewerShellReferenceProfile profile =
                ViewerShellReferencePreset.Profile;

            Assert.AreEqual(
                profile.DefaultLoadingMessage,
                ViewerShellStatusPresenter.ResolveMessage(
                    ViewerShellStatusSnapshot.Loading(),
                    profile));
            Assert.AreEqual(
                profile.DefaultLoadingMessage,
                ViewerShellStatusPresenter.ResolveMessage(
                    ViewerShellStatusSnapshot.Uninitialized(),
                    profile));
            Assert.AreEqual(
                string.Empty,
                ViewerShellStatusPresenter.ResolveMessage(
                    ViewerShellStatusSnapshot.Ready(),
                    profile));
            Assert.AreEqual(
                "Failure",
                ViewerShellStatusPresenter.ResolveMessage(
                    ViewerShellStatusSnapshot.Error("Failure"),
                    profile));
        }

        [Test]
        public void DisplayViewUsesAuthoritativeRenderingContract()
        {
            var controller = new FakeRenderingController(
                new ViewerDisplaySettingsSnapshot(
                    ViewerRenderingMode.ColorFaithful,
                    false,
                    true));
            var view = new ViewerDisplaySettingsView(controller);
            try
            {
                Assert.AreEqual(
                    ViewerRenderingMode.ColorFaithful,
                    view.CurrentSnapshot.RenderingMode);
                Assert.AreEqual(
                    DisplayStyle.None,
                    view.QualityNotice.style.display.value);

                view.RequestRenderingMode(ViewerRenderingMode.Realistic);

                Assert.AreEqual(
                    ViewerDisplaySettingsChangeSource.ViewerUi,
                    controller.LastSource);
                Assert.AreEqual(
                    ViewerRenderingMode.Realistic,
                    controller.LastRequest.RenderingMode);
                Assert.AreEqual(
                    ViewerRenderingMode.Realistic,
                    view.CurrentSnapshot.RenderingMode);

                view.RequestCameraRelativeLight(true);
                Assert.AreEqual(true, controller.LastRequest.CameraRelativeLight);
                Assert.IsTrue(view.CameraRelativeLightToggle.value);

                controller.Publish(
                    new ViewerDisplaySettingsSnapshot(
                        ViewerRenderingMode.Realistic,
                        true,
                        false),
                    ViewerDisplaySettingsChangeSource.QualityChange);
                Assert.AreEqual(
                    DisplayStyle.Flex,
                    view.QualityNotice.style.display.value);
            }
            finally
            {
                view.Dispose();
            }
        }

        internal sealed class FakeRenderingController :
            IViewerRenderingController
        {
            public FakeRenderingController(
                ViewerDisplaySettingsSnapshot initial)
            {
                CurrentSettings = initial;
            }

            public event Action<
                ViewerDisplaySettingsSnapshot,
                ViewerDisplaySettingsChangeSource> SettingsChanged;

            public ViewerDisplaySettingsSnapshot CurrentSettings
            {
                get;
                private set;
            }

            public ViewerDisplaySettingsRequest LastRequest { get; private set; }
            public ViewerDisplaySettingsChangeSource LastSource
            {
                get;
                private set;
            }

            public void ApplyDisplaySettings(
                ViewerDisplaySettingsRequest request,
                ViewerDisplaySettingsChangeSource source)
            {
                LastRequest = request;
                LastSource = source;
                Publish(
                    new ViewerDisplaySettingsSnapshot(
                        request.RenderingMode ??
                            CurrentSettings.RenderingMode,
                        request.CameraRelativeLight ??
                            CurrentSettings.CameraRelativeLight,
                        CurrentSettings.EffectsActive),
                    source);
            }

            public void Publish(
                ViewerDisplaySettingsSnapshot snapshot,
                ViewerDisplaySettingsChangeSource source)
            {
                CurrentSettings = snapshot;
                SettingsChanged?.Invoke(snapshot, source);
            }
        }
    }
}
