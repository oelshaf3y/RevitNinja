using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitNinja.Utils
{
    internal class NinjaSelectionFilter : ISelectionFilter
    {
        Func<Element, bool> filter;

        public NinjaSelectionFilter(Func<Element, bool> validate = null)
        {
            this.filter = validate;
        }
        public bool AllowElement(Element elem)
        {
            return filter(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
