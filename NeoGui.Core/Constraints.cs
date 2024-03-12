namespace NeoGui.Core;

public struct Constraints {
    public Vec2 Min, Max;

    public readonly void Constrain(Element e) {
        ref var size = ref e.Size;
        if (size.X > Max.X) { size.X = Max.X; }
        if (size.Y > Max.Y) { size.Y = Max.Y; }
        if (size.X < Min.X) { size.X = Min.X; }
        if (size.Y < Min.Y) { size.Y = Min.Y; }
    }

    public readonly void ConstrainAndGrow(Element e) {
        ref var size = ref e.Size;
        if (Max.X < float.PositiveInfinity) { size.X = Max.X; }
        if (Max.Y < float.PositiveInfinity) { size.Y = Max.Y; }
        if (size.X < Min.X) { size.X = Min.X; }
        if (size.Y < Min.Y) { size.Y = Min.Y; }
    }

    public static Constraints Unconstrained => new() {
        Min = new Vec2(0, 0),
        Max = new Vec2(float.PositiveInfinity, float.PositiveInfinity),
    };
}
