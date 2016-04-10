using System;

namespace NeoGui.Core
{
    public struct ElementId : IEquatable<ElementId>
    {
        public readonly object Key;
        public readonly int KeyIndex;

        public ElementId(object key, int keyIndex)
        {
            Key = key;
            KeyIndex = keyIndex;
        }
        
        #region Equality
        public bool Equals(ElementId other)
        {
            return Key == other.Key && KeyIndex == other.KeyIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is ElementId && Equals((ElementId)obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (Key.GetHashCode() * 397) ^ KeyIndex;
            }
        }
        
        public static bool operator ==(ElementId a, ElementId b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ElementId a, ElementId b)
        {
            return !a.Equals(b);
        }
        #endregion
    }
}