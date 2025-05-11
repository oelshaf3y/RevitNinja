using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using RevitNinja.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ShowFindRebar : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        FindRFT uc;
        ComboBox partitions;
        TextBox rebNum;
        string background;

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // External Event for the dialog to use (to post requests)
                uidoc = commandData.Application.ActiveUIDocument;
                doc = uidoc.Document;

                if (!doc.getAccess())  return Result.Failed;

                var theme = UIFramework.ApplicationTheme.CurrentTheme;

                uc = new FindRFT(theme, uidoc);
                var panel = uc.stack;
                Button can = VisualUtils.FindVisualChild<Button>(panel, "Cancel");
                Button but = VisualUtils.FindVisualChild<Button>(panel, "but");
                partitions = VisualUtils.FindVisualChild<ComboBox>(panel, "partitionCombo");
                rebNum = VisualUtils.FindVisualChild<TextBox>(panel, "rebNumber");
                List<string> partitionList = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rebar)
                    .WhereElementIsNotElementType().Select(x => x.LookupParameter("Partition").AsString()).Distinct().ToList();
                foreach (var partition in partitionList)
                {
                    partitions.Items.Add(partition);
                }
                //Button can = panel.FindDescendants<Button>().First(x => x.Name == "Cancel");
                //Button but = panel.FindDescendants<Button>().First(x => x.Name == "but");
                if (can != null) can.Click += Cancel;
                else return Result.Cancelled;
                if (but != null) but.Click += uc.click;
                else return Result.Cancelled;
                if (panel != null)
                {
                    RibbonController.ShowOptionsBar(uc, true);
                }
                else
                {
                    doc.print("Null");
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
        public void Cancel(object sender, RoutedEventArgs e)
        {
            RibbonController.HideOptionsBar();
        }
        public void FindRFT(object sender, RoutedEventArgs e)
        {
            int rftNum = 0;
            if (!int.TryParse(rebNum.Text, out rftNum) || !(rftNum > 0))
            {
                doc.print("Please insert a correct rebar number");
                return;
            }
            doc.print("raise");
            try
            {
                //doc.print("button clicked");


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error raising external event: " + ex.Message);
            }
        }

    }
}
