using RevitNinja.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Revit_Ninja.Views
{
    /// <summary>
    /// Interaction logic for feedback.xaml
    /// </summary>
    public partial class feedback : Window
    {
        public feedback()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string name = nameBox.Text;
            string phone = phoneBox.Text;
            string message = messageBox.Text;
            if (message.Trim().Length == 0)
            {
                MessageBox.Show("Please enter a message before submitting.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (File.Exists(Ninja.dbfile))
            {
                Dictionary<string, object> db = new Dictionary<string, object>();
                Dictionary<string, object> feedback = new Dictionary<string, object>();
                db = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(Ninja.dbfile));
                feedback.Add("Name", name);
                feedback.Add("Phone", phone);
                feedback.Add("Message", message);
                db.Add("Feedback", feedback);
                File.WriteAllText(Ninja.dbfile, JsonSerializer.Serialize(db));
            }
            this.Close();
        }
    }
}
