using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace RevitNinja.Views
{
    /// <summary>
    /// Interaction logic for AlignTagView.xaml
    /// </summary>
    public partial class AlignView : UserControl
    {

        public AlignView(UIFramework.ApplicationTheme theme, UIDocument uidoc)
        {
            InitializeComponent();
            ComboBoxItem item1 = new ComboBoxItem();
            item1.Content = "Horizontally";
            ComboBoxItem item2 = new ComboBoxItem();
            item2.Content = "Vertically";
            alignCombo.Items.Add(item1);
            alignCombo.Items.Add(item2);
            alignCombo.SelectedIndex = 0;
            this.Colorize(theme);
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.offVal.IsEnabled = !this.offVal.IsEnabled;
        }

    }
}
