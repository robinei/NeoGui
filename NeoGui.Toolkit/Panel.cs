namespace NeoGui.Toolkit;

using NeoGui.Core;

public struct PanelBackground { public Color Color; }

public static class Panel {
    private static readonly PanelBackground DefaultPanelBackground = new() { Color = Color.White};

    public static Element CreatePanel(this Element parent, Color? backgroundColor = null, object? key = null, StateDomain? domain = null) {
        return parent.CreateElement(key, domain).AddPanelProps(backgroundColor);
    }

    public static Element AddPanelProps(this Element elem, Color? backgroundColor = null) {
        if (backgroundColor != null) {
            elem.Set(new PanelBackground { Color = backgroundColor.Value });
        }
        elem.Draw = Draw;
        return elem;
    }

    public static void Draw(DrawContext dc) {
        var size = dc.Target.Size;
        var color = dc.Target.Get(DefaultPanelBackground).Color;
        dc.SolidRect(new Rect(size), color);
    }
}
