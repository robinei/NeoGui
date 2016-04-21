using System;
using System.Diagnostics;

namespace NeoGui.Core
{
    public struct Vec2
    {
        public float X, Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float this[int i]
        {
            get
            {
                Debug.Assert(i >= 0 && i < 2);
                return i == 0 ? X : Y;
            }
            set
            {
                Debug.Assert(i >= 0 && i < 2);
                if (i == 0) {
                    X = value;
                } else {
                    Y = value;
                }
            }
        }

        public float Length => (float)Math.Sqrt(SqrLength);
        public float SqrLength => X * X + Y * Y;
        public Vec2 Normalized => this * (1.0f / Length);
        public Vec2 Abs => new Vec2(Math.Abs(X), Math.Abs(Y));
        public Vec2 Inverse => new Vec2(1.0f / X, 1.0f / Y);
        public float Angle(Vec2 v) => (float)Math.Acos(Dot(v) / (Length * v.Length));
        public float Dot(Vec2 v) => X * v.X + Y * v.Y;

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 v, float f) => new Vec2(v.X * f, v.Y * f);
        public static Vec2 operator *(float f, Vec2 v) => new Vec2(f * v.X, f * v.Y);
        public static Vec2 operator *(Vec2 a, Vec2 b) => new Vec2(a.X * b.X, a.Y * b.Y);
        public static Vec2 operator /(Vec2 a, Vec2 b) => new Vec2(a.X / b.X, a.Y / b.Y);
        public static Vec2 operator /(Vec2 v, float f) => v * (1.0f / f);

        public static readonly Vec2 Zero = new Vec2(0, 0);

        public override string ToString() => $"Vec2({X}, {Y})";
    }
}