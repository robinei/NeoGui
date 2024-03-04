using System;

namespace NeoGui.Core
{
    /// <summary>
    /// Use for sorting key plus associated value when only the key matters for sorting order.
    /// </summary>
    public readonly struct KeyedValue<TKey, TValue> : IComparable<KeyedValue<TKey, TValue>>
        where TKey: IComparable<TKey>
    {
        public readonly TKey Key;
        public readonly TValue Value;

        public KeyedValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public int CompareTo(KeyedValue<TKey, TValue> other) => Key.CompareTo(other.Key);
    }
}