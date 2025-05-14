using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using Revit_Ninja.Views;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.ReviztoIssues
{

    [TransactionAttribute(TransactionMode.Manual)]
    internal class ViewIssue : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
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
                Issue issue = convertIssueData(ball);
                IssueViewer viewer = new IssueViewer(issue);
                viewer.ShowDialog();
                if (viewer.solved)
                {
                    using (Transaction tr = new Transaction(doc, "Solve Issue"))
                    {
                        tr.Start();
                        ball.LookupParameter("Solved").Set(1);
                        tr.Commit();
                        tr.Dispose();
                    }
                }
            }

            return Result.Succeeded;
        }
        public static Issue convertIssueData(Element ball)
        {
            Issue issue = new Issue()
            {
                Comments = Comment.fromJson(ball.LookupParameter("Comments").AsString()),
                Date = ball.LookupParameter("Date").AsString(),
                GridLocation = ball.LookupParameter("GridLocation").AsString(),
                Id = ball.LookupParameter("Id").AsString(),
                Level = ball.LookupParameter("Issue Level").AsString(),
                SnapshotLink = ball.LookupParameter("SnapshotLink").AsString(),
                Stamp = ball.LookupParameter("Stamp").AsString(),
                StampTitle = ball.LookupParameter("StampTitle").AsString(),
                Status = ball.LookupParameter("Status").AsString(),
                Title = ball.LookupParameter("Title").AsString(),
                Zone = ball.LookupParameter("Zone").AsString()
            };
            return issue;
        }
    }
}
