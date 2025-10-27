using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Text.Json;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Media;
using UIFramework;
using System.Media;

namespace RevitNinja.Commands.ViewState
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class SaveState : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess())  return Result.Failed; 

            //DataStorage storage =;
            View activeView = doc.ActiveView;
            StringBuilder sb = new StringBuilder();
            List<Element> onlyVisible = new List<Element>();
            if (activeView is ViewSheet || activeView is ViewSchedule)
            {
                doc.print("Active view must be a view not a sheet or a schedule");
                return Result.Failed;
            }

            #region Using Parameter
            //Parameter state = activeView.LookupParameter("View State");
            //if (state == null)
            //{
            //    if (!
            //        Ninja.assignParameter(
            //            commandData,
            //            "Ninja-Views",
            //            "View State",
            //            BuiltInCategory.OST_Views,
            //            SpecTypeId.String.Text)
            //        )
            //    {
            //        doc.print("Please Create a new Project Parameter (Instance Parameter) for views\nParameter Name: View State\nType: Text");
            //        return Result.Failed;
            //    }
            //    else
            //    {
            //        state = activeView.LookupParameter("View State");
            //        if (state == null)
            //        {
            //            doc.print("Please Create a new Project Parameter (Instance Parameter) for views\nParameter Name: View State\nType: Text");
            //        }
            //    }
            //}
            //if (state.IsReadOnly)
            //{
            //    doc.print("make sure the View State Parameter is editable!");
            //    return Result.Failed;
            //}
            //if (state.AsString() != null)
            //{
            //    if (state.AsString().Trim().Length > 0)
            //    {
            //        if (doc.YesNoMessage("View State is saved already!.\nAre you sure you want to update?") == TaskDialogResult.No)
            //        {
            //            doc.print("Canceled");
            //            return Result.Cancelled;
            //        }
            //    }
            //}
            #endregion

            FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id).WhereElementIsNotElementType();
            onlyVisible = collector.Where(x => x.CanBeHidden(activeView)).ToList();
            collector.Where(x => x.IsHidden(activeView)).ToList().ForEach(x => onlyVisible.Remove(x));
            List<string> ids = onlyVisible.Select(x => x.Id.ToString()).ToList();

            #region Using DataStorage
            foreach (ElementId id in onlyVisible.Select(x => x.Id))
            {
                sb.Append(id.ToString() + ",");
            }
            #endregion
            using (TransactionGroup tg = new TransactionGroup(doc, "Save View State"))
            {
                tg.Start();
                //state.Set(sb.ToString());
                doc.saveViewState(sb.ToString(), activeView);
                tg.Assimilate();
                tg.Dispose();
            }
            ShowScreenFlash(0,0.7);


            return Result.Succeeded;
        }

        public void ShowScreenFlash(double from, double to)
        {
            var overlay = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.White,
                Opacity = 0,
                ShowInTaskbar = false,
                Topmost = true,
                WindowState = WindowState.Maximized,
            };

            overlay.Show();

            var anim1 = new DoubleAnimation
            {
                From = from,
                To = to / 3,
                AutoReverse = true,
                Duration = TimeSpan.FromMilliseconds(100)
            };

            var anim2 = new DoubleAnimation
            {
                From = to / 3,
                To = to,
                AutoReverse = true,
                Duration = TimeSpan.FromMilliseconds(100)
            };

            anim1.Completed += (s, e) =>
            {
                // Run second animation
                overlay.BeginAnimation(Window.OpacityProperty, anim2);
            };

            anim2.Completed += (s, e) =>
            {
                overlay.Close();
            };

            // Start first animation
            overlay.BeginAnimation(Window.OpacityProperty, anim1);
        }

    }

}
