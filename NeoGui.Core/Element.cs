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
            Debug.Assert(index < context.ElementCount);
            Context = context;
            Index = index;
        }

        public static Element Create(Element parent, object key = null, StateDomain domain = null) =>
            parent.Context.CreateElement(parent, key, domain);
        
        public bool IsRoot => Index == 0;
        public Element Parent => new Element(Context, Context.AttrParent[Index]);
        public Element FirstChild => new Element(Context, Context.AttrFirstChild[Index]);
        public Element NextSibling => new Element(Context, Context.AttrNextSibling[Index]);
        public bool HasChildren => Context.AttrFirstChild[Index] > 0;
        public bool HasNextSibling => Context.AttrNextSibling[Index] > 0;


        public void OnDepthDescent(Action<Element> handler) => Context.AddDepthDescentHandler(Index, handler);
        public void OnDepthAscent(Action<Element> handler) => Context.AddDepthAscentHandler(Index, handler);
        public void OnTreeDescent(Action<Element> handler) => Context.AddTreeDescentHandler(Index, handler);
        public void OnTreeAscent(Action<Element> handler) => Context.AddTreeAscentHandler(Index, handler);
        
        /// <summary>
        /// Invoke in one of the above *Descent/*Ascent pass handlers to have this handler be called after the current pass has finished.
        /// </summary>
        public void OnPassFinished(Action<Element> handler) => Context.RunAfterPass(Index, handler);

        /// <summary>
        /// Run when an element is inserted into the tree, after having not been in it for at least one frame.
        /// </summary>
        public void OnInserted(Action<Element> handler) => Context.AddInsertHandler(Index, handler);

        /// <summary>
        /// Run when an element is removed from the tree, after having been in it for at least one frame.
        /// Use this to clean up state, if necessary.
        /// </summary>
        public void OnRemoved(Action<ElementStateProxy> handler) => Context.AddRemoveHandler(Context.AttrStateId[Index], handler);


        public bool Has<TComponent>() => Context.DataStorage.HasValue<TComponent>(Index);
        public bool TryGet<TComponent>(out TComponent value) => Context.DataStorage.TryGetValue(Index, out value);
        public TComponent Get<TComponent>(TComponent defaultValue = default) => Context.DataStorage.GetValue(Index, defaultValue);
        public TComponent GetOrCreate<TComponent>() where TComponent: new() => Context.DataStorage.GetOrCreateValue<TComponent>(Index);
        public void Set<TComponent>(TComponent value) => Context.DataStorage.SetValue(Index, value);
        

        public bool HasState<TState>() => Context.AttrStateDomain[Index].Storage.HasValue<TState>(Context.AttrStateId[Index]);
        public bool TryGetState<TState>(out TState value) => Context.AttrStateDomain[Index].Storage.TryGetValue(Context.AttrStateId[Index], out value);
        public TState GetState<TState>(TState defaultValue = default) => Context.AttrStateDomain[Index].Storage.GetValue(Context.AttrStateId[Index], defaultValue);
        public TState GetOrCreateState<TState>() where TState: new() => Context.AttrStateDomain[Index].Storage.GetOrCreateValue<TState>(Context.AttrStateId[Index]);
        public void SetState<TState>(TState value) => Context.AttrStateDomain[Index].Storage.SetValue(Context.AttrStateId[Index], value);
        public TState FindState<TState>(TState defaultValue = default)
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
        

        public bool IsUnderMouse => HitTest(Context.Input.MousePos);

        public bool HitTest(Vec2 p)
        {
            if (!ClipRect.Contains(p)) {
                return false;
            }
            var planeIntersect = PlaneIntersection(new Vec3(p));
            if (planeIntersect == null) {
                return false;
            }
            return new Rect(Size).Contains(ToLocalCoord(planeIntersect.Value).XY);
        }

        /// <summary>
        /// The normal vector of the plane of this element, in world space.
        /// </summary>
        public Vec3 Normal
        {
            get
            {
                var a = ToWorldCoord(Vec3.Zero);
                var b = ToWorldCoord(Vec3.UnitZ);
                var n = (b - a);
                return n.Normalized;
            }
        }

        /// <summary>
        /// Return the point in world space at which an infinite line through the screen at the given position
        /// intersects the plane of this element. The result will only differ from the input in Z value.
        /// </summary>
        public Vec3? PlaneIntersection(Vec3 worldPos)
        {
            var n = Normal;
            var l = Vec3.UnitZ;
            var denom = n.Dot(l);
            if (Math.Abs(denom) > 0.000001f) {
                var planeOrigin = ToWorldCoord(Vec3.Zero);
                var toPlaneOrigin = planeOrigin - worldPos;
                var t = toPlaneOrigin.Dot(n) / denom;
                return worldPos + l * t;
            }
            return null;
        }
        
        public Vec3 ToLocalCoord(Vec3 worldPos) => Context.AttrWorldTransform[Index].ApplyInverse(worldPos);
        public Vec3 ToWorldCoord(Vec3 localPos) => Context.AttrWorldTransform[Index].ApplyForward(localPos);
        public Vec2 ToLocalCoord(Vec2 worldPos) => ToLocalCoord(new Vec3(worldPos)).XY;
        public Vec2 ToWorldCoord(Vec2 localPos) => ToWorldCoord(new Vec3(localPos)).XY;


        #region Misc forwarded properties
        public ref string Name =>  ref Context.AttrName[Index];
        public bool ClipContent
        {
            get => Context.GetFlag(Index, ElementFlags.ClipContent);
            set => Context.SetFlag(Index, ElementFlags.ClipContent, value);
        }
        public bool Disabled
        {
            get => Context.GetFlag(Index, ElementFlags.Disabled);
            set => Context.SetFlag(Index, ElementFlags.Disabled, value);
        }
        public bool Opaque
        {
            get => Context.GetFlag(Index, ElementFlags.Opaque);
            set => Context.SetFlag(Index, ElementFlags.Opaque, value);
        }
        public bool SizeToFit
        {
            get => Context.GetFlag(Index, ElementFlags.SizeToFit);
            set => Context.SetFlag(Index, ElementFlags.SizeToFit, value);
        }
        public ref int ZIndex => ref Context.AttrZIndex[Index];
        public ref Transform Transform => ref Context.AttrTransform[Index];
        public ref Vec3 Pivot => ref Context.AttrTransform[Index].Pivot;
        public ref Quat Rotation => ref Context.AttrTransform[Index].Rotation;
        public ref Vec3 Translation => ref Context.AttrTransform[Index].Translation;
        public ref Vec3 Scale => ref Context.AttrTransform[Index].Scale;
        public ref Transform WorldTransform => ref Context.AttrWorldTransform[Index];
        public ref float X => ref Context.AttrRect[Index].Pos.X;
        public ref float Y => ref Context.AttrRect[Index].Pos.Y;
        public ref float Width => ref Context.AttrRect[Index].Size.X;
        public ref float Height => ref Context.AttrRect[Index].Size.Y;
        public ref Rect Rect => ref Context.AttrRect[Index];
        public ref Vec2 Pos => ref Context.AttrRect[Index].Pos;
        public ref Vec2 Size => ref Context.AttrRect[Index].Size;
        public ref readonly Rect ClipRect => ref Context.AttrClipRect[Index];
        public ref Action<DrawContext> Draw => ref Context.AttrDrawFunc[Index];
        public ref Action<Element> Measure => ref Context.AttrMeasureFunc[Index];
        public ref Action<Element> Layout => ref Context.AttrLayoutFunc[Index];
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
        public bool Equals(Element other) => Context == other.Context && Index == other.Index;
        public override bool Equals(object other) => other is Element && Equals((Element)other);
        public override int GetHashCode() => unchecked((Context.GetHashCode() * 397) ^ Index);
        public static bool operator ==(Element a, Element b) => a.Equals(b);
        public static bool operator !=(Element a, Element b) => !a.Equals(b);
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

            public IEnumerator<Element> GetEnumerator() => new ChildEnumerator(context, parentIndex);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

            public void Reset() => elemIndex = 0;

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
