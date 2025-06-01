using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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
            imagePanel.Children.Add(img);
            providerLabel.Content = comment.Provider;
            dateLabel.Content = comment.Date;

        }
    }
}
