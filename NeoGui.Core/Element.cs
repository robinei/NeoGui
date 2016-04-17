using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeoGui.Core
{
    public struct Element : IComparable<Element>, IEquatable<Element>
    {
        public readonly NeoGuiContext Context;
        internal readonly int Index;

        internal Element(NeoGuiContext context, int index)
        {
            Debug.Assert(context != null);
            Debug.Assert(index >= 0);
            Debug.Assert(index < context.AttrId.Length);
            Context = context;
            Index = index;
        }

        public static Element Create(Element parent, object key = null)
        {
            return parent.Context.CreateElement(parent, key);
        }
        
        public ElementId Id => Context.AttrId[Index];
        public object Key => Id.Key;
        public bool IsRoot => Index == 0;
        public Element Parent => new Element(Context, Context.AttrParent[Index]);
        public Element FirstChild => new Element(Context, Context.AttrFirstChild[Index]);
        public Element NextSibling => new Element(Context, Context.AttrNextSibling[Index]);
        public bool HasChildren => Context.AttrFirstChild[Index] > 0;
        public bool HasNextSibling => Context.AttrNextSibling[Index] > 0;

        public bool IsUnderMouse => AbsoluteRect.Contains(Context.Input.MousePos) &&
                                    ClipRect.Contains(Context.Input.MousePos);


        public void OnDepthDescent(Action<Element> handler) { Context.AddDepthDescentHandler(Index, handler); }
        public void OnDepthAscent(Action<Element> handler) { Context.AddDepthAscentHandler(Index, handler); }
        public void OnTreeDescent(Action<Element> handler) { Context.AddTreeDescentHandler(Index, handler); }
        public void OnTreeAscent(Action<Element> handler) { Context.AddTreeAscentHandler(Index, handler); }
        
        /// <summary>
        /// Invoke in one of the above *Descent/*Ascent pass handlers to have this handler be called after the current pass has finished.
        /// </summary>
        public void OnPassFinished(Action<Element> handler) { Context.RunAfterPass(Index, handler); }

        /// <summary>
        /// Run when a node is inserted into the tree, after having not been in it for at least one frame.
        /// </summary>
        public void OnInserted(Action<Element> handler) { Context.AddInsertHandler(Index, handler); }


        public bool Has<TComponent>()
        {
            return Context.DataStorage.HasValue<TComponent>(Index);
        }
        public TComponent Get<TComponent>(TComponent defaultValue = default(TComponent))
        {
            return Context.DataStorage.GetValue(Index, defaultValue);
        }
        public TComponent GetOrCreate<TComponent>()
            where TComponent: new()
        {
            return Context.DataStorage.GetOrCreateValue<TComponent>(Index);
        }
        public void Set<TComponent>(TComponent value)
        {
            Context.DataStorage.SetValue(Index, value);
        }
        

        public bool HasState<TState>()
        {
            return Context.AttrStateHolder[Index].HasValue<TState>(Context.AttrId[Index]);
        }
        public TState GetState<TState>(TState defaultValue = default(TState))
        {
            return Context.AttrStateHolder[Index].GetValue(Context.AttrId[Index], defaultValue);
        }
        public TState GetOrCreateState<TState>()
            where TState: new()
        {
            return Context.AttrStateHolder[Index].GetOrCreateValue<TState>(Context.AttrId[Index]);
        }
        public void SetState<TState>(TState value)
        {
            Context.AttrStateHolder[Index].SetValue(Context.AttrId[Index], value);
        }
        public void AttachStateHolder()
        {
            Context.AttachStateHolder(Index);
        }
        public TState FindState<TState>(TState defaultValue = default(TState))
        {
            var elem = this;
            while (true) {
                if (elem.HasState<TState>()) {
                    return elem.GetState(defaultValue);
                }
                if (elem.IsRoot) {
                    return defaultValue;
                }
                elem = elem.Parent;
            }
        }
        
        
        public float ToLocalScale(float f) // pos assumed to be in absolute coordinates
        {
            return f; // TODO: change this impl when scaling is added
        }
        public float ToLocalScale(float f, Element sourceCoordSys)
        {
            return f; // TODO: change this impl when scaling is added
        }

        public Vec2 ToLocalCoord(Vec2 pos) // pos assumed to be in absolute coordinates
        {
            return pos + (Context.AttrAbsRect[0].Pos - Context.AttrAbsRect[Index].Pos);
        }
        public Vec2 ToLocalCoord(Vec2 pos, Element sourceCoordSys)
        {
            return pos + (Context.AttrAbsRect[sourceCoordSys.Index].Pos - Context.AttrAbsRect[Index].Pos);
        }

        public Rect ToLocalCoord(Rect rect) // rect assumed to be in absolute coordinates
        {
            return rect + (Context.AttrAbsRect[0].Pos - Context.AttrAbsRect[Index].Pos);
        }
        public Rect ToLocalCoord(Rect rect, Element sourceCoordSys)
        {
            return rect + (Context.AttrAbsRect[sourceCoordSys.Index].Pos - Context.AttrAbsRect[Index].Pos);
        }


        #region Misc forwarded properties
        public string Name
        {
            get { return Context.AttrName[Index]; }
            set { Context.AttrName[Index] = value; }
        }
        public bool ClipContent
        {
            get { return Context.GetFlag(Index, ElementFlags.ClipContent); }
            set { Context.SetFlag(Index, ElementFlags.ClipContent, value); }
        }
        public bool Disabled
        {
            get { return Context.GetFlag(Index, ElementFlags.Disabled); }
            set { Context.SetFlag(Index, ElementFlags.Disabled, value); }
        }
        public int ZIndex
        {
            get { return Context.AttrZIndex[Index]; }
            set { Context.AttrZIndex[Index] = value; }
        }
        public float X
        {
            get { return Context.AttrRect[Index].X; }
            set { Context.AttrRect[Index].X = value; }
        }
        public float Y
        {
            get { return Context.AttrRect[Index].Y; }
            set { Context.AttrRect[Index].Y = value; }
        }
        public float Width
        {
            get { return Context.AttrRect[Index].Width; }
            set { Context.AttrRect[Index].Width = value; }
        }
        public float Height
        {
            get { return Context.AttrRect[Index].Height; }
            set { Context.AttrRect[Index].Height = value; }
        }
        public Rect Rect
        {
            get { return Context.AttrRect[Index]; }
            set { Context.AttrRect[Index] = value; }
        }
        public Vec2 Pos
        {
            get { return Context.AttrRect[Index].Pos; }
            set { Context.AttrRect[Index].Pos = value; }
        }
        public Vec2 Size
        {
            get { return Context.AttrRect[Index].Size; }
            set { Context.AttrRect[Index].Size = value; }
        }

        public Rect AbsoluteRect => Context.AttrAbsRect[Index];
        public Rect ClipRect => Context.AttrClipRect[Index];

        public Action<DrawContext> Draw
        {
            get { return Context.AttrDrawFunc[Index]; }
            set { Context.AttrDrawFunc[Index] = value; }
        }

        public Action<Element> Layout
        {
            get { return Context.AttrLayoutFunc[Index]; }
            set { Context.AttrLayoutFunc[Index] = value; }
        }
        #endregion 

        #region Comparison and equality
        public int CompareTo(Element other)
        {
            if (Index < other.Index) {
                return -1;
            }
            if (Index > other.Index) {
                return 1;
            }
            return Context.GetHashCode().CompareTo(other.Context.GetHashCode());
        }
        public bool Equals(Element other)
        {
            return Context == other.Context && Index == other.Index;
        }
        public override bool Equals(object other)
        {
            return other is Element && Equals((Element)other);
        }
        public override int GetHashCode()
        {
            unchecked {
                return (Context.GetHashCode() * 397) ^ Index;
            }
        }
        public static bool operator ==(Element a, Element b) { return a.Equals(b); }
        public static bool operator !=(Element a, Element b) { return !a.Equals(b); }
        #endregion


        public Element? FindChild(Func<Element, bool> predicate)
        {
            if (!HasChildren) {
                return null;
            }
            var child = FirstChild;
            while (true) {
                if (predicate(child)) {
                    return child;
                }
                if (!child.HasNextSibling) {
                    return null;
                }
                child = child.NextSibling;
            }
        }


        #region Child enumeration
        public IEnumerable<Element> Children => new ChildEnumerable(Context, Index);

        private struct ChildEnumerable : IEnumerable<Element>
        {
            private readonly NeoGuiContext context;
            private readonly int parentIndex;

            public ChildEnumerable(NeoGuiContext context, int parentIndex)
            {
                this.context = context;
                this.parentIndex = parentIndex;
            }

            public IEnumerator<Element> GetEnumerator() { return new ChildEnumerator(context, parentIndex); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        }

        private struct ChildEnumerator : IEnumerator<Element>
        {
            private readonly NeoGuiContext context;
            private readonly int parentIndex;
            private int elemIndex;

            public ChildEnumerator(NeoGuiContext context, int parentIndex)
            {
                Debug.Assert(parentIndex >= 0);
                this.context = context;
                this.parentIndex = parentIndex;
                elemIndex = 0;
            }

            public void Reset() { elemIndex = 0; }

            public bool MoveNext()
            {
                if (elemIndex > 0) {
                    elemIndex = context.AttrNextSibling[elemIndex];
                    Debug.Assert(elemIndex != 0);
                    return elemIndex > 0;
                }
                if (elemIndex == 0) {
                    elemIndex = context.AttrFirstChild[parentIndex];
                    Debug.Assert(elemIndex != 0);
                    return elemIndex > 0;
                }
                return false;
            }

            public Element Current
            {
                get
                {
                    if (elemIndex <= 0) {
                        throw new InvalidOperationException();
                    }
                    return new Element(context, elemIndex);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
    #endregion
}
