using System;
using System.Diagnostics;

namespace NeoGui.Core
{
    public struct Vec3
    {
        public float X, Y, Z;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vec3(Vec2 v, float z = 0)
        {
            X = v.X;
            Y = v.Y;
            Z = z;
        }

        public Vec2 XY => new Vec2(X, Y);
        public Vec2 XZ => new Vec2(X, Z);
        public Vec2 YX => new Vec2(Y, X);
        public Vec2 YZ => new Vec2(Y, Z);
        public Vec2 ZX => new Vec2(Z, X);
        public Vec2 ZY => new Vec2(Z, Y);

        public float this[int i]
        {
            get
            {
                Debug.Assert(i >= 0 && i < 3);
                return i == 0 ? X : (i == 1 ? Y : Z);
            }
            set
            {
                Debug.Assert(i >= 0 && i < 3);
                if (i == 0) {
                    X = value;
                } else if (i == 1) {
                    Y = value;
                } else {
                    Z = value;
                }
            }
        }

        public float Length => (float)Math.Sqrt(SqrLength);
        public float SqrLength => X * X + Y * Y + Z * Z;
        public Vec3 Normalized => this * (1.0f / Length);
        public Vec3 Abs => new Vec3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
        public Vec3 Inverse => new Vec3(1.0f / X, 1.0f / Y, 1.0f / Z);
        public float Angle(Vec3 v) => (float)Math.Acos(Dot(v) / (Length * v.Length));
        public float Dot(Vec3 v) => X * v.X + Y * v.Y + Z * v.Z;
        public Vec3 Cross(Vec3 v) => new Vec3(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator *(Vec3 v, float f) => new Vec3(v.X * f, v.Y * f, v.Z * f);
        public static Vec3 operator *(float f, Vec3 v) => new Vec3(f * v.X, f * v.Y, f * v.Z);
        public static Vec3 operator *(Vec3 a, Vec3 b) => new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Vec3 operator /(Vec3 a, Vec3 b) => new Vec3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        public static Vec3 operator /(Vec3 v, float f) => v * (1.0f / f);

        public static readonly Vec3 Zero = new Vec3(0, 0, 0);
        public static readonly Vec3 UnitScale = new Vec3(1, 1, 1);

        public override string ToString() => $"Vec3({X}, {Y}, {Z})";
    }
}
