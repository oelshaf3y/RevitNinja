using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Ninja.Views;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.ReviztoIssues
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class movingIssue : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess()) return Result.Failed;
            if (!(doc.ActiveView is View3D))
            {
                doc.print("Active View must be a 3D View.");
                return Result.Cancelled;
            }
            Element ball = null;
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
            if (uidoc.Selection.GetElementIds().Count == 0)
            {
                try
                {

                    ball = uidoc.Selection.PickObjects(ObjectType.Element,
                        new NinjaSelectionFilter(x => ((FamilyInstance)x).Symbol.FamilyName == "RevitNinja_Revizto_Clash_Ball"),
                        "Please select a Revizto Issue.").Select(x => doc.GetElement(x)).First();
                }
                catch
                {
                    return Result.Cancelled;
                }
            }
            else
            {
                ball = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x))
                    .Where(x => ((FamilyInstance)x).Symbol.FamilyName == "RevitNinja_Revizto_Clash_Ball").First();
            }
            if (ball == null)
            {
                TaskDialog.Show("Error", "Please select a Revizto Issue.");
                return Result.Cancelled;
            }
            else
            {
                Reference refe = uidoc.Selection.PickObject(ObjectType.PointOnElement);
                XYZ point = refe.GlobalPoint;
                using (Transaction tr = new Transaction(doc, "Solve Issue"))
                {
                    tr.Start();
                    ball.Location.Move(point - ((LocationPoint)ball.Location).Point);
                    tr.Commit();
                    tr.Dispose();
                }
            }

            return Result.Succeeded;
        }

        public static Issue convertIssueData(Element ball)
        {
            Issue issue = new Issue
            (
                ball.LookupParameter("Id").AsString(),
                ball.LookupParameter("SnapshotLink").AsString(),
                ball.LookupParameter("Date").AsString(),
                ball.LookupParameter("Reporter").AsString(),
                ball.LookupParameter("Status").AsString(),
                ball.LookupParameter("Title").AsString(),
                ball.LookupParameter("Stamp").AsString(),
                ball.LookupParameter("Issue Level").AsString(),
                ball.LookupParameter("GridLocation").AsString(),
                ball.LookupParameter("Zone").AsString(),
                ball.LookupParameter("StampTitle").AsString(),
                null,
                Comment.fromJson(ball.LookupParameter("Comments").AsString())
            );
            return issue;
        }
    }
}
