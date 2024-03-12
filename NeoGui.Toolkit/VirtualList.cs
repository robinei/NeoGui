namespace NeoGui.Toolkit;

using System;
using NeoGui.Core;

public static class VirtualList {
    public static Element CreateVirtualList(
        this Element parent,
        int itemCount,
        float itemHeight,
        Func<Element, int, Element> createItem,
        object? key = null,
        StateDomain? domain = null)
    {
        var virtualList = parent.CreateScrollArea(ScrollAreaFlags.BounceY | ScrollAreaFlags.FillX, key: key, domain: domain);
        virtualList.OnLayout(Layout);
        
        var state = virtualList.GetOrCreateState<ScrollAreaState>();
        var content = virtualList.GetScrollAreaContentPanel();
        content.Size = new Vec2(0, itemCount * itemHeight);
        
        var index = Math.Max(0, (int)(-state.Pos.Y / itemHeight));
        var bottom = -state.Pos.Y + state.ClientSize.Y;
        while (index < itemCount) {
            var y = index * itemHeight;
            if (y > bottom) {
                break;
            }
            var item = createItem(content, index);
            item.Rect = new Rect(0, y, state.ClientSize.X, itemHeight);
            ++index;
        }

        return virtualList;
    }

    public static void Layout(Element virtualList, Constraints c) {
        var content = virtualList.GetScrollAreaContentPanel();
        var width = virtualList.Width;
        foreach (var item in content.Children) {
            item.Width = width;
        }
        ScrollArea.Layout(virtualList, c);
    }
}
