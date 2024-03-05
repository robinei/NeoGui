namespace NeoGui.Core;

public class DrawContext {
    internal DrawCommandBuffer CommandBuffer { get; set; } = new DrawCommandBuffer();

    public Element Target { get; internal set; }
    public NeoGuiContext Context => Target.Context;

    public void SolidRect(Rect rect, Color color) {
        //rect = new Rect(Target.WorldTransform.ApplyForward(new Vec3(rect.Pos)).XY, rect.Size);
        //rect = new Rect(Target.AbsoluteRect.Pos + rect.Pos, rect.Size);
        CommandBuffer.Add(new DrawCommand {
            Type = DrawCommandType.SolidRect,
            SolidRect = new SolidRectCommand {
                Rect = rect,
                Color = color
            }
        });
    }

    public void Text(Vec2 pos, string text, Color color, int fontId = 0) {
        //pos = Target.WorldTransform.ApplyForward(new Vec3(pos)).XY;
        //pos += Target.AbsoluteRect.Pos;
        CommandBuffer.Add(new DrawCommand {
            Type = DrawCommandType.Text,
            Text = new TextCommand {
                Vec2 = pos,
                StringId = Context.InternString(text),
                FontId = fontId,
                Color = color
            }
        });
    }

    public Vec2 TextSize(string text, int fontId = 0) => Context.GetTextSize(text, fontId);
}
