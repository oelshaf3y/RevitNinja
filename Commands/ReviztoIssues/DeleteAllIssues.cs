using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.ReviztoIssues
{

    [TransactionAttribute(TransactionMode.Manual)]
    class DeleteAllIssues : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess()) return Result.Failed;

            List<Element> clashBalls = new List<Element>();
            try
            {

                clashBalls = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(FamilyInstance))
                    .Where(x => ((FamilyInstance)x).Symbol.FamilyName == "RevitNinja_Revizto_Clash_Ball")
                    .ToList();
            }
            catch
            {
                doc.print("No Revizto Issues found in the model.");
                return Result.Cancelled;
            }

            if (clashBalls.Count == 0)
            {
                doc.print("No Revizto Issues found in the model.");
                return Result.Cancelled;
            }
            using (TransactionGroup tg = new TransactionGroup(doc, "Remove Issues"))
            {
                tg.Start();
                foreach (Element clashBall in clashBalls)
                {
                    using (Transaction tr = new Transaction(doc, "Remove Issue"))
                    {
                        tr.Start();
                        try
                        {
                            doc.Delete(clashBall.Id);
                        }
                        catch { }
                        tr.Commit();
                        tr.Dispose();
                    }
                }
                tg.Assimilate();
            }

            return Result.Succeeded;
        }
    }
}
