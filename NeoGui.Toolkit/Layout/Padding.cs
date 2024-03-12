namespace NeoGui.Toolkit.Layout;

using NeoGui.Core;

public static class Padding
{
    private struct PaddingProps
    {
        public EdgeInsets Padding;
    }

    public static Element SetPaddingLayout(this Element e, float startX, float startY, float endX, float endY)
    {
        return e.SetPaddingLayout(new EdgeInsets(startX, startY, endX, endY));
    }

    public static Element SetPaddingLayout(this Element e, Vec2 start, Vec2 end)
    {
        return e.SetPaddingLayout(new EdgeInsets(start, end));
    }

    public static Element SetPaddingLayout(this Element e, EdgeInsets padding)
    {
        return e.Set(new PaddingProps { Padding = padding })
                .OnLayout(PaddingLayoutFunc);
    }

    private static void PaddingLayoutFunc(Element e, Constraints c)
    {
        var props = e.Get<PaddingProps>();
        var child = e.SingleChild;
        var nc = c;
        ModifyConstraints(ref nc, props.Padding, 0);
        ModifyConstraints(ref nc, props.Padding, 1);
        child.Layout(nc);
    }

    private static void ModifyConstraints(ref Constraints c, EdgeInsets padding, int axis)
    {
        if (c.Max[axis] < float.PositiveInfinity)
        {
            c.Max[axis] -= padding.Start[axis] + padding.End[axis];
            if (c.Max[axis] < 0)
            {
                c.Max[axis] = 0;
            }
            if (c.Min[axis] > c.Max[axis])
            {
                c.Min[axis] = c.Max[axis];
            }
        }
    }
}
