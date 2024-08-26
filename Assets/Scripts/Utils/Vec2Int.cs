using System;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;

namespace Utils
{
    public class Vec2Int
    {
        private int m_X;
        private int m_Y;

        public int x
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.m_X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.m_X = value;
        }

        public int y
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.m_Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.m_Y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec2Int(int x, int y)
        {
            this.m_X = x;
            this.m_Y = y;
        }

        public float magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (float)Math.Sqrt((float)(this.x * this.x + this.y * this.y));
            }
        }

        public int sqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.x * this.x + this.y * this.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vec2Int a, Vec2Int b)
        {
            float num1 = (float)(a.x - b.x);
            float num2 = (float)(a.y - b.y);
            return (float)Math.Sqrt((double)num1 * (double)num1 + (double)num2 * (double)num2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Vec2Int v) => new Vector2((float)v.x, (float)v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator -(Vec2Int v) => new Vec2Int(-v.x, -v.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator +(Vec2Int a, Vec2Int b)
        {
            return new Vec2Int(a.x + b.x, a.y + b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator -(Vec2Int a, Vec2Int b)
        {
            return new Vec2Int(a.x - b.x, a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator *(Vec2Int a, Vec2Int b)
        {
            return new Vec2Int(a.x * b.x, a.y * b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator *(int a, Vec2Int b) => new Vec2Int(a * b.x, a * b.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator *(Vec2Int a, int b) => new Vec2Int(a.x * b, a.y * b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2Int operator /(Vec2Int a, int b) => new Vec2Int(a.x / b, a.y / b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vec2Int lhs, Vec2Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vec2Int lhs, Vec2Int rhs) => !(lhs == rhs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is Vec2Int other1 && this.Equals(other1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vec2Int other) => this.x == other.x && this.y == other.y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => this.ToString((string)null, (IFormatProvider)null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider) => this.ToString(format, (IFormatProvider)null);
    }
}