using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitNinja.Utils;

namespace Revit_Ninja.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class AlignViewToFace : IExternalCommand
    {
        UIDocument uidoc;
        Document doc;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;

            //if (!doc.getAccess())  return Result.Failed; 

            Reference selection;
            XYZ pointOfSelection;
            if (!(doc.ActiveView is View3D view3D)) { TaskDialog.Show("Error", "Active View must be 3D!"); return Result.Failed; }
            try
            {
                selection = uidoc.Selection.PickObject(ObjectType.PointOnElement, "Select a point on the face of an element");
                pointOfSelection = selection.GlobalPoint;
                Line l = Line.CreateBound(pointOfSelection, pointOfSelection.Add(10 * doc.ActiveView.ViewDirection));
                Element element;
                Solid s;
                if (doc.GetElement(selection.ElementId) is RevitLinkInstance linkInstance)
                {
                    Document linkedDoc = linkInstance.GetLinkDocument();
                    element = linkedDoc.GetElement(selection.LinkedElementId);
                    s = linkedDoc.getSolid(element);
                }
                else
                {
                    element = doc.GetElement(selection.ElementId);
                    s = doc.getSolid(element);
                }
                //doc.print(element.Name);
                FaceArray faces = s.Faces;
                foreach (Face face in faces)
                {
                    if (face.Intersect(l) != SetComparisonResult.Disjoint)
                    {
                        XYZ normal = face.ComputeNormal(selection.UVPoint);
                        XYZ viewDirection = -normal;
                        XYZ newViewDirection = normal.CrossProduct(viewDirection).Normalize();
                        XYZ newUpDirection = normal.CrossProduct(newViewDirection).Normalize();
                        PlanarFace pf = face as PlanarFace;
                        XYZ faceOrigin = pf.Origin;
                        XYZ upVector = CalculateUpVector(viewDirection);
                        using (Transaction tx = new Transaction(doc, "Align View"))
                        {
                            tx.Start();

                            ViewOrientation3D newOrientation = new ViewOrientation3D(
                                faceOrigin + viewDirection * GetViewDistance(view3D),
                                upVector,
                            viewDirection
                            );

                            view3D.SetOrientation(newOrientation);
                            tx.Commit();
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                doc.print(ex.Message);
            }



            return Result.Succeeded;
        }
        private double GetViewDistance(View3D view)
        {
            return view.get_Parameter(BuiltInParameter.VIEWER_PERSPECTIVE).AsInteger() == 1
                ? view.GetOrientation().EyePosition.DistanceTo(view.GetOrientation().ForwardDirection)
                : 10;
        }
        private XYZ CalculateUpVector(XYZ viewDirection)
        {
            XYZ upVector = XYZ.BasisZ;
            if (Math.Abs(viewDirection.DotProduct(upVector)) > 0.8)
                upVector = XYZ.BasisY;
            if (Math.Abs(viewDirection.DotProduct(upVector)) > 0.8)
                upVector = XYZ.BasisX;
            return upVector.Normalize();
        }
    }
}
