using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class colorElement
        : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess()) return Result.Failed;
            string filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.txt");
            if (uidoc.Selection.GetElementIds().Count == 0) return Result.Failed;
            List<Element> selected;
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            selected = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).ToList();
            try
            {
                ElementId patId = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement))
                    .Cast<FillPatternElement>()
                    .FirstOrDefault(fpe =>
                    fpe.GetFillPattern().IsSolidFill
                    && fpe.GetFillPattern().Target == FillPatternTarget.Drafting).Id;
                ogs.SetSurfaceForegroundPatternId(patId);
                ogs.SetSurfaceForegroundPatternColor(new Color(255, 0, 0));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "No Fill Pattern Found");
                return Result.Failed;
            }
            View current = doc.ActiveView;
            using (Transaction tr = new Transaction(doc, "Color Element"))
            {
                tr.Start();
                selected.ForEach(s => current.SetElementOverrides(s.Id, ogs));
                tr.Commit();
                tr.Dispose();
            }
            List<string> ids = new List<string>();

            try
            {
                ids = File.ReadAllLines(filepath).ToList();

            }
            catch (Exception ex)
            {
            }
            finally
            {
                foreach(Element s in selected)
                {
                    if (ids.Count == 0) ids.Add(s.Id.ToString());
                    else if (ids.Count > 0 && !ids.Contains(s.Id.ToString())) ids.Add(s.Id.ToString());
                }
                File.WriteAllLines(filepath, ids);
            }
            return Result.Succeeded;
        }
    }
}
