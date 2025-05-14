using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitNinja.Utils;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using Autodesk.Revit.DB.Structure;


namespace Revit_Ninja.Commands.ReviztoIssues
{
    [Transaction(TransactionMode.Manual)]
    internal class Clashes : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            List<Issue> issues = new List<Issue>();
            if (!doc.getAccess()) return Result.Failed;
            FamilySymbol clashBall = null;
            try
            {

                clashBall = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(x => x.FamilyName == "RevitNinja_Revizto_Clash_Ball").First();
            }
            catch
            {
                try
                {
                    string path = doc.ExtractEmbeddedResource("RevitNinja_Revizto_Clash_Ball.rfa");
                    using (Transaction tr = new Transaction(doc))
                    {
                        tr.Start("Load Family");
                        doc.LoadFamily(path);
                        tr.Commit();
                        tr.Dispose();
                    }
                    clashBall = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericModel)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(x => x.FamilyName == "RevitNinja_Revizto_Clash_Ball").First();
                }
                catch
                {
                    doc.print("Something went wrong!\ncontact the developer.");
                    return Result.Failed;
                }
            }



            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Excel |*.xlsx";
            openFile.ShowDialog();
            issues = ReadIssuesFromExcel(openFile.FileName);

            using (TransactionGroup tg = new TransactionGroup(doc, "Create clash balls"))
            {
                tg.Start();
                using (Transaction tr = new Transaction(doc, "Create clash balls"))
                {
                    tr.Start();
                    if (!clashBall.IsActive)
                    {
                        clashBall.Activate();
                    }
                    tr.Commit();
                }

                foreach (Issue issue in issues)
                {
                    string pointLoc = issue.Position.Replace('(', ' ').Replace(')', ' ').Trim();
                    double x = Convert.ToDouble(pointLoc.Split(';')[0].Trim());
                    double y = Convert.ToDouble(pointLoc.Split(';')[1].Trim());
                    double z = Convert.ToDouble(pointLoc.Split(';')[2].Trim());
                    XYZ point = ToInternal(new XYZ(x, y, z));
                    FamilyInstance ball = null;
                    using (Transaction tr = new Transaction(doc))
                    {
                        tr.Start("Create Clash Balls");

                        ball = doc.Create.NewFamilyInstance(point,
                            clashBall,
                            StructuralType.NonStructural);
                        tr.Commit();
                    }
                    using (Transaction tr = new Transaction(doc, "modify params"))
                    {
                        tr.Start();
                        //something is wrong and parameters are readonly !1 
                        try
                        {
                            issue.Comments.RemoveAt(0);
                            ball.LookupParameter("Comments").Set(JsonConvert.SerializeObject(issue.Comments.Select(x => x.ToDictionary()).ToList()));
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Date").Set(issue.Date);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("GridLocation").Set(issue.GridLocation);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Id").Set(issue.Id);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Issue Level").Set(issue.Level);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("SnapshotLink").Set(issue.SnapshotLink);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Stamp").Set(issue.Stamp);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("StampTitle").Set(issue.StampTitle);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Status").Set(issue.Status);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Title").Set(issue.Title);
                        }
                        catch { }
                        try
                        {
                            ball.LookupParameter("Zone").Set(issue.Zone);
                        }
                        catch { }
                        tr.Commit();
                    }
                }
                tg.Assimilate();
            }


            return Result.Succeeded;
        }
        public List<Issue> ReadIssuesFromExcel(string filePath)
        {
            List<Issue> issues = new List<Issue>();

            Application excelApp = new Application();
            Workbook workbook = null;

            try
            {
                workbook = excelApp.Workbooks.Open(filePath);
                Worksheet worksheet = workbook.Sheets[1];

                Range usedRange = worksheet.UsedRange;

                for (int row = 2; row <= usedRange.Rows.Count; row++)
                {
                    string id = GetCellValue(usedRange, row, 1);
                    if (string.IsNullOrEmpty(id))
                    {

                        string content = GetCellValue(usedRange, row, 8);
                        if (content == "Original Markup") continue;
                        issues.Last().Comments.Add(new Comment
                        (
                            content,
                             GetCellValue(usedRange, row, 9),
                             GetCellValue(usedRange, row, 10)
                        ));
                    }
                    else
                    {

                        Issue issue = new Issue
                        {
                            Id = id,
                            SnapshotLink = GetCellValue(usedRange, row, 2),
                            Date = GetCellValue(usedRange, row, 3),
                            Status = GetCellValue(usedRange, row, 4),
                            Title = GetCellValue(usedRange, row, 6),
                            Stamp = GetCellValue(usedRange, row, 15),
                            Level = GetCellValue(usedRange, row, 20),
                            GridLocation = GetCellValue(usedRange, row, 21),
                            Zone = GetCellValue(usedRange, row, 25),
                            StampTitle = GetCellValue(usedRange, row, 26),
                            Position = GetCellValue(usedRange, row, 29)
                        };
                        issues.Add(issue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Excel file: {ex.Message}");
            }
            finally
            {
                if (workbook != null)
                {
                    workbook.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                }

                excelApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }

            return issues;
        }

        private string GetCellValue(Range usedRange, int row, int column)
        {
            Range cell = (Range)usedRange.Cells[row, column];
            if (cell.Hyperlinks.Count > 0)
            {
                Hyperlink link = cell.Hyperlinks[1];
                return link.Address?.ToString() ?? string.Empty;
            }
            return cell.Value?.ToString() ?? string.Empty;
        }
        public XYZ ToInternal(XYZ local)
        {
            // Local coordinates

            // Get the project base point
            BasePoint basePoint = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>()
                .FirstOrDefault(bp => !bp.IsShared); // Internal base point

            // Get the shared transform
            Transform transform = doc.ActiveProjectLocation.GetTotalTransform();

            // Project position from internal coordinates
            ProjectLocation location = doc.ActiveProjectLocation;
            ProjectPosition projPos = location.GetProjectPosition(XYZ.Zero);

            // Internally, Revit does something like this:
            double angle = projPos.Angle; // in radians
            double dx = projPos.EastWest;
            double dy = projPos.NorthSouth;
            double dz = projPos.Elevation;

            // Reverse logic (conceptual)
            Transform rotation = Transform.CreateRotationAtPoint(
                XYZ.BasisZ, -angle, XYZ.Zero);

            XYZ translated = local - new XYZ(dx, dy, dz);
            XYZ shared = rotation.OfPoint(translated);

            return shared;
        }

    }
}
