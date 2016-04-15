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
            CommandBuffer.Add(new DrawCommand {
                Type = DrawCommandType.SolidRect,
                ClipRect = Target.ClipRect,
                SolidRect = new SolidRectCommand {
                    Rect = new Rect(Target.AbsoluteRect.Pos + rect.Pos, rect.Size),
                    Color = color
                }
            });
        }

        public void Text(Vec2 pos, string text, Color color, int fontId = 0)
        {
            CommandBuffer.Add(new DrawCommand {
                Type = DrawCommandType.Text,
                ClipRect = Target.ClipRect,
                Text = new TextCommand {
                    Vec2 = Target.AbsoluteRect.Pos + pos,
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
