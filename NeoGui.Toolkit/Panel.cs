using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public struct PanelBackground { public Color Color; }

    public static class Panel
    {
        private static readonly PanelBackground DefaultPanelBackground = new PanelBackground {Color = Color.White};

        public static Element Create(Element parent, Color? backgroundColor = null, object key = null)
        {
            var panel = Element.Create(parent, key);
            AddProps(panel, backgroundColor);
            return panel;
        }

        public static void AddProps(Element elem, Color? backgroundColor = null)
        {
            if (backgroundColor != null) {
                elem.Set(new PanelBackground { Color = backgroundColor.Value });
            }
            elem.Draw = Draw;
        }

        public static void Draw(DrawContext dc)
        {
            var size = dc.Target.Size;
            var color = dc.Target.Get(DefaultPanelBackground).Color;
            dc.SolidRect(new Rect(size), color);
        }
    }
}
