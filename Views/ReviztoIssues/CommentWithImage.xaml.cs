using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CommentWithImage.xaml
    /// </summary>
    public partial class CommentWithImage : UserControl
    {
        public CommentWithImage(Comment comment)
        {
            InitializeComponent();

            Image img = new Image();
            img.Width = Double.NaN;
            img.Height = Double.NaN;
            string fullFilePath = comment.Content;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fullFilePath, UriKind.Absolute);
            bi.EndInit();

            img.Source = bi;
            imagePanel.Children.Add(img);
            providerLabel.Content = comment.Provider;
            dateLabel.Content = comment.Date;

        }
    }
}
