using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.IO;
using System.Windows.Forms;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class OnGoing : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        UIApplication uiapp;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            
            return Result.Succeeded;
        }
    }
}