using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using RevitNinja.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
namespace RevitNinja.Views
{
    /// <summary>
    /// Interaction logic for FindRFT.xaml
    /// </summary>
    public partial class FindRFT : UserControl
    {
        //public StackPanel _stackPanel;
        private string rftNum;
        UIDocument uidoc;
        Document doc;
        public Brush background;
        UIFramework.ApplicationTheme theme;
        public FindRFT(UIFramework.ApplicationTheme theme, UIDocument uidoc)
        {
            InitializeComponent();


            this.theme = theme;
            this.uidoc = uidoc;
            doc = uidoc.Document;
            this.Colorize(this.theme);
        }

        public void click(object sender, RoutedEventArgs e)
        {
            string partition = partitionCombo.SelectedItem.ToString();
            string rftNumber = rebNumber.Text;
            try
            {
                List<Rebar> found = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Rebar)
                    .WhereElementIsNotElementType()
                    .Where(x => x.LookupParameter("Partition") != null && x.LookupParameter("Partition").AsString() == partition)
                    .Where(x => x.LookupParameter("Rebar Number") != null && x.LookupParameter("Rebar Number").AsString() == rftNumber.ToString())
                    .Cast<Rebar>().ToList();

                if (found.Any())
                {
                    uidoc.Selection.SetElementIds(found.Select(x => x.Id).ToList());
                }
                else
                {
                    TaskDialog.Show("Find RFT", "No rebar found with the specified number and partition.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            RibbonController.HideOptionsBar();
        }
    }
}
