using System.Windows;

namespace Revit_Ninja.Views.ReviztoIssues
{
    /// <summary>
    /// Interaction logic for PickIssueView.xaml
    /// </summary>
    public partial class PickIssueView : Window
    {
        public bool viewIssue = false;
        public bool selectIssue = false;
        public int issueId = 0;
        public PickIssueView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(idBox.Text, out issueId))
            {

                viewIssue = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid issue ID.");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(idBox.Text, out issueId))
            {
                selectIssue = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid issue ID.");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
