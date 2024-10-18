using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class NotOnSheets : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            int count = 0;
            List<View> views = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsViewIndependent()
                .Where(v => v.LookupParameter("View Template") != null)
                .Cast<View>().Distinct().ToList();
            Parameter sheetNumber;
            using (TransactionGroup tg = new TransactionGroup(document, "Delete unused views"))
            {

                tg.Start();
                foreach (View view in views)
                {
                    if (view == null) continue;
                    if (view.GetType() != typeof(ViewSheet) && view.GetType() != typeof(ViewSchedule)
                        && view.ViewType != ViewType.ProjectBrowser && view.ViewType != ViewType.SystemBrowser)
                    {
                        sheetNumber = view.LookupParameter("Sheet Name");
                        if (sheetNumber == null || sheetNumber.AsValueString() == "---")
                        {
                            if (view.LookupParameter("Dependency") != null && view.LookupParameter("Dependency").AsString() != "Primary")
                            {
                                using (Transaction tx = new Transaction(document, "Delete sheet"))
                                {

                                    tx.Start();
                                    if (view.Id.IntegerValue != document.ActiveView.Id.IntegerValue)
                                    {
                                        try
                                        {

                                            document.Delete(view.Id);
                                            count++;
                                        }
                                        catch (Exception ex)
                                        {
                                            document.print(ex.StackTrace);
                                        }

                                    }
                                    tx.Commit();
                                    tx.Dispose();
                                }
                            }
                        }
                    }
                }
                tg.Assimilate();
            }
            if (count > 0)
            {

                document.print("Total of: " + count + " views have been deleted.");
            }
            else
            {
                document.print("No views to delete.");
            }
            return Result.Succeeded;

        }
    }
}
