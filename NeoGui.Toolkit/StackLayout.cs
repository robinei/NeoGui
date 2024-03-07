namespace NeoGui.Toolkit;

using NeoGui.Core;

public struct StackLayoutConfig {
    public bool Horizontal;
}

public static class StackLayout {
    private static readonly StackLayoutConfig DefaultConfig = new();

    public static Element AddStackLayoutProps(this Element elem) {
        elem.Measure = Measure;
        elem.Layout = Layout;
        return elem;
    }

    public static void Measure(Element elem) {
        var maxWidth = 0.0f;
        var maxHeight = 0.0f;
        var sumWidth = 0.0f;
        var sumHeight = 0.0f;
        foreach (var child in elem.Children) {
            var size = child.Size;
            if (size.X > maxWidth) { maxWidth = size.X; }
            if (size.Y > maxHeight) { maxHeight = size.Y; }
            sumWidth += size.X;
            sumHeight += size.Y;
        }
        var config = elem.Get(DefaultConfig);
        if (config.Horizontal) {
            elem.Width = sumWidth;
            elem.Height = maxHeight;
        } else {
            elem.Width = maxWidth;
            elem.Height = sumHeight;
        }
    }

    public static void Layout(Element elem) {
        var config = elem.Get(DefaultConfig);
        var clientSize = elem.Size;
        var offset = 0.0f;
        var axis = config.Horizontal ? 0 : 1;
        foreach (var child in elem.Children) {
            ref var pos = ref child.Pos;
            ref var size = ref child.Size;
            pos[axis] = offset;
            pos[1 - axis] = 0;
            size[1 - axis] = clientSize[1 - axis];
            offset += size[axis];
        }
    }
}
