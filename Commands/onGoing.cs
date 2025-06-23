using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_Ninja.Commands
{

    [TransactionAttribute(TransactionMode.Manual)]
    internal class onGoing : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            FilteredElementCollector coll = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType();

            using (Transaction tr = new Transaction(doc, "Delete Columns"))
            {
                tr.Start();
                foreach (Element e in coll)
                {
                    doc.Delete(e.Id);
                }
                tr.Commit();
            }
                return Result.Succeeded;
            }
        }
    }
