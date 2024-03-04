using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeoGui.Core
{
    [Flags]
    internal enum ElementFlags
    {
        Disabled = 1,
        ClipContent = 2,
        Opaque = 4,
        SizeToFit = 8
    }

    public class NeoGuiContext
    {
        private readonly object rootKey = new object();
        private readonly StateDomain rootStateDomain;
        private Dictionary<long, StateDomain> currStateIds = new Dictionary<long, StateDomain>();
        private Dictionary<long, StateDomain> prevStateIds = new Dictionary<long, StateDomain>();

        internal int ElementCount;
        
        private const int InitialArraySize = 128;
        internal long[] AttrStateId = new long[InitialArraySize];
        internal object[] AttrStateKey = new object[InitialArraySize];
        internal StateDomain[] AttrStateDomain = new StateDomain[InitialArraySize];
        internal string[] AttrName = new string[InitialArraySize];
        internal int[] AttrParent = new int[InitialArraySize];
        internal int[] AttrFirstChild = new int[InitialArraySize];
        internal int[] AttrNextSibling = new int[InitialArraySize];
        internal int[] AttrLevel = new int[InitialArraySize];
        internal int[] AttrZIndex = new int[InitialArraySize];
        internal ElementFlags[] AttrFlags = new ElementFlags[InitialArraySize];
        internal Transform[] AttrTransform = new Transform[InitialArraySize];
        internal Transform[] AttrWorldTransform = new Transform[InitialArraySize];
        internal Rect[] AttrRect = new Rect[InitialArraySize];
        internal Rect[] AttrBoundingRect = new Rect[InitialArraySize]; // axis aligned bounding box in world coordinates
        internal Rect[] AttrClipRect = new Rect[InitialArraySize];
        internal Action<DrawContext>?[] AttrDrawFunc = new Action<DrawContext>?[InitialArraySize];
        internal Action<Element>?[] AttrMeasureFunc = new Action<Element>?[InitialArraySize];
        internal Action<Element>?[] AttrLayoutFunc = new Action<Element>?[InitialArraySize];

        // extra data which we don't deign to make an array for above goes here...
        internal readonly ValueStorage<DataKeys, int> DataStorage = new ValueStorage<DataKeys, int>();
        internal struct DataKeys { }


        public readonly INeoGuiDelegate Delegate;
        public readonly InputContext Input;
        
        public NeoGuiContext(INeoGuiDelegate del)
        {
            rootStateDomain = new StateDomain(this);
            Delegate = del;
            Input = new InputContext(this);
        }

        public void BeginFrame()
        {
            var temp = currStateIds;
            currStateIds = prevStateIds;
            prevStateIds = temp;
            currStateIds.Clear();

            DataStorage.Clear();
            ElementCount = 1; // need to forge this, to get past assert in Element constructor
            var root = Root;
            ElementCount = 0;
            AttrLevel[0] = -1; // will be overwritten by 0 on next line, since root is its own child
            CreateElement(root, rootKey, rootStateDomain); // create root element (pretending it is its own parent)
            AttrFirstChild[0] = -1; // undo root element being its own child
            AttrParent[0] = -1; // undo root element being its own parent
        }

        public void EndFrame()
        {
            // invoke registered OnRemoved handlers for elements which did not make it into the tree this frame
            RunRemoveHandlers();

            // invoke registered OnInserted handlers for elements which made it into the tree this frame, and which were not in it before
            RunInsertHandlers();
            
            // postprocess the just constructed tree
            PropagateDisablement();
            MeasureElements();
            LayoutElements();
            CalcTransformsAndRects();
            CalcBottomToTopIndex();

            // run the update passes (which process input events etc) on the now fully defined elements
            Input.PreUiUpdate();
            RunDepthAscentPass();
            RunDepthDescentPass();
            RunTreeDescentPass();
            RunTreeAscentPass();
            Input.PostUiUpdate();
            
            DrawElements();

            RotateStateDomains();
        }
        
        public Element Root => new Element(this, 0);

        internal Element CreateElement(Element parent, object? key, StateDomain? domain)
        {
            if (ElementCount == AttrStateId.Length) {
                var newLength = AttrStateId.Length * 2;
                Array.Resize(ref AttrStateId, newLength);
                Array.Resize(ref AttrStateKey, newLength);
                Array.Resize(ref AttrStateDomain, newLength);
                Array.Resize(ref AttrName, newLength);
                Array.Resize(ref AttrParent, newLength);
                Array.Resize(ref AttrFirstChild, newLength);
                Array.Resize(ref AttrNextSibling, newLength);
                Array.Resize(ref AttrLevel, newLength);
                Array.Resize(ref AttrZIndex, newLength);
                Array.Resize(ref AttrFlags, newLength);
                Array.Resize(ref AttrTransform, newLength);
                Array.Resize(ref AttrWorldTransform, newLength);
                Array.Resize(ref AttrRect, newLength);
                Array.Resize(ref AttrBoundingRect, newLength);
                Array.Resize(ref AttrClipRect, newLength);
                Array.Resize(ref AttrDrawFunc, newLength);
                Array.Resize(ref AttrMeasureFunc, newLength);
                Array.Resize(ref AttrLayoutFunc, newLength);
            }
            
            if (domain == null) {
                domain = AttrStateDomain[parent.Index];
            }
            long id;
            if (key == null) {
                key = AttrStateKey[parent.Index];
                id = domain.GetStateIdForInheritedKey(key);
            } else {
                id = domain.GetStateIdForOwnKey(key);
            }
            currStateIds[id] = domain;

            AttrStateId[ElementCount] = id;
            AttrStateKey[ElementCount] = key;
            AttrStateDomain[ElementCount] = domain;
            AttrName[ElementCount] = "";
            AttrParent[ElementCount] = parent.Index;
            AttrFirstChild[ElementCount] = -1; // we have no children yet
            AttrNextSibling[ElementCount] = AttrFirstChild[parent.Index]; // set parent's first child as next sibling
            AttrFirstChild[parent.Index] = ElementCount; // set this element as parent's first child
            AttrLevel[ElementCount] = AttrLevel[parent.Index] + 1;
            AttrZIndex[ElementCount] = 0;
            AttrFlags[ElementCount] = 0;
            AttrTransform[ElementCount].MakeIdentity();
            AttrWorldTransform[ElementCount].MakeIdentity();
            AttrRect[ElementCount] = new Rect();
            AttrBoundingRect[ElementCount] = new Rect();
            AttrDrawFunc[ElementCount] = null;
            AttrMeasureFunc[ElementCount] = null;
            AttrLayoutFunc[ElementCount] = null;

            return new Element(this, ElementCount++);
        }
        

        private void PropagateDisablement()
        {
            for (var i = 1; i < ElementCount; ++i) {
                if (GetFlag(AttrParent[i], ElementFlags.Disabled)) {
                    SetFlag(i, ElementFlags.Disabled, true);
                }
            }
        }

        private void MeasureElements()
        {
            for (var i = ElementCount - 1; i >= 0; --i) { // bottom up
                AttrMeasureFunc[i]?.Invoke(new Element(this, i));
            }
        }

        private void LayoutElements()
        {
            for (var i = 0; i < ElementCount; ++i) { // top down
                AttrLayoutFunc[i]?.Invoke(new Element(this, i));
            }
        }

        private void CalcTransformsAndRects()
        {
            // we know parents come before children, so it's OK to just iterate like this and refer back to parents
            AttrWorldTransform[0] = AttrTransform[0];
            AttrBoundingRect[0] = AttrRect[0];
            AttrClipRect[0] = AttrBoundingRect[0];
            for (var i = 1; i < ElementCount; ++i) {
                var rect = AttrRect[i];
                var local = AttrTransform[i];
                if (local.Pivot.SqrLength <= 0) { // TODO: use another way to detect unchanged Pivot
                    local.Pivot = new Vec3(rect.Size * 0.5f);
                }
                local.Translation += new Vec3(rect.Pos);
                AttrWorldTransform[i].Product(ref AttrWorldTransform[AttrParent[i]], ref local);

                var e = new Element(this, i);
                var p0 = e.ToWorldCoord(new Vec2(0, 0));
                var p1 = e.ToWorldCoord(new Vec2(rect.Width, 0));
                var p2 = e.ToWorldCoord(rect.Size);
                var p3 = e.ToWorldCoord(new Vec2(0, rect.Height));
                var minX = Math.Min(p0.X, Math.Min(p1.X, Math.Min(p2.X, p3.X)));
                var minY = Math.Min(p0.Y, Math.Min(p1.Y, Math.Min(p2.Y, p3.Y)));
                var maxX = Math.Max(p0.X, Math.Max(p1.X, Math.Max(p2.X, p3.X)));
                var maxY = Math.Max(p0.Y, Math.Max(p1.Y, Math.Max(p2.Y, p3.Y)));
                AttrBoundingRect[i] = new Rect(minX, minY, maxX - minX, maxY - minY);
                
                var parentClipRect = AttrClipRect[AttrParent[i]];
                if (GetFlag(i, ElementFlags.ClipContent)) {
                    AttrClipRect[i] = parentClipRect.Intersection(AttrBoundingRect[i]);
                } else {
                    AttrClipRect[i] = parentClipRect;
                }
            }
        }

        
        // mapping of (z-index, level) -> element index, to be sorted and used to determine rendering order
        private readonly List<KeyedValue<(int, int), int>> bottomToTopIndex = new List<KeyedValue<(int, int), int>>();
        private void CalcBottomToTopIndex()
        {
            bottomToTopIndex.Clear();
            for (var i = 0; i < ElementCount; ++i) {
                bottomToTopIndex.Add(new KeyedValue<(int, int), int>((AttrZIndex[i], AttrLevel[i]), i));
            }
            bottomToTopIndex.Sort();
        }
        

        private DrawCommandBuffer currDrawCommandBuffer = new DrawCommandBuffer();
        private DrawCommandBuffer prevDrawCommandBuffer = new DrawCommandBuffer();
        
        public readonly List<DrawCommandBuffer> DirtyDrawCommandBuffers = new List<DrawCommandBuffer>();

        private readonly DrawContext drawContext = new DrawContext();
        private void DrawElements()
        {
            var temp = currDrawCommandBuffer;
            currDrawCommandBuffer = prevDrawCommandBuffer;
            prevDrawCommandBuffer = temp;

            currDrawCommandBuffer.Clear();
            drawContext.CommandBuffer = currDrawCommandBuffer;
            var prevClipRect = Rect.Empty;
            foreach (var entry in bottomToTopIndex) {
                var elemIndex = entry.Value;

                if (AttrDrawFunc[elemIndex] == null) {
                    continue;
                }

                var clipRect = AttrClipRect[elemIndex];
                if (!clipRect.Intersects(AttrBoundingRect[elemIndex])) {
                    continue;
                }

                if ((clipRect.Pos - prevClipRect.Pos).SqrLength > 0 || (clipRect.Size - prevClipRect.Size).SqrLength > 0) {
                    currDrawCommandBuffer.Add(new DrawCommand {
                        Type = DrawCommandType.SetClipRect,
                        SetClipRect = new SetClipRectCommand {
                            ClipRect = clipRect
                        }
                    });
                }

                currDrawCommandBuffer.Add(new DrawCommand {
                    Type = DrawCommandType.SetTransform,
                    SetTransform = new SetTransformCommand {
                        Transform = AttrWorldTransform[elemIndex]
                    }
                });
                
                Delegate.DrawDot(AttrWorldTransform[elemIndex].ApplyForward(Vec3.Zero));

                drawContext.Target = new Element(this, elemIndex);
                AttrDrawFunc[elemIndex]?.Invoke(drawContext);
            }

            DirtyDrawCommandBuffers.Clear();
            if (!currDrawCommandBuffer.HasEqualCommands(prevDrawCommandBuffer)) {
                DirtyDrawCommandBuffers.Add(currDrawCommandBuffer);
            }
        }

        public void HitTest(Vec2 worldPos, List<Element> result)
        {
            for (var i = bottomToTopIndex.Count - 1; i >= 0; --i) {
                var e = new Element(this, i);
                if (e.HitTest(worldPos)) {
                    result.Add(e);
                }
            }
        }

        
        
        internal int KeyIdCounter;
        
        private readonly Stack<StateDomain> stateDomainsRelinquishedThisFrame = new Stack<StateDomain>();
        private readonly Stack<StateDomain> stateDomainsPendingReuse = new Stack<StateDomain>();
        private readonly Stack<StateDomain> stateDomainsReadyForReuse = new Stack<StateDomain>();

        public StateDomain CreateStateDomain()
        {
            if (stateDomainsReadyForReuse.Count > 0) {
                return stateDomainsReadyForReuse.Pop();
            }
            return new StateDomain(this);
        }
        internal void RelinquishStateDomain(StateDomain domain)
        {
            stateDomainsRelinquishedThisFrame.Push(domain);
        }
        private void RotateStateDomains()
        {
            // this seemingly needlessly complicated system ensures that even if a StateDomain is disposed the
            // last frame before an element disappears (maybe as a consequence of the same thing that causes the 
            // element to disappear), it will still not have been reused next frame, so that
            // it may be used by the OnRemoved handler (which will then run) to access the element's state
            while (stateDomainsPendingReuse.Count > 0) {
                var domain = stateDomainsPendingReuse.Pop();
                domain.Reset();
                stateDomainsReadyForReuse.Push(domain);
            }
            while (stateDomainsRelinquishedThisFrame.Count > 0) {
                stateDomainsPendingReuse.Push(stateDomainsRelinquishedThisFrame.Pop());
            }
        }




        private int stringIdCounter;
        private readonly Dictionary<string, int> stringToId = new Dictionary<string, int>();
        private readonly Dictionary<int, string> idToString = new Dictionary<int, string>();

        internal int InternString(string str)
        {
            int id;
            if (stringToId.TryGetValue(str, out id)) {
                return id;
            }
            id = ++stringIdCounter;
            stringToId[str] = id;
            idToString[id] = str;
            return id;
        }
        public string GetInternedString(int id)
        {
            string str;
            return idToString.TryGetValue(id, out str) ? str : "";
        }

        private readonly Dictionary<long, Vec2> textSizeCache = new Dictionary<long, Vec2>();

        internal Vec2 GetTextSize(string text, int fontId)
        {
            if (string.IsNullOrEmpty(text)) {
                return Vec2.Zero;
            }
            var stringId = InternString(text);
            var key = Util.TwoIntsToLong(fontId, stringId);
            Vec2 size;
            if (textSizeCache.TryGetValue(key, out size)) {
                return size;
            }
            size = Delegate.TextSize(text, fontId);
            textSizeCache[key] = size;
            return size;
        }


        internal bool GetFlag(int elemIndex, ElementFlags flag)
        {
            return (AttrFlags[elemIndex] & flag) == flag;
        }
        internal void SetFlag(int elemIndex, ElementFlags flag, bool value)
        {
            if (value) {
                AttrFlags[elemIndex] |= flag;
            } else {
                AttrFlags[elemIndex] &= ~flag;
            }
        }



        private readonly List<KeyedValue<int, Action<Element>>> insertHandlers = new List<KeyedValue<int, Action<Element>>>();

        internal void AddInsertHandler(int elemIndex, Action<Element> handler)
        {
            insertHandlers.Add(new KeyedValue<int, Action<Element>>(elemIndex, handler));
        }
        private void RunInsertHandlers()
        {
            foreach (var entry in insertHandlers) {
                if (!prevStateIds.ContainsKey(AttrStateId[entry.Key])) {
                    entry.Value(new Element(this, entry.Key));
                }
            }
            insertHandlers.Clear();
        }

        
        private List<KeyedValue<long, Action<ElementStateProxy>>> currRemoveHandlers = new List<KeyedValue<long, Action<ElementStateProxy>>>();
        private List<KeyedValue<long, Action<ElementStateProxy>>> prevRemoveHandlers = new List<KeyedValue<long, Action<ElementStateProxy>>>();

        internal void AddRemoveHandler(long stateId, Action<ElementStateProxy> handler)
        {
            currRemoveHandlers.Add(new KeyedValue<long, Action<ElementStateProxy>>(stateId, handler));
        }
        private void RunRemoveHandlers()
        {
            foreach (var entry in prevRemoveHandlers) {
                if (!currStateIds.ContainsKey(entry.Key)) {
                    var domain = prevStateIds[entry.Key];
                    entry.Value(new ElementStateProxy(entry.Key, domain.Storage));
                }
            }
            prevRemoveHandlers.Clear();

            var temp = currRemoveHandlers;
            currRemoveHandlers = prevRemoveHandlers;
            prevRemoveHandlers = temp;
        }



        #region Ascent/descent traversal
        private struct TraverseEntry<TKey> : IComparable<TraverseEntry<TKey>>
            where TKey: IComparable<TKey>
        {
            private readonly TKey key;
            public readonly int ElemIndex;
            public readonly Action<Element> Handler;
            public TraverseEntry(TKey key, int elemIndex, Action<Element> handler)
            {
                this.key = key;
                ElemIndex = elemIndex;
                Handler = handler;
            }
            public int CompareTo(TraverseEntry<TKey> other)
            {
                return key.CompareTo(other.key);
            }
        }
        private readonly List<TraverseEntry<long>> depthDescentHandlers = new List<TraverseEntry<long>>();
        private readonly List<TraverseEntry<long>> depthAscentHandlers = new List<TraverseEntry<long>>();
        private readonly List<TraverseEntry<int>> treeDescentHandlers = new List<TraverseEntry<int>>();
        private readonly List<TraverseEntry<int>> treeAscentHandlers = new List<TraverseEntry<int>>();

        internal void AddDepthDescentHandler(int elemIndex, Action<Element> handler)
        {
            // temporarily store 0 as key since the final z-index can't necessarily be known now
            depthDescentHandlers.Add(new TraverseEntry<long>(0, elemIndex, handler));
        }
        internal void AddDepthAscentHandler(int elemIndex, Action<Element> handler)
        {
            // temporarily store 0 as key since the final z-index can't necessarily be known now
            depthAscentHandlers.Add(new TraverseEntry<long>(0, elemIndex, handler));
        }
        internal void AddTreeDescentHandler(int elemIndex, Action<Element> handler)
        {
            treeDescentHandlers.Add(new TraverseEntry<int>(AttrLevel[elemIndex], elemIndex, handler));
        }
        internal void AddTreeAscentHandler(int elemIndex, Action<Element> handler)
        {
            treeAscentHandlers.Add(new TraverseEntry<int>(AttrLevel[elemIndex], elemIndex, handler));
        }

        private void RunDepthAscentPass()
        {
            for (var i = 0; i < depthAscentHandlers.Count; ++i) {
                // rewrite now that we can know z-index
                var elemIndex = depthAscentHandlers[i].ElemIndex;
                var key = Util.TwoIntsToLong(AttrZIndex[elemIndex], AttrLevel[elemIndex]);
                depthAscentHandlers[i] = new TraverseEntry<long>(key, elemIndex, depthAscentHandlers[i].Handler);
            }
            postPassHandlers.Clear();
            depthAscentHandlers.Sort();
            for (var i = 0; i < depthAscentHandlers.Count; ++i) {
                depthAscentHandlers[i].Handler(new Element(this, depthAscentHandlers[i].ElemIndex));
            }
            depthAscentHandlers.Clear();
            RunPostPassHandlers();
        }

        private void RunDepthDescentPass()
        {
            for (var i = 0; i < depthDescentHandlers.Count; ++i) {
                // rewrite now that we can know z-index
                var elemIndex = depthDescentHandlers[i].ElemIndex;
                var key = Util.TwoIntsToLong(AttrZIndex[elemIndex], AttrLevel[elemIndex]);
                depthDescentHandlers[i] = new TraverseEntry<long>(key, elemIndex, depthDescentHandlers[i].Handler);
            }
            postPassHandlers.Clear();
            depthDescentHandlers.Sort();
            for (var i = depthDescentHandlers.Count - 1; i >= 0; --i) {
                depthDescentHandlers[i].Handler(new Element(this, depthDescentHandlers[i].ElemIndex));
            }
            depthDescentHandlers.Clear();
            RunPostPassHandlers();
        }

        private void RunTreeDescentPass()
        {
            postPassHandlers.Clear();
            treeDescentHandlers.Sort();
            for (var i = 0; i < treeDescentHandlers.Count; ++i) {
                treeDescentHandlers[i].Handler(new Element(this, treeDescentHandlers[i].ElemIndex));
            }
            treeDescentHandlers.Clear();
            RunPostPassHandlers();
        }

        private void RunTreeAscentPass()
        {
            postPassHandlers.Clear();
            treeAscentHandlers.Sort();
            for (var i = treeAscentHandlers.Count - 1; i >= 0; --i) {
                treeAscentHandlers[i].Handler(new Element(this, treeAscentHandlers[i].ElemIndex));
            }
            treeAscentHandlers.Clear();
            RunPostPassHandlers();
        }

        private readonly List<KeyedValue<int, Action<Element>>> postPassHandlers = new List<KeyedValue<int, Action<Element>>>();
        internal void RunAfterPass(int elemIndex, Action<Element> handler)
        {
            postPassHandlers.Add(new KeyedValue<int, Action<Element>>(elemIndex, handler));
        }
        private void RunPostPassHandlers()
        {
            foreach (var entry in postPassHandlers) {
                entry.Value(new Element(this, entry.Key));
            }
        }
        #endregion
    }
}
