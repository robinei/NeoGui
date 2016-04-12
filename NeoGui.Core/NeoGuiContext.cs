using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

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


    public class NeoGuiContext
    {
        // categories for TypeKeys
        private struct StateKeys { }
        private struct DataKeys { }
        private struct EventKeys { }

        private const int InitialArraySize = 128;

        public readonly INeoGuiDelegate Delegate;

        private readonly object rootKey = new object();
        private readonly ValueStorage<StateKeys, ElementId> rootStateHolder = new ValueStorage<StateKeys, ElementId>();

        private int elementCount;
        private ElementId[] attrId = new ElementId[InitialArraySize];
        private int[] attrParent = new int[InitialArraySize];
        private int[] attrFirstChild = new int[InitialArraySize];
        private int[] attrNextSibling = new int[InitialArraySize];
        private int[] attrLevel = new int[InitialArraySize];
        private int[] attrZIndex = new int[InitialArraySize];
        private bool[] attrClipContent = new bool[InitialArraySize];
        private int[] attrKeyCounterIndex = new int[InitialArraySize];
        private ValueStorage<StateKeys, ElementId>[] attrStateHolder = new ValueStorage<StateKeys, ElementId>[InitialArraySize];
        private Rect[] attrRect = new Rect[InitialArraySize];
        private Rect[] attrAbsRect = new Rect[InitialArraySize]; // absolute coordinates
        private Rect[] attrClipRect = new Rect[InitialArraySize];
        private Action<DrawContext>[] attrDrawFunc = new Action<DrawContext>[InitialArraySize];
        
        private readonly List<int> keyCounters = new List<int>();


        private bool hasSetInputStateOnce;
        private InputState prevInput = new InputState();
        private InputState currInput = new InputState();
        public InputState Input => currInput;
        public void SetCurrentInputState(RawInputState rawInput)
        {
            RotateInputsAndSetState(rawInput);
            if (!hasSetInputStateOnce) {
                RotateInputsAndSetState(rawInput);
                hasSetInputStateOnce = true;
            }
        }
        private void RotateInputsAndSetState(RawInputState rawInput)
        {
            var temp = prevInput;
            prevInput = currInput;
            currInput = temp;
            currInput.Reset(prevInput, rawInput);
        }

        

        private bool inFrame;


        public NeoGuiContext(INeoGuiDelegate del)
        {
            Delegate = del;
        }

        public void BeginFrame()
        {
            inFrame = true;
            
            FlipStateHolders();

            elementCount = 0;
            keyCounters.Clear();
            attrStateHolder[0] = rootStateHolder;
            attrLevel[0] = -1; // will be overwritten by 0 on next line, since parent is its own child
            CreateElement(new Element(this, 0), rootKey);

            ClearDataStorage();
            ClearEventListeners();
            ClearTraverseHandlers();
        }

        public void EndFrame()
        {
            CalcBottomToTopIndex();
            SortEventListeners();
            CalcRects();
            DrawElements();
            inFrame = false;
        }

        internal ElementId[] AttrId => attrId;
        internal int[] AttrParent => attrParent;
        internal int[] AttrFirstChild => attrFirstChild;
        internal int[] AttrNextSibling => attrNextSibling;
        internal int[] AttrZIndex => attrZIndex;
        internal bool[] AttrClipContent => attrClipContent;
        internal Rect[] AttrRect => attrRect;
        internal Rect[] AttrAbsRect => attrAbsRect;
        internal Rect[] AttrClipRect => attrClipRect;
        internal Action<DrawContext>[] AttrDrawFunc => attrDrawFunc;
        
        public Element Root => new Element(this, 0);

        internal Element CreateElement(Element parent, object key)
        {
            if (elementCount == attrId.Length) {
                var newLength = attrId.Length * 2;
                Array.Resize(ref attrId, newLength);
                Array.Resize(ref attrParent, newLength);
                Array.Resize(ref attrFirstChild, newLength);
                Array.Resize(ref attrNextSibling, newLength);
                Array.Resize(ref attrLevel, newLength);
                Array.Resize(ref attrZIndex, newLength);
                Array.Resize(ref attrClipContent, newLength);
                Array.Resize(ref attrKeyCounterIndex, newLength);
                Array.Resize(ref attrStateHolder, newLength);
                Array.Resize(ref attrRect, newLength);
                Array.Resize(ref attrAbsRect, newLength);
                Array.Resize(ref attrClipRect, newLength);
                Array.Resize(ref attrDrawFunc, newLength);
            }

            int keyIndex;
            int keyCounterIndex;
            if (key == null) {
                key = parent.Key;
                keyCounterIndex = attrKeyCounterIndex[parent.Index];
                keyIndex = ++keyCounters[keyCounterIndex];
            } else {
                keyIndex = 0;
                keyCounterIndex = keyCounters.Count;
                keyCounters.Add(0);
            }

            attrId[elementCount] = new ElementId(key, keyIndex);
            attrParent[elementCount] = parent.Index;
            attrFirstChild[elementCount] = 0; // we have no children yet
            attrNextSibling[elementCount] = attrFirstChild[parent.Index]; // set parent's first child as next sibling
            attrLevel[elementCount] = attrLevel[parent.Index] + 1;
            attrZIndex[elementCount] = 0;
            attrClipContent[elementCount] = false;
            attrFirstChild[parent.Index] = elementCount; // set this element as parent's first child
            attrKeyCounterIndex[elementCount] = keyCounterIndex;
            attrStateHolder[elementCount] = attrStateHolder[parent.Index]; // inherit parent state holder
            attrRect[elementCount] = new Rect();
            attrDrawFunc[elementCount] = null;

            return new Element(this, elementCount++);
        }


        private void CalcRects()
        {
            // we know parents come before children, so it's OK to just iterate like this and refer back to parents
            attrAbsRect[0] = attrRect[0];
            for (var i = 1; i < elementCount; ++i) {
                attrAbsRect[i] = attrRect[i];
                attrAbsRect[i].X += attrAbsRect[attrParent[i]].X;
                attrAbsRect[i].Y += attrAbsRect[attrParent[i]].Y;
            }

            attrClipRect[0] = attrAbsRect[0];
            for (var i = 1; i < elementCount; ++i) {
                var parentClipRect = attrClipRect[attrParent[i]];
                if (attrClipContent[i]) {
                    attrClipRect[i] = parentClipRect.Intersection(attrAbsRect[i]);
                } else {
                    attrClipRect[i] = parentClipRect;
                }
            }
        }
        
        // mapping of (z-index, level) -> element index, to be sorted and used to determine rendering order
        private readonly List<KeyedValue<Pair<int, int>, int>> bottomToTopIndex = new List<KeyedValue<Pair<int, int>, int>>();
        private void CalcBottomToTopIndex()
        {
            bottomToTopIndex.Clear();
            for (var elemIndex = 0; elemIndex < elementCount; ++elemIndex) {
                bottomToTopIndex.Add(
                    new KeyedValue<Pair<int, int>, int>(
                        new Pair<int, int>(attrZIndex[elemIndex], attrLevel[elemIndex]),
                        elemIndex
                    )
                );
            }
            bottomToTopIndex.Sort();
        }
        
        public Element? HitTest(Vec2 absPos)
        {
            for (var i = bottomToTopIndex.Count - 1; i >= 0; --i) {
                var elemIndex = bottomToTopIndex[i].Value;
                if (attrAbsRect[elemIndex].Contains(absPos)) {
                    return new Element(this, elemIndex);
                }
            }
            return null;
        }
        

        public readonly List<DrawCommand>  DrawCommandBuffer = new List<DrawCommand>();
        private readonly DrawContext drawContext = new DrawContext();
        private void DrawElements()
        {
            DrawCommandBuffer.Clear();
            drawContext.CommandBuffer = DrawCommandBuffer;
            foreach (var entry in bottomToTopIndex) {
                var elemIndex = entry.Value;
                if (attrDrawFunc[elemIndex] != null) {
                    drawContext.Target = new Element(this, elemIndex);
                    attrDrawFunc[elemIndex](drawContext);
                }
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
            treeDescentHandlers.Add(new TraverseEntry<int>(attrLevel[elemIndex], elemIndex, handler));
        }
        internal void AddTreeAscentHandler(int elemIndex, Action<Element> handler)
        {
            treeAscentHandlers.Add(new TraverseEntry<int>(attrLevel[elemIndex], elemIndex, handler));
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
                long key = (attrZIndex[elemIndex] << 32) | attrLevel[elemIndex];
                depthDescentHandlers[i] = new TraverseEntry<long>(key, elemIndex, depthDescentHandlers[i].Handler);
            }
            depthDescentHandlers.Sort();
            for (var i = depthDescentHandlers.Count - 1; i >= 0; --i) {
                depthDescentHandlers[i].Handler(new Element(this, depthDescentHandlers[i].ElemIndex));
            }

            for (var i = 0; i < depthAscentHandlers.Count; ++i) { // rewrite now that we can know z-index
                var elemIndex = depthAscentHandlers[i].ElemIndex;
                long key = (attrZIndex[elemIndex] << 32) | attrLevel[elemIndex];
                depthAscentHandlers[i] = new TraverseEntry<long>(key, elemIndex, depthAscentHandlers[i].Handler);
            }
            depthAscentHandlers.Sort();
            for (var i = 0; i < depthAscentHandlers.Count; ++i) {
                depthAscentHandlers[i].Handler(new Element(this, depthAscentHandlers[i].ElemIndex));
            }

            treeDescentHandlers.Sort();
            for (var i = 0; i < treeDescentHandlers.Count; ++i) {
                treeDescentHandlers[i].Handler(new Element(this, treeDescentHandlers[i].ElemIndex));
            }

            treeAscentHandlers.Sort();
            for (var i = treeAscentHandlers.Count - 1; i >= 0; --i) {
                treeAscentHandlers[i].Handler(new Element(this, treeAscentHandlers[i].ElemIndex));
            }
        }
        #endregion

        #region State
        private Dictionary<int, ValueStorage<StateKeys, ElementId>> prevStateHolders = new Dictionary<int, ValueStorage<StateKeys, ElementId>>();
        private Dictionary<int, ValueStorage<StateKeys, ElementId>> currStateHolders = new Dictionary<int, ValueStorage<StateKeys, ElementId>>();
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
            Debug.Assert(elemIndex > 0);
            Debug.Assert(ReferenceEquals(attrStateHolder[attrParent[elemIndex]], attrStateHolder[elemIndex]));
            ValueStorage<StateKeys, ElementId> stateHolder;
            if (prevStateHolders.TryGetValue(elemIndex, out stateHolder)) {
                prevStateHolders.Remove(elemIndex); // remove it. the ones left at end of frame will be dropped
            } else if (cachedStateHolders.Count > 0) {
                stateHolder = cachedStateHolders.Pop();
            } else {
                stateHolder = new ValueStorage<StateKeys, ElementId>();
            }
            attrStateHolder[elemIndex] = stateHolder;
            currStateHolders[elemIndex] = stateHolder;
        }
        internal bool HasState<TState>(int elemIndex)
        {
            return attrStateHolder[elemIndex].HasValue<TState>(attrId[elemIndex]);
        }
        internal TState GetState<TState>(int elemIndex, bool create = false)
            where TState: new()
        {
            return attrStateHolder[elemIndex].GetValue<TState>(attrId[elemIndex], create);
        }
        internal void SetState<TState>(int elemIndex, TState value)
        {
            attrStateHolder[elemIndex].SetValue(attrId[elemIndex], value);
        }
        #endregion

        #region Data
        private readonly ValueStorage<DataKeys, int> dataStorage = new ValueStorage<DataKeys, int>();
        
        internal bool HasData<TComponent>(int elemIndex)
        {
            return dataStorage.HasValue<TComponent>(elemIndex);
        }
        internal TComponent GetData<TComponent>(int elemIndex, bool create)
            where TComponent: new()
        {
            return dataStorage.GetValue<TComponent>(elemIndex, create);
        }
        internal void SetData<TComponent>(int elemIndex, TComponent value)
        {
            dataStorage.SetValue(elemIndex, value);
        }
        private void ClearDataStorage()
        {
            dataStorage.Clear();
        }
        #endregion

        #region Events
        private struct EventListener
        {
            public Delegate Action;
            public bool Capture;
        }
        private int eventListenerCount;
        private KeyedValue<Pair<int, int>, EventListener>[] eventListeners = new KeyedValue<Pair<int, int>, EventListener>[128];

        private void ClearEventListeners()
        {
            eventListenerCount = 0;
        }

        private void SortEventListeners()
        {
            Array.Sort(eventListeners);
        }

        internal void AddEventListener<TEvent>(int elemIndex, Type eventType, Action<TEvent> action, bool capture = false)
            where TEvent : Event
        {
            Debug.Assert(action != null);
            if (eventListenerCount == eventListeners.Length) {
                Array.Resize(ref eventListeners, eventListeners.Length*2);
            }
            eventListeners[eventListenerCount++] = new KeyedValue<Pair<int, int>, EventListener>(
                new Pair<int, int>(elemIndex, TypeKeys<EventKeys, TEvent>.Key),
                new EventListener { Action = action, Capture = capture }
            );
        }

        private void LookupEventListeners(Pair<int, int> key, List<EventListener> result)
        {
            var i = Array.BinarySearch(eventListeners, new KeyedValue<Pair<int, int>, EventListener>(key));
            if (i < 0) return;
            for (; i < eventListenerCount && eventListeners[i].Key == key; ++i) {
                result.Add(eventListeners[i].Value);
            }
        }
        
        // used for storing the dispatcher lists so they can be reused across dispatches
        private struct DispatchLists
        {
            public List<Range> Ranges;
            public List<EventListener> Listeners;
        }
        private struct Range { public int Start, End; }
        private readonly Stack<DispatchLists> dispatchStack = new Stack<DispatchLists>();

        internal void DispatchEvent<TEvent>(Element elem, TEvent e)
            where TEvent : Event
        {
            Debug.Assert(!inFrame);

            e.Target = elem;
            e.PropagationStopped = false;

            DispatchLists lists;
            if (dispatchStack.Count > 0) {
                lists = dispatchStack.Pop();
            } else {
                lists = new DispatchLists {
                    Ranges = new List<Range>(),
                    Listeners = new List<EventListener>()
                };
            }

            try {
                var ranges = lists.Ranges;
                var listeners = lists.Listeners;
                ranges.Clear();
                listeners.Clear();

                // collect all listeners from elem to root in an array, and collect
                // ranges which record which listeners belong to which elems
                while (true) {
                    Range r;
                    r.Start = listeners.Count;
                    LookupEventListeners(new Pair<int, int>(elem.Index, TypeKeys<EventKeys, TEvent>.Key), listeners);
                    r.End = listeners.Count;
                    if (r.End > r.Start) {
                        ranges.Add(r);
                    }
                    if (elem.IsRoot) {
                        break;
                    }
                    elem = elem.Parent;
                }

                if (ranges.Count == 0) {
                    return;
                }

                // capture phase
                for (var i = ranges.Count - 1; i > 0; --i) {
                    var r = ranges[i];
                    for (var j = r.Start; j < r.End; ++j) {
                        if (listeners[j].Capture) {
                            ((Action<TEvent>)listeners[j].Action)(e);
                        }
                    }
                    if (e.PropagationStopped) {
                        return;
                    }
                }

                // at target
                {
                    var r = ranges[0];
                    for (var j = r.Start; j < r.End; ++j) {
                        if (!listeners[j].Capture) {
                            ((Action<TEvent>)listeners[j].Action)(e);
                        }
                    }
                    if (e.PropagationStopped) {
                        return;
                    }
                }

                if (!e.Bubbles) {
                    return;
                }

                // bubble phase
                for (var i = 1; i < ranges.Count; ++i) {
                    var r = ranges[i];
                    for (var j = r.Start; j < r.End; ++j) {
                        if (!listeners[j].Capture) {
                            ((Action<TEvent>)listeners[j].Action)(e);
                        }
                    }
                    if (e.PropagationStopped) {
                        return;
                    }
                }
            } finally {
                dispatchStack.Push(lists);
            }
        }
        #endregion
    }
}
