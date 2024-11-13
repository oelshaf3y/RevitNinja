using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Numerics;
using Plane = Autodesk.Revit.DB.Plane;
using System.IO.Packaging;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace RevitNinja.Utils
{
    public static class Ninja
    {
        public static double meterToFeet(this double distance) => distance / 0.3048;
        public static double mmToFeet(this double distance) => distance / 304.8;
        public static double feetToMeter(this double distance) => distance * 0.3048;
        public static double feetToMM(this double distance) => distance * 304.8;
        public static double toDegree(this double angle) => angle * 180 / Math.PI;
        public static double toRad(this double angle) => angle * Math.PI / 180;

        //public static string ToString(this XYZ point) => $"{point.X},{point.Y},{point.Z}";
        public static XYZ getCG(this Element element) => element.Location is LocationPoint ? getPointLocation(element) : getLineLocation(element);
        public static XYZ getPointLocation(Element element) => ((LocationPoint)element.Location).Point;
        public static XYZ getLineLocation(Element element) => ((Line)((LocationCurve)element.Location).Curve).Evaluate(.5, true);
        public static void print(this Document doc, object mes)
        {
            MessageBox.Show(mes.ToString());
        }

        public static TaskDialogResult YesNoMessage(this Document doc, object mes)
        {
            TaskDialog dialog = new TaskDialog("Question?")
            {
                MainInstruction = mes.ToString(),
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No
            };
            return dialog.Show();
        }
        public static Solid getSolid(this Document doc, Element elem)
        {
            Options options = new Options();
            options.ComputeReferences = true;
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
                    //throw new InvalidOperationException();
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

        public static Face getFace(this Solid s, string location)
        {
            if (location.ToLower() == "top")
            {

                List<PlanarFace> faces = new List<PlanarFace>();
                foreach (Face face in s.Faces)
                {
                    PlanarFace pf = face as PlanarFace;
                    if (pf == null) continue;
                    if (Math.Abs(pf.FaceNormal.AngleTo(new XYZ(0, 0, 1))) < Math.PI / 18)
                    {
                        faces.Add(pf);
                    }
                }
                if (faces.Count == 0) return null;
                return faces.OrderByDescending(x => x.Origin.Z)?.First();
            }
            else if (location.ToLower() == "bot")
            {
                List<PlanarFace> faces = new List<PlanarFace>();
                foreach (Face face in s.Faces)
                {
                    PlanarFace pf = face as PlanarFace;
                    if (pf == null) continue;
                    if (Math.Abs(pf.FaceNormal.AngleTo(new XYZ(0, 0, -1))) < Math.PI / 18)
                    {
                        faces.Add(pf);
                    }
                }
                if (faces.Count == 0) return null;
                return faces.OrderBy(x => x.Origin.Z)?.First();
            }
            else
            {
                return null;
            }
        }

        public static bool assignParameter(ExternalCommandData commandData, string Group, string ParamName, BuiltInCategory category, ForgeTypeId paramType)
        {
            string sharedParametersFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Revit-Ninja-sharedParameters.txt");
            UIDocument uidoc;
            Document doc;
            DefinitionGroup defGroup;
            Definition definition;
            DefinitionFile defFile;
            CategorySet categories;
            string groupName = Group;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!File.Exists(sharedParametersFile))
            {
                File.Create(sharedParametersFile).Dispose();
            }
            Autodesk.Revit.ApplicationServices.Application App = commandData.Application.Application;
            App.SharedParametersFilename = sharedParametersFile;
            defFile = App.OpenSharedParameterFile();
            if (defFile == null) return false;
            if (defFile.Groups.Where(x => x.Name.ToLower() == Group.ToLower()).Any())
            {
                defGroup = defFile.Groups.Where(x => x.Name.ToLower() == Group.ToLower()).FirstOrDefault();
            }
            else defGroup = defFile.Groups.Create(groupName);
            if (defGroup.Definitions.Where(x => x.Name == ParamName).Any())
            {
                if (defGroup.Definitions.Where(x => x.Name == ParamName)
                    .First().GetDataType() != paramType)
                {
                    return false;
                }
                definition = defGroup.Definitions.Where(x => x.Name == ParamName).First();
            }
            else
            {
                ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(ParamName, paramType);
                definition = defGroup.Definitions.Create(options);
            }

            // Add shared parameter to categories
            categories = App.Create.NewCategorySet();
            categories.Insert(doc.Settings.Categories.get_Item(category));

            using (Transaction t = new Transaction(doc, "Add Shared Parameter"))
            {
                t.Start();

                // Bind the shared parameter to the categories
                InstanceBinding binding = App.Create.NewInstanceBinding(categories);
                BindingMap bindingMap = doc.ParameterBindings;
                bool bindingResult = bindingMap.Insert(definition, binding, BuiltInParameterGroup.PG_TEXT);
                if (!bindingResult) return false;

                t.Commit();
            }
            return true;
        }

        public static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public static void SetActiveWorkPlane(this UIDocument uidoc)
        {
            Plane plane = Plane.CreateByNormalAndOrigin(uidoc.ActiveView.ViewDirection, uidoc.ActiveView.Origin);
            Document doc = uidoc.Document;
            SketchPlane sp = SketchPlane.Create(doc, plane);
            uidoc.ActiveView.SketchPlane = sp;
            uidoc.ActiveView.ShowActiveWorkPlane();
        }

        public static void getUri(this UserControl userControl, string baseUri)
        {
            try
            {
                var resourceLocater = new Uri(baseUri, UriKind.Relative);
                var exprCa = (PackagePart)typeof(Application).GetMethod("GetResourceOrContentPart", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { resourceLocater });
                var stream = exprCa.GetStream();
                var uri = new Uri((Uri)typeof(BaseUriHelper).GetProperty("PackAppBaseUri", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null), resourceLocater);
                var parserContext = new ParserContext
                {
                    BaseUri = uri
                };
                typeof(XamlReader).GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { stream, parserContext, userControl, true });
            }
            catch (Exception ex)
            {
                //log
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
