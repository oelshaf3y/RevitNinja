using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Revit_Ninja.Views.PointCoord;
using RevitNinja.Utils;
using Autodesk.Revit.Attributes;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;

namespace Revit_Ninja.Commands.PointsCoord
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class OperateOnPoints : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        List<PointLocation> points = new List<PointLocation>();
        List<FamilyInstance> allPoints = new List<FamilyInstance>();
        List<Element> selectedPoints = new List<Element>();
        SelectionType selectionType;
        OperationType operation;
        ProjectLocation projectLocation;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (doc.ActiveView is View3D || doc.ActiveView.ViewType == ViewType.Schedule)
            {
                doc.print("Please select a 2D view to import points.");
                return Result.Cancelled;
            }

            projectLocation = doc.ActiveProjectLocation;

            allPoints = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilyInstance))
                 .WhereElementIsNotElementType()
                 .Cast<FamilyInstance>()
                 .Where(x => x.Symbol.Family.Name == "RevitNinja_Point")
                 .ToList();



            selectPoint GetPointsView = new selectPoint();
            GetPointsView.ShowDialog();
            if (GetPointsView.DialogResult == false) return Result.Cancelled;
            operation = GetPointsView.operation;
            points = getPoints(selectionType);
            switch (operation)
            {
                case OperationType.EditOrPlot:
                    {
                        if (!EditOrPlot()) return Result.Cancelled;
                        break;
                    }
                case OperationType.ImportPoints:
                    {
                        if (!ImportPoints()) return Result.Cancelled;
                        break;
                    }
                case OperationType.ExportPoints:
                    {
                        if (!ExportPoints()) return Result.Cancelled;
                        break;
                    }

            }
            return Result.Succeeded;
        }

        private bool ExportPoints()
        {
            try
            {
                // Start Excel application
                Excel.Application excelApp = new Excel.Application();
                excelApp.Visible = false; // Set true if you want to see Excel window

                // Create a new workbook
                Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

                // Get first worksheet
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Cordinates";
                Excel.Range titleRange = worksheet.Range["A1", "C1"];
                titleRange.Merge();

                // Set the title text
                titleRange.Value = "Coordinates";
                worksheet.Cells[2, 1] = "Label";
                worksheet.Cells[2, 2] = "Easting";
                worksheet.Cells[2, 3] = "Northing";

                int row = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    row = i + 3;
                    PointLocation point = points[i];
                    if (string.IsNullOrEmpty(point.Easting) || string.IsNullOrEmpty(point.Northing))
                    {
                        doc.print($"Point {point.ID} has no coordinates, skipping.");
                        continue;
                    }
                    worksheet.Cells[row, 1] = point.Prefix + point.ID.ToString() + point.Suffix;
                    worksheet.Cells[row, 2] = point.Easting;
                    worksheet.Cells[row, 3] = point.Northing;

                }


                // Auto-fit columns
                worksheet.Columns.AutoFit();
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files|*.xlsx|All Files|*.*";
                saveDialog.Title = "Save Excel File";
                saveDialog.FileName = "Coordinates.xlsx";
                if (saveDialog.ShowDialog() == true)
                {
                    string filePath = saveDialog.FileName;
                    workbook.SaveAs(filePath);
                    doc.print($"File saved @ {filePath}");

                }


                // Cleanup
                workbook.Close();
                excelApp.Quit();
                return true;
            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
                return false;
            }
        }

        private bool ImportPoints()
        {

            try
            {
                // Show OpenFileDialog
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*";
                openDialog.Title = "Open Excel File";
                List<PointLocation> imported = new List<PointLocation>();
                if (openDialog.ShowDialog() == true)
                {
                    string filePath = openDialog.FileName;

                    // Start Excel application
                    Excel.Application excelApp = new Excel.Application();
                    Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
                    Excel.Worksheet worksheet = workbook.Sheets[1]; // First sheet
                    Excel.Range usedRange = worksheet.UsedRange;

                    int rowCount = usedRange.Rows.Count;
                    int colCount = usedRange.Columns.Count;

                    // Read data from cells
                    for (int i = 3; i <= rowCount; i++)
                    {
                        var pointLabel = (usedRange.Cells[i, 1] as Excel.Range)?.Value2;
                        var pointEasting = (usedRange.Cells[i, 2] as Excel.Range)?.Value2;
                        var pointNorthing = (usedRange.Cells[i, 3] as Excel.Range)?.Value2;
                        string[] label = Regex.Split((pointLabel ?? "").ToString(), @"(?<=\D)(?=\d)|(?<=\d)(?=\D)");

                        int ID = 0;
                        string prefix = "", suffix = "";
                        if (label.Count() > 1)
                        {
                            prefix = label.First();
                            ID = int.Parse(label.ElementAt(1));
                            if (label.Count() > 2)
                                suffix = label.ElementAt(2) + "-imported";
                            else suffix = "-imported";
                        }
                        else
                        {
                            prefix = "";
                            suffix = "-imported";
                            ID = int.Parse(label.First());
                        }
                        string easting = pointEasting?.ToString() ?? "";
                        string northing = pointNorthing?.ToString() ?? "";

                        imported.Add(new PointLocation(ID, prefix, suffix, easting, northing, null));
                    }

                    // Cleanup
                    workbook.Close(false);
                    excelApp.Quit();
                }
                return createImportedPoints(imported);
            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
                return false;
            }
            return true;
        }
        private XYZ SharedToLocal(double x, double y, double z)
        {
            //x in mm, y in mm, z in feet
            XYZ sharedPoint = new XYZ(x.mmToFeet(), y.mmToFeet(), z.mmToFeet());
            ProjectPosition pos = projectLocation.GetProjectPosition(XYZ.Zero);
            XYZ projectOrigin = new XYZ(pos.EastWest, pos.NorthSouth, pos.Elevation);
            double rotation = pos.Angle;
            double rad = -rotation * (Math.PI / 180.0);
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            XYZ translated = sharedPoint - projectOrigin;
            double X = translated.X * cos - translated.Y * sin;
            double Y = translated.X * sin + translated.Y * cos;
            return new XYZ(X, Y, translated.Z);
        }

        private bool createImportedPoints(List<PointLocation> imported)
        {
            #region get existing symbol
            FamilySymbol symbol = null;

            try
            {

                symbol = new FilteredElementCollector(doc)
                     .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                     .Cast<FamilySymbol>()
                     .Where(x => x.Family.Name == "RevitNinja_Point").FirstOrDefault();
            }
            catch (Exception ex) { doc.print(ex.Message); }
            #endregion

            #region load symbol if not found
            if (symbol == null)
            {
                doc.print("Select a point on plan");
                using (Transaction tr = new Transaction(doc, "Load Family"))
                {
                    tr.Start();
                    string path = doc.ExtractEmbeddedResource("RevitNinja_Point.rfa");
                    doc.LoadFamily(path, out Family fam);
                    tr.Commit();
                }
                try
                {
                    symbol = new FilteredElementCollector(doc)
                         .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                         .Cast<FamilySymbol>()
                         .Where(x => x.Family.Name == "RevitNinja_Point").FirstOrDefault();
                }
                catch (Exception ex) { }
            }
            #endregion


            using (TransactionGroup tg = new TransactionGroup(doc, "Create Imported Points"))
            {
                tg.Start();
                XYZ p = XYZ.Zero;
                try
                {
                    p = uidoc.Selection.PickPoint("Select a point or press ESC to finish");
                }
                catch { return false; }

                foreach (PointLocation point in imported)
                {
                    using (Transaction tr = new Transaction(doc, "Create Point"))
                    {
                        tr.Start();
                        try
                        {
                            XYZ location = XYZ.Zero;
                            if (double.TryParse(point.Easting, out double Easting) && double.TryParse(point.Northing, out double Northing))
                            {
                                location = SharedToLocal(Easting, Northing, p.Z);
                            }
                            else
                            {
                                continue;
                            }
                            FamilyInstance newPoint = doc.Create.NewFamilyInstance(location, symbol, doc.ActiveView);
                            ((Element)newPoint).LookupParameter("Easting").Set(point.Easting);
                            ((Element)newPoint).LookupParameter("Northing").Set(point.Northing);
                            ((Element)newPoint).LookupParameter("PointLabel").Set(point.Prefix + point.ID + point.Suffix);
                            ((Element)newPoint).LookupParameter("LEFT").Set(0);
                            ((Element)newPoint).LookupParameter("DOWN").Set(0);
                            OverrideGraphicSettings over = new OverrideGraphicSettings();
                            over.SetProjectionLineColor(new Color(255, 0, 0));
                            doc.ActiveView.SetElementOverrides(newPoint.Id, over);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                        tr.Commit();
                    }
                }
                tg.Assimilate();
            }
            return true;
        }

        private List<PointLocation> getPoints(SelectionType selection)
        {
            switch (selection)
            {
                case SelectionType.AllPoints:
                    {
                        selectedPoints = allPoints.Select(x => x as Element).ToList();
                        break;
                    }
                case SelectionType.ActiveView:
                    {
                        selectedPoints = new FilteredElementCollector(doc, doc.ActiveView.Id)
                             .OfClass(typeof(FamilyInstance))
                             .WhereElementIsNotElementType()
                             .Cast<FamilyInstance>()
                             .Where(x => x.Symbol.Family.Name == "RevitNinja_Point")
                             .Select(x => x as Element)
                             .ToList();
                        break;
                    }
                case SelectionType.SelectedPoints:
                    {

                        try
                        {
                            selectedPoints =
                                uidoc.Selection.PickObjects(ObjectType.Element, new NinjaSelectionFilter(x => allPoints.Select(el => el.Id).Contains(x.Id)), "Select Points")
                                .Select(x => doc.GetElement(x)).ToList();

                        }
                        catch { }
                        break;
                    }
            }
            foreach (Element element in selectedPoints)
            {
                string suffix = "";
                string prefix, easting, northing;
                int ID = 0;
                string PointLabel = element.LookupParameter("PointLabel").AsString();
                easting = element.LookupParameter("Easting").AsString();
                northing = element.LookupParameter("Northing").AsString();
                string[] label = Regex.Split(PointLabel, @"(?<=\D)(?=\d)|(?<=\d)(?=\D)");
                if (label.Count() > 1)
                {
                    prefix = label.First();
                    ID = int.Parse(label.ElementAt(1));
                    if (label.Count() > 2)
                        suffix = label.ElementAt(2);
                }
                else
                {
                    prefix = "";
                    suffix = "";
                    ID = int.Parse(PointLabel);
                }
                points.Add(new PointLocation(ID, prefix, suffix, easting, northing, element));
            }
            return points.OrderBy(x => x.ID).ToList();
        }

        private bool IsDuplicatePointLabel(string label)
        {
            List<Element> listOfElements = allPoints.Select(x => x as Element).ToList();
            bool found = false;
            listOfElements.ForEach(element =>
            {
                if (element.LookupParameter("PointLabel").AsString() == label)
                {
                    if (!points.Where(p => p.element.Id == element.Id).Any())
                    {
                        found = true;
                    }
                }
            });
            return found;
        }

        private bool EditOrPlot()
        {

            EditPoints coordEdit = new EditPoints(points);
            coordEdit.ShowDialog();
            List<PointLocation> editablePoints = new List<PointLocation>();
            string prefix = coordEdit.prefixCombo.SelectedItem?.ToString() ?? "";
            string suffix = coordEdit.suffixCombo.SelectedItem?.ToString() ?? "";
            if (coordEdit.DialogResult == false) return false;
            if (coordEdit.edit)
            {
                if (coordEdit.allPointsCB.IsChecked == true)
                {
                    editablePoints = points;
                }
                else
                {
                    editablePoints = points.Where(x => x.Prefix == prefix && x.Suffix == suffix).ToList();
                }
                int newID = coordEdit.newID;
                if (editablePoints.Count > 0)
                {
                    using (TransactionGroup tg = new TransactionGroup(doc, "Edit Points"))
                    {
                        tg.Start();
                        foreach (PointLocation point in editablePoints)
                        {
                            string label = coordEdit.newPrefix.Text + newID.ToString() + coordEdit.newSuffix.Text;
                            if (!IsDuplicatePointLabel(label))
                            {
                                doc.print($"The label ( {label} ) exists, please choose another criteria.");
                                return false;
                            }
                            using (Transaction tr = new Transaction(doc, "Edit point"))
                            {
                                tr.Start();
                                point.element.LookupParameter("PointLabel").Set(label);
                                tr.Commit();
                            }
                            newID++;
                        }
                        tg.Assimilate();
                    }
                }
            }
            else if (coordEdit.plot)
            {
                //here plot the table of the points
                if (coordEdit.allPointsCB.IsChecked == true)
                {
                    editablePoints = points;
                }
                else
                {
                    editablePoints = points.Where(x => x.Prefix == prefix && x.Suffix == suffix).ToList();
                }
                using (TransactionGroup tg = new TransactionGroup(doc, "Plot Coordinates"))
                {
                    tg.Start();
                    XYZ corner = XYZ.Zero;
                    try
                    {
                        corner = uidoc.Selection.PickPoint("Select a point to plot the table or press ESC to cancel");
                    }
                    catch { return false; }
                    TextNoteType tableText, tableHeader;
                    try
                    {
                        tableText = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().Where(x => x.Name == "Table Text")?.First();
                        tableHeader = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().Where(x => x.Name == "Table Header")?.First();
                    }
                    catch
                    {
                        TextNoteType tt = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().First();
                        using (Transaction tr = new Transaction(doc, "Create Table Text Types"))
                        {
                            tr.Start();
                            ElementType newType = tt.Duplicate("Table Text");
                            tableText = newType as TextNoteType;
                            tableText.LookupParameter("Text Size").Set(2.5.mmToFeet());
                            newType = tt.Duplicate("Table Header");
                            tableHeader = newType as TextNoteType;
                            tableHeader.LookupParameter("Text Size").Set(3.5.mmToFeet());
                            tr.Commit();
                        }
                    }
                    TextNote pId;
                    List<ElementId> TableElements = new List<ElementId>();
                    using (Transaction tr = new Transaction(doc, "Plot the table"))
                    {
                        tr.Start();
                        pId = TextNote.Create(doc, doc.ActiveView.Id, corner + 5.0.mmToFeet() * doc.ActiveView.RightDirection, "Label", tableHeader.Id);
                        TableElements.Add(pId.Id);
                        tr.Commit();
                    }
                    BoundingBoxXYZ bbx = pId.get_BoundingBox(doc.ActiveView);
                    double width = bbx.Max.X - bbx.Min.X;
                    //double height = bbx.Max.Y - bbx.Min.Y;
                    foreach (PointLocation point in editablePoints)
                    {
                        using (Transaction tr = new Transaction(doc, "Plot the table"))
                        {
                            tr.Start();
                            corner = corner -  5.0.mmToFeet() * doc.ActiveView.UpDirection;
                            TextNote pLabel = TextNote.Create(doc, doc.ActiveView.Id, corner, point.Prefix + point.ID + point.Suffix, tableText.Id);
                            tr.Commit();
                            bbx = pLabel.get_BoundingBox(doc.ActiveView);
                            //height = bbx.Max.Y - bbx.Min.Y;
                        }
                    }

                    tg.Assimilate();
                }


            }
            return true;
        }
    }
}
