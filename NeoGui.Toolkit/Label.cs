using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public struct LabelText { public string Value; }
    public struct LabelColor { public Color Value; }

    public static class Label
    {
        private static readonly LabelText DefaultText = new LabelText {Value = ""};
        private static readonly LabelColor DefaultColor = new LabelColor {Value = Color.White};

        public static Element Create(Element parent, string text, Color? color = null)
        {
            var label = Element.Create(parent);
            if (text != null) {
                label.Set(new LabelText { Value = text });
            }
            if (color != null) {
                label.Set(new LabelColor { Value = color.Value });
            }
            label.Draw = Draw;
            return label;
        }

        public static void Draw(DrawContext dc)
        {
            var label = dc.Target;
            var text = label.Get(DefaultText).Value;
            var color = label.Get(DefaultColor).Value;
            var size = label.Size;
            var textSize = dc.TextSize(text);
            dc.Text((size - textSize) * 0.5f, text, color);
        }
    }
}
