using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitNinja.Utils;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ElementSection : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        IList<Element> elems;
        StringBuilder sb;
        Element viewForSection;
        Options options;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            sb = new StringBuilder();
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess()) return Result.Failed;

            options = new Options();
            options.ComputeReferences = true;
            if (doc.ActiveView is View3D)
            {
                td("Active View Must be a Plan View!");
                return Result.Cancelled;
            }
            viewForSection = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Where(x => ((ViewFamilyType)x).ViewFamily == ViewFamily.Section).FirstOrDefault();

            elems = new List<Element>();
            elems = uidoc.Selection.GetElementIds()?.Select(x => doc.GetElement(x)).ToList();
            if (elems.Count == 0)
            {
                try
                {
                    elems = uidoc.Selection.PickObjects(ObjectType.Element, "Select Element or Multiple Elements").Select(x => doc.GetElement(x)).ToList();
                }
                catch { return Result.Cancelled; }
            }
            Transaction tr = new Transaction(doc);
            tr.Start("Sections");
            foreach (Element e in elems)
            {
                Location location = e.Location;
                BoundingBoxXYZ elemBoundingBox = e.get_BoundingBox(null);
                XYZ min = elemBoundingBox.Min;
                XYZ max = elemBoundingBox.Max;
                double minx = min.X; double maxx = max.X;
                double miny = min.Y; double maxy = max.Y;
                double minz = min.Z - 1.5; double maxz = max.Z + 1.5;
                double h = maxz - minz;
                Curve largeCurve;
                if (location is LocationPoint)
                {
                    XYZ p1 = new XYZ(minx, miny, (minz + maxz) / 2);
                    XYZ p2 = new XYZ(maxx, miny, (minz + maxz) / 2);
                    largeCurve = Line.CreateBound(p1, p2);
                    createSection(largeCurve, h);
                }
                else
                {
                    largeCurve = ((LocationCurve)e.Location).Curve;
                    createSection(largeCurve, h);

                }
                //createSection(smallCurve, smallCurve, h);

            }
            tr.Commit();
            tr.Dispose();
            //td(sb.ToString());


            return Result.Succeeded;
        }

        private void createSection(Curve alignedCurve, double h)
        {
            //Curve alignedCurve, perpendicularCurve;
            BoundingBoxXYZ bx = new BoundingBoxXYZ();
            BoundingBoxXYZ bx2 = new BoundingBoxXYZ();

            try
            {
                XYZ viewDir = ((Line)alignedCurve).Direction.CrossProduct(XYZ.BasisZ).Normalize();
                XYZ CG = alignedCurve.Evaluate(0.5, true).Add(-h * 0.5 * viewDir);
                XYZ sectionDirection = ((Line)alignedCurve).Direction.Normalize();
                XYZ bxMin = new XYZ(-alignedCurve.Length / 2 - 1.5, -1.5, -1.5);
                XYZ bxMax = new XYZ(alignedCurve.Length / 2 + 1.5, h + 1.5, h + 1.5);

                XYZ bx2Min = new XYZ(-2.50, -2.50, 0);
                XYZ bx2Max = new XYZ(2.5, 2.5, 5);
                Transform transform = Transform.Identity;
                transform.Origin = CG;
                transform.BasisX = new XYZ(sectionDirection.X, sectionDirection.Y, 0);
                transform.BasisZ = new XYZ(viewDir.X, viewDir.Y, 0);
                transform.BasisY = XYZ.BasisZ;

                Transform transform2 = Transform.Identity;
                transform2.Origin = alignedCurve.Evaluate(0.5, true);
                transform2.BasisX = new XYZ(-sectionDirection.Y, sectionDirection.X, 0);
                transform2.BasisZ = new XYZ(-viewDir.Y, viewDir.X, 0);
                transform2.BasisY = XYZ.BasisZ;

                sb.AppendLine($"bxmin: {bxMin}");
                sb.AppendLine($"bxmax: {bxMax}");
                sb.AppendLine($"origin: {CG}");
                sb.AppendLine($"basisx: {transform.BasisX}");
                sb.AppendLine($"viewDir: {transform.BasisZ}");
                sb.AppendLine($"Updirection: {transform.BasisY}");

                bx.Min = bxMin;
                bx.Max = bxMax;
                bx.Transform = transform;

                bx2.Min = bx2Min;
                bx2.Max = bx2Max;
                bx2.Transform = transform2;

            }
            catch (Exception ex)
            {
                sb.AppendLine("err: " + ex.StackTrace);
            }
            //td(sb.ToString());
            try
            {

                ViewSection section = ViewSection.CreateSection(doc, viewForSection.Id, bx);
                ViewSection section2 = ViewSection.CreateSection(doc, viewForSection.Id, bx2);


            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
            }
        }

        void td(string message)
        {
            TaskDialog.Show("Message", message);
        }

        public Solid getSolid(Element elem)
        {
            IList<Solid> solids = new List<Solid>();
            try
            {

                GeometryElement geo = elem.get_Geometry(options);
                if (geo.FirstOrDefault() is Solid)
                {
                    Solid solid = (Solid)geo.FirstOrDefault();
                    return SolidUtils.Clone(solid);
                }
                foreach (GeometryObject geometryObject in geo)
                {
                    if (geometryObject != null)
                    {
                        Solid solid = geometryObject as Solid;
                        if (solid != null && solid.Volume > 0)
                        {
                            solids.Add(solid);

                        }
                    }
                }
            }
            catch
            {
            }
            if (solids.Count == 0)
            {
                try
                {
                    GeometryElement geo = elem.get_Geometry(options);
                    GeometryInstance geoIns = geo.FirstOrDefault() as GeometryInstance;
                    if (geoIns != null)
                    {
                        GeometryElement geoElem = geoIns.GetInstanceGeometry();
                        if (geoElem != null)
                        {
                            foreach (GeometryObject geometryObject in geoElem)
                            {
                                Solid solid = geometryObject as Solid;
                                if (solid != null && solid.Volume > 0)
                                {
                                    solids.Add(solid);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    throw new InvalidOperationException();
                }
            }
            if (solids.Count > 0)
            {
                try
                {

                    return SolidUtils.Clone(solids.OrderByDescending(x => x.Volume).ElementAt(0));
                }
                catch
                {
                    return solids.OrderByDescending(x => x.Volume).ElementAt(0);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
