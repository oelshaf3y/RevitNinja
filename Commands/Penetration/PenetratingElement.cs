using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_Ninja.Commands.Penetration
{
    public class PenetratingElement
    {
        public Element element;
        public double width, height, insulationThickness;
        public Workset workset;
        public Curve axis;
        public FamilySymbol familySymbol;
        public XYZ sleeveDir;
        RevitLinkInstance rli;
        public PenetratingElement(Element element, Workset workset,
            double width, double height, double insulationThickness, Curve axis,
            FamilySymbol familySymbol)
        {
            this.element = element;
            this.workset = workset;
            this.width = width;
            this.height = height;
            this.insulationThickness = insulationThickness;
            this.axis = axis;
            this.familySymbol = familySymbol;
        }
        public PenetratingElement(Element element, RevitLinkInstance rli)
        {
            this.element = element;
            this.rli = rli;
        }
    }
}
