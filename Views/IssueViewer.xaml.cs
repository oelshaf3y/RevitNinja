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
using Autodesk.Revit.UI;
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
            idBox.Text = issue.Id;
            dateLabel.Content = issue.Date;
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
