using Autodesk.Revit.DB;
using RevitNinja.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_Ninja.Commands.Cable_Management
{
    static class AStarPathFinding
    {
        // Implement A* pathfinding algorithm here
        static Apoint start { get; set; }
        static Apoint end { get; set; }
        static Document doc { get; set; }
        static List<List<Apoint>> grid = new List<List<Apoint>>();
        static List<Apoint> openSet = new List<Apoint>();
        static List<Apoint> closedSet = new List<Apoint>();

        static List<Apoint> FindPath(Document Doc, XYZ a, XYZ b)
        {
            doc = Doc;
            createGrid();
            start = GetNearest(a);
            end = GetNearest(b);
            openSet.Add(start);
            while (openSet.Count > 0)
            {
                Apoint current = openSet.OrderBy(x => x.fCost).First();
                if (current.Equals(end))
                {
                    // Path found
                    List<Apoint> path = new List<Apoint>();
                    Apoint temp = current;
                    path.Add(temp);
                    while (!temp.Equals(start))
                    {
                        // Here you would normally trace back the path using parent references
                        // For simplicity, we will just return the path as is
                        temp = start; // Placeholder to break the loop
                        path.Add(temp);
                    }
                    path.Reverse();
                    return path;
                }
                openSet.Remove(current);
                closedSet.Add(current);
                // Get neighbors (this is a placeholder, actual neighbor calculation needed)
                List<Apoint> neighbors = getNeighbors(current.i, current.j); // You need to implement neighbor finding logic
                foreach (Apoint neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor))
                        continue;
                    double tentativeGCost = current.gCost + current.point.DistanceTo(neighbor.point);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else if (tentativeGCost >= neighbor.gCost)
                        continue;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = neighbor.point.DistanceTo(end.point);
                    // Set parent reference here if needed
                }
            }
            return null;
        }

        private static void drawValidGrid()
        {

            BoundingBoxXYZ bbx = doc.ActiveView.CropBox;
            Transform tf = bbx.Transform.Inverse;
            XYZ p1, p2, p3, p4;
            p1 = tf.OfPoint(new XYZ(bbx.Min.X, bbx.Min.Y, bbx.Min.Z));
            p2 = tf.OfPoint(new XYZ(bbx.Max.X, bbx.Min.Y, bbx.Min.Z));
            p3 = tf.OfPoint(new XYZ(bbx.Max.X, bbx.Max.Y, bbx.Min.Z));
            p4 = tf.OfPoint(new XYZ(bbx.Min.X, bbx.Max.Y, bbx.Min.Z));
            List<Line> lines = new List<Line>();

            for (int i = 0; i < Math.Abs(p2.DistanceTo(p1)); i++)
            {
                lines.Add(Line.CreateBound(grid[i][0].point, grid[i][grid[i].Count - 1].point));
            }
            for (int j = 0; j < Math.Abs(p4.DistanceTo(p1)); j++)
            {
                lines.Add(Line.CreateBound(grid[0][j].point, grid[grid.Count - 1][j].point));
            }
            using (Transaction tr = new Transaction(doc, "Draw CropBox Lines"))
            {
                tr.Start();
                DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Lines)).SetShape(lines.Cast<GeometryObject>().ToList());
                tr.Commit();
            }
        }

        static void createGrid()
        {
            BoundingBoxXYZ bbx = doc.ActiveView.CropBox;
            XYZ min = bbx.Min;
            XYZ max = bbx.Max;
            XYZ p1, p2, p3, p4;
            Transform tf = bbx.Transform.Inverse;
            p1 = tf.OfPoint(new XYZ(bbx.Min.X, bbx.Min.Y, bbx.Min.Z));
            p2 = tf.OfPoint(new XYZ(bbx.Max.X, bbx.Min.Y, bbx.Min.Z));
            p3 = tf.OfPoint(new XYZ(bbx.Max.X, bbx.Max.Y, bbx.Min.Z));
            p4 = tf.OfPoint(new XYZ(bbx.Min.X, bbx.Max.Y, bbx.Min.Z));
            for (int i = 0; i < Math.Abs(p2.DistanceTo(p1)); i++)
            {
                grid.Add(new List<Apoint>());
                for (int j = 0; j < Math.Abs(p4.DistanceTo(p1)); j++)
                {
                    XYZ point = tf.OfPoint(tf.OfPoint(p1.Add(i * tf.BasisX).Add(j * tf.BasisY)));
                    grid[i].Add(new Apoint(point, i, j));
                }
            }
        }

        static List<Apoint> getNeighbors(int x, int y)
        {
            // Placeholder for neighbor finding logic
            List<Apoint> neighbors = new List<Apoint>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    // Implement neighbor logic based on grid indices
                    if (x + i >= 0 && x + i < grid.Count && y + j >= 0 && y + j < grid[0].Count && !(i == 0 && j == 0))
                    {
                        neighbors.Add(grid[x + i][y + j]);
                    }
                }
            }
            return neighbors;
        }

        static Apoint GetNearest(XYZ point)
        {
            List<Apoint> neighbors = new List<Apoint>();
            for (int i = 0; i < grid.Count; i++)
            {
                for (int j = 0; j < grid[i].Count; j++)
                {
                    if (grid[i][j].point.IsAlmostEqualTo(point) || Math.Abs(grid[i][j].point.DistanceTo(point)) <= 1.5)
                        neighbors.Add(grid[i][j]);
                }
            }
            if (neighbors.Count > 0)
                return neighbors.OrderBy(x => Math.Abs(x.point.DistanceTo(point))).First();
            return null;
        }

    }

    public class Apoint
    {
        public XYZ point { get; set; }
        public double gCost { get; set; }
        public double hCost { get; set; }
        public double fCost { get { return gCost + hCost; } }
        public int i { get; set; }
        public int j { get; set; }
        public Apoint(XYZ point, int i, int j)
        {
            this.point = point;
            gCost = 0;
            hCost = 0;
            this.i = i;
            this.j = j;
        }

        public override bool Equals(object obj)
        {
            if (obj is Apoint other)
            {
                return point.IsAlmostEqualTo(other.point);
            }
            return false;
        }

    }
}
