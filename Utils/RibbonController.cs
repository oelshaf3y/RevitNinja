using UIFramework;
using System.Windows.Controls;
using System.Windows;
using Autodesk.Windows;

namespace RevitNinja.Utils
{

    public static class RibbonController
    {
        private static readonly Grid RootGrid;
        private static ContentPresenter _panelPresenter;
        private static readonly FrameworkElement InternalToolPanel;
        private static bool hideOther;

        static RibbonController()
        {
            RootGrid = VisualUtils.FindVisualParent<Grid>(ComponentManager.Ribbon, "rootGrid");
            if (RootGrid is null) throw new InvalidOperationException("Cannot find root grid in Revit UI");

            InternalToolPanel = VisualUtils.FindVisualChild<DialogBarControl>(RootGrid, string.Empty);
            if (InternalToolPanel is null) throw new InvalidOperationException("Cannot find internal tool panel in Revit UI");
            hideOther = true;
        }

        public static void ShowOptionsBar(FrameworkElement content, bool hide)
        {
            hideOther = hide;
            if (!(_panelPresenter is null))
            {
                _panelPresenter.Content = content;
                _panelPresenter.Visibility = Visibility.Visible;
                if (hideOther) InternalToolPanel.Height = 0;
                return;
            }

            _panelPresenter = CreateOptionsBar();
            _panelPresenter.Content = content;
            if (hideOther) InternalToolPanel.Height = 0;
        }

        public static void HideOptionsBar()
        {
            if (_panelPresenter is null) return;

            _panelPresenter.Content = null;
            _panelPresenter.Visibility = Visibility.Collapsed;
            if (hideOther) InternalToolPanel.Height = 26;
        }

        public static async void HideOptionsBar(TimeSpan delay)
        {
            await Task.Delay(delay);
            HideOptionsBar();
        }

        private static ContentPresenter CreateOptionsBar()
        {
            const int panelRow = 2;

            RootGrid.RowDefinitions.Insert(2, new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Auto)
            });

            foreach (UIElement child in RootGrid.Children)
            {
                var row = Grid.GetRow(child);
                if (row > 1) Grid.SetRow(child, row + 1);
            }

            var panelPresenter = new ContentPresenter();
            Grid.SetRow(panelPresenter, panelRow);
            RootGrid.Children.Add(panelPresenter);

            return panelPresenter;
        }
    }
}
