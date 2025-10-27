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
using Revit_Ninja.Views;
using RevitNinja.Utils;

namespace RevitNinja.Views
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class InfoView : Window
    {
        public InfoView()
        {
            InitializeComponent();
            this.VersionLabel.Content = "Version: +" + Ninja.version;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void linkedin(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.linkedin.com/in/oelshaf3y/");
        }
        private void gitHub(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/oelshaf3y/RevitNinja");
        }

        private void giveFeedback(object sender, RoutedEventArgs e)
        {
            feedback feedbackWindow = new feedback();
            feedbackWindow.Show();

        }
    }
}
