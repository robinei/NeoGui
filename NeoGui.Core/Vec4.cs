namespace NeoGui.Core;

using System;

public struct Vec4 {
    public float X, Y, Z, W;

    public Vec4(float x, float y, float z, float w) {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4(Vec2 v, float z = 0, float w = 0) {
        X = v.X;
        Y = v.Y;
        Z = z;
        W = w;
    }

    public Vec4(Vec3 v, float w = 0) {
        X = v.X;
        Y = v.Y;
        Z = v.Z;
        W = w;
    }
    
    public readonly Vec2 XY => new(X, Y);
    public readonly Vec3 XYZ => new(X, Y, Z);

    public float this[int i] {
        readonly get => i switch {
            0 => X,
            1 => Y,
            2 => Z,
            3 => W,
            _ => throw new ArgumentOutOfRangeException(nameof(i))
        };
        set {
            switch (i) {
                case 0: X = value; break;
                case 1: Y = value; break;
                case 2: Z = value; break;
                case 3: W = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(i));
            }
        }
    }

    public readonly float Length => (float)Math.Sqrt(SqrLength);
    public readonly float SqrLength => X * X + Y * Y + Z * Z + W * W;
    public readonly Vec4 Normalized => this * (1f / Length);
    public readonly Vec4 Abs => new(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
    public readonly Vec4 Inverse => new(1f / X, 1f / Y, 1f / Z, 1f / W);
    public readonly float Angle(Vec4 v) => (float)Math.Acos(Dot(v) / (Length * v.Length));
    public readonly float Dot(Vec4 v) => X * v.X + Y * v.Y + Z * v.Z + W * v.W;

    public static Vec4 operator +(Vec4 a, Vec4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static Vec4 operator -(Vec4 a, Vec4 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    public static Vec4 operator *(Vec4 v, float f) => new(v.X * f, v.Y * f, v.Z * f, v.W * f);
    public static Vec4 operator *(float f, Vec4 v) => new(f * v.X, f * v.Y, f * v.Z, f * v.W);
    public static Vec4 operator *(Vec4 a, Vec4 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
    public static Vec4 operator /(Vec4 a, Vec4 b) => new(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
    public static Vec4 operator /(Vec4 v, float f) => v * (1f / f);

    public static readonly Vec4 Zero = new(0, 0, 0, 0);
    public static readonly Vec4 UnitX = new(1, 0, 0, 0);
    public static readonly Vec4 UnitY = new(0, 1, 0, 0);
    public static readonly Vec4 UnitZ = new(0, 0, 1, 0);
    public static readonly Vec4 UnitW = new(0, 0, 0, 1);
    public static readonly Vec4 ScaleIdentity = new(1, 1, 1, 1);

    public readonly bool Equals(ref Vec4 v) => X == v.X && Y == v.Y && Z == v.Z && W == v.W;

    public override readonly string ToString() => $"Vec4({X}, {Y}, {Z}, {W})";
}
