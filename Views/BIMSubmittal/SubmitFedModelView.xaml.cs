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
using Revit_Ninja.Commands.BIMSubmittal;

namespace Revit_Ninja.Views.BIMSubmittal
{
    /// <summary>
    /// Interaction logic for SubmitFedModelView.xaml
    /// </summary>
    public partial class SubmitFedModelView : Window
    {
        public List<linkforset> RLIS = new List<linkforset>();
        public SubmitFedModelView(List<linkforset> rlis)
        {
            InitializeComponent();
            RLIS = rlis;
            list3D.ItemsSource = RLIS;
        }

        public void checkAll(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is linkforset item)
            {
                // Toggle the state for this row only

                item.IsChecked = true;
                item.create3D = true;
                item.deleteWIP = true;
                item.removeCad = true;
                item.removeLinks = true;
                item.populateSections = true;
                item.purgeFilters = true;
                item.purgeSets = true;
                item.resetBrowser = true;


                list3D.Items.Refresh();
            }
        }
        public void unCheckAll(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is linkforset item)
            {
                // Toggle the state for this row only

                item.IsChecked = false;
                item.create3D = false;
                item.deleteWIP = false;
                item.removeCad = false;
                item.removeLinks = false;
                item.populateSections = false;
                item.purgeFilters = false;
                item.purgeSets = false;
                item.resetBrowser = false;


                list3D.Items.Refresh();
            }
        }
        private void ok_click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void cancel_click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
