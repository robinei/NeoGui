using System.Runtime.InteropServices;

namespace NeoGui.Core
{
    public enum DrawCommandType
    {
        SolidRect,
        GradientRect,
        TexturedRect,
        Text
    }

    public struct SolidRectCommand
    {
        public Rect Rect;
        public Color Color;
    }

    public struct GradientRectCommand
    {
        public Rect Rect;
        public Color Color0, Color1;
        public bool Vertical;
    }

    public struct TexturedRectCommand
    {
        public Rect Rect;
        public Rect SourceRect;
        public int TextureId;
    }

    public struct TextCommand
    {
        public Vec2 Vec2;
        public int StringId;
        public int FontId;
        public Color Color;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DrawCommand
    {
        [FieldOffset(0)]
        public DrawCommandType Type;

        [FieldOffset(4)]
        public SolidRectCommand SolidRect;

        [FieldOffset(4)]
        public GradientRectCommand GradientRect;

        [FieldOffset(4)]
        public TexturedRectCommand TexturedRect;

        [FieldOffset(4)]
        public TextCommand Text;
    }
}
