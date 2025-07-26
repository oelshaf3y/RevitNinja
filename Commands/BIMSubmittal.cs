using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;
using Revit_Ninja.Views;
using RevitNinja.Utils;
using Parameter = Autodesk.Revit.DB.Parameter;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class BIMSubmittal : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        View3D new3DView, duplicate;
        List<ElementId> excluded = new List<ElementId>();
        StringBuilder sb = new StringBuilder();
        List<ViewSection> sections = new List<ViewSection>();
        List<View> allViews = new List<View>();
        List<ViewSheet> sheets = new List<ViewSheet>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            if (!doc.getAccess()) return Result.Failed;

            BIMSubmittalView window = new BIMSubmittalView();
            window.ShowDialog();

            sections = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSection))
                .Cast<ViewSection>()
                .Where(v => !v.IsTemplate && v.ViewType == ViewType.Section)
                .ToList();
            allViews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Cast<View>().ToList();
            sheets = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType()
                .Cast<ViewSheet>().ToList();

            using (TransactionGroup TG = new TransactionGroup(doc, "BIM Submittal"))
            {
                TG.Start();
                if (window.DialogResult == false) return Result.Cancelled;
                if (window.CreateViewsCB.IsChecked == true) try { Create3DViews(); } catch { }
                if (window.DeleteNOSCB.IsChecked == true) try { NOS(); } catch { sb.AppendLine("Failed to delete unused views!"); }
                if (window.DelCADCB.IsChecked == true) try { delCad(); } catch { sb.AppendLine("Failed to remove CAD drawings!"); }
                if (window.RemoveLinksCB.IsChecked == true) try { removeLinks(); } catch { sb.AppendLine("Failed to remove links!"); }
                if (window.SectionsAndSheetsCB.IsChecked == true) try { populateSections(commandData); } catch { sb.AppendLine("Failed to populate sections!"); }
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
            foreach (View view in views)
            {
                if (view.Name.ToLower().Contains("acc") || view.Name.ToLower().Contains("revizto"))
                {
                    //view.Name = "ACC/Revizto View";
                    excluded.Add(view.Id);
                }
                else if (view.Name.ToLower().Contains("assemble"))
                {
                    //view.Name = "Assemble View";
                    excluded.Add(view.Id);
                }
            }
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


        public void populateSections(ExternalCommandData commandData)
        {

            string seactionHeadPath = doc.ExtractEmbeddedResource("RevitNinja_Section_Head.rfa");
            string calloutPath = doc.ExtractEmbeddedResource("RevitNinja_CallOut_Head.rfa");


            #region assign parameter to sections
            if (sections.First().LookupParameter("TRSDC_Sheet Number") is null)
            {
                try
                {

                    Ninja.assignParameter(commandData, "Ninja-Views", "TRSDC_Sheet Number", BuiltInCategory.OST_Views, SpecTypeId.String.Text);
                }
                catch (Exception ex)
                {
                    doc.print("Failed to create TRSDC_Sheet Number parameter.\n" + ex.Message);
                }
            }
            #endregion

            #region assign parameters to sheets
            try
            {
                if (sheets.First().LookupParameter("TRSDC_Sheet Number") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "TRSDC_Sheet Number", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);

                if (sheets.First().LookupParameter("TRSDC_Volume/System") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "TRSDC_Volume/System", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);

                if (sheets.First().LookupParameter("TRSDC_Building level") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "TRSDC_Building level", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);

                if (sheets.First().LookupParameter("Sheet Title Line 1") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "Sheet Title Line 1", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);

                if (sheets.First().LookupParameter("Sheet Title Line 2") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "Sheet Title Line 2", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);

                if (sheets.First().LookupParameter("Sheet Title Line 3") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "Sheet Title Line 3", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);

                if (sheets.First().LookupParameter("Sheet Title Line 4") is null)
                    Ninja.assignParameter(commandData, "Ninja-Sheets", "Sheet Title Line 4", BuiltInCategory.OST_Sheets, SpecTypeId.String.Text);
            }
            catch (Exception ex) { }
            #endregion

            #region get/set sheet meta data
            using (Transaction tr = new Transaction(doc, "renumbering sheets"))
            {
                tr.Start();

                ProjectInfo information = doc.ProjectInformation;
                sheets.ForEach(sheet =>
                {
                    StringBuilder sbuilder = new StringBuilder();
                    string oldNumber = "";
                    if (!(sheet.SheetNumber.ToLower().Trim() == "model issue screen" || sheet.Name.ToLower().Trim() == "model issue screen"))
                    {
                        try
                        {
                            if (sheet.SheetNumber.Split('-').Length > 2) oldNumber = sheet.SheetNumber.Split('-').Last();
                            else oldNumber = sheet.SheetNumber;
                            sbuilder.Append(information.LookupParameter("TRSDC_Program Code").AsString() + "-");
                            sbuilder.Append(information.LookupParameter("TRSDC_Project Code").AsString());
                            sbuilder.Append(information.LookupParameter("TRSDC_Contract Code").AsString() + "-");
                            sbuilder.Append(information.LookupParameter("TRSDC_Originator Code").AsString() + "-");
                            sbuilder.Append(sheet.LookupParameter("TRSDC_Volume/System").AsString() + "-");

                            string BL = sheet.LookupParameter("TRSDC_Building level").AsString();
                            if (BL.Trim().Length > 0)
                                sbuilder.Append(BL + "-");
                            else if (sheet.LookupParameter("TRSDC_Building Level").AsString().Trim().Length > 0)
                                sbuilder.Append(sheet.LookupParameter("TRSDC_Building Level").AsString() + "-");
                            else
                                sbuilder.Append(information.LookupParameter("TRSDC_Model Level").AsString() + "-");

                            sbuilder.Append(information.LookupParameter("TRSDC_Document Type").AsString() + "-");
                            if (sheet.SheetNumber.Split('-').Length >= 2) sbuilder.Append(sheet.SheetNumber.Split('-').ElementAt(sheet.SheetNumber.Split('-').Length - 2) + "-");
                            else sbuilder.Append(information.LookupParameter("TRSDC_Discipline").AsString() + "-");
                            sbuilder.Append(sheet.LookupParameter("TRSDC_Sheet Number").AsString());

                            sheet.LookupParameter("TRSDC_Sheet Number").Set(oldNumber);
                            if (sheet.SheetNumber != sbuilder.ToString()) sheet.SheetNumber = sbuilder.ToString();
                            if (sbuilder.Length > 0) sbuilder.Clear();

                            if (sheet.LookupParameter("Sheet Title Line 1").AsString() != null && sheet.LookupParameter("Sheet Title Line 1").AsString().Trim().Length > 0) sbuilder.Append(sheet.LookupParameter("Sheet Title Line 1").AsString());
                            if (sheet.LookupParameter("Sheet Title Line 2").AsString() != null && sheet.LookupParameter("Sheet Title Line 2").AsString().Trim().Length > 0) sbuilder.Append("-" + sheet.LookupParameter("Sheet Title Line 2").AsString());
                            if (sheet.LookupParameter("Sheet Title Line 3").AsString() != null && sheet.LookupParameter("Sheet Title Line 3").AsString().Trim().Length > 0) sbuilder.Append("-" + sheet.LookupParameter("Sheet Title Line 3").AsString());
                            if (sheet.LookupParameter("Sheet Title Line 4").AsString() != null && sheet.LookupParameter("Sheet Title Line 4").AsString().Trim().Length > 0) sbuilder.Append("-" + sheet.LookupParameter("Sheet Title Line 4").AsString());


                            sheet.Name = sbuilder.ToString();
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine(sheet.SheetNumber + "\n" + ex.Message + "\n" + sbuilder.ToString());
                        }
                    }
                });

                tr.Commit();
            }
            #endregion

            #region load section head family
            using (Transaction tr = new Transaction(doc, "Load family"))
            {
                tr.Start();
                doc.LoadFamily(seactionHeadPath);
                doc.LoadFamily(calloutPath);

                //get section head family
                FamilySymbol fs = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .FirstOrDefault(x => x.FamilyName == "RevitNinja_Section_Head");
                FamilySymbol fco = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .FirstOrDefault(x => x.FamilyName == "RevitNinja_CallOut_Head");
                foreach (ViewFamilyType sectionType in sections.Select(x => x.GetTypeId()).Distinct().Select(x => doc.GetElement(x)).Cast<ViewFamilyType>())
                {

                    //var sectionType = doc.GetElement(sections.First().GetTypeId()) as ViewFamilyType;
                    try
                    {

                        var sectionTag = doc.GetElement(sectionType.LookupParameter("Section Tag").AsElementId()) as LineAndTextAttrSymbol;
                        sectionTag.LookupParameter("Section Head").Set(fs.Id);
                        ElementType calloutTag = doc.GetElement(sectionType.LookupParameter("Callout Tag").AsElementId()) as ElementType;
                        calloutTag.LookupParameter("Callout Head").Set(fco.Id);
                    }
                    catch (Exception ex){ doc.print(ex.Message); }
                }
                tr.Commit();
            }
            #endregion

            #region remove the parameter from the section templates
            List<View> sectionTemplates = sections.Select(x => x.ViewTemplateId).Distinct().Where(x => x != ElementId.InvalidElementId).Select(x => doc.GetElement(x)).Cast<View>().ToList();
            ElementId paramId = sections.First().LookupParameter("TRSDC_Sheet Number").Id;
            using (Transaction tr = new Transaction(doc, "release parameter from template"))
            {
                tr.Start();
                try
                {
                    foreach (View template in sectionTemplates)
                    {
                        var parametersIds = template.GetNonControlledTemplateParameterIds();
                        if (parametersIds.Contains(paramId)) continue;
                        parametersIds.Add(paramId);
                        template.SetNonControlledTemplateParameterIds(parametersIds);
                    }
                }
                catch (Exception ex)
                {
                    doc.print("Failed to release the parameter from the template.\n" + ex.Message);
                }
                tr.Commit();
            }
            #endregion

            #region modify sheet numbers
            using (TransactionGroup tg = new TransactionGroup(doc, "rectify sheet numbers"))
            {
                tg.Start();

                foreach (ViewSection section in sections)
                {
                    using (Transaction t = new Transaction(doc, "Assign parameter"))
                    {
                        t.Start();
                        string newNumber = "";
                        try
                        {
                            newNumber = section.LookupParameter("Sheet Number").AsString().Split('-').Last();
                            section.LookupParameter("TRSDC_Sheet Number").Set(newNumber);
                            //sb.AppendLine(sectionHead.Name);
                        }
                        catch (Exception ex)
                        {
                            doc.print($"The Parameter TRSDC_Sheet Number is Read-Only for sheet: {section.LookupParameter("Sheet Number").AsString()}.\n" +
                                 $"Please check the parameter and try again.\n" +
                                 $"Edit the section view template to Not Include the parameter TRSDC_Sheet Number.");
                            break;
                        }
                        t.Commit();
                    }
                }
                if (sb.Length > 0)
                    doc.print(sb);
                else doc.print("Mission Accomplished!");
                tg.Assimilate();
            }
            #endregion
        }
    }

}
