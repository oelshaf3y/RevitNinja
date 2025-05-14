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
using System.Windows.Navigation;
using System.Windows.Shapes;
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
