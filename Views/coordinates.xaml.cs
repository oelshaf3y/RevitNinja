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

namespace Revit_Ninja.Views
{
    /// <summary>
    /// Interaction logic for coordinates.xaml
    /// </summary>
    public partial class coordinates : Window
    {
        public bool cancel = false;
        public coordinates(List<Parameter> parameters)
        {
            InitializeComponent();
            foreach (var item in parameters)
            {
                eastingCombo.Items.Add(item.Definition.Name);
                northingCombo.Items.Add(item.Definition.Name);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.cancel = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}