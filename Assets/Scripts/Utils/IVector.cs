﻿using System;

namespace Utils
{
    public interface IVector : IEquatable<IVector>
    {
        float X { get; set; }
        float Y { get; set; }

        IVector Normalized();
        float Distance(IVector other);

        public static float Distance(IVector a, IVector b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static MyVector operator *(IVector vector, float scalar)
        {
            return new MyVector(vector.X * scalar, vector.Y * scalar);
        }

        public static MyVector operator +(IVector a, IVector b)
        {
            float x1 = a?.X ?? 0;
            float y1 = a?.Y ?? 0;
            float x2 = b?.X ?? 0;
            float y2 = b?.Y ?? 0;

            return new MyVector(x1 + x2, y1 + y2);
        }

        public static MyVector operator -(IVector a, IVector b)
        {
            return new MyVector(a.X - b.X, a.Y - b.Y);
        }

        public static MyVector operator /(IVector a, int integer)
        {
            return new MyVector(a.X / integer, a.Y / integer);
        }
        
        public static bool operator <(IVector a, IVector b)
        {
            return (a.X < b.X  && a.Y < b.Y );
        }

        public static bool operator >(IVector a, IVector b)
        {
            return (a.X > b.X && a.Y > b.Y);
        }

        static float Dot(IVector a, IVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        
        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
    }

    public class MyVector : IVector, IEquatable<MyVector>
    {
        public float X { get; set; }
        public float Y { get; set; }

        public MyVector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public MyVector()
        {
            X = 0;
            Y = 0;
        }

        public IVector Normalized()
        {
            float magnitude = (float)Math.Sqrt(X * X + Y * Y);
            return new MyVector(X / magnitude, Y / magnitude);
        }

        public float Distance(IVector other)
        {
            return (float)Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
        }

        public static float Distance(IVector a, IVector b)
        {
            return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static MyVector operator +(MyVector a, MyVector b)
        {
            return new MyVector(a.X + b.X, a.Y + b.Y);
        }

        public static MyVector operator -(MyVector a, MyVector b)
        {
            return new MyVector(a.X - b.X, a.Y - b.Y);
        }

        public static MyVector operator /(MyVector a, float scalar)
        {
            return new MyVector(a.X / scalar, a.Y / scalar);
        }

        public static MyVector operator *(MyVector a, float scalar)
        {
            return new MyVector(a.X * scalar, a.Y * scalar);
        }

        public static float Dot(MyVector a, MyVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static MyVector zero()
        {
            return new MyVector(0, 0);
        }

        public static MyVector NoTarget()
        {
            return new MyVector(-1, -1);
        }

        public bool Equals(IVector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MyVector)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public bool Equals(MyVector other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }
    }
}