using System;
using NeoGui.Core;

namespace NeoGui.Toolkit
{
    public static class VirtualList
    {
        public static Element Create(Element parent, int itemCount, float itemHeight, Func<Element, int, Element> createItem, object key = null)
        {
            var virtualList = ScrollArea.Create(parent, ScrollAreaFlags.BounceY, key);
            virtualList.Layout = Layout;
            
            var state = virtualList.GetOrCreateState<ScrollAreaState>();
            var content = ScrollArea.GetContentPanel(virtualList);
            content.Size = new Vec2(state.ClientSize.X, itemCount * itemHeight);
            
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

        public static void Layout(Element virtualList)
        {
            var content = ScrollArea.GetContentPanel(virtualList);
            var width = virtualList.Width;
            content.Width = width;
            foreach (var itm in content.Children) {
                var item = itm; // wtf?
                item.Width = width;
            }
            ScrollArea.Layout(virtualList);
        }
    }
}
