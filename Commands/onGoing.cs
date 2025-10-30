using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNinja.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Revit_Ninja.Commands
{
    static class OnGoing
    {
        // Implement A* pathfinding algorithm here
        static Apoint start { get; set; }
        static Apoint end { get; set; }
        static Transform tf { get; set; }
        static Document doc { get; set; }
        static List<List<Apoint>> grid = new List<List<Apoint>>();
        static List<Apoint> openSet = new List<Apoint>();
        static List<Apoint> closedSet = new List<Apoint>();
        static List<ElementId> negligibleElements = new List<ElementId>();
        public static List<Element> collided = new List<Element>();
        static List<Element> Collideable = new List<Element>();

        public static List<Apoint> FindPath(Document Doc, XYZ a, XYZ b, List<ElementId> negligible, List<Element> collideable)
        {
            doc = Doc;
            negligibleElements = negligible;
            Collideable = collideable;
            // clear previous state
            grid.Clear();
            openSet.Clear();
            closedSet.Clear();
            start = new Apoint(a, 0, 0);
            createGrid();
            drawValidGrid();

            start = GetNearest(a);
            end = GetNearest(b);

            // validate start / end
            if (start == null || end == null)
            {
                doc.print("start or end is null");
                return null;
            }
            if (!start.walkable || !end.walkable)
            {
                doc.print("start or end is not walkable");
                return null;
            }

            // init start costs
            start.gCost = 0;
            start.hCost = start.point.DistanceTo(end.point);
            start.parent = null;

            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Apoint current = openSet.OrderBy(x => x.fCost).First();
                if (current.Equals(end))
                {
                    // Path found - reconstruct using parent references
                    List<Apoint> path = new List<Apoint>();
                    Apoint temp = current;
                    while (temp != null)
                    {
                        path.Add(temp);
                        if (temp.Equals(start))
                            break;
                        temp = temp.parent;
                    }
                    path.Reverse();

                    return path;
                }
                openSet.Remove(current);
                closedSet.Add(current);

                List<Apoint> neighbors = getNeighbors(current.i, current.j);
                foreach (Apoint neighbor in neighbors)
                {
                    if (!neighbor.walkable)
                        continue;
                    if (closedSet.Contains(neighbor))
                        continue;

                    Line line = Line.CreateBound(current.point, neighbor.point);
                    if (IsLineBlocked(line))
                        continue;

                    double tentativeGCost = current.gCost + current.point.DistanceTo(neighbor.point);

                    if (!openSet.Contains(neighbor))
                    {
                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = neighbor.point.DistanceTo(end.point);
                        neighbor.parent = current;
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGCost < neighbor.gCost)
                    {
                        neighbor.gCost = tentativeGCost;
                        neighbor.parent = current;
                    }
                }
            }
            return null;
        }
        private static void drawValidGrid()
        {
            List<Line> lines = new List<Line>();

            if (grid == null || grid.Count == 0) return;

            for (int i = 0; i < grid.Count; i++)
            {
                for (int j = 0; j < grid[i].Count; j++)
                {
                    try
                    {
                        if (grid[i][j].walkable)
                        {
                            if (i + 1 < grid.Count && grid[i + 1][j].walkable)
                                lines.Add(Line.CreateBound(grid[i][j].point, grid[i + 1][j].point));
                            if (j + 1 < grid[i].Count && grid[i][j + 1].walkable)
                                lines.Add(Line.CreateBound(grid[i][j].point, grid[i][j + 1].point));
                        }
                    }
                    catch { }
                }
            }

            if (lines.Count == 0) return;

            using (Transaction tr = new Transaction(doc, "Draw Grid"))
            {
                tr.Start();
                DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_Lines)).SetShape(lines.Cast<GeometryObject>().ToList());
                tr.Commit();
            }
        }
        static void createGrid()
        {
            if (doc?.ActiveView == null) throw new InvalidOperationException("ActiveView is null");
            BoundingBoxXYZ bbx = doc.ActiveView.CropBox;
            if (bbx == null) throw new InvalidOperationException("CropBox is null");

            // transform from bounding-box local coords -> model coords
            tf = bbx.Transform;

            XYZ localMin = bbx.Min;
            XYZ localMax = bbx.Max;

            // normalize
            double minX = Math.Min(localMin.X, localMax.X);
            double maxX = Math.Max(localMin.X, localMax.X);
            double minY = Math.Min(localMin.Y, localMax.Y);
            double maxY = Math.Max(localMin.Y, localMax.Y);
            double z = 0;

            // grid cell size in feet (adjust to taste)
            double cellSize = 1.0;

            int width = Math.Max(1, (int)Math.Ceiling((maxX - minX) / cellSize));
            int height = Math.Max(1, (int)Math.Ceiling((maxY - minY) / cellSize));

            grid.Clear();

            for (int i = 0; i < width; i++)
            {
                var row = new List<Apoint>();
                for (int j = 0; j < height; j++)
                {
                    XYZ localPoint = new XYZ(minX + (i) * cellSize, minY + (j) * cellSize, z);
                    XYZ modelPoint = tf.OfPoint(localPoint);
                    var ap = new Apoint(modelPoint, i, j);
                    ap.walkable = !IsPointBlocked(modelPoint);
                    row.Add(ap);
                }
                grid.Add(row);
            }
        }

        static bool IsPointBlocked(XYZ point)
        {
            if (doc == null || point == null) return true;

            Line vertical = Line.CreateUnbound(point, XYZ.BasisZ);
            return IsLineBlocked(vertical);
        }

        static bool IsLineBlocked(Line line)
        {


            foreach (Element el in Collideable)
            {
                if (negligibleElements != null && negligibleElements.Contains(el.Id))
                    continue;

                Solid solid = null;
                try { solid = doc.getSolid(el); } catch { solid = null; }
                if (solid == null) continue;

                try
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (face.Intersect(line) != SetComparisonResult.Disjoint)
                        {
                            collided.Add(el);
                            return true;
                        }
                    }
                }
                catch { /* ignore per-element failures */ }
            }
            return false;
        }

        static List<Apoint> getNeighbors(int x, int y)
        {
            // Placeholder for neighbor finding logic
            List<Apoint> neighbors = new List<Apoint>();
            int maxCount = 0;
            foreach (var row in grid)
            {
                if (row.Count > maxCount) maxCount = row.Count;
            }
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    // Implement neighbor logic based on grid indices
                    if (x + i > 0 && x + i < grid.Count && y + j > 0 && y + j < maxCount )
                    {
                        var candidate = grid[x + i][y + j];
                        if (candidate.walkable) // skip non-walkable immediately
                            neighbors.Add(candidate);
                    }
                }
            }
            return neighbors;
        }

        static Apoint GetNearest(XYZ point)
        {
            if (point == null) return null;
            if (grid == null || grid.Count == 0 || grid[0].Count == 0) return null;

            Apoint best = null;
            double bestDist2 = double.MaxValue;

            for (int i = 0; i < grid.Count; i++)
            {
                for (int j = 0; j < grid[i].Count; j++)
                {
                    var ap = grid[i][j];
                    double dx = ap.point.X - point.X;
                    double dy = ap.point.Y - point.Y;
                    double d2 = dx * dx + dy * dy;
                    if (d2 < bestDist2)
                    {
                        bestDist2 = d2;
                        best = ap;
                    }
                }
            }

            double maxSearch = 2.0; // feet threshold
            return (best != null && Math.Sqrt(bestDist2) <= maxSearch) ? best : null;
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

        public Apoint parent { get; set; }
        public bool walkable { get; set; }

        public Apoint(XYZ point, int i, int j)
        {
            this.point = point;
            // initialize gCost to a large value so comparisons work properly
            gCost = double.MaxValue;
            hCost = 0;
            this.i = i;
            this.j = j;
            parent = null;
            walkable = true;
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