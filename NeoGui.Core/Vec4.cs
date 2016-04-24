using System;
using System.Diagnostics;

namespace NeoGui.Core
{
    public struct Vec4
    {
        public float X, Y, Z, W;

        public Vec4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vec4(Vec2 v, float z = 0, float w = 0)
        {
            X = v.X;
            Y = v.Y;
            Z = z;
            W = w;
        }

        public Vec4(Vec3 v, float w = 0)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = w;
        }
        
        public Vec2 XY => new Vec2(X, Y);
        public Vec3 XYZ => new Vec3(X, Y, Z);

        public float this[int i]
        {
            get
            {
                Debug.Assert(i >= 0 && i < 4);
                return i == 0 ? X : (i == 1 ? Y : (i == 2 ? Z : W));
            }
            set
            {
                Debug.Assert(i >= 0 && i < 3);
                if (i == 0) {
                    X = value;
                } else if (i == 1) {
                    Y = value;
                } else if (i == 2) {
                    Z = value;
                } else {
                    W = value;
                }
            }
        }

        public float Length => (float)Math.Sqrt(SqrLength);
        public float SqrLength => X * X + Y * Y + Z * Z + W * W;
        public Vec4 Normalized => this * (1f / Length);
        public Vec4 Abs => new Vec4(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
        public Vec4 Inverse => new Vec4(1f / X, 1f / Y, 1f / Z, 1f / W);
        public float Angle(Vec4 v) => (float)Math.Acos(Dot(v) / (Length * v.Length));
        public float Dot(Vec4 v) => X * v.X + Y * v.Y + Z * v.Z + W * v.W;

        public static Vec4 operator +(Vec4 a, Vec4 b) => new Vec4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        public static Vec4 operator -(Vec4 a, Vec4 b) => new Vec4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        public static Vec4 operator *(Vec4 v, float f) => new Vec4(v.X * f, v.Y * f, v.Z * f, v.W * f);
        public static Vec4 operator *(float f, Vec4 v) => new Vec4(f * v.X, f * v.Y, f * v.Z, f * v.W);
        public static Vec4 operator *(Vec4 a, Vec4 b) => new Vec4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        public static Vec4 operator /(Vec4 a, Vec4 b) => new Vec4(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
        public static Vec4 operator /(Vec4 v, float f) => v * (1f / f);

        public static Vec4 Zero => new Vec4(0, 0, 0, 0);
        public static Vec4 UnitX => new Vec4(1, 0, 0, 0);
        public static Vec4 UnitY => new Vec4(0, 1, 0, 0);
        public static Vec4 UnitZ => new Vec4(0, 0, 1, 0);
        public static Vec4 UnitW => new Vec4(0, 0, 0, 1);
        public static Vec4 ScaleIdentity => new Vec4(1, 1, 1, 1);

        public override string ToString() => $"Vec4({X}, {Y}, {Z}, {W})";
    }
}
