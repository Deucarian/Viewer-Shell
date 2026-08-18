using System;
using Deucarian.Theming;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Integration hooks around the shared reference shell. Layout and motion
    /// values stay in the immutable profile; consumers provide only ownership
    /// boundaries such as theme and navigation input capture.
    /// </summary>
    public sealed class ViewerShellConfiguration
    {
        public ViewerShellReferenceProfile Profile { get; set; } =
            ViewerShellReferencePreset.Profile;
        public DeucarianThemeProvider ThemeProvider { get; set; }
        public bool ShowDiagnostics { get; set; } = true;
        public Func<bool> ShouldAnimate { get; set; }
        public Func<VisualElement, IDisposable> BindInputGuard { get; set; }

        internal ViewerShellReferenceProfile ResolveProfile()
        {
            return Profile ?? ViewerShellReferencePreset.Profile;
        }

        internal bool ResolveShouldAnimate()
        {
            return ShouldAnimate == null || ShouldAnimate();
        }
    }
}
