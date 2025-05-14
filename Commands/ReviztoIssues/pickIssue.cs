using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Ninja.Views;
using Revit_Ninja.Views.ReviztoIssues;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.ReviztoIssues
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class pickIssue : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            Element ball = null;
            PickIssueView picker = new PickIssueView();
            picker.ShowDialog();
            if ((picker.viewIssue))
            {
                try
                {

                    ball = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(FamilyInstance))
                    .Where(x => ((FamilyInstance)x).Symbol.FamilyName == "RevitNinja_Revizto_Clash_Ball")
                    .Where(x => x.LookupParameter("Id").AsString() == picker.idBox.Text).First();
                }
                catch { }
                if (ball == null)
                {
                    doc.print("No Revizto Issues found in the model.");
                    return Result.Cancelled;
                }
                uidoc.Selection.SetElementIds(new List<ElementId>() { ball.Id });
                IssueViewer viewer = new IssueViewer(ViewIssue.convertIssueData(ball));
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
            else if (picker.selectIssue)
            {
                uidoc.Selection.SetElementIds(new List<ElementId>() { ball.Id });
            }

            return Result.Succeeded;
        }
    }
}
