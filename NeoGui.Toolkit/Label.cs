using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public struct LabelText { public string Value; }
    public struct LabelColor { public Color Value; }
    public struct LabelAlignment { public TextAlignment Value; }

    public static class Label
    {
        private static readonly LabelText DefaultText = new LabelText {Value = ""};
        private static readonly LabelColor DefaultColor = new LabelColor {Value = Color.Black};
        private static readonly LabelAlignment DefaultAlignment = new LabelAlignment {Value = TextAlignment.Left};

        public static Element Create(Element parent, string text, Color? color = null, TextAlignment? alignment = null)
        {
            var label = Element.Create(parent);
            if (text != null) {
                label.Set(new LabelText { Value = text });
                label.Size = label.Context.Delegate.TextSize(text, 0);
            }
            if (color != null) {
                label.Set(new LabelColor { Value = color.Value });
            }
            if (alignment != null) {
                label.Set(new LabelAlignment { Value = alignment.Value });
            }
            label.Draw = Draw;
            return label;
        }

        public static void Draw(DrawContext dc)
        {
            var label = dc.Target;
            var text = label.Get(DefaultText).Value;
            var color = label.Get(DefaultColor).Value;
            var alignment = label.Get(DefaultAlignment).Value;
            var size = label.Size;
            var textSize = dc.TextSize(text);
            var y = (size.Y - textSize.Y) * 0.5f;
            float x = 0;
            if (alignment == TextAlignment.Center) {
                x = (size.X - textSize.X) * 0.5f;
            }else if (alignment == TextAlignment.Right) {
                x = size.X - textSize.X;
            }
            dc.Text(new Vec2(x, y), text, color);
        }
    }
}
