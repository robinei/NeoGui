namespace NeoGui.Core
{
    public interface INeoGuiDelegate
    {
        Vec2 TextSize(string text, int fontId);

        void DrawDot(Vec3 p, Color? c = null);
    }
}