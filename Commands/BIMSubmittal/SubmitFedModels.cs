using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Revit_Ninja.Views.BIMSubmittal;
using RevitNinja.Utils;
using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace Revit_Ninja.Commands.BIMSubmittal
{
    [Transaction(TransactionMode.Manual)]
    internal class SubmitFedModels : IExternalCommand
    {
        UIDocument uiDOC;
        Document DOC;
        View3D new3DView, duplicate;
        UIApplication uiapp;
        ExternalCommandData CommandData;
        List<ElementId> excluded = new List<ElementId>();
        StringBuilder log = new StringBuilder();
        List<ViewSection> sections = new List<ViewSection>();
        List<View> allViews = new List<View>();
        List<ViewSheet> sheets = new List<ViewSheet>();
        string folderPath = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            CommandData = commandData;
            uiDOC = commandData.Application.ActiveUIDocument;
            DOC = uiDOC.Document;

            if (!DOC.getAccess()) return Result.Failed;

            uiapp = commandData.Application;
            var collector = new FilteredElementCollector(DOC).OfClass(typeof(RevitLinkInstance));
            List<linkforset> linksForSet = new List<linkforset>();
            foreach (RevitLinkInstance instance in collector)
            {
                linksForSet.Add(new linkforset(instance));
            }

            SubmitFedModelView submittalWindow = new SubmitFedModelView(linksForSet);
            submittalWindow.ShowDialog();
            if (submittalWindow.DialogResult == false) return Result.Cancelled;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "NWC,RVT Files|*.nwc,*.rvt|All Files|*.*";
            saveDialog.Title = "Save Excel File";
            saveDialog.FileName = linksForSet.First().Name.Split('.').First();
            if (saveDialog.ShowDialog() == true)
            {
                string filePath = saveDialog.FileName;
                folderPath = Directory.GetParent(filePath).FullName;
            }
            else return Result.Cancelled;
            foreach (linkforset link in submittalWindow.RLIS)
            {
                RevitLinkInstance linkInstance = link.RLI;
                bool anyTrue = link.GetType()
                          .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                          .Where(f => f.PropertyType == typeof(bool))
                          .Select(f => (bool)f.GetValue(link))
                          .Any(v => v);
                DOC.print(anyTrue);
                if (!anyTrue) continue;
                try
                {
                    RevitLinkType revitLinkType = DOC.GetElement(linkInstance.GetTypeId()) as RevitLinkType;
                    Document LINK = linkInstance.GetLinkDocument();
                    if (LINK == null) continue;

                    //ModelPath modelPath = LINK.GetCloudModelPath();
                    var region = ModelPathUtils.CloudRegionUS;
                    ModelPath cmp = LINK.GetCloudModelPath();
                    Guid projectID = cmp.GetProjectGUID();
                    Guid modelGUID = cmp.GetModelGUID();
                    var modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(region, projectID, modelGUID);
                    //revitLinkType.Unload(null);
                    if (modelPath.CloudPath == true)
                    {
                        // Open detached from cloud
                        OpenOptions openOpts = new OpenOptions
                        {
                            Audit = false
                        };

                        if (link.saveLocal) openOpts.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
                        else openOpts.DetachFromCentralOption = DetachFromCentralOption.DoNotDetach;

                        DefaultOpenFromCloudCallback callback = new DefaultOpenFromCloudCallback();
                        UIDocument linkedUIDoc = uiapp.OpenAndActivateDocument(modelPath, openOpts, false, callback);
                        Document linkedDoc = linkedUIDoc.Document;

                        string localPath = System.IO.Path.Combine(folderPath, linkedDoc.Title.Split('_').First() + ".rvt");
                        ModelPath stringModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(localPath);
                        SaveAsOptions saveOpts = new SaveAsOptions { OverwriteExistingFile = true };
                        saveOpts.SetWorksharingOptions(new WorksharingSaveAsOptions
                        {
                            SaveAsCentral = true,

                        });
                        submitBimModel(linkedDoc, link);
                        if (link.saveLocal) linkedDoc.SaveAs(stringModelPath, saveOpts);

                    }
                }
                catch (Exception ex)
                {
                    log.AppendLine(ex.Message);
                }
            }
            //DOC.print($"Finished\n{log.ToString()}");
            return Result.Succeeded;
        }

        private void submitBimModel(Document doc, linkforset link)
        {
            sections = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSection))
                .Cast<ViewSection>()
                .Where(v => !v.IsTemplate && (v.ViewType == ViewType.Section || v.ViewType == ViewType.Detail))
                .ToList();

            allViews = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType().Cast<View>().ToList();
            sheets = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType()
                .Cast<ViewSheet>().ToList();

            ElementId AccViewId = ElementId.InvalidElementId;
            using (TransactionGroup TG = new TransactionGroup(doc, "BIM Submittal"))
            {
                TG.Start();
                if (link.create3D) try { AccViewId = Create3DViews(doc); } catch { log.AppendLine("Failed to create 3D Views"); }
                if (link.deleteWIP) try { NOS(doc); } catch { log.AppendLine("Failed to delete unused views!"); }
                if (link.removeCad) try { delCad(doc); } catch { log.AppendLine("Failed to remove CAD drawings!"); }
                if (link.removeLinks) try { removeLinks(doc); } catch { log.AppendLine("Failed to remove links!"); }
                if (link.populateSections) try { populateSections(doc); } catch (Exception ex) { log.AppendLine("Failed to populate sections!"); }
                if (link.purgeFilters) try { purgeFilters(doc); } catch { log.AppendLine("Failed to purge filters"); }
                if (link.purgeSets) try { purgeSets(doc); } catch { log.AppendLine("Failed to purge/create publish sets"); }
                if (link.resetBrowser) try { purgeBrowser(doc); } catch { log.AppendLine("Failed to reset browser organization"); }
                if (link.exportNwc) try { ExportNavis(doc, AccViewId); } catch (Exception ex) { doc.print(ex.Message); }
                if (link.exportIFC) try { ExportIFC(doc, AccViewId); } catch (Exception ex) { doc.print(ex.Message); }
                if (link.exportDWFx) try { ExportDWFx(doc, AccViewId); } catch (Exception ex) { doc.print(ex.Message); }
                TG.Assimilate();
            }
            //if (sb.Length > 0) doc.print(sb.ToString());
        }

        private void ExportDWFx(Document doc, ElementId accViewId)
        {
            using (Transaction tr = new Transaction(doc, "Export DWFx"))
            {
                tr.Start();
                ElementId AccViewId = accViewId;
                if (AccViewId == null || AccViewId == ElementId.InvalidElementId)
                {
                    List<View> views = new FilteredElementCollector(doc)
                       .OfCategory(BuiltInCategory.OST_Views)
                       .WhereElementIsViewIndependent()
                       .Cast<View>().Where(x => x is View3D)
                       .ToList();

                    foreach (View view in views)
                    {
                        if (view.Name.ToLower().Contains("acc") || view.Name.ToLower().Contains("revizto"))
                        {
                            AccViewId = view.Id;
                            break;
                        }
                    }
                    if (AccViewId == null || AccViewId == ElementId.InvalidElementId)
                    {
                        doc.print("No ACC/Revizto view found. Please create one and try again.");
                        return;
                    }
                }
                ViewSet set = new ViewSet();
                set.Insert(doc.GetElement(AccViewId) as View);
                DWFXExportOptions opts = new DWFXExportOptions()
                {
                    CropBoxVisible = false,
                    ExportingAreas = false,
                    ExportTexture = true,
                    ExportOnlyViewId = AccViewId,
                };
                doc.Export(folderPath, doc.Title.Split('_').First(), set, opts);
                tr.Commit();
            }
        }

        private void ExportIFC(Document doc, ElementId accViewId)
        {
            using (Transaction tr = new Transaction(doc, "Export IFC"))
            {
                tr.Start();
                ElementId AccViewId = accViewId;
                if (AccViewId == null || AccViewId == ElementId.InvalidElementId)
                {
                    List<View> views = new FilteredElementCollector(doc)
                       .OfCategory(BuiltInCategory.OST_Views)
                       .WhereElementIsViewIndependent()
                       .Cast<View>().Where(x => x is View3D)
                       .ToList();

                    foreach (View view in views)
                    {
                        if (view.Name.ToLower().Contains("acc") || view.Name.ToLower().Contains("revizto"))
                        {
                            AccViewId = view.Id;
                            break;
                        }
                    }
                    if (AccViewId == null || AccViewId == ElementId.InvalidElementId)
                    {
                        doc.print("No ACC/Revizto view found. Please create one and try again.");
                        return;
                    }
                }
                IFCExportOptions ifcOpts = new IFCExportOptions()
                {
                    FileVersion = IFCVersion.IFC2x3,
                };
                doc.Export(folderPath, doc.Title.Split('_').First(), ifcOpts);
                tr.Commit();
            }
        }

        private ElementId Create3DViews(Document doc)
        {
            ElementId AccViewId = ElementId.InvalidElementId;
            #region create 3D views
            List<string> viewNames = new List<string>() { "ACC/Revizto View", "Assemble View" };
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
                        view.Name = viewNames[0];
                        excluded.Add(view.Id);
                        AccViewId = view.Id;
                    }
                    else if (view.Name.ToLower().Contains("assemble"))
                    {
                        view.Name = viewNames[1];
                        excluded.Add(view.Id);
                    }
                }
                if (views.Any(x => x.Name == "ACC/Revizto View") && views.Any(x => x.Name == "Assemble View"))
                {
                    tx.Commit();
                    return AccViewId;
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
                    log.AppendLine("Create 3D View Failed: No 3D View Family Type found.");
                    return ElementId.InvalidElementId;
                }

                // Create the 3D view
                new3DView = View3D.CreateIsometric(doc, viewFamilyType.Id);
                try
                {

                    new3DView.Name = "ACC/Revizto View";
                    AccViewId = new3DView.Id;
                }
                catch (Exception ex)
                {
                    log.AppendLine("Create 3D View Failed: " + ex.Message + "\nMaybe the views exist!");
                    tx.RollBack();
                    return ElementId.InvalidElementId;
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

                        if (category != null && category.CategoryType == CategoryType.Model)
                        {
                            if (new3DView.GetCategoryHidden(category.Id) == false)
                            {
                                new3DView.SetCategoryHidden(category.Id, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine("Failed to hide category: " + category.Name + "\n" + ex.Message);
                    }
                }
                ViewDuplicateOption option = new ViewDuplicateOption();
                duplicate = doc.GetElement(new3DView.Duplicate(option)) as View3D;
                duplicate.Name = "Assemble View";
                tr.Commit();
            }
            excluded.Add(new3DView.Id);
            excluded.Add(duplicate.Id);
            //doc.print("Additional views have been created successfully!");
            return AccViewId;
            #endregion
            #endregion
        }

        public bool NOS(Document doc)
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
                                            log.AppendLine("Failed to delete view: " + view.Name + "\n" + ex.Message);
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

                log.AppendLine("Total of: " + count + " views have been deleted.");
            }
            else
            {
                log.AppendLine("No views to delete.");
            }
            return true;
        }

        public bool delCad(Document doc)
        {

            List<CADLinkType> DWGs = new FilteredElementCollector(doc).OfClass(typeof(CADLinkType)).Cast<CADLinkType>().ToList();
            int count = 0;
            if (DWGs.Count() == 0) { log.AppendLine($"No DWG Imports In The Project{doc.Title}"); return true; }

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
                                log.AppendLine("Failed to remove CAD: " + cad.Name + "\n" + ex.Message);
                            }
                            tt.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine("Failed to remove CAD: " + ex.Message);

                    }
                }
                tg.Assimilate();
            }
            //doc.Delete(fec.Select(x => x.Id).ToArray());
            log.AppendLine($"Successfully deleted {count} CAD Files.");
            return true;
        }


        public bool removeLinks(Document doc)
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
                        log.AppendLine("Failed to remove links: ]n" + ex.ToString());
                    }
                }
                tr.Commit();
                tr.Dispose();
            }
            log.AppendLine($"Total of {count} linked models have been removed");
            #endregion
            return true;
        }


        public void populateSections(Document doc)
        {
            ExternalCommandData commandData = CommandData;
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
                    log.AppendLine("Failed to create TRSDC_Sheet Number parameter.\n" + ex.Message);
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
            catch (Exception ex) { log.AppendLine("Failed to populate sections: " + ex.Message); }
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
                            try
                            {

                                if (sheet.SheetNumber.Split('-').Length > 2) oldNumber = sheet.SheetNumber.Split('-').Last();
                                else oldNumber = sheet.SheetNumber;

                                if (information.LookupParameter("TRSDC_Program Code") != null)
                                {
                                    if (information.LookupParameter("TRSDC_Program Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(information.LookupParameter("TRSDC_Program Code").AsString() + "-");
                                }
                                else if (sheet.LookupParameter("TRSDC_Program Code") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Program Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Program Code").AsString() + "-");
                                }
                                else doc.print("TRSDC_Program Code");

                                if (information.LookupParameter("TRSDC_Project Code") != null)
                                {
                                    if (information.LookupParameter("TRSDC_Project Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(information.LookupParameter("TRSDC_Project Code").AsString());
                                }
                                else if (sheet.LookupParameter("TRSDC_Project Code") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Project Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Project Code").AsString());
                                }
                                else doc.print("TRSDC_Project Code");

                                if (information.LookupParameter("TRSDC_Contract Code") != null)
                                {
                                    if (information.LookupParameter("TRSDC_Contract Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(information.LookupParameter("TRSDC_Contract Code").AsString() + "-");
                                }
                                else if (sheet.LookupParameter("TRSDC_Contract Code") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Contract Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Contract Code").AsString() + "-");
                                }
                                else doc.print("TRSDC_Contract Code");

                                if (information.LookupParameter("TRSDC_Originator Code") != null)
                                {
                                    if (information.LookupParameter("TRSDC_Originator Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(information.LookupParameter("TRSDC_Originator Code").AsString() + "-");
                                }
                                else if (sheet.LookupParameter("TRSDC_Originator Code") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Originator Code").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Originator Code").AsString() + "-");
                                }
                                else doc.print("TRSDC_Originator Code");

                                if (sheet.LookupParameter("TRSDC_Volume/System") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Volume/System").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Volume/System").AsString() + "-");
                                }
                                else if (information.LookupParameter("TRSDC_Volume/System") != null)
                                {
                                    if (information.LookupParameter("TRSDC_Volume/System").AsString().Trim().Length > 0)
                                        sbuilder.Append(information.LookupParameter("TRSDC_Volume/System").AsString() + "-");
                                }
                                else doc.print("TRSDC_Volume/System");

                                string BL = sheet.LookupParameter("TRSDC_Building level").AsString();
                                if (BL.Trim().Length > 0)
                                { sbuilder.Append(BL + "-"); }
                                else if (sheet.LookupParameter("TRSDC_Building Level").AsString().Trim().Length > 0)
                                { sbuilder.Append(sheet.LookupParameter("TRSDC_Building Level").AsString() + "-"); }
                                else
                                    sbuilder.Append(information.LookupParameter("TRSDC_Model Level").AsString() + "-");
                                if (sheet.LookupParameter("TRSDC_Document Type") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Document Type").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Document Type").AsString() + "-");
                                }
                                else if (information.LookupParameter("TRSDC_Document Type") != null)
                                {
                                    if (information.LookupParameter("TRSDC_Document Type").AsString().Trim().Length > 0)
                                        sbuilder.Append(information.LookupParameter("TRSDC_Document Type").AsString() + "-");
                                }
                                if (sheet.SheetNumber.Split('-').Length >= 2)
                                {
                                    sbuilder.Append(sheet.SheetNumber.Split('-').ElementAt(sheet.SheetNumber.Split('-').Length - 2) + "-");
                                }
                                else if (sheet.LookupParameter("TRSDC_Discipline") != null)
                                {
                                    if (sheet.LookupParameter("TRSDC_Discipline").AsString().Trim().Length > 0)
                                        sbuilder.Append(sheet.LookupParameter("TRSDC_Discipline").AsString() + "-");
                                }
                                else sbuilder.Append(information.LookupParameter("TRSDC_Discipline").AsString() + "-");

                                sbuilder.Append(sheet.LookupParameter("TRSDC_Sheet Number").AsString());

                                sheet.LookupParameter("TRSDC_Sheet Number").Set(oldNumber);
                                if (sheet.SheetNumber != sbuilder.ToString()) sheet.SheetNumber = sbuilder.ToString();
                                if (sbuilder.Length > 0) sbuilder.Clear();
                            }
                            catch (Exception ex)
                            {
                                log.AppendLine(sheet.SheetNumber + "\n some parameters do not exist or empty!");

                            }
                            try
                            {

                                if (sheet.LookupParameter("Sheet Title Line 1") != null)
                                    if (sheet.LookupParameter("Sheet Title Line 1").AsString() != null && sheet.LookupParameter("Sheet Title Line 1").AsString().Trim().Length > 0) sbuilder.Append(sheet.LookupParameter("Sheet Title Line 1").AsString());
                                if (sheet.LookupParameter("Sheet Title Line 2") != null)
                                    if (sheet.LookupParameter("Sheet Title Line 2").AsString() != null && sheet.LookupParameter("Sheet Title Line 2").AsString().Trim().Length > 0) sbuilder.Append("-" + sheet.LookupParameter("Sheet Title Line 2").AsString());
                                if (sheet.LookupParameter("Sheet Title Line 3") != null)
                                    if (sheet.LookupParameter("Sheet Title Line 3").AsString() != null && sheet.LookupParameter("Sheet Title Line 3").AsString().Trim().Length > 0) sbuilder.Append("-" + sheet.LookupParameter("Sheet Title Line 3").AsString());
                                if (sheet.LookupParameter("Sheet Title Line 4") != null)
                                    if (sheet.LookupParameter("Sheet Title Line 4").AsString() != null && sheet.LookupParameter("Sheet Title Line 4").AsString().Trim().Length > 0) sbuilder.Append("-" + sheet.LookupParameter("Sheet Title Line 4").AsString());
                            }
                            catch
                            {
                                log.AppendLine(sheet.SheetNumber + "\n some sheet title parameters do not exist or empty!");
                            }

                            sheet.Name = sbuilder.ToString();
                        }
                        catch (Exception ex)
                        {
                            log.AppendLine(sheet.SheetNumber + "\n" + ex.Message + "\n" + sbuilder.ToString());
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
                    catch (Exception ex) { doc.print(ex.Message); }
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
                if (log.Length > 0)
                    doc.print(log);
                //else doc.print("Mission Accomplished!");
                tg.Assimilate();
            }
            #endregion
        }

        public void purgeFilters(Document doc)
        {
            List<ElementId> ids = new List<ElementId>();
            List<View> views = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).Cast<View>().Where(x => !x.IsTemplate).ToList();

            List<ElementId> allFilters = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).ToElementIds().ToList();
            //doc.print(allFilters.Count + " filters found in the document.");
            foreach (View view in views)
            {
                if (view.GetFilters().Count > 0)
                {
                    try
                    {
                        view.GetFilters().ToList().ForEach(x =>
                        {
                            if (!ids.Contains(x) && view.GetIsFilterEnabled(x))
                                ids.Add(x);
                        });
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"Failed to get filters for view {view.Name} - {ex.Message}");
                    }
                }
            }
            allFilters = allFilters.Where(x => !ids.Contains(x)).ToList();
            int count = 0;
            using (Transaction tr = new Transaction(doc, "Remove Filters"))
            {
                tr.Start();
                foreach (ElementId id in allFilters)
                {
                    try
                    {

                        if (doc.GetElement(id).GetType() == typeof(ParameterFilterElement))
                        {
                            doc.Delete(id);
                            count++;
                        }
                    }
                    catch
                    {
                        log.AppendLine("Failed to delete filter with id: " + id.ToString() + ". It might be used in a view.");
                    }
                }
                tr.Commit();
            }
            log.AppendLine("Removed " + count + " filters.");
        }

        public void purgeSets(Document doc)
        {
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
            setWindow.setName.Text = "3D Views";
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
                        log.AppendLine($"Error deleting set {s.Name}: {ex.Message}");
                    }
                }

                tr.Commit();
            }

        }

        public void purgeBrowser(Document doc)
        {
            var browser = new FilteredElementCollector(doc).OfClass(typeof(BrowserOrganization)).ToList();
            using (Transaction tr = new Transaction(doc, "Purge view organization"))
            {
                tr.Start();
                foreach (var elem in browser)
                {
                    try
                    {

                        if (elem.Name == "all") continue;
                        doc.Delete(elem.Id);
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine("Error resetting project browser organization: " + ex.Message);
                    }
                }
                tr.Commit();
            }

        }
        public void ExportNavis(Document doc, ElementId accViewId = null)
        {
            ElementId AccViewId = accViewId;
            if (AccViewId == null || AccViewId == ElementId.InvalidElementId)
            {
                List<View> views = new FilteredElementCollector(doc)
                   .OfCategory(BuiltInCategory.OST_Views)
                   .WhereElementIsViewIndependent()
                   .Cast<View>().Where(x => x is View3D)
                   .ToList();

                foreach (View view in views)
                {
                    if (view.Name.ToLower().Contains("acc") || view.Name.ToLower().Contains("revizto"))
                    {
                        AccViewId = view.Id;
                        break;
                    }
                }
                if (AccViewId == null || AccViewId == ElementId.InvalidElementId)
                {
                    doc.print("No ACC/Revizto view found. Please create one and try again.");
                    return;
                }
            }
            NavisworksExportOptions navisOpts = new NavisworksExportOptions()
            {
                ConvertLinkedCADFormats = true,
                ExportElementIds = true,
                Coordinates = NavisworksCoordinates.Shared,
                Parameters = NavisworksParameters.All,
                ExportScope = NavisworksExportScope.View,
                ViewId = AccViewId
            };
            doc.Export(folderPath, doc.Title.Split('_').First(), navisOpts);
        }

    }
}
