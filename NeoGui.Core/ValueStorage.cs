namespace NeoGui.Core;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Static members of generic classes are generated once for each combination of type arguments.
/// We exploit this to maintain a static mapping from types to integer values.
/// In fact we maintain several mappings: one per category type.
/// </summary>
internal static class TypeKeys<TCategory, T> {
    public static readonly int Key = TypeKeyMap<TCategory>.KeyOf(typeof(T));
}

internal static class TypeKeyMap<TCategory> {
    private static readonly Dictionary<Type, int> Map = [];

    public static int KeyOf(Type type) {
        if (Map.TryGetValue(type, out int key)) {
            return key;
        }
        key = Map.Count;
        Map[type] = key;
        return key;
    }
}

internal class ValueStorage<TTypeCategory, TKey> {
    // one dictionary per type of value, all stored in an array indexed by the TypeKeys integer
    private IDictionary?[] dictionaries = new IDictionary?[64];

    public bool HasValue<TValue>(TKey key) {
        var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
        if (typeKey >= dictionaries.Length) {
            return false;
        }
        var dict = (Dictionary<TKey, TValue>?)dictionaries[typeKey];
        return dict?.ContainsKey(key) == true;
    }

    public bool TryGetValue<TValue>(TKey key, out TValue? value) {
        var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
        if (typeKey >= dictionaries.Length) {
            value = default;
            return false;
        }
        var dict = (Dictionary<TKey, TValue>?)dictionaries[typeKey];
        if (dict == null) {
            value = default;
            return false;
        }
        return dict.TryGetValue(key, out value);
    }

    public TValue GetValue<TValue>(TKey key) {
        var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
        if (typeKey < dictionaries.Length) {
            var dict = (Dictionary<TKey, TValue>?)dictionaries[typeKey];
            if (dict?.TryGetValue(key, out var value) == true) {
                return value;
            }
        }
        throw new Exception("value not found");
    }

    public TValue GetValue<TValue>(TKey key, TValue defaultValue) {
        var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
        if (typeKey >= dictionaries.Length) {
            return defaultValue;
        }
        var dict = (Dictionary<TKey, TValue>?)dictionaries[typeKey];
        return dict?.TryGetValue(key, out var value) == true ? value : defaultValue;
    }

    public TValue GetOrCreateValue<TValue>(TKey key) where TValue: new() {
        var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
        while (typeKey >= dictionaries.Length) {
            Array.Resize(ref dictionaries, dictionaries.Length * 2);
        }
        var dict = (Dictionary<TKey, TValue>?)dictionaries[typeKey];
        if (dict == null) {
            dict = [];
            dictionaries[typeKey] = dict;
        }
        if (dict.TryGetValue(key, out TValue value)) {
            return value;
        }
        value = new TValue();
        dict[key] = value;
        return value;
    }

    public void SetValue<TValue>(TKey key, TValue value) {
        var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
        while (typeKey >= dictionaries.Length) {
            Array.Resize(ref dictionaries, dictionaries.Length * 2);
        }
        var dict = (Dictionary<TKey, TValue>?)dictionaries[typeKey];
        if (dict == null) {
            dict = [];
            dictionaries[typeKey] = dict;
        }
        dict[key] = value;
    }

    public void Clear() {
        foreach (var dict in dictionaries) {
            dict?.Clear();
        }
    }
}
