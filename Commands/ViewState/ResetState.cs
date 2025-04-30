using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

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
            if (!doc.getAccess())
            {
                return Result.Failed;
            }
            List<ElementId> ids = new List<ElementId>();
            View activeView = doc.ActiveView;
            if (activeView is ViewSheet || activeView is ViewSchedule)
            {
                doc.print("Active view must be a view not a sheet or a schedule");
                return Result.Failed;
            }
            Parameter state = activeView.LookupParameter("View State");
            if (state == null || state.AsString() == null)
            {
                doc.print("view state is not stored or parameter doesn't exist!");
                return Result.Failed;
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
            FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id).WhereElementIsNotElementType();
            using (Transaction tr = new Transaction(doc, "restore view state"))
            {
                tr.Start();
                foreach (ElementId id in collector.Select(x => x.Id).Except(ids.ToList()).ToList())
                {
                    try
                    {
                        activeView.HideElements(new List<ElementId>() { id });
                    }
                    catch (Exception ex)
                    {

                    }
                }
                activeView.UnhideElements(ids.ToList());
                tr.Commit();
                tr.Dispose();
            }
            return Result.Succeeded;
        }
    }
}
