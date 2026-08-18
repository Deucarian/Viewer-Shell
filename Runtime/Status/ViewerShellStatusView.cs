using System;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    internal sealed class ViewerShellStatusView : IDisposable
    {
        public IDisposable InputGuard { get; set; }
        public VisualElement Root { get; set; }
        public VisualElement Card { get; set; }
        public VisualElement Indicator { get; set; }
        public VisualElement Spinner { get; set; }
        public Label StateLabel { get; set; }
        public Label MessageLabel { get; set; }

        public void Dispose()
        {
            InputGuard?.Dispose();
            InputGuard = null;
            Root?.RemoveFromHierarchy();
        }
    }
}
