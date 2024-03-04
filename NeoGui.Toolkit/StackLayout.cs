using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public struct StackLayoutConfig
    {
        public bool Horizontal;
    }

    public static class StackLayout
    {
        private static readonly StackLayoutConfig DefaultConfig = new();

        public static void AddProps(Element elem)
        {
            elem.Measure = e => Measure(e);
            elem.Layout = e => Layout(e);
        }

        public static void Measure(Element elem)
        {
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

        public static void Layout(Element elem)
        {
            var config = elem.Get(DefaultConfig);
            var clientSize = elem.Size;
            var offset = 0.0f;
            var axis = config.Horizontal ? 0 : 1;
            foreach (var chld in elem.Children) {
                var child = chld;
                var pos = child.Pos;
                var size = child.Size;
                pos[axis] = offset;
                pos[1 - axis] = 0;
                size[1 - axis] = clientSize[1 - axis];
                child.Pos = pos;
                child.Size = size;
                offset += size[axis];
            }
        }
    }
}
