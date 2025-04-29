using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using RevitNinja.Utils;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class DeleteCAD : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess())
            {
                doc.print("Please contact the developer");
                return Result.Failed;
            }

            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(CADLinkType));
            int count = fec.Count();
            if (count == 0) { TaskDialog.Show("Info", "No more DWG Imports In The Project."); return Result.Succeeded; }
            else
            {
                TaskDialogResult dia = doc.YesNoMessage($"Are You Sure You Want To Delete {count} CAD Files?\nThis CAN NOT BE UNDONE!");
                if (dia == TaskDialogResult.No) return Result.Cancelled;
            }
            Transaction tr = new Transaction(doc, "Delete CAD Imports");
            tr.Start();
            doc.Delete(fec.Select(x => x.Id).ToArray());
            TaskDialog.Show("Done", $"Successfully deleted {count} CAD Files.");
            tr.Commit();
            tr.Dispose();
            return Result.Succeeded;
        }
    }
}
