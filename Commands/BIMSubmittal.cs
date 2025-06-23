using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class BIMSubmittal : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        View3D new3DView, duplicate;
        List<ElementId> excluded = new List<ElementId>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess()) return Result.Failed;

            StringBuilder sb = new StringBuilder();
            using (TransactionGroup TG = new TransactionGroup(doc, "BIM Submittal"))
            {
                TG.Start();
                TaskDialogResult tdr = doc.YesNoMessage("are you sure you want to remove:\n\n     Links,\n     Unused Views\n     and create 2 additional views\n\nthis step can not be UNDONE!!\n©Omar Elshafey | 2025");
                if (tdr == TaskDialogResult.No) return Result.Cancelled;
                try { Create3DViews(); } catch { }
                try
                {

                    NOS();
                }
                catch { sb.AppendLine("Failed to delete unused views!"); }
                if (doc.YesNoMessage("do you want to remove all DWG ?") == TaskDialogResult.Yes)
                {
                    try
                    {

                        delCad();
                    }
                    catch { sb.AppendLine("Failed to remove CAD drawings!"); }
                }
                if (doc.YesNoMessage("do you want to remove all Links ?") == TaskDialogResult.Yes)
                {
                    try
                    {

                        removeLinks();
                    }
                    catch { sb.AppendLine("Failed to remove links!"); }
                }
                TG.Assimilate();
            }
            if (sb.Length > 0) doc.print(sb.ToString());

            return Result.Succeeded;
        }

        private bool Create3DViews()
        {
            #region create 3D views

            using (Transaction tx = new Transaction(doc, "Create 3D View and Hide Categories"))
            {
                tx.Start();

                #region if the views exist
                List<View> views = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Views)
                    .WhereElementIsViewIndependent()
                    .Cast<View>().Where(x => x is View3D)
                    .ToList();
                foreach (View view in views)
                {
                    if (view.Name.ToLower().Contains("acc") || view.Name.ToLower().Contains("revizto"))
                    {
                        view.Name = "ACC/Revizto View";
                        excluded.Add(view.Id);
                    }
                    else if (view.Name.ToLower().Contains("assemble"))
                    {
                        view.Name = "Assemble View";
                        excluded.Add(view.Id);
                    }
                }
                if (views.Any(x => x.Name == "ACC/Revizto View") && views.Any(x => x.Name == "Assemble View"))
                {
                    tx.Commit();
                    return true;
                }
                #endregion

                #region create new views
                // Get the default 3D View type
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

                if (viewFamilyType == null)
                {
                    TaskDialog.Show("Error", "No 3D view family type found.");
                    return false;
                }

                // Create the 3D view
                new3DView = View3D.CreateIsometric(doc, viewFamilyType.Id);
                try
                {

                    new3DView.Name = "ACC/Revizto View";
                }
                catch (Exception ex)
                {
                    doc.print("something went wrong, or maybe the views exists.");
                    tx.RollBack();
                    return false;
                }

                tx.Commit();
            }

            using (Transaction tr = new Transaction(doc, "Hide"))
            {
                tr.Start();

                Categories categories = doc.Settings.Categories;
                foreach (Category category in categories)
                {
                    try
                    {

                        if (category != null && category.CategoryType == CategoryType.Annotation)
                        {
                            if (new3DView.GetCategoryHidden(category.Id) == false)
                            {
                                new3DView.SetCategoryHidden(category.Id, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                ViewDuplicateOption option = new ViewDuplicateOption();
                duplicate = doc.GetElement(new3DView.Duplicate(option)) as View3D;
                duplicate.Name = "Assemble View";
                tr.Commit();
            }
            uidoc.ActiveView = new3DView;
            excluded.Add(new3DView.Id);
            excluded.Add(duplicate.Id);
            doc.print("Additional views have been created successfully!");
            return true;
            #endregion
            #endregion
        }

        public bool NOS()
        {
            int count = 0;
            List<View> views = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsViewIndependent()
                .Where(v => v.LookupParameter("View Template") != null)
                .Cast<View>().Distinct().ToList();
            Parameter sheetNumber;
            using (TransactionGroup tg = new TransactionGroup(doc, "Delete unused views"))
            {

                tg.Start();
                foreach (View view in views)
                {
                    if (view == null) continue;
                    if (view.GetType() != typeof(ViewSheet) && view.GetType() != typeof(ViewSchedule)
                        && view.ViewType != ViewType.ProjectBrowser && view.ViewType != ViewType.SystemBrowser
                        && view.ViewType != ViewType.DraftingView && view.ViewType != ViewType.Legend
                        )
                    {
                        sheetNumber = view.LookupParameter("Sheet Name");
                        if (sheetNumber == null || sheetNumber.AsValueString() == "---")
                        {
                            if (view.LookupParameter("Dependency") != null && view.LookupParameter("Dependency").AsString() != "Primary")
                            {
                                using (Transaction tx = new Transaction(doc, "Delete sheet"))
                                {

                                    tx.Start();
                                    if (!excluded.Contains(view.Id))
                                    {
                                        try
                                        {
                                            if (doc.GetElement(view.Id) != null)
                                            {

                                                doc.Delete(view.Id);
                                                count++;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            doc.print(ex.StackTrace);
                                        }

                                    }
                                    tx.Commit();
                                    tx.Dispose();
                                }
                            }
                        }
                    }
                }
                tg.Assimilate();
            }
            if (count > 0)
            {

                doc.print("Total of: " + count + " views have been deleted.");
            }
            else
            {
                doc.print("No views to delete.");
            }
            return true;
        }

        public bool delCad()
        {

            List<CADLinkType> DWGs = new FilteredElementCollector(doc).OfClass(typeof(CADLinkType)).Cast<CADLinkType>().ToList();
            int count = 0;
            if (DWGs.Count() == 0) { TaskDialog.Show("Info", "No more DWG Imports In The Project."); return true; }
            else
            {
                TaskDialogResult dia = doc.YesNoMessage($"Are You Sure You Want To Delete {DWGs.Count()} CAD Files?\nThis CAN NOT BE UNDONE!");
                if (dia == TaskDialogResult.No) return false;
            }
            using (TransactionGroup tg = new TransactionGroup(doc, "Delete Cads"))
            {
                tg.Start();
                foreach (CADLinkType cad in DWGs)
                {
                    try
                    {
                        if (cad.Pinned)
                        {
                            using (Transaction tr = new Transaction(doc, "Unpin"))
                            {
                                tr.Start();
                                cad.Pinned = false;
                                tr.Commit();
                            }
                        }
                        using (Transaction tt = new Transaction(doc, "Remove CAD"))
                        {
                            tt.Start();
                            try
                            {

                                doc.Delete(cad.Id);
                                count++;
                            }
                            catch (Exception ex)
                            {
                                //doc.print(ex.ToString());
                            }
                            tt.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        //doc.print(ex.ToString());
                    }
                }
                tg.Assimilate();
            }
            //doc.Delete(fec.Select(x => x.Id).ToArray());
            TaskDialog.Show("Done", $"Successfully deleted {count} CAD Files.");
            return true;
        }


        public bool removeLinks()
        {

            #region remove links

            List<ElementId> rlis = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkType)).ToElementIds().ToList();
            //doc.print(doc.GetElement(rlis.First()).Name);
            int count = 0;
            using (Transaction tr = new Transaction(doc, "Remove links"))
            {
                tr.Start();
                foreach (ElementId r in rlis)
                {
                    try
                    {
                        doc.Delete(r);
                        count++;

                    }
                    catch (Exception ex)
                    {
                        //doc.print(ex.ToString());
                    }
                }
                tr.Commit();
                tr.Dispose();
            }
            doc.print($"Total of {count} linked models have been removed");
            #endregion
            return true;
        }
    }


}
