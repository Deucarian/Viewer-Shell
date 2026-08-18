using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.ViewerShell
{
    internal static class ViewerShellStatusViewFactory
    {
        public static ViewerShellStatusView Build(
            UIDocument document,
            Func<VisualElement, IDisposable> bindInputGuard)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            VisualElement documentRoot = document.rootVisualElement;
            documentRoot.Clear();
            documentRoot.pickingMode = PickingMode.Ignore;
            ApplyFullScreen(documentRoot);

            ViewerShellStatusView view = new ViewerShellStatusView
            {
                Root = CreateRoot()
            };
            BuildCard(view);
            Attach(document, view, bindInputGuard);
            return view;
        }

        public static void Attach(
            UIDocument document,
            ViewerShellStatusView view,
            Func<VisualElement, IDisposable> bindInputGuard)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            VisualElement documentRoot = document.rootVisualElement;
            view.InputGuard?.Dispose();
            view.InputGuard = null;
            view.Root?.RemoveFromHierarchy();
            documentRoot.Clear();
            documentRoot.pickingMode = PickingMode.Ignore;
            ApplyFullScreen(documentRoot);
            documentRoot.Add(view.Root);
            view.InputGuard = bindInputGuard?.Invoke(documentRoot);
        }

        private static VisualElement CreateRoot()
        {
            VisualElement root = new VisualElement
            {
                name = ViewerShellElementNames.StatusRoot,
                pickingMode = PickingMode.Ignore
            };
            root.AddToClassList("viewer-root");
            root.AddToClassList("viewer-shell-status-root");
            ApplyFullScreen(root);
            return root;
        }

        private static void BuildCard(ViewerShellStatusView view)
        {
            VisualElement card = new VisualElement
            {
                name = ViewerShellElementNames.StatusCard,
                pickingMode = PickingMode.Position
            };
            card.AddToClassList("viewer-panel");
            card.AddToClassList("viewer-panel-raised");
            card.AddToClassList("viewer-shell-status-card");
            card.style.position = Position.Absolute;
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Stretch;
            card.style.paddingLeft = 0f;
            card.style.paddingRight = 16f;
            card.style.paddingTop = 14f;
            card.style.paddingBottom = 14f;

            VisualElement indicator = new VisualElement
            {
                name = ViewerShellElementNames.StatusIndicator,
                pickingMode = PickingMode.Ignore
            };
            indicator.style.width = 4f;
            indicator.style.minWidth = 4f;
            indicator.style.marginRight = 14f;
            indicator.style.borderTopRightRadius = 2f;
            indicator.style.borderBottomRightRadius = 2f;

            VisualElement content = new VisualElement
            {
                name = "ViewerShellStatusContent",
                pickingMode = PickingMode.Ignore
            };
            content.style.flexGrow = 1f;
            content.style.flexShrink = 1f;

            VisualElement heading = new VisualElement
            {
                name = "ViewerShellStatusHeading",
                pickingMode = PickingMode.Ignore
            };
            heading.style.flexDirection = FlexDirection.Row;
            heading.style.alignItems = Align.Center;

            VisualElement spinner = CreateSpinner();
            Label stateLabel = new Label("Viewer")
            {
                name = ViewerShellElementNames.StatusState,
                pickingMode = PickingMode.Ignore
            };
            stateLabel.style.fontSize = 16f;
            stateLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            stateLabel.style.flexGrow = 1f;
            stateLabel.style.whiteSpace = WhiteSpace.Normal;

            Label messageLabel = new Label(string.Empty)
            {
                name = ViewerShellElementNames.StatusMessage,
                pickingMode = PickingMode.Ignore
            };
            messageLabel.style.fontSize = 13f;
            messageLabel.style.marginTop = 5f;
            messageLabel.style.whiteSpace = WhiteSpace.Normal;

            heading.Add(spinner);
            heading.Add(stateLabel);
            content.Add(heading);
            content.Add(messageLabel);
            card.Add(indicator);
            card.Add(content);
            view.Root.Add(card);

            view.Card = card;
            view.Indicator = indicator;
            view.Spinner = spinner;
            view.StateLabel = stateLabel;
            view.MessageLabel = messageLabel;
        }

        private static VisualElement CreateSpinner()
        {
            VisualElement spinner = new VisualElement
            {
                name = ViewerShellElementNames.LoadingSpinner,
                pickingMode = PickingMode.Ignore
            };
            spinner.style.width = 18f;
            spinner.style.height = 18f;
            spinner.style.minWidth = 18f;
            spinner.style.minHeight = 18f;
            spinner.style.marginRight = 10f;
            spinner.style.borderLeftWidth = 2f;
            spinner.style.borderRightWidth = 2f;
            spinner.style.borderTopWidth = 2f;
            spinner.style.borderBottomWidth = 2f;
            spinner.style.borderTopLeftRadius = 9f;
            spinner.style.borderTopRightRadius = 9f;
            spinner.style.borderBottomLeftRadius = 9f;
            spinner.style.borderBottomRightRadius = 9f;
            return spinner;
        }

        private static void ApplyFullScreen(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.right = 0f;
            element.style.top = 0f;
            element.style.bottom = 0f;
            element.style.width = Length.Percent(100f);
            element.style.height = Length.Percent(100f);
            element.style.backgroundColor = StyleKeyword.Null;
        }
    }
}
