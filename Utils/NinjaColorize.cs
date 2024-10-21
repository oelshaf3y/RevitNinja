using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace RevitNinja.Utils
{
    internal static class NinjaColorize
    {
        public static Brush border, background, text;
        public static void Colorize(this UserControl uc, UIFramework.ApplicationTheme theme)
        {
            border = theme.RibbonTheme.Ribbon.MainTab.PanelSeparatorBrush;
            background = theme.RibbonTheme.Ribbon.MainTab.PanelContentBackground;
            text = theme.RibbonTheme.Ribbon.MainTab.TabHeaderForeground;
            StackPanel stack = uc.FindName("stack") as StackPanel;
            //StackPanel stack = VisualUtils.FindVisualChild<StackPanel>(uc,"stack");
            uc.Background = background;
            uc.Foreground = text;
            VisualUtils.FindVisualChildren<Border>(stack).ForEach(x =>
            {
                x.Background = border;

            });
            VisualUtils.FindVisualChildren<TextBox>(stack).ForEach(x =>
            {
                x.Background = background;
                x.BorderBrush = border;
                x.Foreground = text;
                x.TextAlignment = System.Windows.TextAlignment.Center;

            });
            VisualUtils.FindVisualChildren<ComboBox>(stack).ForEach(x =>
            {
                //x.SelectionChanged += SelectionChanged;
                double width = 100;
                foreach (ComboBoxItem item in x.Items)
                {
                    //item.Background = background;
                    //item.Foreground = text;
                    item.BorderThickness = new System.Windows.Thickness(0, 0, 0, 0);
                    if (item.Content.ToString().Length * 7 > width) width = item.Content.ToString().Length * 7;
                }

                x.Width = width;
            });
            VisualUtils.FindVisualChildren<Button>(stack).ForEach(x =>
            {
                x.Background = background;
                x.BorderBrush = border;
                x.Foreground = text;
                x.MouseEnter += Button_MouseEnter;
                x.MouseLeave += Button_MouseLeave;
            });
            VisualUtils.FindVisualChildren<CheckBox>(stack).ForEach(x =>
            {
                x.Background = background;
                x.BorderBrush = border;
                x.Foreground = text;
            });
        }
        //private static void SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    // Get the selected item
        //    ComboBoxItem selectedItem = (ComboBoxItem)(sender as ComboBox).SelectedItem;
        //    foreach (ComboBoxItem item in (sender as ComboBox).Items)
        //    {
        //        item.Background = background;
        //        item.Foreground = text;
        //        item.BorderThickness = new System.Windows.Thickness(0, 0, 0, 0);
        //    }
        //    if (selectedItem != null)
        //    {
        //        // Set the background and foreground for the selected item
        //        selectedItem.Background =text;
        //        selectedItem.Foreground = background;
        //    }
        //}
        private static void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            btn.Foreground = new BrushConverter().ConvertFromString("#000000") as SolidColorBrush;
        }

        private static void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            btn.Background = NinjaColorize.background;
            btn.Foreground = NinjaColorize.text;
        }
    }
}
