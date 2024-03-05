namespace NeoGui.Core;

using System;
using System.Diagnostics;

public struct Vec3 {
    public float X, Y, Z;

    public Vec3(float x, float y, float z) {
        X = x;
        Y = y;
        Z = z;
    }

    public Vec3(Vec2 v, float z = 0) {
        X = v.X;
        Y = v.Y;
        Z = z;
    }

    public readonly Vec2 XY => new(X, Y);

    public float this[int i] {
        readonly get {
            Debug.Assert(i >= 0 && i < 3);
            return i == 0 ? X : (i == 1 ? Y : Z);
        }
        set {
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

    public readonly float Length => (float)Math.Sqrt(SqrLength);
    public readonly float SqrLength => X * X + Y * Y + Z * Z;
    public readonly Vec3 Normalized => this * (1f / Length);
    public readonly Vec3 Abs => new(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    public readonly Vec3 Inverse => new(1f / X, 1f / Y, 1f / Z);
    public readonly float Angle(Vec3 v) => (float)Math.Acos(Dot(v) / (Length * v.Length));
    public readonly float Dot(Vec3 v) => X * v.X + Y * v.Y + Z * v.Z;
    public readonly Vec3 Cross(Vec3 v) => new(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);

    public static Vec3 operator +(Vec3 a, Vec3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vec3 operator *(Vec3 v, float f) => new(v.X * f, v.Y * f, v.Z * f);
    public static Vec3 operator *(float f, Vec3 v) => new(f * v.X, f * v.Y, f * v.Z);
    public static Vec3 operator *(Vec3 a, Vec3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    public static Vec3 operator /(Vec3 a, Vec3 b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    public static Vec3 operator /(Vec3 v, float f) => v * (1f / f);

    public static Vec3 Zero => new(0, 0, 0);
    public static Vec3 UnitX => new(1, 0, 0);
    public static Vec3 UnitY => new(0, 1, 0);
    public static Vec3 UnitZ => new(0, 0, 1);
    public static Vec3 ScaleIdentity => new(1, 1, 1);

    public override readonly string ToString() => $"Vec3({X}, {Y}, {Z})";
}
