using System.Windows.Media.Animation;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace RevitNinja.Commands.ViewState
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ResetState : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            //if (!doc.getAccess()) return Result.Failed;

            View activeView = doc.ActiveView;
            if (activeView is ViewSheet || activeView is ViewSchedule)
            {
                doc.print("Active view must be a view not a sheet or a schedule");
                return Result.Failed;
            }

            doc.resetView(activeView);
            ShowScreenFlash();
            return Result.Succeeded;
        }

        public void ShowScreenFlash(double maxOpacity = 0.8)
        {
            var overlay = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Black,
                Opacity = 0,
                ShowInTaskbar = false,
                Topmost = true,
                WindowState = WindowState.Maximized,
            };

            overlay.Show();

            var flashAnim = new DoubleAnimation
            {
                From = 0,
                To = maxOpacity,
                Duration = TimeSpan.FromMilliseconds(300),
                AutoReverse = true,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            flashAnim.Completed += (s, e) => overlay.Close();

            overlay.BeginAnimation(Window.OpacityProperty, flashAnim);
        }


    }
}
