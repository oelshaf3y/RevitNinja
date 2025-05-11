using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Ninja.Views;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class CopyFilters : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess())  return Result.Failed;

            CopyFiltersView cpf = new CopyFiltersView(uidoc);
            cpf.ShowDialog();
            return Result.Succeeded;
        }
    }
}
