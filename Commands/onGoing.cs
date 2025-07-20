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

            return Result.Succeeded;
        }
    }
}
