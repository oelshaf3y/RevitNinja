using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using RevitNinja.Views;

namespace RevitNinja.Commands
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class AlignTags : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        int count;
        AlignView uc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            if (!doc.getAccess())
            {
                return Result.Failed;
            }
            bool horizontal = true;
            //var options = SetupOptionsBar();
            //System.Windows.Controls.TextBox txtbox = null;
            var theme = UIFramework.ApplicationTheme.CurrentTheme;

            uc = new AlignView(theme,uidoc);

            double offset = 0;
            bool ByElement = false;
            List<IndependentTag> tags = new List<IndependentTag>();
            try
            {
                RibbonController.ShowOptionsBar(uc, false);
                tags = pickTags();
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
                if (tags.Count == 0)
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
                //TextBox tx = (TextBox)uc.FindName("offval");
                alignTags(tags, horizontal, ByElement, offset.mmToFeet() / doc.ActiveView.Scale);

            }
            catch (Exception e)
            {
                doc.print(e);
            }
            //RibbonController.HideOptionsBar();



            return Result.Succeeded;

        }
        //private TagsViewModel SetupOptionsBar()
        //{
        //var options = new TagsViewModel(0, new string[] { "Horizontally,Vertically" });

        //var view = new AlignTagsView();

        //    RibbonController.ShowOptionsBar();
        //    return options;
        //}

        private List<IndependentTag> pickTags()
        {
            List<Reference> selection;
            try
            {
                selection = uidoc.Selection.PickObjects(
                Autodesk.Revit.UI.Selection.ObjectType.Element,
                new NinjaSelectionFilter(x => x is IndependentTag),
                "Pick Tags"
                ).ToList();
            }
            catch (Exception e)
            {
                selection = new List<Reference>();
            }
            return selection.Select(x => doc.GetElement(x))
                .Cast<IndependentTag>().ToList();
        }

        private void alignTags(List<IndependentTag> tags, bool horizontal, bool byElement, double offset)
        {
            XYZ point;
            if (!byElement) point = uidoc.Selection.PickPoint("Select Point");
            else
            {
                IndependentTag sourceTag = doc.GetElement(uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element,
                    new NinjaSelectionFilter(x => x is IndependentTag),
                    "pick source tag.")) as IndependentTag;
                point = sourceTag.TagHeadPosition;
            }
            List<IndependentTag> attached = tags.Where(x => x.LeaderEndCondition == LeaderEndCondition.Attached).ToList();
            using (TransactionGroup tg = new TransactionGroup(doc, "Align Tags"))
            {
                tg.Start();
                using (Transaction tr = new Transaction(doc, "Align Tags"))
                {
                    tr.Start();
                    //if (horizontal) tags = tags.OrderBy(x => x.TagHeadPosition.X).ToList();
                    //else
                    foreach (IndependentTag tag in tags)
                    {
                        if (tag.LeaderEndCondition == LeaderEndCondition.Attached) tag.LeaderEndCondition = LeaderEndCondition.Free;
                    }
                    tags = tags.OrderBy(tag => tag.GetLeaderEnd(tag.GetTaggedReferences().First()).DistanceTo(point)).ToList();


                    XYZ prevHead = XYZ.Zero;
                    for (int i = 0; i < tags.Count; i++)
                    {
                        IndependentTag tag = tags[i];
                        XYZ temp = tag.TagHeadPosition;
                        if (byElement)
                        {
                            if (horizontal) tag.TagHeadPosition = new XYZ(temp.X, point.Y, temp.Z);
                            else tag.TagHeadPosition = new XYZ(point.X, temp.Y, temp.Z);
                        }

                        else
                        {
                            if (horizontal)
                            {
                                if (prevHead == XYZ.Zero)
                                {
                                    tag.TagHeadPosition = point;
                                    point = point.Add(getHorizontalLength(
                                            (tag.get_BoundingBox(doc.ActiveView).Max - tag.get_BoundingBox(doc.ActiveView).Min))
                                        * doc.ActiveView.CropBox.Transform.BasisX);
                                }
                                else
                                {
                                    point = point.Add(offset * doc.ActiveView.CropBox.Transform.BasisX);
                                    tag.TagHeadPosition = point;
                                    point = point.Add(
                                        getHorizontalLength(
                                            (tag.get_BoundingBox(doc.ActiveView).Max - tag.get_BoundingBox(doc.ActiveView).Min))
                                        * doc.ActiveView.CropBox.Transform.BasisX);
                                }
                            }
                            else
                            {
                                if (prevHead == XYZ.Zero)
                                {
                                    tag.TagHeadPosition = point;
                                    point = point.Add(getVerticalLength(
                                            (tag.get_BoundingBox(doc.ActiveView).Max - tag.get_BoundingBox(doc.ActiveView).Min))
                                        * doc.ActiveView.CropBox.Transform.BasisY);
                                }
                                else
                                {
                                    point = point.Add(offset * doc.ActiveView.CropBox.Transform.BasisY);
                                    tag.TagHeadPosition = point;
                                    point = point.Add(
                                        getVerticalLength(
                                            (tag.get_BoundingBox(doc.ActiveView).Max - tag.get_BoundingBox(doc.ActiveView).Min))
                                        * doc.ActiveView.CropBox.Transform.BasisY);
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
                using (Transaction tt = new Transaction(doc, "align Tags Again"))
                {
                    tt.Start();
                    checkTagsIntersections(tags);
                    foreach (IndependentTag tag in tags)
                    {
                        XYZ head = tag.TagHeadPosition;
                        XYZ end = tag.GetLeaderEnd(tag.GetTaggedReferences().First());
                        double length = getVerticalLength(head - end);
                        tag.SetLeaderElbow(tag.GetTaggedReferences().First(), end.Add(length * doc.ActiveView.CropBox.Transform.BasisY));
                    }
                    foreach (IndependentTag tag in attached)
                    {
                        tag.LeaderEndCondition = LeaderEndCondition.Attached;
                    }
                    tt.Commit();
                }
                tg.Assimilate();
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
