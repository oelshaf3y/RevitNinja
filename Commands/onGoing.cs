using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace Revit_Ninja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class OnGoing : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }
    }

}