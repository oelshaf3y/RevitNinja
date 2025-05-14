using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.ReviztoIssues
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ToggleIssues : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
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

            using (Transaction tr = new Transaction(doc, "Toggle Issues"))
            {
                tr.Start();
                if (clashBalls.Where(x => x.IsHidden(doc.ActiveView)).Any())
                {
                    doc.ActiveView.UnhideElements(clashBalls.Select(x => x.Id).ToList());
                }
                else if (clashBalls.Where(x => x.CanBeHidden(doc.ActiveView)).Any())
                {
                    doc.ActiveView.HideElements(clashBalls.Select(x => x.Id).ToList());
                }
                tr.Commit();
                tr.Dispose();
            }

            return Result.Succeeded;
        }
    }
}
