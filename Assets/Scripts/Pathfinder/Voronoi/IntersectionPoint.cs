using System;

namespace VoronoiDiagram
{
    /// <summary>
    /// Represents an intersection point in the Voronoi diagram with a position and an angle.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of the coordinate used for the intersection point.</typeparam>
    public class IntersectionPoint<TCoordinate>
        where TCoordinate : IEquatable<TCoordinate>
    {
        private TCoordinate position;  // The position of the intersection point
        private float angle;  // The angle of the intersection point

        /// <summary>
        /// Gets the position of the intersection point.
        /// </summary>
        public TCoordinate Position { get => position; }

        /// <summary>
        /// Gets or sets the angle associated with the intersection point.
        /// </summary>
        public float Angle { get => angle; set => angle = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntersectionPoint{TCoordinate}"/> class with the specified position.
        /// The angle is initialized to 0.
        /// </summary>
        /// <param name="position">The position of the intersection point.</param>
        public IntersectionPoint(TCoordinate position)
        {
            this.position = position;
            angle = 0f;  // Default angle is set to 0
        }
    }
}