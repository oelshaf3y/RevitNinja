using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class HideUnhosted : IExternalCommand
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

            FilteredElementCollector rebar = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar);
            List<ElementId> ids = new List<ElementId>();
            foreach (Rebar bar in rebar)
            {
                Element elem = doc.GetElement(bar.GetHostId());
                if (elem.IsHidden(doc.ActiveView))
                {
                    ids.Add(bar.Id);
                }
            }
            if (ids.Count == 0)
            {
                return Result.Cancelled;
            }
            using (Transaction tr = new Transaction(doc, "Hide Unhosted Rebar"))
            {
                tr.Start();
                doc.ActiveView.HideElements(ids.ToArray());
                tr.Commit();
                tr.Dispose();
            }
            //doc.print("every thing is fine");
            return Result.Succeeded;
        }


    }
}
