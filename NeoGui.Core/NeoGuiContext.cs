using System;
using System.Collections.Generic;

namespace NeoGui.Core
{
    /// <summary>
    /// Static members of generic classes are generated once for each combination of type arguments.
    /// We exploit this to maintain a static mapping from types to integer values.
    /// In fact we maintain several mappings: one per category type.
    /// </summary>
    internal static class TypeKeys<TCategory, T>
    {
        public static readonly int Key = TypeKeyMap<TCategory>.KeyOf(typeof(T));
    }
    
    // ReSharper disable once UnusedTypeParameter
    internal static class TypeKeyMap<TCategory>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, int> Map = new Dictionary<Type, int>();

        public static int KeyOf(Type type)
        {
            int key;
            if (Map.TryGetValue(type, out key)) {
                return key;
            }
            key = Map.Count;
            Map[type] = key;
            return key;
        }
    }

    [Flags]
    internal enum ElementFlags
    {
        Disabled = 1,
        ClipContent = 2,
        Opaque = 4
    }

    public class NeoGuiContext
    {
        // categories for TypeKeys
        internal struct StateKeys { }
        internal struct DataKeys { }
        internal struct EventKeys { }

        private const int InitialArraySize = 128;

        private readonly object rootKey = new object();
        private readonly List<int> keyCounters = new List<int>();
        private readonly ValueStorage<StateKeys, ElementId> rootStateHolder = new ValueStorage<StateKeys, ElementId>();
        private Dictionary<ElementId, int> currIdToIndexMap = new Dictionary<ElementId, int>();
        private Dictionary<ElementId, int> prevIdToIndexMap = new Dictionary<ElementId, int>();

        private int elementCount;

        internal ElementId[] AttrId = new ElementId[InitialArraySize];
        internal string[] AttrName = new string[InitialArraySize];
        internal int[] AttrParent = new int[InitialArraySize];
        internal int[] AttrFirstChild = new int[InitialArraySize];
        internal int[] AttrNextSibling = new int[InitialArraySize];
        internal int[] AttrLevel = new int[InitialArraySize];
        internal int[] AttrZIndex = new int[InitialArraySize];
        internal ElementFlags[] AttrFlags = new ElementFlags[InitialArraySize];
        internal int[] AttrKeyCounterIndex = new int[InitialArraySize];
        internal ValueStorage<StateKeys, ElementId>[] AttrStateHolder = new ValueStorage<StateKeys, ElementId>[InitialArraySize];
        internal Rect[] AttrRect = new Rect[InitialArraySize];
        internal Rect[] AttrAbsRect = new Rect[InitialArraySize]; // absolute coordinates
        internal Rect[] AttrClipRect = new Rect[InitialArraySize];
        internal Action<DrawContext>[] AttrDrawFunc = new Action<DrawContext>[InitialArraySize];
        internal Action<Element>[] AttrLayoutFunc = new Action<Element>[InitialArraySize];

        // extra data which we don't deign to make an array for above goes here...
        internal readonly ValueStorage<DataKeys, int> DataStorage = new ValueStorage<DataKeys, int>();
        

        public readonly INeoGuiDelegate Delegate;
        public readonly InputContext Input;
        
        public NeoGuiContext(INeoGuiDelegate del)
        {
            Delegate = del;
            Input = new InputContext(this);
        }

        public void BeginFrame()
        {
            FlipStateHolders();

            var temp = currIdToIndexMap;
            currIdToIndexMap = prevIdToIndexMap;
            prevIdToIndexMap = temp;
            currIdToIndexMap.Clear();

            elementCount = 0;
            keyCounters.Clear();
            AttrStateHolder[0] = rootStateHolder;
            AttrLevel[0] = -1; // will be overwritten by 0 on next line, since root is its own child
            CreateElement(new Element(this, 0), rootKey); // create root element (pretending it is its own parent)
            AttrFirstChild[0] = -1; // undo root element being its own child
            AttrParent[0] = -1; // undo root element being its own parent

            DataStorage.Clear();
            ClearTraverseHandlers();
            insertHandlers.Clear();
        }

        public void EndFrame()
        {
            RunInsertHandlers();
            PropagateDisablement();
            LayoutElements();
            CalcBottomToTopIndex();
            CalcRects();
            DrawElements();
        }
        
        public Element Root => new Element(this, 0);

        internal Element CreateElement(Element parent, object key)
        {
            if (elementCount == AttrId.Length) {
                var newLength = AttrId.Length * 2;
                Array.Resize(ref AttrId, newLength);
                Array.Resize(ref AttrName, newLength);
                Array.Resize(ref AttrParent, newLength);
                Array.Resize(ref AttrFirstChild, newLength);
                Array.Resize(ref AttrNextSibling, newLength);
                Array.Resize(ref AttrLevel, newLength);
                Array.Resize(ref AttrZIndex, newLength);
                Array.Resize(ref AttrFlags, newLength);
                Array.Resize(ref AttrKeyCounterIndex, newLength);
                Array.Resize(ref AttrStateHolder, newLength);
                Array.Resize(ref AttrRect, newLength);
                Array.Resize(ref AttrAbsRect, newLength);
                Array.Resize(ref AttrClipRect, newLength);
                Array.Resize(ref AttrDrawFunc, newLength);
                Array.Resize(ref AttrLayoutFunc, newLength);
            }

            int keyIndex;
            int keyCounterIndex;
            if (key == null) {
                key = parent.Key;
                keyCounterIndex = AttrKeyCounterIndex[parent.Index];
                keyIndex = ++keyCounters[keyCounterIndex];
            } else {
                keyIndex = 0;
                keyCounterIndex = keyCounters.Count;
                keyCounters.Add(0);
            }

            var id = new ElementId(key, keyIndex);
            currIdToIndexMap[id] = elementCount;

            AttrId[elementCount] = id;
            AttrName[elementCount] = "";
            AttrParent[elementCount] = parent.Index;
            AttrFirstChild[elementCount] = -1; // we have no children yet
            AttrNextSibling[elementCount] = AttrFirstChild[parent.Index]; // set parent's first child as next sibling
            AttrFirstChild[parent.Index] = elementCount; // set this element as parent's first child
            AttrLevel[elementCount] = AttrLevel[parent.Index] + 1;
            AttrZIndex[elementCount] = 0;
            AttrFlags[elementCount] = 0;
            AttrKeyCounterIndex[elementCount] = keyCounterIndex;
            AttrStateHolder[elementCount] = AttrStateHolder[parent.Index]; // inherit parent state holder
            AttrRect[elementCount] = new Rect();
            AttrDrawFunc[elementCount] = null;
            AttrLayoutFunc[elementCount] = null;

            return new Element(this, elementCount++);
        }
        

        private void PropagateDisablement()
        {
            for (var i = 1; i < elementCount; ++i) {
                if (GetFlag(AttrParent[i], ElementFlags.Disabled)) {
                    SetFlag(i, ElementFlags.Disabled, true);
                }
            }
        }

        private void LayoutElements()
        {
            for (var i = 0; i < elementCount; ++i) {
                AttrLayoutFunc[i]?.Invoke(new Element(this, i));
            }
        }

        private void CalcRects()
        {
            // we know parents come before children, so it's OK to just iterate like this and refer back to parents
            AttrAbsRect[0] = AttrRect[0];
            for (var i = 1; i < elementCount; ++i) {
                AttrAbsRect[i] = AttrRect[i];
                AttrAbsRect[i].X += AttrAbsRect[AttrParent[i]].X;
                AttrAbsRect[i].Y += AttrAbsRect[AttrParent[i]].Y;
            }

            AttrClipRect[0] = AttrAbsRect[0];
            for (var i = 1; i < elementCount; ++i) {
                var parentClipRect = AttrClipRect[AttrParent[i]];
                if (GetFlag(i, ElementFlags.ClipContent)) {
                    AttrClipRect[i] = parentClipRect.Intersection(AttrAbsRect[i]);
                } else {
                    AttrClipRect[i] = parentClipRect;
                }
            }
        }

        
        // mapping of (z-index, level) -> element index, to be sorted and used to determine rendering order
        private readonly List<KeyedValue<Pair<int, int>, int>> bottomToTopIndex = new List<KeyedValue<Pair<int, int>, int>>();
        private void CalcBottomToTopIndex()
        {
            bottomToTopIndex.Clear();
            for (var i = 0; i < elementCount; ++i) {
                bottomToTopIndex.Add(
                    new KeyedValue<Pair<int, int>, int>(
                        new Pair<int, int>(AttrZIndex[i], AttrLevel[i]), i));
            }
            bottomToTopIndex.Sort();
        }
        
        public Element? HitTest(Vec2 absPos)
        {
            for (var i = bottomToTopIndex.Count - 1; i >= 0; --i) {
                var elemIndex = bottomToTopIndex[i].Value;
                if (AttrAbsRect[elemIndex].Contains(absPos)) {
                    return new Element(this, elemIndex);
                }
            }
            return null;
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
            foreach (var entry in bottomToTopIndex) {
                var elemIndex = entry.Value;
                if (AttrDrawFunc[elemIndex] != null) {
                    drawContext.Target = new Element(this, elemIndex);
                    AttrDrawFunc[elemIndex](drawContext);
                }
            }

            DirtyDrawCommandBuffers.Clear();
            if (!currDrawCommandBuffer.HasEqualCommands(prevDrawCommandBuffer)) {
                DirtyDrawCommandBuffers.Add(currDrawCommandBuffer);
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
            var key = TwoIntsToLong(fontId, stringId);
            Vec2 size;
            if (textSizeCache.TryGetValue(key, out size)) {
                return size;
            }
            size = Delegate.TextSize(text, fontId);
            textSizeCache[key] = size;
            return size;
        }

        private static long TwoIntsToLong(int a, int b)
        {
            unchecked {
                return ((long)a << 32) | (uint)b;
            }
        }


        internal bool GetFlag(int elemIndex, ElementFlags flag)
        {
            return (AttrFlags[elemIndex] & flag) != 0;
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
                if (!prevIdToIndexMap.ContainsKey(AttrId[entry.Key])) {
                    entry.Value(new Element(this, entry.Key));
                }
            }
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

        private void ClearTraverseHandlers()
        {
            depthDescentHandlers.Clear();
            depthAscentHandlers.Clear();
            treeDescentHandlers.Clear();
            treeAscentHandlers.Clear();
        }
        public void RunUpdateTraversals()
        {
            for (var i = 0; i < depthDescentHandlers.Count; ++i) { // rewrite now that we can know z-index
                var elemIndex = depthDescentHandlers[i].ElemIndex;
                var key = TwoIntsToLong(AttrZIndex[elemIndex], AttrLevel[elemIndex]);
                depthDescentHandlers[i] = new TraverseEntry<long>(key, elemIndex, depthDescentHandlers[i].Handler);
            }
            postPassHandlers.Clear();
            depthDescentHandlers.Sort();
            for (var i = depthDescentHandlers.Count - 1; i >= 0; --i) {
                depthDescentHandlers[i].Handler(new Element(this, depthDescentHandlers[i].ElemIndex));
            }
            RunPostPassHandlers();

            for (var i = 0; i < depthAscentHandlers.Count; ++i) { // rewrite now that we can know z-index
                var elemIndex = depthAscentHandlers[i].ElemIndex;
                var key = TwoIntsToLong(AttrZIndex[elemIndex], AttrLevel[elemIndex]);
                depthAscentHandlers[i] = new TraverseEntry<long>(key, elemIndex, depthAscentHandlers[i].Handler);
            }
            postPassHandlers.Clear();
            depthAscentHandlers.Sort();
            for (var i = 0; i < depthAscentHandlers.Count; ++i) {
                depthAscentHandlers[i].Handler(new Element(this, depthAscentHandlers[i].ElemIndex));
            }
            RunPostPassHandlers();

            postPassHandlers.Clear();
            treeDescentHandlers.Sort();
            for (var i = 0; i < treeDescentHandlers.Count; ++i) {
                treeDescentHandlers[i].Handler(new Element(this, treeDescentHandlers[i].ElemIndex));
            }
            RunPostPassHandlers();

            postPassHandlers.Clear();
            treeAscentHandlers.Sort();
            for (var i = treeAscentHandlers.Count - 1; i >= 0; --i) {
                treeAscentHandlers[i].Handler(new Element(this, treeAscentHandlers[i].ElemIndex));
            }
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

        #region State
        private Dictionary<ElementId, ValueStorage<StateKeys, ElementId>> prevStateHolders = new Dictionary<ElementId, ValueStorage<StateKeys, ElementId>>();
        private Dictionary<ElementId, ValueStorage<StateKeys, ElementId>> currStateHolders = new Dictionary<ElementId, ValueStorage<StateKeys, ElementId>>();
        private readonly Stack<ValueStorage<StateKeys, ElementId>> cachedStateHolders = new Stack<ValueStorage<StateKeys, ElementId>>(); // for reuse, so we don't generate garbage
        
        private void FlipStateHolders()
        {
            // any state holder left in prevStateHolders after a frame, was not reattached to its element, and should be cleaned up / reused
            foreach (var stateHolder in prevStateHolders) {
                stateHolder.Value.Clear();
                cachedStateHolders.Push(stateHolder.Value);
            }
            var temp = prevStateHolders;
            prevStateHolders = currStateHolders;
            currStateHolders = temp;
            currStateHolders.Clear();
        }
        internal void AttachStateHolder(int elemIndex)
        {
            if (elemIndex == 0) {
                return; // root already has one
            }
            if (!ReferenceEquals(AttrStateHolder[AttrParent[elemIndex]], AttrStateHolder[elemIndex])) {
                return; // if we don't have the same as our parent, the we have already gotten one attached
            }
            var id = AttrId[elemIndex];
            ValueStorage<StateKeys, ElementId> stateHolder;
            if (prevStateHolders.TryGetValue(id, out stateHolder)) {
                prevStateHolders.Remove(id); // remove it. the ones left at end of frame will be dropped
            } else if (cachedStateHolders.Count > 0) {
                stateHolder = cachedStateHolders.Pop();
            } else {
                stateHolder = new ValueStorage<StateKeys, ElementId>();
            }
            AttrStateHolder[elemIndex] = stateHolder;
            currStateHolders[id] = stateHolder;
        }
        #endregion
    }
}
