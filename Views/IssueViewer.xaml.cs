using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Revit_Ninja.Commands.ReviztoIssues;
using Revit_Ninja.Views.ReviztoIssues;

namespace Revit_Ninja.Views
{
    /// <summary>
    /// Interaction logic for IssueViewer.xaml
    /// </summary>
    public partial class IssueViewer : Window
    {
        public bool solved = false;
        public IssueViewer(Issue issue)
        {
            InitializeComponent();
            if (issue == null)
            {
                MessageBox.Show("No issue data provided.");
                return;
            }
            idBox.Text = issue.Id;
            dateLabel.Content = issue.Date;
            reporterLabel.Text = issue.Reporter;
            statusLabel.Content = issue.Status;
            titleLabel.Text = issue.Title;
            stampLabel.Content = issue.Stamp;
            levelLabel.Text = issue.Level;
            gridLabel.Text = issue.GridLocation;
            zoneLabel.Text = issue.Zone;
            stampTitleLabel.Text = issue.StampTitle;

            Image img = new Image();
            img.Width = Double.NaN;
            img.Height = Double.NaN;

            string fullFilePath = issue.SnapshotLink;

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bi.EndInit();

            img.Source = bi;
            imagePanel.Children.Add(img);

            img.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) =>
            {
                Window imageWindow = new Window();
                WrapPanel wrap = new WrapPanel();
                Image img2 = new Image();
                img2.Width = Double.NaN;
                img2.Height = Double.NaN;
                img2.Source = bi;
                wrap.Children.Add(img2);
                imageWindow.Content = wrap;
                imageWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                imageWindow.Show();
                //img.Width = 1000;
            };
            foreach (Comment comment in issue.Comments)
            {
                if (comment.Content.StartsWith("http"))
                {
                    CommentWithImage uc = new CommentWithImage(comment);
                    commentsPanel.Children.Add(uc);
                }
                else
                {
                    CommentViewer uc = new CommentViewer(comment);
                    commentsPanel.Children.Add(uc);
                }
            }
        }

        private void closeBut_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void copyId_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(idBox.Text);
        }

        private void solveBut_Click(object sender, RoutedEventArgs e)
        {
            solved = true;
            this.Close();
        }
    }
}
