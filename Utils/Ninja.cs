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
using UIFramework;
using Xceed.Wpf.AvalonDock.Controls;
using Color = System.Windows.Media.Color;
using System.Windows.Media.Animation;
using System.Text;

namespace RevitNinja.Utils
{
    public static class Ninja
    {
        public static string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming", "Autodesk", "Revit", "Addins", "RevitNinja");
        public static string dbfile = Path.Combine(folderPath, "ninjadb.json");
        public static string version = "1.2.1"; // this version updates the updater exe 
        public static Guid dataStorageGUID = new Guid("8998EC47-2E53-472B-9663-E1817A64F76F");

        public static List<List<Color>> colors = new List<List<Color>>()
        {

            new List<Color>()
            {
                Color.FromRgb(244,244,248),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(111,111,133),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(134,115,70),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(254,215,102),Color.FromRgb(0, 0, 0)
            },

            new List<Color>()
            {
                Color.FromRgb(84,160,91),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(42,183,202),Color.FromRgb(0,0,0)
            },

            new List<Color>()
            {
                Color.FromRgb(53,101,110),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(99,84,91),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(156,68,71),Color.FromRgb(255,255,255)
            },

            new List<Color>()
            {
                Color.FromRgb(254,74,73),Color.FromRgb(0,0,0)
            },
        };
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
            if (!Directory.Exists(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "RevitNinja"))) Directory.CreateDirectory(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "RevitNinja"));
            string sharedParametersFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "RevitNinja",
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
                ExtractEmbeddedResource(null, "Revit-Ninja-sharedParameters.txt", sharedParametersFile);
                //File.Create(sharedParametersFile).Dispose();
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

        public static void createDBFile()
        {
            Dictionary<string, object> defaultDb = new Dictionary<string, object>();

            Dictionary<string, object> revitNinja = new Dictionary<string, object>();
            Dictionary<string, object> accessList = new Dictionary<string, object>();
            Dictionary<string, object> user = new Dictionary<string, object>();

            user.Add("username", "default");
            user.Add("machineId", "default");
            user.Add("access", false);
            user.Add("date", DateTime.Today.Date.ToString("yyyy-MM-dd"));
            user.Add("daysleft", 30);
            user.Add("version", Ninja.version);
            accessList.Add("default", user);
            revitNinja.Add("Access", false);
            revitNinja.Add("AccessList", accessList);
            revitNinja.Add("UpdaterLink", "https://github.com/oelshaf3y/RevitNinja/releases/download/Publish/RevitNinja.exe");
            revitNinja.Add("Version", Ninja.version);
            revitNinja.Add("foreground", "#FF000000");
            revitNinja.Add("background", "#FFF4F4F8");
            revitNinja.Add("color", true);
            defaultDb.Add("RevitNinja", revitNinja);
            System.IO.File.WriteAllText(Ninja.dbfile, JsonSerializer.Serialize(defaultDb));
        }

        public static bool checkUserAccess()
        {
            if (File.Exists(dbfile))
            {
                try
                {
                    Dictionary<string, object> db = new Dictionary<string, object>();
                    string outputContent = File.ReadAllText(dbfile);
                    db = JsonSerializer.Deserialize<Dictionary<string, object>>(outputContent);
                    Dictionary<string, object> revitninja = new Dictionary<string, object>();
                    object accessValue = null, mess = null;
                    db.TryGetValue("RevitNinja", out object rn);

                    if (rn is JsonElement rn2)
                    {
                        revitninja = JsonSerializer.Deserialize<Dictionary<string, object>>(rn2.GetRawText());
                        revitninja.TryGetValue("Access", out accessValue);
                        revitninja.TryGetValue("Message", out mess);
                    }


                    if (accessValue.ToString().ToLower() == "true")
                    {
                        if (mess != null && mess.ToString().Length > 0)
                        {
                            TaskDialog.Show("Revit Ninja", mess.ToString());
                        }
                        return true;
                    }
                    else // no access
                    {
                        if (mess != null && mess.ToString().Length > 0)
                        {
                            TaskDialog.Show("Revit Ninja", mess.ToString());
                        }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", $"An error occurred while checking access in check user access:\n {ex.Message}");
                    return false;
                }
            }
            else // output file not found
            {
                TaskDialog.Show("Error", "You don't have access to this addin. contact the developer using the info tab.");
                return false;
            }
        }

        public static bool tryAccess(Document doc)
        {
            string tempDir = null;
            if (!File.Exists(dbfile)) createDBFile();

            try // extract and run ninjaDB.exe from embedded resources
            {
                string tempExePath = ExtractEmbeddedResource(doc, "ninjaDB.exe");
                tempDir = Path.GetDirectoryName(tempExePath);

                if (tempExePath == null) return checkUserAccess();

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

                return checkUserAccess();

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"An error occurred while try access:\n {ex.Message}");
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
            try
            {
                if (File.Exists(dbfile))
                {
                    Dictionary<string, object> db = new Dictionary<string, object>();
                    Dictionary<string, object> revitninja = new Dictionary<string, object>();
                    Dictionary<string, object> accessList = new Dictionary<string, object>();
                    Dictionary<string, object> User = new Dictionary<string, object>();


                    db = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(dbfile));
                    db.TryGetValue("RevitNinja", out object rn);
                    if (rn is JsonElement rn2)
                    {
                        revitninja = JsonSerializer.Deserialize<Dictionary<string, object>>(rn2.GetRawText());
                    }
                    revitninja.TryGetValue("AccessList", out object AccessListObj);
                    if (AccessListObj is JsonElement alo)
                    {
                        accessList = JsonSerializer.Deserialize<Dictionary<string, object>>(alo.GetRawText());
                    }
                    var user = accessList.First().Value;
                    if (user is JsonElement us)
                    {
                        User = JsonSerializer.Deserialize<Dictionary<string, object>>(us.GetRawText());
                    }
                    User.TryGetValue("date", out object date);
                    //updated today
                    if (date.ToString() == DateTime.Today.Date.ToString("yyyy-MM-dd"))
                    {
                        revitninja.TryGetValue("Access", out object av);
                        if (av.ToString().ToLower() == "true") return true;
                        else
                        {
                            revitninja.TryGetValue("Message", out object mess);

                            if (tryAccess(doc))
                            {
                                return true;
                            }
                            else
                            {

                                doc.print(mess.ToString());
                                return false;
                            }
                        }
                    }
                    // not updated today
                    else
                    {
                        return tryAccess(doc);
                    }
                }
                else
                {
                    return tryAccess(doc);
                }
            }
            catch (Exception ex)
            {
                doc.print("An error occurred while checking access: \n" + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        public static string ExtractEmbeddedResource(this Document doc, string resourceName, string copyto = null)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
                if (!Directory.Exists(tempDir))
                {
                    return "";
                }
            }

            string tempResPath = Path.Combine(tempDir, resourceName);

            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(resourceName));

            if (string.IsNullOrEmpty(resourcePath))
            {
                return "";
            }

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    return "";
                }

                using (FileStream fileStream = new FileStream(tempResPath, FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }

            if (!File.Exists(tempResPath))
            {
                return "";
            }
            if (copyto != null)
            {
                File.Copy(tempResPath, copyto, true);
                return copyto;
            }
            return tempResPath;
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

        public static void ColorTabs()
        {
            List<string> docs = new List<string>();
            string outputContent = File.ReadAllText(Ninja.dbfile);
            Dictionary<string, object> db = JsonSerializer.Deserialize<Dictionary<string, object>>(outputContent);
            Dictionary<string, object> revitninja = new Dictionary<string, object>();
            db.TryGetValue("RevitNinja", out object rn3);
            if (rn3 is JsonElement rn4)
            {
                revitninja = JsonSerializer.Deserialize<Dictionary<string, object>>(rn4.GetRawText());
            }
            revitninja.TryGetValue("color", out object color);
            revitninja.TryGetValue("foreground", out object foreground);
            revitninja.TryGetValue("background", out object background);
            System.Windows.Media.BrushConverter converter = new System.Windows.Media.BrushConverter();
            System.Windows.Media.Brush fore = (System.Windows.Media.Brush)converter.ConvertFromString(foreground.ToString());
            System.Windows.Media.Brush back = (System.Windows.Media.Brush)converter.ConvertFromString(background.ToString());
            bool colorize;
            if (color.ToString().ToLower() == "true") colorize = true;
            else colorize = false;
            var docPanes = FindVisualChildren<LayoutDocumentPaneControl>(MainWindow.getMainWnd());
            foreach (var pane in docPanes)
            {
                var tabs = FindVisualChildren<TabItem>(pane);
                docs = tabs.Select(x => x.ToolTip.ToString().Split('.').First()).ToList();
                foreach (var tab in tabs)
                {
                    string tabName = tab.ToolTip.ToString();
                    int docind = docs.IndexOf(tabName.Split('.').First());
                    if (docind > 9) docind = docind - 9;
                    if (colorize)
                    {
                        tab.Background = new SolidColorBrush(colors[docind][0]);
                        tab.Foreground = new SolidColorBrush(colors[docind][1]);
                        //TaskDialog.Show("Color Tabs", $"Automatic Tab Coloring is On");
                    }
                    else
                    {
                        tab.Background = back;
                        tab.Foreground = fore;
                        //TaskDialog.Show("Color Tabs", $"Automatic Tab Coloring is Off");
                    }
                    //tab.BorderBrush
                }
            }
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


    }
}