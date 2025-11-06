using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Text;
using System.Windows;

namespace Revit_Ninja.Commands.Cable_Management
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class cablesAstar : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public List<ElementId> negligible;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            XYZ start = XYZ.Zero, end = XYZ.Zero;
            try
            {
                doc.print("Click to select Start point");
                start = uidoc.Selection.PickPoint();
            }
            catch (Exception ex) { }
            try
            {
                doc.print("Click to select Finish point");
                end = uidoc.Selection.PickPoint();
            }
            catch (Exception ex) { }

            //negligible = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element).Select(x => x.ElementId).ToList();
            try
            {

                List<Curve> foundCurves = PathFinder.FindPath(doc, start, end, negligible);
                //List<Apoint> foundPath = PathFinder.FindPath(doc, start, end, negligible);
                //    if (foundPath is null)
                //    {
                //        doc.print("No path found");
                //        return Result.Succeeded;
                //    }
                //    else
                //    {
                //        //needs the path to be smoothed
                //        List<XYZ> path = foundPath.Select(x => x.point).ToList();
                //        StringBuilder sb = new StringBuilder();
                //        foreach (XYZ point in path)
                //        {
                //            sb.AppendLine(point.ToString());
                //        }
                //        Clipboard.SetText(sb.ToString());
                //        List<Line> pathLines = new List<Line>();
                //        for (int i = 0; i < path.Count - 1; i++)
                //        {
                //            pathLines.Add(Line.CreateBound(path[i], path[i + 1]));
                //        }
                //        using (Transaction tr = new Transaction(doc, "Draw CropBox Lines"))
                //        {
                //            tr.Start();
                //            DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Lines)).SetShape(pathLines.Cast<GeometryObject>().ToList());
                //            tr.Commit();
                //        }
                //    }
                if (foundCurves.Count > 0)
                {
                    using (Transaction tr = new Transaction(doc, "Draw path"))
                    {
                        tr.Start();

                        foreach (Curve curve in foundCurves)
                        {
                            doc.Create.NewDetailCurve(doc.ActiveView, curve);
                        }

                        tr.Commit();
                    }
                }
                else
                {
                    doc.print("No valid path found");
                }
            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
            }

            return Result.Succeeded;
        }
    }


}
