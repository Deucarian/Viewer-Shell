using System;

namespace Deucarian.ViewerShell
{
    /// <summary>
    /// Immutable state projected into the shared status toast and diagnostics
    /// body. Diagnostics text is already formatted by the consumer adapter.
    /// </summary>
    public readonly struct ViewerShellStatusSnapshot :
        IEquatable<ViewerShellStatusSnapshot>
    {
        public ViewerShellStatusSnapshot(
            ViewerShellStatusState state,
            string message,
            string diagnosticsText = null)
        {
            State = state;
            Message = message ?? string.Empty;
            DiagnosticsText = diagnosticsText ?? string.Empty;
        }

        public ViewerShellStatusState State { get; }
        public string Message { get; }
        public string DiagnosticsText { get; }

        public static ViewerShellStatusSnapshot Uninitialized(
            string message = null,
            string diagnosticsText = null) =>
            new ViewerShellStatusSnapshot(
                ViewerShellStatusState.Uninitialized,
                message,
                diagnosticsText);

        public static ViewerShellStatusSnapshot Loading(
            string message = null,
            string diagnosticsText = null) =>
            new ViewerShellStatusSnapshot(
                ViewerShellStatusState.Loading,
                message,
                diagnosticsText);

        public static ViewerShellStatusSnapshot Ready(
            string message = null,
            string diagnosticsText = null) =>
            new ViewerShellStatusSnapshot(
                ViewerShellStatusState.Ready,
                message,
                diagnosticsText);

        public static ViewerShellStatusSnapshot Error(
            string message,
            string diagnosticsText = null) =>
            new ViewerShellStatusSnapshot(
                ViewerShellStatusState.Error,
                message,
                diagnosticsText);

        public bool Equals(ViewerShellStatusSnapshot other)
        {
            return State == other.State &&
                   string.Equals(
                       Message,
                       other.Message,
                       StringComparison.Ordinal) &&
                   string.Equals(
                       DiagnosticsText,
                       other.DiagnosticsText,
                       StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is ViewerShellStatusSnapshot other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)State;
                hashCode = (hashCode * 397) ^ Message.GetHashCode();
                hashCode = (hashCode * 397) ^
                           DiagnosticsText.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(
            ViewerShellStatusSnapshot left,
            ViewerShellStatusSnapshot right) => left.Equals(right);

        public static bool operator !=(
            ViewerShellStatusSnapshot left,
            ViewerShellStatusSnapshot right) => !left.Equals(right);
    }
}
