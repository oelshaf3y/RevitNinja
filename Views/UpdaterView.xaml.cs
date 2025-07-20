using System.Diagnostics;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Shapes;
using Path = System.IO.Path;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Windows.Controls;
using RevitNinja.Utils;
using File = System.IO.File;

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
            //update the db
            string path = Path.Combine(Ninja.folderPath, "NinjaUpdater.exe");
            StartDownload(Link, path);

            //if (!File.Exists(path))
            //{
            //    // 1. Download the updater
            //    StartDownload(Link, path);
            //}

            if (File.Exists(Ninja.dbfile))
            {
                Dictionary<string, object> db = new Dictionary<string, object>();
                db = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(Ninja.dbfile));
                db.Add("UpdateOnClose", true);
                db.Add("UpdaterPath", path);
                File.WriteAllText(Ninja.dbfile, JsonSerializer.Serialize(db));
            }


            this.Close();

        }
        private async Task StartDownload(string url, string destination)
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
