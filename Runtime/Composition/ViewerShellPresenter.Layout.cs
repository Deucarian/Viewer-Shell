using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    public sealed partial class ViewerShellPresenter
    {
        private void ApplyResponsiveLayout()
        {
            if (statusPresenter?.Card == null || profile == null)
            {
                return;
            }

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyResponsiveLayout(ResolvePanelWidth());
        }

        internal void ApplyResponsiveLayout(float panelWidth)
        {
            if (statusPresenter?.Card == null || profile == null)
            {
                return;
            }

            VisualElement card = statusPresenter.Card;
            card.style.left = profile.EdgeMargin;
            card.style.top = profile.EdgeMargin;
            if (panelWidth < profile.NarrowBreakpoint)
            {
                card.style.right = profile.EdgeMargin;
                card.style.width = StyleKeyword.Auto;
            }
            else
            {
                card.style.right = StyleKeyword.Auto;
                card.style.width = profile.StatusCardWidth;
            }
        }

        private float ResolvePanelWidth()
        {
            float width = statusView?.Root?.contentRect.width ?? 0f;
            if (IsFinitePositive(width))
            {
                return width;
            }

            width = statusView?.Root?.resolvedStyle.width ?? 0f;
            return IsFinitePositive(width)
                ? width
                : Mathf.Max(0f, Screen.width);
        }

        private void OnStatusRootGeometryChanged(GeometryChangedEvent evt)
        {
            ApplyResponsiveLayout();
        }

        private static bool IsFinitePositive(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }
    }
}
