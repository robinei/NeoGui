using System;

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

        public float Length => (float)Math.Sqrt(SqrLength);
        public float SqrLength => X * X + Y * Y;

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X + b.X, a.Y + b.Y);
        }
        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2 operator *(Vec2 v, float f)
        {
            return new Vec2(v.X * f, v.Y * f);
        }
        public static Vec2 operator *(float f, Vec2 v)
        {
            return new Vec2(f * v.X, f * v.Y);
        }

        public static readonly Vec2 Zero = new Vec2(0, 0);

        public override string ToString()
        {
            return $"Vec2({X}, {Y})";
        }
    }
}