﻿using Autodesk.Revit.UI;
using Revit_Ninja.Commands;
using RevitNinja.Commands;
using RevitNinja.Commands.ViewState;
using RevitNinja.Utils;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RevitNinja
{
    internal class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Failed;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string assemblyName = Assembly.GetExecutingAssembly().Location;
            string asPath = System.IO.Path.GetDirectoryName(assemblyName);
            string TabName = "RSCC";
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
            //infoPanel = application.CreateRibbonPanel(TabName, "About The Developer");
            RibbonPanel viewsPanel;
            viewsPanel = application.CreateRibbonPanel(TabName, "Views");
            RibbonPanel rebarPanel;
            //rebarPanel = application.CreateRibbonPanel(TabName, "Rebar");
            RibbonPanel generalToolsPanel;
            generalToolsPanel = application.CreateRibbonPanel(TabName, "General Tools");
            PushButtonData INFO = null, SAVESTATE = null, RESETSTATE = null, RESETSHEET = null, ALIGN2PTS = null, ALIGNELEMENTS = null;
            PushButtonData ALIGNTAGS = null, DELETECAD = null, HIDEUNHOSTED = null, NOS = null, REBARHOST = null, ROTATELOCALLY = null; ;
            PushButtonData SELECTBY = null, FINDREBAR = null, TOGGLEREBAR = null, BIMSUB=null;
            try
            {
                INFO = new PushButtonData("About me", "About Me", assemblyName, typeof(Info).FullName)
                {
                    LargeImage = new BitmapImage(new Uri("pack://application:,,,/RevitNinja;component/Resources/ninja.ico")),
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

            }
            catch { }



            // DELETECAD = null,
            // ROTATELOCALLY = null; ;
            // SELECTBY = null,

            try
            {
                //if (!(INFO is null)) infoPanel.AddItem(INFO);
                //else TaskDialog.Show("Error", "INFO");

                if (!(SAVESTATE is null)) viewsPanel.AddItem(SAVESTATE);
                else TaskDialog.Show("Error", "SAVESTATE");
                if (!(RESETSTATE is null)) viewsPanel.AddItem(RESETSTATE);
                else TaskDialog.Show("Error", "RESETSTATE");
                if (!(RESETSHEET is null)) viewsPanel.AddItem(RESETSHEET);
                else TaskDialog.Show("Error", "RESETSHEET");
                //if (!(NOS is null)) viewsPanel.AddItem(NOS);
                //else TaskDialog.Show("Error", "NOS");

                if (!(BIMSUB is null)) viewsPanel.AddItem(BIMSUB);
                else TaskDialog.Show("Error", "NOS");


                //if (!(HIDEUNHOSTED is null)) rebarPanel.AddItem(HIDEUNHOSTED);
                //else TaskDialog.Show("Error", "HIDEUNHOSTED");
                //if (!(TOGGLEREBAR is null)) rebarPanel.AddItem(TOGGLEREBAR);
                //else TaskDialog.Show("Error", "TOGGLEREBAR");
                //if (!(REBARHOST is null)) rebarPanel.AddItem(REBARHOST);
                //else TaskDialog.Show("Error", "REBARHOST");
                //if (!(FINDREBAR is null)) rebarPanel.AddItem(FINDREBAR);
                //else TaskDialog.Show("Error", "FINDREBAR");

                if (!(DELETECAD is null)) generalToolsPanel.AddItem(DELETECAD);
                else TaskDialog.Show("Error", "DELETECAD");
                //if (!(SELECTBY is null)) generalToolsPanel.AddItem(SELECTBY);
                //else TaskDialog.Show("Error", "SELECTBY");
                //if (!(ROTATELOCALLY is null)) generalToolsPanel.AddItem(ROTATELOCALLY);
                //else TaskDialog.Show("Error", "ROTATELOCALLY");
                //if (ALIGN2PTS is null)
                //    TaskDialog.Show("Error", "ALIGN2PTS");
                //if (ALIGNELEMENTS is null)
                //    TaskDialog.Show("Error", "ALIGNELEMENTS");
                //if (ALIGNTAGS is null)
                //    TaskDialog.Show("Error", "ALIGNTAGS");
                //if (!(ALIGNELEMENTS is null && ALIGN2PTS is null && ALIGNTAGS is null))
                //    generalToolsPanel.AddStackedItems(ALIGN2PTS, ALIGNELEMENTS, ALIGNTAGS);
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("exception", ex.ToString());
                //TaskDialog.Show("exception", ex.Message);

            }
            return Result.Succeeded;
        }
    }
}
