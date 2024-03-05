namespace NeoGui.Core;

using System;
using System.Diagnostics;

public struct Vec2 {
    public float X, Y;

    public Vec2(float x, float y) {
        X = x;
        Y = y;
    }

    public float this[int i] {
        readonly get {
            Debug.Assert(i >= 0 && i < 2);
            return i == 0 ? X : Y;
        }
        set {
            Debug.Assert(i >= 0 && i < 2);
            if (i == 0) {
                X = value;
            } else {
                Y = value;
            }
        }
    }

    public readonly float Length => (float)Math.Sqrt(SqrLength);
    public readonly float SqrLength => X * X + Y * Y;
    public readonly Vec2 Normalized => this * (1f / Length);
    public readonly Vec2 Abs => new(Math.Abs(X), Math.Abs(Y));
    public readonly Vec2 Inverse => new(1f / X, 1f / Y);
    public readonly float Angle(Vec2 v) => (float)Math.Acos(Dot(v) / (Length * v.Length));
    public readonly float Dot(Vec2 v) => X * v.X + Y * v.Y;

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 v, float f) => new(v.X * f, v.Y * f);
    public static Vec2 operator *(float f, Vec2 v) => new(f * v.X, f * v.Y);
    public static Vec2 operator *(Vec2 a, Vec2 b) => new(a.X * b.X, a.Y * b.Y);
    public static Vec2 operator /(Vec2 a, Vec2 b) => new(a.X / b.X, a.Y / b.Y);
    public static Vec2 operator /(Vec2 v, float f) => v * (1f / f);

    public static Vec2 Zero => new(0, 0);
    public static Vec2 UnitX => new(1, 0);
    public static Vec2 UnitY => new(0, 1);
    public static Vec2 ScaleIdentity => new(1, 1);

    public override readonly string ToString() => $"Vec2({X}, {Y})";
}
