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
using RevitNinja.Views;

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
               .WhereElementIsNotElementType().Cast<View>().Where(v => v !=null).Where(x => doc.GetElement(x.GetTypeId()) != null).ToList();
            if (doc.ActiveView == null)
            {
                doc.print("shit");
                return;
            }
            currentView = doc.ActiveView;
            allViews.ForEach(view => copyFromCombo.Items.Add(view.Name + " - " + doc.GetElement(view.GetTypeId())?.Name));
            foreach (View v in allViews.Where(x=> doc.GetElement(x.GetTypeId()) != null))
            {
                if (v == null) continue;
                CheckBox cb = new CheckBox();
                cb.Content = v.Name + " - " + doc.GetElement(v.GetTypeId())?.Name;
                listBox.Items.Add(cb);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ok_button_clicked(object sender, RoutedEventArgs e)
        {
            using (Transaction tr = new Transaction(doc, "Add View Filter"))
            {
                tr.Start();
                View sourceView = allViews[copyFromCombo.SelectedIndex];
                filters = sourceView.GetFilters().ToList();
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    try
                    {
                        View selectedView = allViews[i];
                        if (sourceView.Id == selectedView.Id) continue;
                        if (removeCurrent.IsChecked == true)
                        {
                            List<ElementId> viewFilters = selectedView.GetFilters().ToList();
                            viewFilters.ForEach(id => selectedView.RemoveFilter(id));
                        }
                        filters.ForEach(filter =>
                        {
                            selectedView.AddFilter(filter);
                            selectedView.SetFilterVisibility(filter, true);
                            OverrideGraphicSettings over = sourceView.GetFilterOverrides(filter);
                            selectedView.SetFilterOverrides(filter, over);
                        });
                    }
                    catch (Exception ee)
                    {

                        doc.print(ee.Message);
                    }
                }
                tr.Commit();
            }
        }

        private void currentViewCB_Checked(object sender, RoutedEventArgs e)
        {
            //doc.print();
            int index = allViews.IndexOf(allViews.Where(v => v.Id == currentView.Id).First());
            copyFromCombo.SelectedIndex = index;
            copyFromCombo.IsEnabled = false;
        }

        private void currentViewCB_Unchecked(object sender, RoutedEventArgs e)
        {
            copyFromCombo.IsEnabled = true;
        }

        private void allViewsCB_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in listBox.Items)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    cb.IsChecked = true;
                }
            }
        }

        private void allViewsCB_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in listBox.Items)
            {
                CheckBox cb = item as CheckBox;
                if (cb != null)
                {
                    cb.IsChecked = false;
                }
            }
        }
    }
}
