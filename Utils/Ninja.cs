using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;
using Plane = Autodesk.Revit.DB.Plane;
using System.IO.Packaging;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Text.Json;
using Autodesk.Revit.DB.ExtensibleStorage;
using Microsoft.Office.Interop.Excel;
using Line = Autodesk.Revit.DB.Line;
using Icon = System.Drawing.Icon;

namespace RevitNinja.Utils
{
    public static class Ninja
    {
        public static string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming", "Autodesk", "Revit", "Addins", "RevitNinja");
        public static string dbfile = Path.Combine(folderPath, "dbaccess.json");
        public static string version = "1.1.9"; // this version updates the updater exe 
        public static Guid dataStorageGUID = new Guid("8998EC47-2E53-472B-9663-E1817A64F76F");
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

        public static TaskDialogResult YesNoMessage(this Document doc, object mes, string Title = null)
        {
            if (Title == null) Title = "Question?";

            TaskDialog dialog = new TaskDialog(Title)
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
            if (App.SharedParametersFilename == null) App.SharedParametersFilename = sharedParametersFile;
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
            //uidoc.ActiveView.ShowActiveWorkPlane();
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


        public static bool tryAccess(Document doc)
        {
            Dictionary<string, object> db = new Dictionary<string, object>();
            string tempDir = null;
            try
            {
                string tempExePath = ExtractEmbeddedResource(doc, "ninjaDB.exe");
                tempDir = Path.GetDirectoryName(tempExePath);
                string expectedOutputPath = Path.Combine(tempDir, "ninjadb.json");

                if (tempExePath == null) return false;
                // Run process
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = tempExePath,
                        WorkingDirectory = tempDir,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                string processOutput = process.StandardOutput.ReadToEnd();
                string processError = process.StandardError.ReadToEnd();
                process.WaitForExit();


                // Check for output file
                if (File.Exists(expectedOutputPath))
                {
                    string outputContent = File.ReadAllText(expectedOutputPath);
                    db = JsonSerializer.Deserialize<Dictionary<string, object>>(outputContent);
                    db.TryGetValue("Access", out object accessValue);
                    db.Add("Date", DateTime.Today.Date.ToString("yyyy-MM-dd"));
                    File.WriteAllText(dbfile, JsonSerializer.Serialize(db));
                    if (accessValue.ToString().ToLower() == "true")
                    {
                        return true;
                    }
                    else return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
                return false;
            }
            finally
            {
                // Single cleanup point
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Cleanup Error", $"An error occurred during cleanup: {ex.Message}");
                }
            }

        }

        public static bool getAccess(this Document doc)
        {
            //CreateAdmin();
            if (File.Exists(dbfile))
            {
                Dictionary<string, object> db = new Dictionary<string, object>();
                db = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(dbfile));
                db.TryGetValue("Date", out object date);
                if (date.ToString() == DateTime.Today.Date.ToString("yyyy-MM-dd"))
                {
                    db.TryGetValue("Access", out object av);
                    if (av.ToString().ToLower() == "true") return true;
                    else
                    {
                        doc.print("You don't have access to this revit addin\n please contact the developer using the info tab");
                        return false;
                    }
                }
                else
                {
                    File.Delete(dbfile);
                    if (tryAccess(doc))
                    {
                        return true;
                    }
                    else
                    {
                        doc.print("You don't have access to this revit addin\n please contact the developer using the info tab");
                        return false;
                    }
                }
            }
            else
            {
                if (!tryAccess(doc))
                {
                    doc.print("You don't have access to this revit addin\n please contact the developer using the info tab");
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static string ExtractEmbeddedResource(this Document doc, string resourceName)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
                if (!Directory.Exists(tempDir))
                {
                    return null;
                }
            }

            string tempExePath = Path.Combine(tempDir, resourceName);

            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(resourceName));

            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    return null;
                }

                using (FileStream fileStream = new FileStream(tempExePath, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }

            if (!File.Exists(tempExePath))
            {
                return null;
            }
            return tempExePath;
        }

        public static Schema getSchema()
        {
            Schema schema;

            if (Schema.Lookup(dataStorageGUID) == null)
            {
                SchemaBuilder builder = new SchemaBuilder(dataStorageGUID);
                builder.SetSchemaName("Ninja_DataStorage");
                builder.SetReadAccessLevel(AccessLevel.Public);
                builder.SetWriteAccessLevel(AccessLevel.Public);
                builder.AddSimpleField("storage", typeof(string));
                schema = builder.Finish();
            }
            else
            {
                schema = Schema.Lookup(dataStorageGUID);
            }
            return schema;
        }
        public static Entity getStorage(this View view)
        {
            Entity entity = view.GetEntity(getSchema());
            return entity.IsValid() ? entity : null;
        }
        public static void saveViewState(this Document doc, string storedIDs, View view)
        {
            using (Transaction tr = new Transaction(doc, "Save View State"))
            {
                tr.Start();
                Entity entity = view.getStorage();
                if (entity == null)
                {
                    entity = new Entity(getSchema());
                }
                entity.Set<string>("storage", storedIDs);
                view.SetEntity(entity);
                tr.Commit();
            }
        }
        public static string resetView(this Document doc, View view)
        {
            List<ElementId> ids = new List<ElementId>();
            #region using DataStorage
            Entity entity = view.getStorage();
            if (entity != null)
            {
                string storedData = entity.Get<string>("storage");
                if (storedData != null)
                {
                    foreach (string s in storedData.Split(','))
                    {
                        int a = 0;
                        int.TryParse(s, out a);
                        if (a != 0) ids.Add(new ElementId(a));
                    }
                }
            }
            #endregion


            #region Using Parameter
            else
            {
                //no stored data 
                Autodesk.Revit.DB.Parameter state = view.LookupParameter("View State");
                if (state == null || state.AsString() == null)
                {
                    return $"view state for View: {view.Name} is not stored!";
                }
                else
                {
                    string savedIDs = state.AsString();
                    foreach (string s in state.AsString().Split(','))
                    {
                        int a = 0;
                        int.TryParse(s, out a);
                        if (a != 0) ids.Add(new ElementId(a));
                        doc.saveViewState(savedIDs, view);
                    }

                }
            }
            #endregion

            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType();
            using (Transaction tr = new Transaction(doc, "restore view state"))
            {
                tr.Start();
                foreach (ElementId id in collector.Select(x => x.Id).Except(ids.ToList()).ToList())
                {
                    try
                    {
                        view.HideElements(new List<ElementId>() { id });
                    }
                    catch (Exception ex)
                    {

                    }
                }
                view.UnhideElements(ids.ToList());
                tr.Commit();
                tr.Dispose();
            }
            return "";
        }

    }
}
