using System.Text.RegularExpressions;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Win32;
using Revit_Ninja.Views;
using Revit_Ninja.Views.PointCoord;
using RevitNinja.Utils;
using Excel = Microsoft.Office.Interop.Excel;
using UIFramework;
using Autodesk.Windows;
using System.Windows;
using System.Windows.Controls;
using Grid = System.Windows.Controls.Grid;

namespace Revit_Ninja.Commands
{

    [TransactionAttribute(TransactionMode.Manual)]
    internal class onGoing : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc=commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            //Grid rootGrid = VisualUtils.FindVisualParent<Grid>(ComponentManager.Ribbon, "rootGrid");
            //foreach (UIElement child in rootGrid.Children)
            //{
            //    doc.print(child.ToString());
            //}

            return Result.Succeeded;
        }
    }
}
