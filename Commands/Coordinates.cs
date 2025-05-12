using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Revit_Ninja.Commands.Penetration;
using Revit_Ninja.Utils;
using Revit_Ninja.Views;
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
            if (!doc.getAccess()) return Result.Failed;
            ProjectLocation location = doc.ActiveProjectLocation;
            
            List<Element> selection = new List<Element>();
            try
            {
                selection = uidoc.Selection.PickObjects(
                    ObjectType.Element, new coordinatesFilter(), "Pick Elements")
                    .Select(x => doc.GetElement(x)).ToList();
            }
            catch { return Result.Cancelled; }
            List<Parameter> parameters = selection.First().GetOrderedParameters().OrderBy(x => x.Definition.Name).ToList();
            coordinates coordinates = new coordinates(parameters);
            coordinates.ShowDialog();
            if (coordinates.cancel) return Result.Cancelled;
            Parameter easting = parameters[coordinates.eastingCombo.SelectedIndex];
            Parameter northing = parameters[coordinates.northingCombo.SelectedIndex];
            int count = 0;
            using (TransactionGroup tg = new TransactionGroup(doc, "Get Coordinates"))
            {
                tg.Start();
                foreach (Element elem in selection)
                {
                    LocationPoint loc = elem.Location as LocationPoint;
                    XYZ point = loc.Point;
                    try
                    {
                        using (Transaction tr = new Transaction(doc, "set for element"))
                        {
                            tr.Start();
                            elem.LookupParameter(easting.Definition.Name).Set(location.GetProjectPosition(point).EastWest.feetToMeter());
                            elem.LookupParameter(northing.Definition.Name).Set(location.GetProjectPosition(point).NorthSouth.feetToMeter());
                            tr.Commit();
                        }

                    }
                    catch
                    {
                        count++;
                    }
                }
                tg.Assimilate();
            }

            //doc.print(sb);
            return Result.Succeeded;
        }
    }
}
