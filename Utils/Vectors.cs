using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitNinja.Utils
{
    public static class Vectors
    {
        /// <summary>
        /// Create an Object To Create a Direct Shape and Visualize it as any Geometry Object
        /// </summary>
        /// <param Current Document="doc"></param>
        /// <param The Geometry Object you wanna Visualize="geometryObjects"></param>
        /// <param Optional param for the Category of the created Object="CategoryID"></param>
        ///                                             Call                                                   
        ///               doc.CreateDirectShape(new List<GeometryObject>() { Point.Create(point) });            
        public static void CreateDirectShape(this Document doc, List<GeometryObject> geometryObjects, ElementId CategoryID = null)
        {

            if (CategoryID == null) CategoryID = new ElementId(BuiltInCategory.OST_GenericModel);
            DirectShape.CreateElement(doc, CategoryID).SetShape(geometryObjects);

        }
        /// <summary>
        /// To Vizualize Point XYZ IN doc
        /// </summary>
        /// <param POINT TO VISUALIZE="point"></param>
        /// <param DOC TO VISUALIZE IN ="doc"></param>
        public static void Visualize(this XYZ point, Document doc)
        {
            doc.CreateDirectShape(new List<GeometryObject>() { Point.Create(point) });
        }
        /// <summary>
        /// Vector to Curve
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="origin"></param>
        /// <param name="legnth"></param>
        /// <returns></returns>
        public static Curve AsCurve(this XYZ vector, XYZ origin = null, double? legnth = null)
        {
            if (origin == null) origin = XYZ.Zero;
            if (legnth == null) legnth = vector.GetLength();
            return Line.CreateBound(origin, origin.MoveAlongVector(vector.Normalize(), legnth.GetValueOrDefault()));

        }
        /// <summary>
        /// To Visualize Curve
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="document"></param>
        public static void Visualize(this Curve curve, Document document)
        {
            document.CreateDirectShape(new List<GeometryObject> { curve });
        }
        /// <summary>
        /// Move Point Along Vector
        /// </summary>
        /// <param name="pointToMove"></param>
        /// <param name="vector"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static XYZ MoveAlongVector(this XYZ pointToMove, XYZ vector, double distance) => pointToMove.Add(vector * distance);
        /// <summary>
        /// Move Point Along Vector by distance
        /// </summary>
        /// <param name="pointToMove"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static XYZ MoveAlongVector(this XYZ pointToMove, XYZ vector) => pointToMove.Add(vector);

        /// <summary>
        /// To Points To Vector between Them
        /// </summary>
        /// <param name="fromPoint"></param>
        /// <param name="toPoint"></param>
        /// <returns></returns>
        public static XYZ ToVector(this XYZ fromPoint, XYZ toPoint) => toPoint - fromPoint;
        /// <summary>
        /// To Convert Line to Vector at the same Direction
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static XYZ ToNormalizedVector(this Curve curve)
        {
            return (curve.GetEndPoint(1) - curve.GetEndPoint(0).Normalize());
        }
        /// <summary>
        ///  Measure the Distance Between two Points Along a Vector
        /// </summary>
        /// <param name="fromPoint"></param>
        /// <param name="toPoint"></param>
        /// <param name="vectorToMeasureBy"></param>
        /// <returns></returns>
        public static double DistanceAlongVector(this XYZ fromPoint, XYZ toPoint, XYZ vectorToMeasureBy)
        {
            return Math.Abs(fromPoint.ToVector(toPoint).DotProduct(vectorToMeasureBy));
        }

    }
}
