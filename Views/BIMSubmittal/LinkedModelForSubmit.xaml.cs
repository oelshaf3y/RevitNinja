using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Ninja.Commands.BIMSubmittal;
using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;

namespace Revit_Ninja.Views.BIMSubmittal
{
    /// <summary>
    /// Interaction logic for LinkedModelForSubmit.xaml
    /// </summary>
    public partial class LinkedModelForSubmit : UserControl
    {
        linkforset LINK;
        public LinkedModelForSubmit(linkforset link)
        {
            InitializeComponent();
            LINK = link;
            modelName.Content = LINK.Name;

        }
        public void checkAll(object sender, RoutedEventArgs e)
        {
            ToggleButton cb = sender as ToggleButton;
            if (cb.IsChecked == true)
            {
                create3D.IsChecked = true;
                deleteWIP.IsChecked = true;
                removeCad.IsChecked = true;
                removeLinks.IsChecked = true;
                populateSections.IsChecked = true;
                purgeFilters.IsChecked = true;
                purgeSets.IsChecked = true;
                resetBrowser.IsChecked = true;
                exportNwc.IsChecked = true;
                exportIFC.IsChecked = true;
                exportDWFx.IsChecked = true;
                saveLocal.IsChecked = true;

            }
        }
        public void uncheckAll(object sender, RoutedEventArgs e)
        {
            ToggleButton cb = sender as ToggleButton;
            if (cb.IsChecked == false)
            {
                create3D.IsChecked = false;
                deleteWIP.IsChecked = false;
                removeCad.IsChecked = false;
                removeLinks.IsChecked = false;
                populateSections.IsChecked = false;
                purgeFilters.IsChecked = false;
                purgeSets.IsChecked = false;
                resetBrowser.IsChecked = false;
                exportNwc.IsChecked = false;
                exportIFC.IsChecked = false;
                exportDWFx.IsChecked = false;
                saveLocal.IsChecked = false;

            }

        }
        public linkforset updateLink()
        {
            LINK.IsChecked = (bool)isChecked.IsChecked;
            LINK.create3D = (bool)create3D.IsChecked;
            LINK.deleteWIP = (bool)deleteWIP.IsChecked;
            LINK.removeCad = (bool)removeCad.IsChecked;
            LINK.removeLinks = (bool)removeLinks.IsChecked;
            LINK.populateSections = (bool)populateSections.IsChecked;
            LINK.purgeFilters = (bool)purgeFilters.IsChecked;
            LINK.purgeSets = (bool)purgeSets.IsChecked;
            LINK.resetBrowser = (bool)resetBrowser.IsChecked;
            LINK.exportNwc = (bool)exportNwc.IsChecked;
            LINK.exportIFC = (bool)exportIFC.IsChecked;
            LINK.exportDWFx = (bool)exportDWFx.IsChecked;
            LINK.saveLocal = (bool)saveLocal.IsChecked;


            return LINK;
        }
    }
}
