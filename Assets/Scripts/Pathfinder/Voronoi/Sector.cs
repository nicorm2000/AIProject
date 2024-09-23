using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using VoronoiDiagram;

namespace Pathfinder.Voronoi
{
    /// <summary>
    /// Represents a sector defined by segments and intersections.
    /// </summary>
    /// <typeparam name="TCoordinate">Type of the coordinate.</typeparam>
    /// <typeparam name="TCoordinateType">Type of the coordinate value.</typeparam>
    public class Sector<TCoordinate, TCoordinateType>
        where TCoordinate : IEquatable<TCoordinate>, ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        private readonly List<Segment<TCoordinate, TCoordinateType>> segments = new();
        private readonly List<TCoordinate> intersections = new();
        private List<Node<TCoordinate>> nodesInsideSector = new();
        private List<TCoordinate> points;
        private static TCoordinate _wrongPoint;

        public Node<TCoordinateType> Mine { get; }

        public Sector(Node<TCoordinateType> mine)
        {
            _wrongPoint = new TCoordinate();
            _wrongPoint.SetCoordinate(-1, -1);
            this.Mine = mine;
        }

        #region SEGMENTS

        /// <summary>
        /// Adds segment limits based on the provided limits to define the boundaries of the sector.
        /// </summary>
        /// <param name="limits">A list of limits that define the boundaries.</param>
        public void AddSegmentLimits(List<Limit<TCoordinate, TCoordinateType>> limits)
        {
            // Calculate segments using the map limits
            foreach (var limit in limits)
            {
                TCoordinate origin = new TCoordinate();
                origin.SetCoordinate(Mine.GetCoordinate()); // Get the position of the mine
                TCoordinate final = limit.GetMapLimitPosition(origin); // Get the final position of the segment
                segments.Add(new Segment<TCoordinate, TCoordinateType>(origin, final));
            }
        }

        /// <summary>
        /// Adds a segment defined by its origin and final coordinates.
        /// </summary>
        /// <param name="origin">The starting point of the segment.</param>
        /// <param name="final">The ending point of the segment.</param>
        public void AddSegment(TCoordinate origin, TCoordinate final)
        {
            segments.Add(new Segment<TCoordinate, TCoordinateType>(origin, final));
        }

        #endregion

        #region INTERSECTION

        /// <summary>
        /// Sets the intersections between segments, validating them and determining their positions.
        /// </summary>
        public void SetIntersections()
        {
            intersections.Clear();

            // Calculate intersections between each segment (excluding itself)
            for (int i = 0; i < segments.Count; i++)
            {
                for (int j = 0; j < segments.Count; j++)
                {
                    if (i == j) continue;

                    // Get the intersection point
                    TCoordinate intersectionPoint = GetIntersection(segments[i], segments[j]);

                    if (intersectionPoint.Equals(_wrongPoint)) continue;

                    // Check if this intersection already exists
                    if (intersections.Contains(intersectionPoint)) continue;

                    // Calculate the maximum distance between the intersection and the origin point of the segment
                    float maxDistance = intersectionPoint.Distance(segments[i].Origin.GetCoordinate());

                    // Determine if the intersection is valid
                    bool checkValidPoint = false;
                    for (int k = 0; k < segments.Count; k++)
                    {
                        // Check all segments again to verify if another segment is closer to the intersection than the current segment
                        // Each intersection must represent a point where the two segments are the closest
                        if (k == i || k == j) continue;
                        if (CheckIfHaveAnotherPositionCloser(intersectionPoint, segments[k].Final, maxDistance))
                        {
                            checkValidPoint = true; // Intersection is invalid
                            break;
                        }
                    }

                    if (!checkValidPoint)
                    {
                        intersections.Add(intersectionPoint);
                        segments[i].Intersections.Add(intersectionPoint);
                        segments[j].Intersections.Add(intersectionPoint);
                    }
                }
            }

            // Each segment must have exactly two intersections with other segments; otherwise, it is invalid
            segments.RemoveAll((s) => s.Intersections.Count != 2);

            // Sort the intersections according to their angle with respect to a given center
            // This is necessary to ensure segments connect properly and avoid errors
            SortIntersections();

            // Create a set of points to define the boundaries of the sectors
            SetPointsInSector();
        }

        /// <summary>
        /// Calculates the intersection point of two segments.
        /// </summary>
        /// <param name="seg1">The first segment.</param>
        /// <param name="seg2">The second segment.</param>
        /// <returns>The intersection point, or a predefined point indicating no intersection.</returns>
        private TCoordinate GetIntersection(Segment<TCoordinate, TCoordinateType> seg1, Segment<TCoordinate, TCoordinateType> seg2)
        {
            TCoordinate intersection = new TCoordinate();
            intersection.Zero();

            // Define the endpoints of the first segment
            TCoordinate p1 = seg1.Mediatrix;
            TCoordinate p2 = new TCoordinate();
            p2.SetCoordinate(seg1.Mediatrix.GetCoordinate());
            p2.Add(seg1.Direction.Multiply(MapGenerator<TCoordinate, TCoordinateType>.MapDimensions.GetMagnitude()));

            // Define the endpoints of the second segment
            TCoordinate p3 = seg2.Mediatrix;
            TCoordinate p4 = new TCoordinate();
            p4.SetCoordinate(seg2.Mediatrix.GetCoordinate());
            p4.Add(seg2.Direction.Multiply(MapGenerator<TCoordinate, TCoordinateType>.MapDimensions.GetMagnitude()));

            // Check if the two segments are parallel; if so, there is no intersection
            float denominator = (p1.GetX() - p2.GetX()) * (p3.GetY() - p4.GetY()) - (p1.GetY() - p2.GetY()) * (p3.GetX() - p4.GetX());

            if (Approximately(denominator, 0))
            {
                return _wrongPoint; // Indicate no intersection
            }
            else
            {
                // Calculate the intersection point using line intersection formulas:
                /*
                   1. Line equations:                             Ax + By = C1      and      Ax + By = C2
                   2. Calculate the main determinant (D):          D = A1 * B2 - A2 * B1
                   3. Calculate determinant x (Dx):                Dx = C1 * B2 - C2 * B1
                   4. Calculate determinant y (Dy):                Dy = A1 * C2 - A2 * C1
                   5. Coordinates of the intersection point (x, y): x = Dx / D      and      y = Dy / D

                   A1 = p1.x - p2.x
                   B1 = p1.y - p2.y
                   A2 = p3.x - p4.x
                   B2 = p3.y - p4.y
                   C1 = p1.x * p2.y - p1.y * p2.x
                   C2 = p3.x * p4.y - p3.y * p4.x
                */
                float C1 = p1.GetX() * p2.GetY() - p1.GetY() * p2.GetX();
                float C2 = p3.GetX() * p4.GetY() - p3.GetY() * p4.GetX();

                intersection.SetX((C1 * (p3.GetX() - p4.GetX()) - (p1.GetX() - p2.GetX()) * C2) / denominator);
                intersection.SetY((C1 * (p3.GetY() - p4.GetY()) - (p1.GetY() - p2.GetY()) * C2) / denominator);
                return intersection;
            }
        }


        /// <summary>
        /// Checks if two floating-point numbers are approximately equal within a small tolerance.
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>True if the numbers are approximately equal; otherwise, false.</returns>
        private bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }

        /// <summary>
        /// Checks if there is another position closer to the intersection point than the maximum distance allowed.
        /// </summary>
        /// <param name="intersectionPoint">The intersection point to check.</param>
        /// <param name="pointEnd">The endpoint of the segment.</param>
        /// <param name="maxDistance">The maximum distance allowed from the intersection point.</param>
        /// <returns>True if there is a closer position; otherwise, false.</returns>
        private bool CheckIfHaveAnotherPositionCloser(TCoordinate intersectionPoint, TCoordinate pointEnd,
            float maxDistance)
        {
            return (intersectionPoint.Distance(pointEnd.GetCoordinate()) < maxDistance);
        }

        /// <summary>
        /// Sorts the intersections based on their angle relative to the centroid of the intersection points.
        /// </summary>
        private void SortIntersections()
        {
            List<IntersectionPoint<TCoordinate>> intersectionPoints =
                intersections.Select(coord => new IntersectionPoint<TCoordinate>(coord)).ToList();

            // Calculate the minimum and maximum X and Y values of the intersections to determine the centroid
            float minX = intersectionPoints[0].Position.GetX();
            float maxX = intersectionPoints[0].Position.GetX();
            float minY = intersectionPoints[0].Position.GetY();
            float maxY = intersectionPoints[0].Position.GetY();

            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersectionPoints[i].Position.GetX() < minX) minX = intersectionPoints[i].Position.GetX();
                if (intersectionPoints[i].Position.GetX() > maxX) maxX = intersectionPoints[i].Position.GetX();
                if (intersectionPoints[i].Position.GetY() < minY) minY = intersectionPoints[i].Position.GetY();
                if (intersectionPoints[i].Position.GetY() > maxY) maxY = intersectionPoints[i].Position.GetY();
            }

            // Determine the center (centroid) of the intersection points
            TCoordinate center = new TCoordinate();
            center.SetCoordinate(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);

            // Calculate the angle of each intersection relative to the centroid
            // The angle is calculated in radians between the intersection point and the horizontal axis
            foreach (var coord in intersectionPoints)
            {
                TCoordinate pos = coord.Position;
                coord.Angle = (float)Math.Acos((pos.GetX() - center.GetX()) /
                                               Math.Sqrt(Math.Pow(pos.GetX() - center.GetX(), 2f) +
                                                         Math.Pow(pos.GetY() - center.GetY(), 2f)));

                // If the Y coordinate of the intersection is greater than the Y coordinate of the center,
                // adjust the angle to ensure it is in the correct range (0 to 2π radians)
                if (pos.GetY() > center.GetY())
                    coord.Angle = (float)(Math.PI + Math.PI - coord.Angle);
            }

            // Sort the intersections based on their angles (ascending, counter-clockwise)
            intersectionPoints = intersectionPoints.OrderBy(p => p.Angle).ToList();
            intersections.Clear();

            // Add the sorted intersections back to the list
            foreach (var coord in intersectionPoints)
            {
                intersections.Add(coord.Position);
            }
        }

        /// <summary>
        /// Sets the points in the sector based on the intersections.
        /// </summary>
        private void SetPointsInSector()
        {
            points = new List<TCoordinate>();
            foreach (var coord in intersections)
            {
                // Assign each intersection as a point in the sector
                points.Add(coord);
            }

            // Create an additional point equal to the first point to complete the boundary of the last sector
            points.Add(points[0]);
        }

        #endregion

        /// <summary>
        /// Checks if a given position is inside the sector defined by the points.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is inside the sector; otherwise, false.</returns>
        public bool CheckPointInSector(TCoordinate position) // Calculate if "position" is within a sector of the diagram
        {
            if (points == null) return false;

            bool inside = false;

            // Initialize "point" with the last point (^1) in the "points" array
            TCoordinate point = new TCoordinate();
            point.SetCoordinate(points[^1].GetCoordinate());

            foreach (var coord in points)
            {
                // Store the X and Y values of the previous point and the current point
                float previousX = point.GetX();
                float previousY = point.GetY();
                point.SetCoordinate(coord.GetCoordinate());

                // The operator ^ toggles the value of the bool
                // Check if "position" crosses a line formed by two consecutive points in the polygon:
                // 1. Check if "position" is below the current and previous points on the vertical axis (1 comparison is V = V)
                // 2. Check if "position" is to the left of the line connecting the current and previous points
                bool condition1 = point.GetY() > position.GetY() ^ previousY > position.GetY();
                bool condition2 = (position.GetX() - point.GetX()) <
                                  (position.GetY() - point.GetY()) * (previousX - point.GetX()) /
                                  (previousY - point.GetY());

                // If both conditions are true, the point is outside the polygon
                inside ^= condition1 && condition2;
            }

            return inside;
        }

        /// <summary>
        /// Retrieves a list of nodes that are located within the sector.
        /// </summary>
        /// <param name="allNodes">The list of all nodes to check against.</param>
        /// <returns>A list of nodes that are within the sector.</returns>
        public List<Node<TCoordinate>> GetNodesInSector(List<Node<TCoordinate>> allNodes)
        {
            List<Node<TCoordinate>> nodesInSector = new List<Node<TCoordinate>>();

            foreach (Node<TCoordinate> node in allNodes)
            {
                // Check if the node's coordinate is inside the sector
                if (CheckPointInSector(node.GetCoordinate()))
                {
                    nodesInSector.Add(node);
                }
            }

            return nodesInSector;
        }

        /// <summary>
        /// Calculates the total weight of the nodes within the sector.
        /// </summary>
        /// <param name="nodesInSector">The list of nodes in the sector.</param>
        /// <returns>The total weight of the nodes.</returns>
        public int CalculateTotalWeight(List<Node<TCoordinate>> nodesInSector)
        {
            int totalWeight = 0;

            //foreach (Node<TCoordinate> node in nodesInSector)
            //{
            //    // totalWeight += node.GetPathNodeCost();
            //    totalWeight += 1; // Placeholder: each node currently contributes a weight of 1
            //}

            return totalWeight;
        }

        /// <summary>
        /// Converts the points in the sector to an array for drawing purposes.
        /// </summary>
        /// <returns>An array of points in the sector.</returns>
        public TCoordinate[] PointsToDraw()
        {
            return points.ToArray();
        }
    }
}