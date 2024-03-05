namespace NeoGui;

using Microsoft.Xna.Framework;
using NeoGui.Core;
using Color = Microsoft.Xna.Framework.Color;

public static class Extensions {
    public static Color ToMonoGameColor(this Core.Color c) {
        return new Color(c.R, c.G, c.B, c.A);
    }

    public static Rectangle ToMonoGameRectangle(this Rect r) {
        return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
    }

    public static Vector2 ToMonoGameVector2(this Vec2 c) {
        return new Vector2((int)c.X, (int)c.Y);
    }
}
