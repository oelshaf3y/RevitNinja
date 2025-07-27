using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using UIFramework;
using Xceed.Wpf.AvalonDock.Controls;
using Autodesk.Revit.Attributes;
using Grid = System.Windows.Controls.Grid;
using Color = System.Windows.Media.Color;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ColorTabs : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        private Grid RootGrid;
        public static List<List<Color>> colors = new List<List<Color>>()
        {

            new List<Color>()
            {
                Color.FromRgb(244,244,248),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(111,111,133),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(134,115,70),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(254,215,102),Color.FromRgb(0, 0, 0)
            },

            new List<Color>()
            {
                Color.FromRgb(84,160,91),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(42,183,202),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(53,101,110),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(99,84,91),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(156,68,71),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(254,74,73),Color.FromRgb(0,0,0)
            },
        };
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            Application app = commandData.Application.Application;
            var appTheme = UIFramework.ApplicationTheme.CurrentTheme;
            appTheme.ActiveTabBackgroundColor = Colors.Coral;
            List<string> docs = new List<string>();
            var docPanes = FindVisualChildren<LayoutDocumentPaneControl>(MainWindow.getMainWnd());
            foreach (var pane in docPanes)
            {
                var tabs = FindVisualChildren<TabItem>(pane);
                docs = tabs.Select(x => x.ToolTip.ToString().Split('.').First()).ToList();
                foreach (var tab in tabs)
                {
                    string tabName = tab.ToolTip.ToString();
                    int docind = docs.IndexOf(tabName.Split('.').First());
                    if (docind > 9) docind = docind - 9;
                    tab.Background = new SolidColorBrush(colors[docind][0]);
                    tab.Foreground = new SolidColorBrush(colors[docind][1]);
                    //tab.BorderBrush
                }
            }
            return Result.Succeeded;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
