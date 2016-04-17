using System.Collections.Generic;

namespace NeoGui.Core
{
    public class DrawContext
    {
        internal DrawCommandBuffer CommandBuffer { get; set; }

        public Element Target { get; internal set; }
        public NeoGuiContext Context => Target.Context;

        public void SolidRect(Rect rect, Color color)
        {
            var clipRect = Target.ClipRect;
            rect = new Rect(Target.AbsoluteRect.Pos + rect.Pos, rect.Size);
            if (!clipRect.Intersects(rect)) {
                return;
            }
            CommandBuffer.Add(new DrawCommand {
                Type = DrawCommandType.SolidRect,
                ClipRect = clipRect,
                SolidRect = new SolidRectCommand {
                    Rect = rect,
                    Color = color
                }
            });
        }

        public void Text(Vec2 pos, string text, Color color, int fontId = 0)
        {
            var clipRect = Target.ClipRect;
            pos += Target.AbsoluteRect.Pos;
            // conservatively assume that the label can be some big size
            if (!clipRect.Intersects(new Rect(pos.X, pos.Y, 1000, 200))) {
                return;
            }
            CommandBuffer.Add(new DrawCommand {
                Type = DrawCommandType.Text,
                ClipRect = clipRect,
                Text = new TextCommand {
                    Vec2 = pos,
                    StringId = Context.InternString(text),
                    FontId = fontId,
                    Color = color
                }
            });
        }

        public Vec2 TextSize(string text, int fontId = 0)
        {
            return Context.GetTextSize(text, fontId);
        }
    }
}
