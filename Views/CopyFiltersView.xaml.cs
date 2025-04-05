using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace Revit_Ninja.Views
{
    /// <summary>
    /// Interaction logic for CopyFiltersView.xaml
    /// </summary>
    public partial class CopyFiltersView : Window
    {
        UIDocument uidoc;
        Document doc;
        List<View> allViews = new List<View>();
        List<ElementId> filters = new List<ElementId>();
        View currentView;
        public CopyFiltersView(UIDocument uidoc)
        {
            this.uidoc = uidoc;
            this.doc = uidoc.Document;
            InitializeComponent();
            allViews = new FilteredElementCollector(this.doc).OfCategory(BuiltInCategory.OST_Views)
               .WhereElementIsNotElementType().Cast<View>().ToList();
            View currentView = doc.ActiveView;
            filters = currentView.GetFilters().ToList();
            allViews.ForEach(view => copyFromCombo.Items.Add(view.Name+" - "+doc.GetElement(view.GetTypeId()).Name));
            allViews.ForEach(view => copyToCombo.Items.Add(view.Name+" - " + doc.GetElement(view.GetTypeId()).Name));

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (Transaction tr = new Transaction(doc, "Add View Filter"))
            {
                tr.Start();
                foreach (View view in allViews)
                {
                    if (view.Id != currentView.Id)
                    {
                        foreach (ElementId filter in filters)
                        {
                            if (!view.GetFilters().Contains(filter))
                            {
                                try
                                {
                                    view.AddFilter(filter);
                                    view.SetFilterVisibility(filter, true);
                                    OverrideGraphicSettings over = currentView.GetFilterOverrides(filter);
                                    view.SetFilterOverrides(filter, over);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
                tr.Commit();
            }
        }

        private void currentViewCB_Checked(object sender, RoutedEventArgs e)
        {
            doc.print(allViews.Where(v => v.Id == currentView.Id).First().Name);
            //int index = allViews.IndexOf();
            //copyFromCombo.SelectedIndex = index;
            copyFromCombo.IsEnabled = false;
        }

        private void currentViewCB_Unchecked(object sender, RoutedEventArgs e)
        {
            copyFromCombo.IsEnabled = true;
        }
    }
}
