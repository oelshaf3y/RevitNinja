using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_Ninja.Commands.PointsCoord
{
    public class PointLocation
    {
        public int ID { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public string Easting { get; set; }
        public string Northing { get; set; }
        public Element element { get; set; }

        public PointLocation(int id, string prefix, string suffix, string easting, string northing, Element element)
        {
            ID = id;
            Prefix = prefix;
            Suffix = suffix;
            Easting = easting;
            Northing = northing;
            this.element = element;
        }
    }
}
