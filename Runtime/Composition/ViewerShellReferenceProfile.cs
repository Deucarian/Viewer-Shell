namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Immutable resolved values defining the canonical Deucarian viewer
    /// shell. Consumers compare the shared profile identity for parity.
    /// </summary>
    public sealed class ViewerShellReferenceProfile
    {
        internal ViewerShellReferenceProfile()
        {
        }

        public string Id => "deucarian.viewer-shell.reference";
        public float EdgeMargin => 24f;
        public float StatusCardWidth => 390f;
        public float NarrowBreakpoint => 520f;
        public float MenuHorizontalGap => 12f;
        public float DiagnosticsMenuMaximumWidth => 340f;
        public float DiagnosticsExpandedFallbackHeight => 80f;
        public float DisplayMenuMaximumWidth => 300f;
        public float DisplayExpandedFallbackHeight => 196f;
        public float ReadyToastDurationSeconds => 3f;
        public float ReadyFadeDurationSeconds => 0.22f;
        public float SpinnerDegreesPerSecond => 210f;
        public string LoadingTitle => "Loading viewer";
        public string ReadyTitle => "Ready";
        public string ErrorTitle => "Something went wrong";
        public string DefaultLoadingMessage =>
            "Preparing the 3D experience\u2026";
        public string DefaultDiagnosticsText => "No viewer context";
    }
}
