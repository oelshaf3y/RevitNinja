using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Ninja.Views;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.PointsCoord
{
    [TransactionAttribute(TransactionMode.Manual)]

    internal class PointCoords : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        FamilySymbol symbol;
        Family fam;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            ProjectLocation projectLocation = doc.ActiveProjectLocation;
            List<FamilyInstance> allPoints = new List<FamilyInstance>();
            if (doc.ActiveView is View3D || doc.ActiveView.ViewType == ViewType.Schedule)
            {
                doc.print("Please select a 2D view to import points.");
                return Result.Cancelled;
            }
            #region get existing symbol
            try
            {
                var alls = new FilteredElementCollector(doc)
                     .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                     .OfClass(typeof(FamilySymbol))
                     .Cast<FamilySymbol>()
                     .Where(x => x.Name == "Ninja_Point").First();
                symbol = alls;
            }
            catch (Exception ex) {  }
            #endregion

            #region load symbol if not found
            if (symbol == null)
            {
                using (Transaction tr = new Transaction(doc, "Load Family"))
                {
                    tr.Start();
                    string path = doc.ExtractEmbeddedResource("RevitNinja_Point.rfa");
                    doc.LoadFamily(path, out fam);
                    tr.Commit();
                }
                try
                {
                    symbol = doc.GetElement(fam.GetFamilySymbolIds().First()) as FamilySymbol;
                    //symbol = new FilteredElementCollector(doc)
                    //     .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                    //     .Cast<FamilySymbol>()
                    //     .Where(x => x.Family.Name == "RevitNinja_Point").FirstOrDefault();
                }
                catch (Exception ex) {  }
            }
            #endregion

            #region get existing points
            try
            {
                allPoints = new FilteredElementCollector(doc)
                 .OfClass(typeof(FamilyInstance))
                 .WhereElementIsNotElementType()
                 .Cast<FamilyInstance>()
                 .Where(x => x.Symbol.Family.Name == "RevitNinja_Point")
                 .ToList();
            }
            catch (Exception ex) { doc.print("get all points \n" + ex.Message); }
            #endregion

            #region insert new point
            using (TransactionGroup tg = new TransactionGroup(doc, "Points Coordinates"))
            {
                tg.Start();

                CoordView window = new CoordView(allPoints.Count() + 1);
                window.ShowDialog();

                if (window.DialogResult == false) return Result.Cancelled;
                int startNumber = 0;
                if (int.TryParse(window.numberBox.Text, out int x)) startNumber = x;
                try
                {
                    symbol.Activate();
                }
                catch (Exception ex)
                {
                    doc.print("Activate symbol \n" + ex.Message);
                    return Result.Failed;
                }
                while (true)
                {
                    using (Transaction tr = new Transaction(doc, "Create Point"))
                    {
                        tr.Start();
                        Ninja.SetActiveWorkPlane(uidoc);
                        try
                        {
                            XYZ p = uidoc.Selection.PickPoint("Select a point or press ESC to finish");
                            FamilyInstance newPoint = doc.Create.NewFamilyInstance(p, symbol, doc.ActiveView);
                            ((Element)newPoint).LookupParameter("Easting").Set(Math.Round(projectLocation.GetProjectPosition(p).EastWest.feetToMM()).ToString());
                            ((Element)newPoint).LookupParameter("Northing").Set(Math.Round(projectLocation.GetProjectPosition(p).NorthSouth.feetToMM()).ToString());
                            ((Element)newPoint).LookupParameter("PointLabel").Set(window.prefixBox.Text + startNumber.ToString() + window.suffixBox.Text);
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                        tr.Commit();
                    }
                    startNumber++;
                }
                tg.Assimilate();
            }
            #endregion

            return Result.Succeeded;
        }
    }
}
