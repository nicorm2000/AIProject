using System;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;

namespace Utils
{
    /// <summary>
    /// Represents a 2D vector with integer components.
    /// </summary>
    public class Vec2Int : IEquatable<Vec2Int>
    {
        private int m_X; // X component of the vector
        private int m_Y; // Y component of the vector

        /// <summary>
        /// Gets or sets the X component of the vector.
        /// </summary>
        public int x
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.m_X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.m_X = value;
        }

        /// <summary>
        /// Gets or sets the Y component of the vector.
        /// </summary>
        public int y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.m_Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.m_Y = value;
        }

        /// <summary>
        /// Initializes a new instance of the Vec2Int class with the specified X and Y components.
        /// </summary>
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2Int(int x, int y)
        {
            this.m_X = x;
            this.m_Y = y;
        }

        /// <summary>
        /// Gets the magnitude (length) of the vector.
        /// </summary>
        public float magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Calculate the magnitude using the Pythagorean theorem
                return (float)Math.Sqrt((float)(this.x * this.x + this.y * this.y));
            }
        }

        /// <summary>
        /// Gets the squared length of this vector (Read Only).
        /// </summary>
        public int sqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.x * this.x + this.y * this.y;
        }

        /// <summary>
        /// Returns the distance between two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vec2Int a, Vec2Int b)
        {
            // Calculate the differences in each component
            float num1 = (float)(a.x - b.x);
            float num2 = (float)(a.y - b.y);
            // Return the Euclidean distance
            return (float)Math.Sqrt((double)num1 * (double)num1 + (double)num2 * (double)num2);
        }

        /// <summary>
        /// Converts a Vec2Int to a Vector2.
        /// </summary>
        /// <param name="v">The Vec2Int to convert.</param>
        public static implicit operator Vector2(Vec2Int v) => new Vector2((float)v.x, (float)v.y);

        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="v">The vector to negate.</param>
        public static Vec2Int operator -(Vec2Int v) => new Vec2Int(-v.x, -v.y);

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Vec2Int operator +(Vec2Int a, Vec2Int b)
        {
            // Return a new vector with summed components
            return new Vec2Int(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The vector to subtract.</param>
        public static Vec2Int operator -(Vec2Int a, Vec2Int b)
        {
            // Return a new vector with subtracted components
            return new Vec2Int(a.x - b.x, a.y - b.y);
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Vec2Int operator *(Vec2Int a, Vec2Int b)
        {
            // Return a new vector with multiplied components
            return new Vec2Int(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// Multiplies a vector by a scalar (int) from the left.
        /// </summary>
        /// <param name="a">The scalar to multiply.</param>
        /// <param name="b">The vector to multiply.</param>
        public static Vec2Int operator *(int a, Vec2Int b) => new Vec2Int(a * b.x, a * b.y);

        /// <summary>
        /// Multiplies a vector by a scalar (int) from the right.
        /// </summary>
        /// <param name="a">The vector to multiply.</param>
        /// <param name="b">The scalar to multiply.</param>
        public static Vec2Int operator *(Vec2Int a, int b) => new Vec2Int(a.x * b, a.y * b);

        /// <summary>
        /// Divides a vector by a scalar (int).
        /// </summary>
        /// <param name="a">The vector to divide.</param>
        /// <param name="b">The scalar to divide by.</param>
        public static Vec2Int operator /(Vec2Int a, int b) => new Vec2Int(a.x / b, a.y / b);

        /// <summary>
        /// Determines whether two vectors are equal.
        /// </summary>
        /// <param name="lhs">The first vector.</param>
        /// <param name="rhs">The second vector.</param>
        public static bool operator ==(Vec2Int lhs, Vec2Int rhs)
        {
            // Return true if both components are equal
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        /// <summary>
        /// Determines whether two vectors are not equal.
        /// </summary>
        /// <param name="lhs">The first vector.</param>
        /// <param name="rhs">The second vector.</param>
        public static bool operator !=(Vec2Int lhs, Vec2Int rhs) => !(lhs == rhs);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is Vec2Int other1 && this.Equals(other1);

        /// <summary>
        /// Determines whether the specified Vec2Int is equal to the current Vec2Int.
        /// </summary>
        /// <param name="other">The Vec2Int to compare with the current instance.</param>
        /// <returns>true if the specified Vec2Int is equal to the current Vec2Int; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vec2Int other) => this.x == other.x && this.y == other.y;

        /// <summary>
        /// Returns a string that represents the current Vec2Int.
        /// </summary>
        /// <returns>A string representation of the Vec2Int.</returns>
        public override string ToString() => $"({this.x}, {this.y})";
    }
}