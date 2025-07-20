using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Revit_Ninja.Commands.PointsCoord;

namespace Revit_Ninja.Views.PointCoord
{
    /// <summary>
    /// Interaction logic for EditPoints.xaml
    /// </summary>
    public partial class EditPoints : Window
    {
        public List<PointLocation> points = new List<PointLocation>();
        public bool edit = false;
        public bool plot = false;
        public int newID = 0;
        public EditPoints(List<PointLocation> points)
        {
            this.points = points;
            InitializeComponent();
            var prefixes = points.Select(x => x.Prefix).Distinct().ToList();
            prefixes.ForEach(p => prefixCombo.Items.Add(p));
            prefixCombo.Items.Add("All");
            numberBox.Text = points.Min(x => x.ID).ToString();
            if (prefixCombo.Items.Count > 0) prefixCombo.SelectedIndex = prefixCombo.Items.Count - 1;
        }

        private void IntegerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private static bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }
        private void editButClick(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(newNumber.Text, out newID))
            {
                MessageBox.Show("Please enter an integer.");
                return;
            }
            edit = true;
            DialogResult = true;
            Close();
        }

        private void cancelButClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void prefixCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suffixCombo.Items.Count > 0) suffixCombo.Items.Clear();
            var suffixes = points.Where(x => x.Prefix == prefixCombo.Items[prefixCombo.SelectedIndex].ToString()).Select(x => x.Suffix).Distinct().ToList();
            suffixes.ForEach(s => suffixCombo.Items.Add(s));
            if (suffixCombo.Items.Count > 0) suffixCombo.SelectedIndex = 0;
            newPrefix.Text = prefixCombo.Items[prefixCombo.SelectedIndex].ToString();
        }

        private void numberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            newNumber.Text = numberBox.Text;
        }

        private void suffixCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newSuffix.Text = suffixCombo.Items[suffixCombo.SelectedIndex].ToString();
        }

        private void plotButClick(object sender, RoutedEventArgs e)
        {
            plot = true;
            DialogResult = true;
            Close();
        }

        private void allPointsCB_Checked(object sender, RoutedEventArgs e)
        {
            suffixCombo.IsEnabled = false;
            prefixCombo.IsEnabled = false;
        }
        private void allPointsCB_UnChecked(object sender, RoutedEventArgs e)
        {
            suffixCombo.IsEnabled = true;
            prefixCombo.IsEnabled = true;
        }
    }
}
