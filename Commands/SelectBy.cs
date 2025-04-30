using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using RevitNinja.Views;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class SelectBy : IExternalCommand
    {
        public Document doc { get; set; }
        public UIDocument uidoc { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<ElementId> ids = new List<ElementId>();
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess())
            {
                return Result.Failed;
            }
            FilteredElementCollector AllElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
            //FilteredElementCollector allElements = new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType();
            var theme = UIFramework.ApplicationTheme.CurrentTheme;

            SelectByView selectBy = new SelectByView(theme, uidoc);
            
            selectBy.but.Click += selectBy.click;
            selectBy.Cancel.Click += selectBy.cancel;
            RibbonController.ShowOptionsBar(selectBy, true);
            return Result.Succeeded;
        }
    }
}
