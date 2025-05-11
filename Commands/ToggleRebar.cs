using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ToggleRebar : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess())  return Result.Failed;

            View activeView = uidoc.ActiveView;
            using (Transaction tr = new Transaction(doc, "Toggle Rebar"))
            {
                tr.Start();

                // Get the rebar category from the built-in categories
                Category rebarCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Rebar);

                // Hide the rebar category in the current view
                if (rebarCategory != null && activeView.CanCategoryBeHidden(rebarCategory.Id))
                {
                    if (activeView.GetCategoryHidden(rebarCategory.Id))
                    {

                        activeView.SetCategoryHidden(rebarCategory.Id, false);
                    }
                    else
                    {
                        activeView.SetCategoryHidden(rebarCategory.Id, true);
                    }
                }
                else
                {
                    TaskDialog.Show("Error", "Rebar category cannot be hidden in this view.");
                }

                tr.Commit();
                tr.Dispose();
            }

            return Result.Succeeded;
        }
    }
}
