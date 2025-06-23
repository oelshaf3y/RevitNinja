using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

            if (!doc.getAccess()) return Result.Failed;

            //FilteredElementCollector fec =;
            List<CADLinkType> DWGs = new FilteredElementCollector(doc).OfClass(typeof(CADLinkType)).Cast<CADLinkType>().ToList();
            int count = 0;
            if (DWGs.Count() == 0) { TaskDialog.Show("Info", "No more DWG Imports In The Project."); return Result.Succeeded; }
            else
            {
                TaskDialogResult dia = doc.YesNoMessage($"Are You Sure You Want To Delete {DWGs.Count()} CAD Files?\nThis CAN NOT BE UNDONE!");
                if (dia == TaskDialogResult.No) return Result.Cancelled;
            }
            Transaction tr = new Transaction(doc, "Delete CAD Imports");
            tr.Start();
            foreach (CADLinkType cad in DWGs)
            {
                if (cad.Pinned) doc.print("pinned");
                try
                {
                    doc.Delete(cad.Id);
                    count++;
                }
                catch (Exception ex)
                {
                    //doc.print(ex.ToString());
                }
            }
            //doc.Delete(fec.Select(x => x.Id).ToArray());
            TaskDialog.Show("Done", $"Successfully deleted {count} CAD Files.");
            tr.Commit();
            tr.Dispose();
            return Result.Succeeded;
        }
    }
}
