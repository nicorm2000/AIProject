using System;

namespace Pathfinder.Voronoi
{
    /// <summary>
    /// Defines the possible directions for map limits.
    /// </summary>
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    /// <summary>
    /// Represents a boundary or limit on the map in the Voronoi diagram.
    /// </summary>
    /// <typeparam name="TCoordinate">The type of coordinate used.</typeparam>
    /// <typeparam name="TCoordinateType">The type of the coordinate's value type (e.g., float, int).</typeparam>
    public class Limit<TCoordinate, TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        private TCoordinate origin;  // The origin point of the limit
        private readonly Direction direction;  // The direction of the limit (Up, Down, Left, Right)

        /// <summary>
        /// Initializes a new instance of the <see cref="Limit{TCoordinate, TCoordinateType}"/> class.
        /// </summary>
        /// <param name="origin">The origin coordinate for the limit.</param>
        /// <param name="direction">The direction of the limit (Up, Down, Left, Right).</param>
        public Limit(TCoordinate origin, Direction direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        /// <summary>
        /// Calculates the position of the map limit based on the given position and the direction of the limit.
        /// </summary>
        /// <param name="position">The current position of the point to calculate the limit for.</param>
        /// <returns>A new coordinate representing the limit based on the position and direction.</returns>
        public TCoordinate GetMapLimitPosition(TCoordinate position)
        {
            // Calculation of the distance to the limit:
            // 1. Calculate the distance between the current position and the origin of the limit.
            // 2. Take the absolute value to ensure a positive distance.
            // 3. Multiply this distance by 2 to extend the limit beyond the original distance.
            // Compute the distance between the given position and the origin of the limit
            TCoordinate distance = new TCoordinate();
            distance.SetCoordinate(
                Math.Abs(position.GetX() - origin.GetX()) * 2f, // Extend distance on the X-axis
                Math.Abs(position.GetY() - origin.GetY()) * 2f  // Extend distance on the Y-axis
            );

            // Start with the given position as the limit
            TCoordinate limit = new TCoordinate();
            limit.SetCoordinate(position.GetCoordinate());

            // Adjust the limit position based on the direction
            switch (direction)
            {
                case Direction.Left:
                    limit.SetX(position.GetX() - distance.GetX()); // Move left by the calculated distance
                    break;
                case Direction.Up:
                    limit.SetY(position.GetY() + distance.GetY()); // Move up by the calculated distance
                    break;
                case Direction.Right:
                    limit.SetX(position.GetX() + distance.GetX()); // Move right by the calculated distance
                    break;
                case Direction.Down:
                    limit.SetY(position.GetY() - distance.GetY()); // Move down by the calculated distance
                    break;
            }

            return limit;
        }
    }
}