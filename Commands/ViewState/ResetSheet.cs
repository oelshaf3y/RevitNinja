using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Text;

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
            if (!doc.getAccess())
            {
                doc.print("Please contact the developer");
                return Result.Failed;
            }
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
            if (activeView is ViewPlan || activeView is ViewSection)
            {
                if (activeView.LookupParameter("View State") == null)
                {
                    doc.print("No saved states to be restored");
                    return Result.Failed;
                }
            }
            else if ((activeView is ViewSheet))
            {
                if (doc.GetElement(((ViewSheet)activeView).GetAllViewports().First()).LookupParameter("View State") == null)
                {
                    doc.print("No saved states to be restored");
                    return Result.Failed;
                }
            }
            if (doc.YesNoMessage("Do you want to reset all sheets?") == TaskDialogResult.No)
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
            if (viewPorts.First().LookupParameter("View State") == null) return false;
            try
            {

                foreach (Viewport viewPort in viewPorts)
                {

                    if (!resetView(viewPort.ViewId))
                    {
                        sb.AppendLine("Viewport " + doc.GetElement(viewPort.ViewId).Name + " in Sheet No: " + sheet.SheetNumber + " is not reset.");
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                doc.print(ex.StackTrace);
                return false;
            }
        }

        public bool resetView(ElementId viewID)
        {
            List<ElementId> ids = new List<ElementId>();
            View view = doc.GetElement(viewID) as View;
            Parameter state = view.LookupParameter("View State");
            if (state.AsString() == null)
            {
                return false;
            }
            else
            {
                foreach (string s in state.AsString().Split(','))
                {
                    int a = 0;
                    int.TryParse(s, out a);
                    if (a != 0) ids.Add(new ElementId(a));
                }
            }
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType();
            using (Transaction tr = new Transaction(doc, "restore view state"))
            {
                tr.Start();
                foreach (ElementId id in collector.Select(x => x.Id).Except(ids.ToList()).ToList())
                {
                    try
                    {
                        view.HideElements(new List<ElementId>() { id });
                    }
                    catch (Exception ex)
                    {

                    }
                }
                view.UnhideElements(ids.ToList());
                tr.Commit();
                tr.Dispose();

                return true;
            }
        }
    }
}
