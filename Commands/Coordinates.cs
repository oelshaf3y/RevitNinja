using System;
using System.Collections.Generic;
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
    internal class Coordinates : IExternalCommand
    {
        Document doc;
        UIDocument uidoc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if(!doc.getAccess()) return Result.Failed;

            List<Family> familyList = new FilteredElementCollector(doc).OfClass(typeof(Family))
                .Cast<Family>()
                .ToList();
            StringBuilder sb = new StringBuilder();
            foreach (Family family in familyList)
            {
                string familyName = family.Name;
                sb.AppendLine($"Family Name: {familyName}");
            }
            doc.print(sb);
            return Result.Succeeded;
        }
    }
}
