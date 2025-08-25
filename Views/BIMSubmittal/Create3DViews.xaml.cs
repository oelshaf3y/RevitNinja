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

namespace Revit_Ninja.Views.BIMSubmittal
{
    /// <summary>
    /// Interaction logic for Create3DViews.xaml
    /// </summary>
    public partial class Create3DViews : Window
    {
        public Create3DViews()
        {
            InitializeComponent();
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();

        }

        private void saveViewNames(object sender, RoutedEventArgs e)
        {
            DialogResult=true;
            this.Close();
        }
    }
}
