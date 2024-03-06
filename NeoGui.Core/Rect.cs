namespace NeoGui.Core;

using System;

public struct Rect {
    public Vec2 Pos;
    public Vec2 Size;

    public float X { readonly get => Pos.X; set => Pos.X = value; }
    public float Y { readonly get => Pos.Y; set => Pos.Y = value; }
    public float Width { readonly get => Size.X; set => Size.X = value; }
    public float Height { readonly get => Size.Y; set => Size.Y = value; }

    public Rect(Vec2 pos, Vec2 size) {
        Pos = pos;
        Size = size;
    }
    public Rect(float x, float y, float width, float height) {
        Pos = new Vec2(x, y);
        Size = new Vec2(width, height);
    }
    public Rect(Vec2 size) {
        Pos = Vec2.Zero;
        Size = size;
    }
    public Rect(float width, float height) {
        Pos = Vec2.Zero;
        Size = new Vec2(width, height);
    }

    public readonly Vec2 Center => Pos + Size * 0.5f;
    public readonly Vec2 Min => Pos;
    public readonly Vec2 Max => Pos + Size;

    public readonly bool Contains(Vec2 p) => !(p.X < X || p.Y < Y || p.X > X + Width || p.Y > Y + Height);

    public readonly bool Intersects(Rect r) {
        return !(X >= r.X + r.Width ||
                 r.X >= X + Width ||
                 Y >= r.Y + r.Height ||
                 r.Y >= Y + Height);
    }

    public readonly Rect Intersection(Rect r) {
        var x = Math.Max(X, r.X);
        var x1 = Math.Min(X + Width, r.X + r.Width);
        var y = Math.Max(Y, r.Y);
        var y1 = Math.Min(Y + Height, r.Y + r.Height);
        if (x1 >= x && y1 >= y) {
            return new Rect(x, y, x1 - x, y1 - y);
        }
        return Empty;
    }

    public static Rect operator +(Rect r, Vec2 v) => new(r.Pos + v, r.Size);
    public static Rect operator -(Rect r, Vec2 v) => new(r.Pos - v, r.Size);
    public static Rect operator +(Vec2 v, Rect r) => new(v + r.Pos, r.Size);

    public override readonly string ToString() => $"Rect({X}, {Y}, {Width}, {Height})";

    public static Rect Empty => new(0, 0, 0, 0);
}
