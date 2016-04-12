﻿using System;
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

        public bool IsRoot => Index == 0;
        public Element Parent => new Element(Context, Context.AttrParent[Index]);
        public ElementId Id => Context.AttrId[Index];
        public object Key => Id.Key;

        public Element CreateElement(object key = null)
        {
            return Context.CreateElement(this, key);
        }

        public void AddEventListener<TEvent>(Action<TEvent> listener, bool capture = false)
            where TEvent : Event
        {
            Context.AddEventListener(Index, typeof(TEvent), listener, capture);
        }
        public void DispatchEvent<TEvent>(TEvent e)
            where TEvent : Event
        {
            Context.DispatchEvent(this, e);
        }

        public bool IntersectsMouse => AbsoluteRect.Contains(Context.Input.MousePos) &&
                                       ClipRect.Contains(Context.Input.MousePos);

        public void OnDepthDescent(Action<Element> handler) { Context.AddDepthDescentHandler(Index, handler); }
        public void OnDepthAscent(Action<Element> handler) { Context.AddDepthAscentHandler(Index, handler); }
        public void OnTreeDescent(Action<Element> handler) { Context.AddTreeDescentHandler(Index, handler); }
        public void OnTreeAscent(Action<Element> handler) { Context.AddTreeAscentHandler(Index, handler); }
        
        public bool Has<TComponent>()
        {
            return Context.HasData<TComponent>(Index);
        }
        public TComponent Get<TComponent>(bool create = false)
            where TComponent: new()
        {
            return Context.GetData<TComponent>(Index, create);
        }
        public void Set<TComponent>(TComponent value)
        {
            Context.SetData(Index, value);
        }
        
        public bool HasState<TState>()
        {
            return Context.HasState<TState>(Index);
        }
        public TState GetState<TState>(bool create = false)
            where TState: new()
        {
            return Context.GetState<TState>(Index, create);
        }
        public void SetState<TState>(TState value)
        {
            Context.SetState(Index, value);
        }
        public void AttachStateHolder()
        {
            Context.AttachStateHolder(Index);
        }
        
        public Vec2 ToLocalCoord(Vec2 pos)
        {
            return pos + (Context.AttrAbsRect[0].Pos - Context.AttrAbsRect[Index].Pos);
        }
        public Rect ToLocalCoord(Rect rect)
        {
            return rect + (Context.AttrAbsRect[0].Pos - Context.AttrAbsRect[Index].Pos);
        }
        public Vec2 ToLocalCoord(Vec2 pos, Element sourceCoordSys)
        {
            return pos + (Context.AttrAbsRect[sourceCoordSys.Index].Pos - Context.AttrAbsRect[Index].Pos);
        }
        public Rect ToLocalCoord(Rect rect, Element sourceCoordSys)
        {
            return rect + (Context.AttrAbsRect[sourceCoordSys.Index].Pos - Context.AttrAbsRect[Index].Pos);
        }

        #region Misc forwarded properties
        public bool ClipContent
        {
            get { return Context.AttrClipContent[Index]; }
            set { Context.AttrClipContent[Index] = value; }
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
        
        public static bool operator ==(Element a, Element b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Element a, Element b)
        {
            return !a.Equals(b);
        }
        #endregion
        
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

            public IEnumerator<Element> GetEnumerator()
            {
                return new ChildEnumerator(context, parentIndex);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
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
                elemIndex = -1;
            }

            public void Reset()
            {
                elemIndex = -1;
            }

            public bool MoveNext()
            {
                if (elemIndex > 0) {
                    elemIndex = context.AttrNextSibling[elemIndex];
                    Debug.Assert(elemIndex >= 0);
                    return elemIndex > 0;
                }
                if (elemIndex < 0) {
                    elemIndex = context.AttrFirstChild[parentIndex];
                    return elemIndex > 0;
                }
                return false;
            }

            public Element Current => new Element(context, elemIndex);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
    #endregion
}