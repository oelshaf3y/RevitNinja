using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class selectColored : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            //if (!doc.getAccess())  return Result.Failed; 

            string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.txt");
            List<string> ids = new List<string>();
            try
            {
                ids = File.ReadAllLines(filepath).ToList();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "No File Found");
                return Result.Failed;
            }
            uidoc.Selection.SetElementIds(ids.Select(x => new ElementId(int.Parse(x))).ToList());
            return Result.Succeeded;
        }
    }
}
