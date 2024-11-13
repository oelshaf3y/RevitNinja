using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitNinja.Views
{
    /// <summary>
    /// Interaction logic for SelectByView.xaml
    /// </summary>
    public partial class SelectByView : UserControl
    {
        UIDocument uidoc;
        Document doc;
        public Brush background;
        FilteredElementCollector AllElements;
        List<ElementId> ids;
        List<string> Parameters { get; set; } = new List<string>();
        UIFramework.ApplicationTheme theme;
        public SelectByView(UIFramework.ApplicationTheme theme, UIDocument uidoc)
        {
            InitializeComponent();

            this.theme = theme;
            this.uidoc = uidoc;
            doc = uidoc.Document;
            AllElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
            ids = new List<ElementId>();
            Parameters.AddRange(AllElements.SelectMany(x => x.GetOrderedParameters()).Select(x => x.Definition.Name).Distinct().ToList());
            Parameters.Sort();
            Parameters.ForEach(x =>
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = x;
                paramCombo.Items.Add(item);
            });
            this.Colorize(this.theme);

        }
        public void cancel(object sender, RoutedEventArgs e)
        {
            RibbonController.HideOptionsBar();
        }
        public void click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = paramVal.Text;
                string parName = paramCombo.Text;
                foreach (Element element in AllElements)
                {
                    if (element.LookupParameter(parName) != null)
                    {
                        if (element.LookupParameter(parName).AsString() != null)
                            if (element.LookupParameter(parName).AsString().ToLower().Contains(val.ToLower()))
                            {
                                if (element.Id != null) ids.Add(element.Id);
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
            }
            uidoc.Selection.SetElementIds(ids);
            RibbonController.HideOptionsBar();
        }
        

    }
}
