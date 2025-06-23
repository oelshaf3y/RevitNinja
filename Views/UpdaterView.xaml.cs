using System.Diagnostics;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace RevitNinja.Views
{
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class UpdaterView : Window
    {
        string Link;
        public UpdaterView(string Version, string Link)
        {
            this.Link = Link;
            InitializeComponent();
            this.versionLabel.Content = $"There's an Update to Version {Version}!\nDo you want to update to the latest version?";
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "NinjaUpdater.exe");
            if (!File.Exists(path))
            {
                // 1. Download the updater
                StartDownload(Link, path);
            }
            // 2. Launch the updater with a user-friendly message
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true // required for showing any UI or running elevated
            });
            this.Close();

        }
        private void StartDownload(string url, string destination)
        {
            WebClient client = new WebClient();

            client.DownloadProgressChanged += (s, e) =>
            {
                DownloadProgressBar.Value = e.ProgressPercentage;
                ProgressText.Text = $"{e.ProgressPercentage}% ({e.BytesReceived / 1024} KB / {e.TotalBytesToReceive / 1024} KB)";
            };

            client.DownloadFileCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    MessageBox.Show("Download failed: " + e.Error.Message);
                }
                else
                {
                    MessageBox.Show("Download completed!");
                }
                client.Dispose(); // Don't forget to dispose
            };

            client.DownloadFileAsync(new Uri(url), destination);
        }
    }
}
