using Autodesk.Revit.UI;
using Revit_Ninja.Commands;
using Revit_Ninja.Commands.Penetration;
using Revit_Ninja.Commands.ReviztoIssues;
using RevitNinja.Commands;
using RevitNinja.Commands.ViewState;
using RevitNinja.Utils;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.Diagnostics;
using System.Net;
using RevitNinja.Views;
using System.Windows.Shapes;
using Revit_Ninja.Commands.PointsCoord;
using Revit_Ninja.Commands.BIMSubmittal;

namespace RevitNinja
{
    internal class Application : IExternalApplication
    {
        string assemblyName, asPath, Link;
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                string dllPath = "";
                if (File.Exists(Ninja.dbfile))
                {
                    Dictionary<string, object> db = new Dictionary<string, object>();
                    db = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(Ninja.dbfile));
                    bool update = db.TryGetValue("UpdateOnClose", out object updateOnClose);
                    if (update && updateOnClose.ToString().ToLower() == "true")
                    {
                        db.TryGetValue("UpdaterPath", out object path);
                        dllPath = path.ToString();
                        // 2. Launch the updater with a user-friendly message
                        System.Diagnostics.Process.Start(new ProcessStartInfo
                        {
                            FileName = dllPath,
                            UseShellExecute = true // required for showing any UI or running elevated
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result.Failed;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            if (!Directory.Exists(Ninja.folderPath))
            {
                Directory.CreateDirectory(Ninja.folderPath);
            }
            assemblyName = Assembly.GetExecutingAssembly().Location;
            asPath = System.IO.Path.GetDirectoryName(assemblyName);
            Ninja.tryAccess(null);
            if (File.Exists(Ninja.dbfile))
            {

                string jsonString = File.ReadAllText(Ninja.dbfile);
                Dictionary<string, object> db = new Dictionary<string, object>();
                db = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                db.TryGetValue("Version", out object Version);
                db.TryGetValue("UpdaterLink", out object updateLink);
                Link = updateLink.ToString();
                Version = Version.ToString();
                if (!Version.Equals(Ninja.version))
                {
                    UpdaterView updater = new UpdaterView(Version.ToString(), Link);
                    updater.ShowDialog();
                }
            }
            else
            {
                TaskDialog.Show("Error", "Please make sure you are connected to the internet!");
                return Result.Failed;
            }

            string TabName = "RevitNinja";
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return Result.Cancelled;
            }
            RibbonPanel infoPanel;
            infoPanel = application.CreateRibbonPanel(TabName, "About The Developer");

            RibbonPanel viewsPanel;
            viewsPanel = application.CreateRibbonPanel(TabName, "Views");

            RibbonPanel generalToolsPanel;
            generalToolsPanel = application.CreateRibbonPanel(TabName, "General Tools");

            RibbonPanel rebarPanel;
            rebarPanel = application.CreateRibbonPanel(TabName, "Rebar");

            RibbonPanel reviztoPanel;
            reviztoPanel = application.CreateRibbonPanel(TabName, "Revizto Tools");

            PushButtonData INFO = null, SAVESTATE = null, RESETSTATE = null, RESETSHEET = null, ALIGN2PTS = null, ALIGNELEMENTS = null;
            PushButtonData ALIGNTAGS = null, DELETECAD = null, HIDEUNHOSTED = null, NOS = null, REBARHOST = null, ROTATELOCALLY = null;
            PushButtonData SELECTBY = null, FINDREBAR = null, TOGGLEREBAR = null, BIMSUB = null, PENETRATION = null, COORDINATES = null;
            PushButtonData POINTSCOORDS = null, COORDSTABLE = null,GETLINKLOCATION=null;
            PushButtonData LOADISSUES = null, TOGGLEISSUES = null, PICKISSUE = null, VIEWISSUE = null, DELETEISSUES = null, MOVEISSUE = null, COLORTABS = null;

            try
            {
                INFO = new PushButtonData("About me", "About Me", assemblyName, typeof(Info).FullName)
                {
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/ninja.ico")),
                    ToolTip = "About the developer!"
                };
                COLORTABS = new PushButtonData("Color tabs by doc", "Color Tabs", assemblyName, typeof(ColorTabs).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/colorS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/colorL.ico")),
                    ToolTip = "About the developer!"
                };
                SAVESTATE = new PushButtonData("Save State", "Save View", assemblyName, typeof(SaveState).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/captures.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/capture.ico")),
                    ToolTip = "Save the visibility state of all elements in the current view for future restoration."
                };
                RESETSTATE = new PushButtonData("Reset State", "Reset View", assemblyName, typeof(ResetState).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/ResetViewS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/ResetView.ico")),
                    ToolTip = "Show only the elements previously saved in the view's state."
                };
                RESETSHEET = new PushButtonData("Reset Sheet State", "Reset Sheet", assemblyName, typeof(ResetSheet).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/resetSheetsS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/resetSheets.ico")),
                    ToolTip = "Restore the saved visibility states for all views on the active sheet or across all sheets."
                };
                ALIGN2PTS = new PushButtonData("Align Between 2 points", "Mid 2 Pts", assemblyName, typeof(AlignBetween2Pts).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/midBet2ptsS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/MidBet2ptsL.ico")),
                    ToolTip = "Align an element between two other elements in the current view."
                };
                ALIGNELEMENTS = new PushButtonData("Align Elements", "Element Align", assemblyName, typeof(AlignElements).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/alignElemsS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/alignElemsL.ico")),
                    ToolTip = "Align elements vertically or horizontally relative to another element or a point."
                };
                ALIGNTAGS = new PushButtonData("Align Tags", "Tags Align", assemblyName, typeof(AlignTags).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/alignTagsS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/alignTagsL.ico")),
                    ToolTip = " Align tags vertically or horizontally based on another tag or to a specific point."

                };
                DELETECAD = new PushButtonData("Delete CAD", "Delete DWG", assemblyName, typeof(DeleteCAD).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/deleteCADS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/deleteCADL.ico")),
                    ToolTip = "Remove all imported or linked DWG files from the project."
                };
                HIDEUNHOSTED = new PushButtonData("Hide Unhosted rebar", "Hide Unhosted", assemblyName, typeof(HideUnhosted).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/hideUnhostedS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/hideUnhosted.ico")),
                    ToolTip = "Automatically hide all rebar elements whose hosts are permanently hidden in the view."
                };
                NOS = new PushButtonData("Delete Not on sheets", "Delete unused Views", assemblyName, typeof(NotOnSheets).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/NOSsmall.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/NOS.ico")),
                    ToolTip = "Delete views that are not currently placed on any sheets."
                };
                BIMSUB = new PushButtonData("BIM Submittion", "Submit BIM Model", assemblyName, typeof(BIMSubmittal).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/NOSsmall.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/NOS.ico")),
                    ToolTip = "Delete views that are not currently placed on any sheets."
                };
                REBARHOST = new PushButtonData("Rebar By Host", "Hosted rebar", assemblyName, typeof(RebarByHost).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/HostS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/HostL.ico")),
                    ToolTip = "Select all rebar elements hosted by the chosen element."
                };
                ROTATELOCALLY = new PushButtonData("Rotate in place", "Rotate Locally", assemblyName, typeof(RotateElementsLocally).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/rotateLocallyS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/rotateLocallyL.ico")),
                    ToolTip = "Rotate selected elements around their individual center points."
                };
                SELECTBY = new PushButtonData("Select By Parameter", "Find By Param", assemblyName, typeof(SelectBy).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/byParamS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/byParamL.ico")),
                    ToolTip = "Select elements based on a specified parameter and its value."
                };
                FINDREBAR = new PushButtonData("Rebar Search", "Find Rebar", assemblyName, typeof(ShowFindRebar).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/selectbys.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/selectbyl.ico")),
                    ToolTip = "Locate rebar by its partition and number."
                };
                TOGGLEREBAR = new PushButtonData("Toggle Rebar", "Rebar On/Off", assemblyName, typeof(ToggleRebar).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/Rebs.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/Rebl.ico")),
                    ToolTip = "Toggle the visibility of the rebar category in the current view."
                };
                PENETRATION = new PushButtonData("Penetration", "Builder Work", assemblyName, typeof(Penetration).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/builderWorks.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/builderWork.ico")),
                    ToolTip = "Creates Builder Work on local project or linked projects."
                };
                PENETRATION = new PushButtonData("Penetration", "Builder Work", assemblyName, typeof(Penetration).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/builderWorks.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/builderWork.ico")),
                    ToolTip = "Creates Builder Work on local project or linked projects."
                };
                COORDINATES = new PushButtonData("Coordinates", "Piles Coordinates", assemblyName, typeof(Coordinates).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/coordS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/coordL.ico")),
                    ToolTip = "Gets the element location for the structural foundations / structural columns categories"
                };
                LOADISSUES = new PushButtonData("Load Revizto Issues", "Load Issues", assemblyName, typeof(ReviztoClashes).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/loadIssues.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/loadIssuesL.ico")),
                    ToolTip = "Load Revizto issues from excel file."
                };
                TOGGLEISSUES = new PushButtonData("Issues On/Off", "Toggle Issues", assemblyName, typeof(ToggleIssues).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/toggleIssues.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/toggleIssuesL.ico")),
                    ToolTip = "Hide/Unhide Clash Balls."
                };
                PICKISSUE = new PushButtonData("Find Issue", "Find", assemblyName, typeof(pickIssue).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/selectbys.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/selectbyl.ico")),
                    ToolTip = "Select Issue by issue ID."
                };
                VIEWISSUE = new PushButtonData("View Issue Details", "View Issue", assemblyName, typeof(ViewIssue).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/viewIssue.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/viewIssueL.ico")),
                    ToolTip = "Expand issue to see the full details."
                };
                DELETEISSUES = new PushButtonData("Delete all Revizto Issues", "Delete Issues", assemblyName, typeof(DeleteAllIssues).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/DeleteIssuesS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/DeleteIssuesL.ico")),
                    ToolTip = "Delete all imported issues."
                };
                MOVEISSUE = new PushButtonData("Move Issue in 3D", "Move Issue", assemblyName, typeof(movingIssue).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/MoveS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/MoveS.ico")),
                    ToolTip = "Move issue to selected point in 3D."
                };
                POINTSCOORDS = new PushButtonData("Points To Coordinates", "Points Coordinates", assemblyName, typeof(PointCoords).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/pointsS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/pointsL.ico")),
                    ToolTip = "Create points as spot coordinates."
                };
                COORDSTABLE = new PushButtonData("Operate on points", "Points Operations", assemblyName, typeof(OperateOnPoints).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/TableS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/TableL.ico")),
                    ToolTip = "Modify, Plot, Import and Export points."
                };
                GETLINKLOCATION = new PushButtonData("Getlocation", "Link Location", assemblyName, typeof(GetLinkLocation).FullName)
                {
                    Image = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/LocationS.ico")),
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/LocationL.ico")),
                    ToolTip = "Get the nearest shared location to a selected point from another rvt file."
                };

            }
            catch { }

            try
            {
                #region info Panel
                if (!(INFO is null)) infoPanel.AddItem(INFO);
                else TaskDialog.Show("Error", "INFO");
                #endregion

                #region view panel
                if (!(SAVESTATE is null)) viewsPanel.AddItem(SAVESTATE);
                else TaskDialog.Show("Error", "SAVESTATE");
                if (!(RESETSTATE is null && RESETSHEET is null)) viewsPanel.AddStackedItems(RESETSTATE, RESETSHEET);

                if (BIMSUB is null) TaskDialog.Show("Error", "BIMSUB");
                if (DELETECAD is null) TaskDialog.Show("Error", "DELETECAD");
                if (!(DELETECAD is null && BIMSUB is null))
                    viewsPanel.AddStackedItems(BIMSUB, DELETECAD);
                #endregion

                #region general tool panel
                if (!(COLORTABS is null)) generalToolsPanel.AddItem(COLORTABS);
                else TaskDialog.Show("Error", "COLORTABS");
                if (!(PENETRATION is null)) generalToolsPanel.AddItem(PENETRATION);
                else TaskDialog.Show("Error", "PENETRATION");

                if ((COORDINATES is null))
                    TaskDialog.Show("Error", "COORDINATES");
                if (POINTSCOORDS is null)
                    TaskDialog.Show("Error", "POINTSCOORDS");
                if (COORDSTABLE is null)
                    TaskDialog.Show("Error", "COORDSTABLE");
                if (!(POINTSCOORDS is null && COORDSTABLE is null && COORDINATES is null))
                    generalToolsPanel.AddStackedItems(COORDINATES, POINTSCOORDS, COORDSTABLE);

                if (!(SELECTBY is null)) generalToolsPanel.AddItem(SELECTBY);
                else TaskDialog.Show("Error", "SELECTBY");

                if (!(GETLINKLOCATION is null)) generalToolsPanel.AddItem(GETLINKLOCATION);
                else TaskDialog.Show("Error", "GETLINKLOCATION");

                //if (ALIGN2PTS is null)
                //    TaskDialog.Show("Error", "ALIGN2PTS");
                //if (ALIGNELEMENTS is null)
                //    TaskDialog.Show("Error", "ALIGNELEMENTS");
                //if (ALIGNTAGS is null)
                //    TaskDialog.Show("Error", "ALIGNTAGS");

                //if (!(ALIGNELEMENTS is null && ALIGN2PTS is null && ALIGNTAGS is null))
                //    generalToolsPanel.AddStackedItems(ALIGN2PTS, ALIGNELEMENTS, ALIGNTAGS);

                //if (!(ROTATELOCALLY is null)) generalToolsPanel.AddItem(ROTATELOCALLY);
                //else TaskDialog.Show("Error", "ROTATELOCALLY");
                #endregion

                #region rebar panel
                if (!(FINDREBAR is null)) rebarPanel.AddItem(FINDREBAR);
                else TaskDialog.Show("Error", "FINDREBAR");

                if (HIDEUNHOSTED is null) TaskDialog.Show("Error", "HIDEUNHOSTED");
                if (TOGGLEREBAR is null) TaskDialog.Show("Error", "TOGGLEREBAR");
                if (!(HIDEUNHOSTED is null && TOGGLEREBAR is null)) rebarPanel.AddStackedItems(TOGGLEREBAR, HIDEUNHOSTED);

                if (!(REBARHOST is null)) rebarPanel.AddItem(REBARHOST);
                else TaskDialog.Show("Error", "REBARHOST");
                #endregion

                #region revizto panel
                if (!(LOADISSUES is null)) reviztoPanel.AddItem(LOADISSUES);
                else TaskDialog.Show("Error", "LOADISSUES");

                if (VIEWISSUE is null) TaskDialog.Show("Error", "VIEWISSUE");
                if (MOVEISSUE is null) TaskDialog.Show("Error", "MOVEISSUE");
                if (!(VIEWISSUE is null && MOVEISSUE is null))
                    reviztoPanel.AddStackedItems(VIEWISSUE, MOVEISSUE);

                if (!(PICKISSUE is null || TOGGLEISSUES is null || DELETEISSUES is null)) reviztoPanel.AddStackedItems(PICKISSUE, TOGGLEISSUES, DELETEISSUES);
                else TaskDialog.Show("Error", "PICKISSUE,TOGGLEISSUES, DELETEISSUES");
                #endregion
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("exception", ex.ToString());
            }

            return Result.Succeeded;
        }
    }
}
