namespace NeoGui.Toolkit;

using NeoGui.Core;

public enum TextAlignment {
    Left,
    Center,
    Right
}

public struct LabelText { public string Value; }
public struct LabelColor { public Color Value; }

public static class Label {
    private static readonly LabelText DefaultText = new() { Value = string.Empty };
    private static readonly LabelColor DefaultColor = new() { Value = Color.Black };
    private const TextAlignment DefaultAlignment = TextAlignment.Left;

    public static string GetText(Element label) => label.Get(DefaultText).Value;
    public static Color GetColor(Element label) => label.Get(DefaultColor).Value;
    public static TextAlignment GetAlignment(Element label) => label.Get(DefaultAlignment);

    public static Element Create(Element parent, string text, Color? color = null, TextAlignment? alignment = null) {
        var label = Element.Create(parent);
        if (text != null) {
            label.Set(new LabelText { Value = text });
        }
        if (color != null) {
            label.Set(new LabelColor { Value = color.Value });
        }
        if (alignment != null) {
            label.Set(alignment.Value);
        }
        label.SizeToFit = true;
        label.Measure = Measure;
        label.Draw = Draw;
        return label;
    }

    public static void Measure(Element label) {
        if (label.SizeToFit) {
            var text = label.Get(DefaultText).Value;
            if (text != null) {
                label.Size = label.Context.Delegate.TextSize(text, 0); // TODO: fontId
            }
        }
    }

    public static void Draw(DrawContext dc) {
        var label = dc.Target;
        var text = GetText(label);
        if (string.IsNullOrEmpty(text)) {
            return;
        }
        var color = GetColor(label);
        var alignment = GetAlignment(label);
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
