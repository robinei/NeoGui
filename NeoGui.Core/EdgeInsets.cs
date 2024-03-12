namespace NeoGui.Core;

public struct EdgeInsets {
    public Vec2 Start, End;

    public EdgeInsets(Vec2 start, Vec2 end) {
        Start = start;
        End = end;
    }

    public EdgeInsets(float value) {
        Start = new Vec2(value, value);
        End = new Vec2(value, value);
    }

    public EdgeInsets(float startX, float startY, float endX, float endY) {
        Start = new Vec2(startX, startY);
        End = new Vec2(endX, endY);
    }
}
