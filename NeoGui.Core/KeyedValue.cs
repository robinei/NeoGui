namespace NeoGui.Core;

using System;

/// <summary>
/// Use for sorting key plus associated value when only the key matters for sorting order.
/// </summary>
public readonly struct KeyedValue<TKey, TValue>(TKey key, TValue value) : IComparable<KeyedValue<TKey, TValue>>
    where TKey: IComparable<TKey>
{
    public readonly TKey Key = key;
    public readonly TValue Value = value;

    public int CompareTo(KeyedValue<TKey, TValue> other) => Key.CompareTo(other.Key);
}
