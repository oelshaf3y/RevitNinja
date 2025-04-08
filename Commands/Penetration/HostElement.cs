using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_Ninja.Commands.Penetration
{
    public class HostElement
    {
        public Element element;
        public RevitLinkInstance rli;

        public HostElement(Element element, RevitLinkInstance rli)
        {
            this.element = element;
            this.rli = rli;
        }
    }
}
