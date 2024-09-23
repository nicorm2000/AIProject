using System;
using System.Collections.Generic;

namespace Pathfinder.Voronoi
{
    /// <summary>
    /// Represents a segment in a Voronoi diagram defined by an origin, a final point, 
    /// its mediatrix (the perpendicular bisector), and its direction.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of the coordinate.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinate's value type (e.g., float, int).</typeparam>
    public class Segment<TCoordinate, TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        private readonly TCoordinate direction;   // The direction of the segment (a perpendicular vector)
        private readonly TCoordinate mediatrix;   // The perpendicular bisector of the segment

        /// <summary>
        /// Gets the origin point of the segment.
        /// </summary>
        public TCoordinate Origin { get; }

        /// <summary>
        /// Gets the final point of the segment.
        /// </summary>
        public TCoordinate Final { get; }

        /// <summary>
        /// Gets the mediatrix (the perpendicular bisector) of the segment.
        /// </summary>
        public TCoordinate Mediatrix => mediatrix;

        /// <summary>
        /// Gets the direction of the segment, which is a perpendicular vector to the segment.
        /// </summary>
        public TCoordinate Direction => direction;

        /// <summary>
        /// Gets the list of intersection points for the segment.
        /// </summary>
        public List<TCoordinate> Intersections { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Segment{TCoordinate, TCoordinateType}"/> class with 
        /// an origin and a final point. It calculates the mediatrix and direction based on these points.
        /// </summary>
        /// <param name="origin">The origin point of the segment.</param>
        /// <param name="final">The final point of the segment.</param>
        public Segment(TCoordinate origin, TCoordinate final)
        {
            this.Origin = origin;
            this.Final = final;

            // Mediatrix: the perpendicular bisector passing through the midpoint of the segment:
            // 1. Add the X-coordinates of the origin and final points
            // 2. Divide by 2 to get the X-coordinate of the mediatrix
            // 3. Repeat the process for the Y-coordinates
            mediatrix = new TCoordinate();
            mediatrix.SetCoordinate((origin.GetX() + final.GetX()) / 2, (origin.GetY() + final.GetY()) / 2);

            // Calculate the direction of the segment:
            // 1. Calculate the vector from the origin to the final point
            // 2. Get the perpendicular vector to this segment
            direction = new TCoordinate();
            direction.SetCoordinate(final.GetX() - origin.GetX(), final.GetY() - origin.GetY());
            direction.Perpendicular();  // Make the direction vector perpendicular to the original segment
        }
    }
}