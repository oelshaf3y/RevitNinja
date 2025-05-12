using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace Revit_Ninja.Utils
{
    internal class coordinatesFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.BuiltInCategory == BuiltInCategory.OST_StructuralColumns ||
                elem.Category.BuiltInCategory == BuiltInCategory.OST_StructuralFoundation;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
