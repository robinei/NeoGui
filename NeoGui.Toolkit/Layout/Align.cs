namespace NeoGui.Toolkit.Layout;

using NeoGui.Core;

public static class Align
{
    private struct AlignProps
    {
        public Vec2 Align;
    }

    private struct ChildAlignment
    {
        public Vec2 Align;
    }

    public static Element SetAlignment(this Element e, float alignX, float alignY)
    {
        return e.SetAlignment(new Vec2(alignX, alignY));
    }

    public static Element SetAlignment(this Element e, Vec2 align)
    {
        return e.Set(new ChildAlignment { Align = align });
    }

    public static Element SetAlignLayout(this Element e)
    {
        return e.SetAlignLayout(Vec2.Zero);
    }

    public static Element SetAlignLayout(this Element e, float alignX, float alignY)
    {
        return e.SetAlignLayout(new Vec2(alignX, alignY));
    }

    public static Element SetAlignLayout(this Element e, Vec2 align)
    {
        return e.Set(new AlignProps { Align = align })
                .OnLayout(AlignLayoutFunc);
    }

    private static void AlignLayoutFunc(Element e, Constraints c)
    {
        c.ConstrainAndGrow(e);
        if (!e.HasChildren)
        {
            return;
        }
        var props = e.Get<AlignProps>();
        ref Rect rect = ref e.Rect;
        foreach (var child in e.Children)
        {
            child.Layout(Constraints.Unconstrained);
            ref Rect childRect = ref child.Rect;
            Vec2 align = props.Align;
            if (child.TryGet(out ChildAlignment childProps))
            {
                align = childProps.Align;
            }
            childRect.X = (rect.Width - childRect.Width) * (align.X + 1.0f) * 0.5f;
            childRect.Y = (rect.Height - childRect.Height) * (align.Y + 1.0f) * 0.5f;
        }
    }
}
