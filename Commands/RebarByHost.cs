using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class RebarByHost : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            List<ElementId> ids = new List<ElementId>();
            Element SelectedElement;
            if (uidoc.Selection.GetElementIds().Count == 0)
            {
                SelectedElement = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick Host Element."));

            }
            else if (uidoc.Selection.GetElementIds().Count > 1)
            {
                doc.print("You should Select only one element!");
                return Result.Cancelled;
            }
            else
            {


                SelectedElement = doc.GetElement(uidoc.Selection.GetElementIds().First());
            }

            FilteredElementCollector rebars = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rebar);
            foreach (Element rebar in rebars)
            {
                Rebar reb = null;
                if (rebar is Rebar)
                {
                    reb = rebar as Rebar;
                }
                else if (reb == null) { continue; }
                if (reb.GetHostId() == SelectedElement.Id) ids.Add(reb.Id);
            }
            if (ids.Count > 0)
            {
                uidoc.Selection.SetElementIds(ids);
            }
            else
            {
                doc.print("No RFT Found.");
            }
            return Result.Succeeded;

        }
    }
}
