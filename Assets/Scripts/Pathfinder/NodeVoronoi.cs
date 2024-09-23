using System;
using UnityEngine;

namespace Pathfinder
{
    /// <summary>
    /// Defines an interface for coordinates that support various operations.
    /// </summary>
    /// <typeparam name="T">The type of the coordinate.</typeparam>
    public interface ICoordinate<T> : IEquatable<T>
        where T : IEquatable<T>
    {
        void Add(T a);
        T Multiply(float b);
        float GetX();
        float GetY();
        void SetX(float x);
        void SetY(float y);
        float Distance(T b);
        float GetMagnitude();
        T GetCoordinate();
        void SetCoordinate(float x, float y);
        void SetCoordinate(T coordinate);
        void Zero();
        void Perpendicular();
    }

    /// <summary>
    /// Represents a node in the Voronoi diagram with coordinates.
    /// Implements ICoordinate for vector operations.
    /// </summary>
    public class NodeVoronoi : ICoordinate<Vector2>, IEquatable<NodeVoronoi>
    {
        private Vector2 coordinate;  // The coordinate of the node

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeVoronoi"/> class with a specific coordinate.
        /// </summary>
        /// <param name="coordinate">The coordinate of the node.</param>
        public NodeVoronoi(Vector2 coordinate)
        {
            this.coordinate = coordinate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeVoronoi"/> class with specified X and Y values.
        /// </summary>
        /// <param name="x">The X-coordinate of the node.</param>
        /// <param name="y">The Y-coordinate of the node.</param>
        public NodeVoronoi(int x, int y)
        {
            coordinate = new Vector2(x, y);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeVoronoi"/> class at the origin (0,0).
        /// </summary>
        public NodeVoronoi()
        {
            coordinate = Vector2.zero;
        }

        /// <summary>
        /// Adds a specified vector to the node's coordinate.
        /// </summary>
        /// <param name="a">The vector to add.</param>
        public void Add(Vector2 a)
        {
            coordinate += a; // Add the vector to the current coordinate
        }

        /// <summary>
        /// Multiplies the node's coordinate by a scalar value.
        /// </summary>
        /// <param name="b">The scalar value to multiply by.</param>
        /// <returns>The resulting scaled vector.</returns>
        public Vector2 Multiply(float b)
        {
            return coordinate * b; // Scale the coordinate
        }

        /// <summary>
        /// Gets the X-coordinate of the node.
        /// </summary>
        /// <returns>The X-coordinate.</returns>
        public float GetX()
        {
            return coordinate.x;
        }

        /// <summary>
        /// Gets the Y-coordinate of the node.
        /// </summary>
        /// <returns>The Y-coordinate.</returns>
        public float GetY()
        {
            return coordinate.y;
        }

        /// <summary>
        /// Sets the X-coordinate of the node.
        /// </summary>
        /// <param name="x">The new X-coordinate.</param>
        public void SetX(float x)
        {
            coordinate.x = x;
        }

        /// <summary>
        /// Sets the Y-coordinate of the node.
        /// </summary>
        /// <param name="y">The new Y-coordinate.</param>
        public void SetY(float y)
        {
            coordinate.y = y;
        }

        /// <summary>
        /// Gets the coordinate of the node.
        /// </summary>
        /// <returns>The coordinate as a Vector2.</returns>
        public Vector2 GetCoordinate()
        {
            return coordinate;
        }

        /// <summary>
        /// Calculates the distance to another coordinate.
        /// </summary>
        /// <param name="b">The other coordinate.</param>
        /// <returns>The distance between the two coordinates.</returns>
        public float Distance(Vector2 b)
        {
            return Vector2.Distance(coordinate, b);
        }

        /// <summary>
        /// Gets the magnitude (length) of the node's coordinate vector.
        /// </summary>
        /// <returns>The magnitude of the vector.</returns>
        public float GetMagnitude()
        {
            return coordinate.magnitude;
        }

        /// <summary>
        /// Sets the coordinate of the node using X and Y values.
        /// </summary>
        /// <param name="x">The new X-coordinate.</param>
        /// <param name="y">The new Y-coordinate.</param>
        public void SetCoordinate(float x, float y)
        {
            this.coordinate = new Vector2(x, y);
        }

        /// <summary>
        /// Sets the coordinate of the node using a Vector2.
        /// </summary>
        /// <param name="coordinate">The new coordinate.</param>
        public void SetCoordinate(Vector2 coordinate)
        {
            this.coordinate = coordinate;
        }

        /// <summary>
        /// Resets the coordinate of the node to the origin (0,0).
        /// </summary>
        public void Zero()
        {
            coordinate = Vector2.zero;
        }

        /// <summary>
        /// Rotates the coordinate vector to find the perpendicular vector.
        /// </summary>
        public void Perpendicular()
        {
            coordinate = new Vector2(-coordinate.y, coordinate.x);
        }

        /// <summary>
        /// Checks if this instance is equal to another Vector2.
        /// </summary>
        /// <param name="other">The other Vector2 to compare with.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public bool Equals(Vector2 other)
        {
            return coordinate.Equals(other);
        }

        /// <summary>
        /// Checks if this instance is equal to another NodeVoronoi instance.
        /// </summary>
        /// <param name="other">The other NodeVoronoi to compare with.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public bool Equals(NodeVoronoi other)
        {
            return coordinate.Equals(other.coordinate);
        }
    }
}