using Autodesk.Revit.DB;

namespace Revit_Ninja.Commands.BIMSubmittal
{
    class viewforset
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public ElementId Id { get; set; }
        public viewforset(string name, bool isChecked, ElementId id)
        {
            Name = name;
            IsChecked = isChecked;
            Id = id;
        }
    }

}
