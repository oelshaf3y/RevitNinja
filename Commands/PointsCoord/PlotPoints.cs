using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands.PointsCoord
{
    internal class PlotPoints
    {
        UIDocument uidoc;
        Document doc;
        List<PointLocation> points;
        public PlotPoints(UIDocument Uidoc, Document Doc, List<PointLocation> Points)
        {
            uidoc = Uidoc;
            Doc = Doc;
            points = Points;

            if (Doc.ActiveView is View3D || Doc.ActiveView.ViewType == ViewType.Schedule)
            {
                Doc.print("Please select a 2D view to import points.");
                return;
            }

        }
    }
}
