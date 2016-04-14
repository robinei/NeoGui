using System;

namespace NeoGui.Core
{
    public struct Rect
    {
        public Vec2 Pos;
        public Vec2 Size;

        public float X
        {
            get { return Pos.X; }
            set { Pos.X = value; }
        }
        public float Y
        {
            get { return Pos.Y; }
            set { Pos.Y = value; }
        }
        public float Width
        {
            get { return Size.X; }
            set { Size.X = value; }
        }
        public float Height
        {
            get { return Size.Y; }
            set { Size.Y = value; }
        }

        public Rect(Vec2 pos, Vec2 size)
        {
            Pos = pos;
            Size = size;
        }
        public Rect(Vec2 size)
        {
            Pos = Vec2.Zero;
            Size = size;
        }
        public Rect(float x, float y, float width, float height)
        {
            Pos.X = x;
            Pos.Y = y;
            Size.X = width;
            Size.Y = height;
        }

        public bool Contains(Vec2 p)
        {
            return !(p.X < X || p.Y < Y || p.X > X + Width || p.Y > Y + Height);
        }

        public Rect Intersection(Rect r)
        {
            var x = Math.Max(X, r.X);
            var x1 = Math.Min(X + Width, r.X + r.Width);
            var y = Math.Max(Y, r.Y);
            var y1 = Math.Min(Y + Height, r.Y + r.Height);
            if (x1 >= x && y1 >= y) {
                return new Rect(x, y, x1 - x, y1 - y);
            }
            return Empty;
        }

        public static Rect operator +(Rect r, Vec2 v)
        {
            return  new Rect(r.Pos + v, r.Size);
        }
        public static Rect operator +(Vec2 v, Rect r)
        {
            return  new Rect(v + r.Pos, r.Size);
        }

        public override string ToString()
        {
            return $"Rect({X}, {Y}, {Width}, {Height})";
        }

        public static readonly Rect Empty = new Rect(0, 0, 0, 0);
    }
}
