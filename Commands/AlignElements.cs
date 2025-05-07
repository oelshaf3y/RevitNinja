using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using RevitNinja.Views;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class AlignElements : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        int count;
        AlignView uc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            //if (!doc.getAccess())  return Result.Failed; 

            bool horizontal = true;
            var theme = UIFramework.ApplicationTheme.CurrentTheme;

            uc = new AlignView(theme, uidoc);
            double offset = 0;
            bool ByElement = false;
            List<Element> selectedElements = new List<Element>();
            try
            {
                RibbonController.ShowOptionsBar(uc, false);
                selectedElements = pickElements();
                ByElement = uc.byElement.IsChecked == true;
                if (uc.alignCombo.SelectedIndex == 1) horizontal = false;
                if (uc.byElement.IsChecked == false)
                {
                    if (uc.offVal.Text.Trim().Length == 0) offset = 0;
                    else if (!double.TryParse(uc.offVal.Text, out offset))
                    {
                        RibbonController.HideOptionsBar();

                        doc.print("You entered an invalid value for offset!");
                        return Result.Cancelled;
                    }
                }
                RibbonController.HideOptionsBar();
                if (selectedElements.Count == 0)
                {
                    return Result.Failed;
                }
            }
            catch (Exception ex)
            {
                RibbonController.HideOptionsBar();
                doc.print(ex);
                return Result.Failed;
            }
            try
            {
                alignElements(selectedElements, horizontal, ByElement, offset.mmToFeet() / doc.ActiveView.Scale);

            }
            catch (Exception e)
            {
                doc.print(e);
            }



            return Result.Succeeded;

        }

        private List<Element> pickElements()
        {
            List<Reference> selection;
            try
            {
                selection = uidoc.Selection.PickObjects(
                Autodesk.Revit.UI.Selection.ObjectType.Element,
                "Pick Elements"
                ).ToList();
            }
            catch (Exception e)
            {
                selection = new List<Reference>();
            }
            return selection.Select(x => doc.GetElement(x)).ToList();
        }

        private void alignElements(List<Element> elements, bool horizontal, bool byElement, double offset)
        {
            XYZ point;
            if (!byElement) point = uidoc.Selection.PickPoint("Select Point");
            else
            {
                Element sourceElement = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element,
                    "pick source element."));
                point = sourceElement.getCG();
            }
            using (Transaction tr = new Transaction(doc, "Align Elements"))
            {
                tr.Start();
                if (horizontal)
                {
                    Line sourceLine = Line.CreateUnbound(point.Add(offset * doc.ActiveView.CropBox.Transform.BasisY), doc.ActiveView.CropBox.Transform.BasisX);
                    foreach (Element element in elements)
                    {
                        Line line = Line.CreateUnbound(element.getCG(), doc.ActiveView.CropBox.Transform.BasisY);
                        IntersectionResultArray ira = new IntersectionResultArray();
                        if (line.Intersect(sourceLine, out ira) != SetComparisonResult.Disjoint)
                        {
                            XYZ newLocation = ira.get_Item(0).XYZPoint;
                            element.Location.Move(newLocation - element.getCG());
                        }
                    }
                }
                else
                {
                    Line sourceLine = Line.CreateUnbound(point.Add(offset * doc.ActiveView.CropBox.Transform.BasisX), doc.ActiveView.CropBox.Transform.BasisY);
                    foreach (Element element in elements)
                    {
                        Line line = Line.CreateUnbound(element.getCG(), doc.ActiveView.CropBox.Transform.BasisX);
                        IntersectionResultArray ira = new IntersectionResultArray();
                        if (line.Intersect(sourceLine, out ira) != SetComparisonResult.Disjoint)
                        {
                            XYZ newLocation = ira.get_Item(0).XYZPoint;
                            element.Location.Move(newLocation - element.getCG());
                        }
                    }
                }
                tr.Commit();
            }
        }

        private void checkTagsIntersections(List<IndependentTag> tags)
        {
            foreach (IndependentTag tag in tags)
            {
                XYZ head = new XYZ(tag.TagHeadPosition.X, tag.TagHeadPosition.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                XYZ leaderEnd = tag.GetLeaderEnd(tag.GetTaggedReferences().First());
                XYZ end = new XYZ(leaderEnd.X, leaderEnd.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                Line tagLine = Line.CreateBound(head, end);
                foreach (IndependentTag other in tags)
                {
                    if (other != tag)
                    {
                        XYZ otherHead = new XYZ(other.TagHeadPosition.X, other.TagHeadPosition.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                        XYZ otherLeaderEnd = other.GetLeaderEnd(other.GetTaggedReferences().First());
                        XYZ otherEnd = new XYZ(otherLeaderEnd.X, otherLeaderEnd.Y, doc.ActiveView.CropBox.Transform.Origin.Z);
                        Line otherLine = Line.CreateBound(otherHead, otherEnd);
                        if (tagLine.Intersect(otherLine) != SetComparisonResult.Disjoint)
                        {
                            XYZ temp = tag.TagHeadPosition;
                            tag.TagHeadPosition = other.TagHeadPosition;
                            other.TagHeadPosition = temp;
                            checkTagsIntersections(tags);
                            return;
                        }
                    }
                }
            }
        }

        private double getVerticalLength(XYZ a) => (a.DotProduct(doc.ActiveView.CropBox.Transform.BasisY)) / doc.ActiveView.CropBox.Transform.BasisY.GetLength();
        private double getHorizontalLength(XYZ a) => (a.DotProduct(doc.ActiveView.CropBox.Transform.BasisX)) / doc.ActiveView.CropBox.Transform.BasisX.GetLength();
    }
}
