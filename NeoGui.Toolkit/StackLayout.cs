namespace NeoGui.Toolkit;

using NeoGui.Core;

public struct StackLayoutConfig {
    public bool Horizontal;
}

public static class StackLayout {
    private static readonly StackLayoutConfig DefaultConfig = new();

    public static Element AddStackLayoutProps(this Element elem) {
        elem.OnMeasure(Measure);
        elem.OnLayout(Layout);
        return elem;
    }

    public static void Measure(Element e) {
        var maxWidth = 0.0f;
        var maxHeight = 0.0f;
        var sumWidth = 0.0f;
        var sumHeight = 0.0f;
        foreach (var child in e.Children) {
            var size = child.Size;
            if (size.X > maxWidth) { maxWidth = size.X; }
            if (size.Y > maxHeight) { maxHeight = size.Y; }
            sumWidth += size.X;
            sumHeight += size.Y;
        }
        var config = e.Get(DefaultConfig);
        if (config.Horizontal) {
            e.Width = sumWidth;
            e.Height = maxHeight;
        } else {
            e.Width = maxWidth;
            e.Height = sumHeight;
        }
    }

    public static void Layout(Element e, Constraints c) {
        var config = e.Get(DefaultConfig);
        var clientSize = e.Size;
        var offset = 0.0f;
        var axis = config.Horizontal ? 0 : 1;
        foreach (var child in e.Children) {
            ref var pos = ref child.Pos;
            ref var size = ref child.Size;
            pos[axis] = offset;
            pos[1 - axis] = 0;
            size[1 - axis] = clientSize[1 - axis];
            offset += size[axis];
        }
    }
}
