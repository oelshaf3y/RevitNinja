using System.Windows.Controls;
using Revit_Ninja.Commands.ReviztoIssues;

namespace Revit_Ninja.Views.ReviztoIssues
{
    /// <summary>
    /// Interaction logic for CommentViewer.xaml
    /// </summary>
    public partial class CommentViewer : UserControl
    {
        public CommentViewer(Comment comment)
        {
            InitializeComponent();
            contentBox.Text = comment.Content;
            providerLabel.Content = comment.Provider;
            dateLabel.Content = comment.Date;
        }
    }
}
