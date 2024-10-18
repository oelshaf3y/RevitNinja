using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows;

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
            string TabName = "Ninja-wpf";
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return Result.Cancelled;
            }
            RibbonPanel panel;
            panel = application.CreateRibbonPanel(TabName, "Revit Ninja");
            //PushButtonData FINDRFT = new PushButtonData("Find Rebar", "Find RFT", assemblyName, "Revit_Ninja_WPF.RFT.ShowFindRebar")
            //{
            //    Image = Properties.Resources.selectbys.ToImageSource(),
            //    LargeImage = Properties.Resources.selectbyl.ToImageSource(),
            //    ToolTip = "Save visible elements in the current view to be reset later"
            //};

            try
            {
                //panel.AddItem(BatchPrint);
                //panel.AddSeparator();

                //panel.AddItem(FINDRFT);
                //panel.AddSeparator();

                //panel.AddSeparator();
            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("exception", ex.StackTrace);
                TaskDialog.Show("exception", ex.Message);

            }
            //panel.AddItem(SaveState);
            //panel.AddItem(restoreState);
            //panel.AddItem(ResetSheets);

            return Result.Succeeded;
        }
    }
}
