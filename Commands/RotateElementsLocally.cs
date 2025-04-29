using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using RevitNinja.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class RotateElementsLocally : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        RotateElementsView uc;
        double angle;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess())
            {
                doc.print("Please contact the developer");
                return Result.Failed;
            }
            angle = 0;
            var theme = UIFramework.ApplicationTheme.CurrentTheme;
            List<Element> selected = new List<Element>();
            uc = new RotateElementsView(theme, uidoc);
            RibbonController.ShowOptionsBar(uc, false);
            try
            {

                selected = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick Elements To rotate")
                    .Select(x => doc.GetElement(x)).ToList();
                RibbonController.HideOptionsBar();
            }
            catch (Exception ex)
            {
                RibbonController.HideOptionsBar();
                return Result.Cancelled;
            }
            using (Transaction tr = new Transaction(doc, "Rotate Elements Locally."))
            {
                tr.Start();
                if (!double.TryParse(uc.angleVal.Text, out angle))
                {
                    doc.print("You entered an invalid angle");
                    return Result.Failed;
                }
                else if (angle == 0)
                {
                    doc.print("you didn't enter any rotation angle.");
                    return Result.Cancelled;
                }
                angle = angle * Math.PI / 180;
                foreach (Element element in selected)
                {
                    element.Location.Rotate(
                        Line.CreateUnbound(element.getCG(), doc.ActiveView.CropBox.Transform.BasisZ),
                        angle
                        );
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
