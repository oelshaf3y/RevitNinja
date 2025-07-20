using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
using Autodesk.Revit.DB;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.Penetration
{
    /// <summary>
    /// Interaction logic for PenetrationForm.xaml
    /// </summary>
    public partial class PenetrationForm : Window
    {
        public List<string> linksnames;
        public List<int> linksIndices = new List<int>();
        public bool bySelection, click;
        List<FamilySymbol> familySymbols;
        List<string> families;
        public FamilySymbol famSymb;
        IList<FamilySymbol> symbols;
        public bool state;
        Document doc;
        public PenetrationForm(List<string> linksnames, List<FamilySymbol> famSymbols, List<string> families, Document doc)
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 3;
            comboBox2.SelectedIndex = 1;
            elementTypeCombo.SelectedIndex=0;
            this.doc = doc;
            this.linksnames = linksnames;
            click = false;
            this.familySymbols = famSymbols;
            this.families = families;
            this.families.Select(x => x).Distinct().ToList().ForEach(x => comboBox1.Items.Add(x));
            elementTypeCombo.Items.Add("Pipe");
            elementTypeCombo.Items.Add("Duct");
            elementTypeCombo.Items.Add("Cable Tray");
            elementTypeCombo.Items.Add("Conduit");
            foreach (string s in linksnames)
            {
                CheckBox c = new CheckBox();
                c.Content = s;
                checkedListBox1.Items.Add(c);
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.linkedin.com/in/oelshaf3y");

        }

        private void label6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.linkedin.com/in/tarek-mahmoud-ahmed-103041204");

        }

        private void cancelBut_click(object sender, RoutedEventArgs e)
        {
            state = false;
            this.Close();
        }

        private void okBut_click(object sender, RoutedEventArgs e)
        {

            if (radioButton2.IsChecked == true)
            {
                if (comboBox1.SelectedIndex == -1)
                {
                    doc.print("Please select a family name");
                    return;
                }
                else if (comboBox2.SelectedIndex == -1)
                {
                    doc.print("Please Select Family Type");
                    return;

                }
                else if (elementTypeCombo.SelectedIndex == -1)
                {
                    doc.print("Please Select a Category");
                    return;
                }
                else
                {

                    famSymb = this.symbols.Where(x => x.Name == comboBox2.SelectedItem.ToString() && x.Family.Name == comboBox1.SelectedItem.ToString()).FirstOrDefault();
                }
            }
            else
            {
                if (linksIndices.Count == 0 && (nativeStr.IsChecked == false || nativeMec.IsChecked == false))
                {
                    doc.print("Please select a link to create the penetrations in it");
                    return;
                }
            }
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                CheckBox c = checkedListBox1.Items[i] as CheckBox;
                if (c.IsChecked == true)
                {
                    linksIndices.Add(i);
                }
            }
            bySelection = checkBox1.IsChecked == true ? true : false;
            state = true;
            this.Close();
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            label1.Visibility = radioButton2.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            label2.Visibility = radioButton2.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            label3.Visibility = radioButton2.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            comboBox1.Visibility = radioButton2.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            comboBox2.Visibility = radioButton2.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            elementTypeCombo.Visibility = radioButton2.IsChecked == true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            comboBox2.Items.Clear();
            symbols = this.familySymbols.Where(x => x.Family.Name == comboBox1.Items[comboBox1.SelectedIndex].ToString()).ToList();
            symbols.Select(x => x.Name).ToList().ForEach(x => comboBox2.Items.Add(x));
        }

    }
}
