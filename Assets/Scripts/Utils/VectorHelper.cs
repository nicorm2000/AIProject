using System;
using System.Collections.Generic;
using System.Numerics;

namespace Utils
{
    /// <summary>
    /// Provides helper methods for operations on vectors of various types.
    /// </summary>
    public class VectorHelper<TVector>
    {
        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to multiply.</param>
        /// <param name="scalar">The scalar value to multiply by.</param>
        /// <returns>The resulting vector after multiplication.</returns>
        public static TVector MultiplyVector(TVector vector, float scalar)
        {
            var type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                var v = (Vector2)(object)vector;
                return (TVector)(object)(v * scalar);
            }
            else if (type == typeof(Vector3))
            {
                var v = (Vector3)(object)vector;
                return (TVector)(object)(v * scalar);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The vector to subtract.</param>
        /// <returns>The resulting vector after subtraction.</returns>
        public static TVector SubtractVectors(TVector a, TVector b)
        {
            var type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                var va = (Vector2)(object)a;
                var vb = (Vector2)(object)b;
                return (TVector)(object)(va - vb);
            }
            else if (type == typeof(Vector3))
            {
                var va = (Vector3)(object)a;
                var vb = (Vector3)(object)b;
                return (TVector)(object)(va - vb);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        /// <summary>
        /// Adds multiple vectors together.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <param name="c">An optional third vector.</param>
        /// <param name="d">An optional fourth vector.</param>
        /// <returns>The resulting vector after addition.</returns>
        public static TVector AddVectors(TVector a, TVector b, TVector c = default, TVector d = default)
        {
            var type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                var va = (Vector2)(object)a;
                var vb = (Vector2)(object)b;
                var vc = c != null ? (Vector2)(object)c : Vector2.Zero;
                var vd = d != null ? (Vector2)(object)d : Vector2.Zero;
                return (TVector)(object)(va + vb + vc + vd);
            }
            else if (type == typeof(Vector3))
            {
                var va = (Vector3)(object)a;
                var vb = (Vector3)(object)b;
                var vc = c != null ? (Vector3)(object)c : Vector3.Zero;
                var vd = d != null ? (Vector3)(object)d : Vector3.Zero;
                return (TVector)(object)(va + vb + vc + vd);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        /// <summary>
        /// Divides a vector by a scalar.
        /// </summary>
        /// <param name="vector">The vector to divide.</param>
        /// <param name="scalar">The scalar value to divide by.</param>
        /// <returns>The resulting vector after division.</returns>
        public static TVector DivideVector(TVector vector, int scalar)
        {
            var type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                var v = (Vector2)(object)vector;
                return (TVector)(object)(v / scalar);
            }
            else if (type == typeof(Vector3))
            {
                var v = (Vector3)(object)vector;
                return (TVector)(object)(v / scalar);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        /// <summary>
        /// Normalizes a vector to a unit vector.
        /// </summary>
        /// <param name="vector">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        public static TVector NormalizeVector(TVector vector)
        {
            var type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                var v = (Vector2)(object)vector;
                if (v.Length() == 0)
                {
                    return (TVector)(object)Vector2.Zero; // Avoid division by zero
                }
                return (TVector)(object)Vector2.Normalize(v);
            }
            else if (type == typeof(Vector3))
            {
                var v = (Vector3)(object)vector;
                if (v.Length() == 0)
                {
                    return (TVector)(object)Vector3.Zero; // Avoid division by zero
                }
                return (TVector)(object)Vector3.Normalize(v);
            }

            throw new InvalidOperationException("Unsupported vector type");
        }

        /// <summary>
        /// Retrieves a list of boids that are within a certain radius of a given boid.
        /// </summary>
        /// <param name="boid">The reference boid.</param>
        /// <param name="positionComponents">A dictionary of position components.</param>
        /// <returns>A list of boids inside the specified radius.</returns>
        public static List<PositionComponent<TVector>> GetBoidsInsideRadius(PositionComponent<TVector> boid,
            IDictionary<uint, PositionComponent<TVector>> positionComponents)
        {
            List<PositionComponent<TVector>> insideRadiusBoids = new List<PositionComponent<TVector>>();
            foreach (var otherBoid in positionComponents.Values)
            {
                // Check if the other boid is not the same as the reference boid
                // and if it is within the specified radius
                if (!otherBoid.Equals(boid) && VectorHelper<TVector>.IsWithinRadius(boid.Position, otherBoid.Position))
                {
                    insideRadiusBoids.Add(otherBoid);
                }
            }

            return insideRadiusBoids;
        }

        /// <summary>
        /// Checks if two positions are within a certain radius.
        /// </summary>
        /// <param name="position1">The first position.</param>
        /// <param name="position2">The second position.</param>
        /// <returns>True if within radius, otherwise false.</returns>
        public static bool IsWithinRadius(TVector position1, TVector position2)
        {
            double distanceSquared = 0.0;
            var type = typeof(TVector);

            if (type == typeof(Vector2))
            {
                var p1 = (Vector2)(object)position1;
                var p2 = (Vector2)(object)position2;
                // Calculate the squared distance
                distanceSquared = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            }
            else if (type == typeof(Vector3))
            {
                var p1 = (Vector3)(object)position1;
                var p2 = (Vector3)(object)position2;
                // Calculate the squared distance in 3D space
                distanceSquared = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) +
                                  (p1.Z - p2.Z) * (p1.Z - p2.Z);
            }

            double radiusSquared = 3.0; // Define the radius for checking
            return distanceSquared <= radiusSquared; // Compare with squared radius for efficiency
        }

        /// <summary>
        /// Checks if the given position is valid (not NaN).
        /// </summary>
        /// <param name="newPosition">The position to validate.</param>
        /// <returns>True if valid, otherwise false.</returns>
        public static bool IsValid<TVector>(TVector newPosition)
        {
            var type = typeof(TVector);
            if (type == typeof(Vector2))
            {
                var v = (Vector2)(object)newPosition;
                return !float.IsNaN(v.X) && !float.IsNaN(v.Y);
            }
            else if (type == typeof(Vector3))
            {
                var v = (Vector3)(object)newPosition;
                return !float.IsNaN(v.X) && !float.IsNaN(v.Y) && !float.IsNaN(v.Z);
            }
            throw new InvalidOperationException("Unsupported vector type");
        }
    }
}