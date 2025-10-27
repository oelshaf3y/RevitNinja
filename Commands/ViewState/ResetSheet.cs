using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace RevitNinja.Commands.ViewState
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ResetSheet : IExternalCommand
    {
        public UIDocument uidoc { get; set; }
        public Document doc { get; set; }
        public StringBuilder sb { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess()) return Result.Failed;

            sb = new StringBuilder();
            View activeView = doc.ActiveView;
            List<ViewSheet> sheets = new FilteredElementCollector(doc)
                   .OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType()
                   .Where(x => x is ViewSheet).Cast<ViewSheet>().ToList();
            if (sheets.Count == 0)
            {
                doc.print("No sheets found to be restored");
                return Result.Failed;
            }
            if (doc.YesNoMessage("Do you want to reset all sheets?\nThis might take a long time!") == TaskDialogResult.No)
            {

                if (!(activeView is ViewSheet))
                {
                    doc.print("Active View must be a Sheet!");
                    return Result.Cancelled;
                }

                using (TransactionGroup tg = new TransactionGroup(doc, "Fix Sheet"))
                {
                    tg.Start();
                    if (!(sheetReset(doc.ActiveView as ViewSheet)))
                    {
                        tg.RollBack();
                        return Result.Failed;
                    }
                    tg.Assimilate();

                    return Result.Succeeded;
                }
            }
            else
            {
                using (TransactionGroup tg = new TransactionGroup(doc, "Fix All Sheets"))
                {
                    tg.Start();
                    foreach (ViewSheet sheet in sheets)
                    {
                        sheetReset(sheet);
                    }
                    tg.Assimilate();
                }
                if (sb.Length > 0)
                {
                    doc.print("Some views have not been reset because no stored data was found.");
                }
                return Result.Succeeded;
            }
        }
        public bool sheetReset(ViewSheet sheet)
        {
            List<Viewport> viewPorts = sheet.GetAllViewports().Select(x => doc.GetElement(x)).Cast<Viewport>().ToList();
            if (viewPorts.Count == 0) return false;
            StringBuilder sb = new StringBuilder();
            try
            {

                foreach (Viewport viewPort in viewPorts)
                {
                    string res = doc.resetView(doc.GetElement(viewPort.ViewId) as View);
                    if (res.Length > 0)
                        sb.AppendLine(res);
                }
                return true;
            }
            catch (Exception ex)
            {
                doc.print(ex.StackTrace);
                return false;
            }
            doc.print(sb);
        }

    }
}
