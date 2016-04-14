using System.Diagnostics;
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
        static DrawCommand()
        {
            Debug.Assert(Marshal.SizeOf(typeof(ComparisonHelper)) >= Marshal.SizeOf(typeof(DrawCommand)));
        }

        [FieldOffset(0)]
        private ComparisonHelper cmp;

        [FieldOffset(0)]
        public DrawCommandType Type;

        [FieldOffset(4)]
        public Rect ClipRect;

        [FieldOffset(20)]
        public SolidRectCommand SolidRect;

        [FieldOffset(20)]
        public GradientRectCommand GradientRect;

        [FieldOffset(20)]
        public TexturedRectCommand TexturedRect;

        [FieldOffset(20)]
        public TextCommand Text;

        public static bool AreEqual(ref DrawCommand a, ref DrawCommand b)
        {
            return a.cmp._0 == b.cmp._0 &&
                   a.cmp._8 == b.cmp._8 &&
                   a.cmp._16 == b.cmp._16 &&
                   a.cmp._24 == b.cmp._24 &&
                   a.cmp._32 == b.cmp._32 &&
                   a.cmp._40 == b.cmp._40 &&
                   a.cmp._48 == b.cmp._48;
        }

        private struct ComparisonHelper
        {
#pragma warning disable 649
            // ReSharper disable InconsistentNaming
            public long _0;
            public long _8;
            public long _16;
            public long _24;
            public long _32;
            public long _40;
            public long _48;
            // ReSharper restore InconsistentNaming
#pragma warning restore 649
        }
    }
}
