using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitNinja.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            //BoundingBoxXYZ bbx = doc.ActiveView.CropBox;
            //Transform tf = bbx.Transform.Inverse;
            //XYZ p1, p2, p3, p4;
            //p1 = tf.OfPoint(new XYZ(bbx.Min.X, bbx.Min.Y, bbx.Min.Z));
            //p2 = tf.OfPoint(new XYZ(bbx.Max.X, bbx.Min.Y, bbx.Min.Z));
            //p3 = tf.OfPoint(new XYZ(bbx.Max.X, bbx.Max.Y, bbx.Min.Z));
            //p4 = tf.OfPoint(new XYZ(bbx.Min.X, bbx.Max.Y, bbx.Min.Z));

            //List<Line> lines = new List<Line>();
            //lines.Add(Line.CreateBound(p1, p2));
            //lines.Add(Line.CreateBound(p2, p3));
            //lines.Add(Line.CreateBound(p3, p4));
            //lines.Add(Line.CreateBound(p4, p1));

            //List<List<XYZ>> points = new List<List<XYZ>>();
            //try
            //{

            //    for (int i = 0; i < Math.Abs(p2.DistanceTo(p1)); i++)
            //    {
            //        points.Add(new List<XYZ>());
            //        for (int j = 0; j < Math.Abs(p4.DistanceTo(p1)); j++)
            //        {
            //            XYZ point = tf.OfPoint(tf.OfPoint(p1.Add(i * tf.BasisX).Add(j * tf.BasisY)));
            //            points[i].Add(point);
            //            //grid.Add(new Apoint(point));
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    doc.print(ex.Message);
            //}
            //StringBuilder sb = new StringBuilder();
            //try
            //{
            //    for (int i = 0; i < Math.Abs(p2.DistanceTo(p1)); i++)
            //    {
            //        sb.AppendLine(points[i].Count.ToString());
            //        lines.Add(Line.CreateBound(points[i][0], points[i][points[i].Count - 1]));
            //    }
            //    for (int j = 0; j < Math.Abs(p4.DistanceTo(p1)); j++)
            //    {
            //        sb.AppendLine(points.Count.ToString());
            //        lines.Add(Line.CreateBound(points[0][j], points[points.Count - 1][j]));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    doc.print(sb.ToString());
            //    doc.print(ex.Message);
            //}
            //using (Transaction tr = new Transaction(doc, "Draw CropBox Lines"))
            //{
            //    tr.Start();
            //    DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Lines)).SetShape(lines.Cast<GeometryObject>().ToList());
            //    tr.Commit();
            //}

            XYZ start = uidoc.Selection.PickPoint();
            XYZ end = uidoc.Selection.PickPoint();
            //List<Element> els = new FilteredElementCollector(doc, doc.ActiveView.Id)
            //    .WhereElementIsNotElementType().ToList();

            //foreach(Element el in els)
            //{
            //    Solid solid = el.getSolid();
            //    if(solid is null) continue;


            //}


            negligible = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element).Select(x => x.ElementId).ToList();
            List<Element> collideable = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new NinjaSelectionFilter(x => !negligible.Contains(x.Id))).Select(x => doc.GetElement(x)).ToList();
            try
            {

                List<Commands.Apoint> foundPath = OnGoing.FindPath(doc, start, end, negligible, collideable);
                List<Element> collided = OnGoing.collided;
                if (foundPath is null)
                {
                    doc.print("No path found");
                    uidoc.Selection.SetElementIds(collided.Select(x => x.Id).ToList());
                    return Result.Succeeded;
                }
                else
                {
                    //doc.print(foundPath.Count);
                    uidoc.Selection.SetElementIds(collided.Select(x => x.Id).ToList());
                    List<XYZ> path = foundPath.Select(x => x.point).ToList();
                    List<Line> pathLines = new List<Line>();
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        pathLines.Add(Line.CreateBound(path[i], path[i + 1]));
                    }
                    using (Transaction tr = new Transaction(doc, "Draw CropBox Lines"))
                    {
                        tr.Start();
                        DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Lines)).SetShape(pathLines.Cast<GeometryObject>().ToList());
                        tr.Commit();
                    }
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
