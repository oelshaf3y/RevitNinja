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
using Revit_Ninja.Commands;

namespace Revit_Ninja.Views.PointCoord
{
    /// <summary>
    /// Interaction logic for selectPoint.xaml
    /// </summary>
    public partial class selectPoint : Window
    {
        public SelectionType selectionType;
        public OperationType operation;
        public selectPoint()
        {
            InitializeComponent();
        }
        private void plotButClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            operation = OperationType.EditOrPlot;
            Close();
        }

        private void cancelButClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        

        private void exportPointsClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            operation = OperationType.ExportPoints;
            Close();
        }

        private void importPointsClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            //getselectionType();
            operation = OperationType.ImportPoints;
            Close();
        }
    }
    public enum SelectionType
    {
        AllPoints,
        ActiveView,
        SelectedPoints
    }

    public enum OperationType
    {
        EditOrPlot,
        ImportPoints,
        ExportPoints
    }
}
