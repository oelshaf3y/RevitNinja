using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using RevitNinja.Utils;
using System.Text;
using Revit_Ninja.Views.BIMSubmittal;
using System.Windows;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class OnGoing : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            //StringBuilder sb = new StringBuilder();
            List<Element> sets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheetSet)).ToList();
            List<View> allviews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Cast<View>().Where(x => x.IsTemplate == false && x.ViewType != ViewType.Internal)
                .Where(x => x.GetType() != typeof(ViewSheet) && x.GetType() != typeof(ViewSchedule)
                        && x.ViewType != ViewType.ProjectBrowser && x.ViewType != ViewType.SystemBrowser
                        && x.ViewType != ViewType.DraftingView && x.ViewType != ViewType.Legend)
                .ToList();

            List<viewforset> view3dForSet = new List<viewforset>();
            List<viewforset> view2dForSet = new List<viewforset>();
            List<viewforset> sheetsForSet = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets)
                .WhereElementIsNotElementType()
                .Select(x => new viewforset(x.Name, false, x.Id)).ToList();

            foreach (View view in allviews)
            {
                if (view is View3D)
                    view3dForSet.Add(new viewforset(view.Name, false, view.Id));
                else view2dForSet.Add(new viewforset(view.Name, false, view.Id));
            }

            publishSet setWindow = new publishSet();
            setWindow.list3D.ItemsSource = view3dForSet;
            setWindow.list2D.ItemsSource = view2dForSet;
            setWindow.listSheets.ItemsSource = sheetsForSet;
            setWindow.ShowDialog();
            using (Transaction tr2 = new Transaction(doc, "Create new set"))
            {
                tr2.Start();
                PrintManager pm = doc.PrintManager;
                pm.PrintRange = PrintRange.Select;
                ViewSheetSetting sheetSetting = pm.ViewSheetSetting;
                var set = sheetSetting.CurrentViewSheetSet;
                set.Views = new ViewSet();
                // Add selected 3D views
                foreach (viewforset v in view3dForSet)
                {
                    if (v.IsChecked)
                    {
                        View3D view3D = doc.GetElement(v.Id) as View3D;
                        if (view3D != null)
                        {
                            set.Views.Insert(view3D);
                        }
                    }
                }
                // Add selected 2D views
                foreach (viewforset v in view2dForSet)
                {
                    if (v.IsChecked)
                    {
                        View view2D = doc.GetElement(v.Id) as View;
                        if (view2D != null)
                        {
                            set.Views.Insert(view2D);
                        }
                    }
                }
                // Add selected sheets
                foreach (viewforset v in sheetsForSet)
                {
                    if (v.IsChecked)
                    {
                        ViewSheet sheet = doc.GetElement(v.Id) as ViewSheet;
                        if (sheet != null)
                        {
                            set.Views.Insert(sheet);
                        }
                    }
                }
                sheetSetting.SaveAs(setWindow.setName.Text);
                
                tr2.Commit();
            }



            using (Transaction tr = new Transaction(doc, "Modify sets"))
            {
                tr.Start();

                foreach (var s in sets)
                {
                    try
                    {
                        if (s.Name == setWindow.setName.Text)
                        {
                            continue;
                        }
                        doc.Delete(s.Id);
                    }
                    catch (Exception ex)
                    {
                        doc.print($"Error deleting set {s.Name}: {ex.Message}");
                    }
                }

                tr.Commit();
            }


            //sb.Append($"View set name: {set.Name}\n");
            //foreach (View view in v)
            //{
            //    sb.AppendLine(view.Name);
            //}

            ////sets.ForEach(x => sb.AppendLine(x.Name));

            //doc.print(sb);

            return Result.Succeeded;
        }
    }

    class viewforset
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public ElementId Id { get; set; }
        public viewforset(string name, bool isChecked, ElementId id)
        {
            Name = name;
            IsChecked = isChecked;
            Id = id;
        }
    }
}