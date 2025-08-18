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
using Revit_Ninja.Commands;

namespace Revit_Ninja.Views.BIMSubmittal
{
    /// <summary>
    /// Interaction logic for publishSet.xaml
    /// </summary>
    public partial class publishSet : Window
    {
        public publishSet()
        {
            InitializeComponent();
        }

        private void all3d_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list3D.Items)
            {
                viewforset v = item as viewforset;
                v.IsChecked = true;
            }
            list3D.Items.Refresh();
        }

        private void none3d_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list3D.Items)
            {
                viewforset v = item as viewforset;
                v.IsChecked = false;
            }
            list3D.Items.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void all2d_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list2D.Items)
            {
                viewforset v = item as viewforset;
                v.IsChecked = true;
            }
            list2D.Items.Refresh();
        }

        private void none2d_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in list2D.Items)
            {
                viewforset v = item as viewforset;
                v.IsChecked = false;
            }
            list2D.Items.Refresh();
        }

        private void allsheets_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in listSheets.Items)
            {
                viewforset v = item as viewforset;
                v.IsChecked = true;
            }
            listSheets.Items.Refresh();
        }

        private void nonesheets_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in listSheets.Items)
            {
                viewforset v = item as viewforset;
                v.IsChecked = false;
            }
            listSheets.Items.Refresh();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(setName.Text.Length == 0)
            {
                MessageBox.Show("Please enter a name for the set.");
                return;
            }
            else if(setName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter a valid name for the set.");
                return;
            }
            DialogResult = true;
            this.Close();
        }
    }
}
