using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System.Text.Json;

namespace RevitNinja.Commands.ViewState
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class SaveState : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            //if (!doc.getAccess())  return Result.Failed; 

            //DataStorage storage =;
            View activeView = doc.ActiveView;
            StringBuilder sb = new StringBuilder();
            List<Element> onlyVisible = new List<Element>();
            if (activeView is ViewSheet || activeView is ViewSchedule)
            {
                doc.print("Active view must be a view not a sheet or a schedule");
                return Result.Failed;
            }

            #region Using Parameter
            //Parameter state = activeView.LookupParameter("View State");
            //if (state == null)
            //{
            //    if (!
            //        Ninja.assignParameter(
            //            commandData,
            //            "Ninja-Views",
            //            "View State",
            //            BuiltInCategory.OST_Views,
            //            SpecTypeId.String.Text)
            //        )
            //    {
            //        doc.print("Please Create a new Project Parameter (Instance Parameter) for views\nParameter Name: View State\nType: Text");
            //        return Result.Failed;
            //    }
            //    else
            //    {
            //        state = activeView.LookupParameter("View State");
            //        if (state == null)
            //        {
            //            doc.print("Please Create a new Project Parameter (Instance Parameter) for views\nParameter Name: View State\nType: Text");
            //        }
            //    }
            //}
            //if (state.IsReadOnly)
            //{
            //    doc.print("make sure the View State Parameter is editable!");
            //    return Result.Failed;
            //}
            //if (state.AsString() != null)
            //{
            //    if (state.AsString().Trim().Length > 0)
            //    {
            //        if (doc.YesNoMessage("View State is saved already!.\nAre you sure you want to update?") == TaskDialogResult.No)
            //        {
            //            doc.print("Canceled");
            //            return Result.Cancelled;
            //        }
            //    }
            //}
            #endregion

            FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id).WhereElementIsNotElementType();
            onlyVisible = collector.Where(x => x.CanBeHidden(activeView)).ToList();
            collector.Where(x => x.IsHidden(activeView)).ToList().ForEach(x => onlyVisible.Remove(x));
            List<string> ids = onlyVisible.Select(x => x.Id.ToString()).ToList();

            #region Using DataStorage
            string storedData = doc.getDataStorage().getStoredData();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (storedData != null)
            {
                dict = JsonSerializer.Deserialize<Dictionary<string, string>>(storedData);
            }
            foreach (ElementId id in onlyVisible.Select(x => x.Id))
            {
                sb.Append(id.ToString() + ",");
            }
            dict.Add("View State", sb.ToString());
            #endregion
            //uidoc.Selection.SetElementIds(onlyVisible.Select(x => x.Id).ToList());
            using (TransactionGroup tg = new TransactionGroup(doc, "Save View State"))
            {
                tg.Start();
                //state.Set(sb.ToString());
                doc.setDataStorage(JsonSerializer.Serialize(dict));
                doc.print(Ninja.dataStorage.Name);
                tg.Assimilate();
                tg.Dispose();
            }
            return Result.Succeeded;
        }
    }

}
