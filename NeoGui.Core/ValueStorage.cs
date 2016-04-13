using System;
using System.Collections;
using System.Collections.Generic;

namespace NeoGui.Core
{
    public class ValueStorage<TTypeCategory, TKey>
    {
        private object[] dictionaries = new object[64];

        public bool HasValue<TValue>(TKey key)
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            if (typeKey >= dictionaries.Length) {
                return false;
            }
            var dict = (Dictionary<TKey, TValue>)dictionaries[typeKey];
            return dict != null && dict.ContainsKey(key);
        }

        public TValue GetValue<TValue>(TKey key, TValue defaultValue = default(TValue))
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            while (typeKey >= dictionaries.Length) {
                return defaultValue;
            }
            var dict = (Dictionary<TKey, TValue>)dictionaries[typeKey];
            if (dict == null) {
                return defaultValue;
            }
            TValue value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public TValue GetOrCreateValue<TValue>(TKey key)
            where TValue: new()
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            while (typeKey >= dictionaries.Length) {
                Array.Resize(ref dictionaries, dictionaries.Length * 2);
            }
            var dict = (Dictionary<TKey, TValue>)dictionaries[typeKey];
            if (dict == null) {
                dict = new Dictionary<TKey, TValue>();
                dictionaries[typeKey] = dict;
            }
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }
            value = new TValue();
            dict[key] = value;
            return value;
        }

        public void SetValue<TValue>(TKey key, TValue value)
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            while (typeKey >= dictionaries.Length) {
                Array.Resize(ref dictionaries, dictionaries.Length * 2);
            }
            var dict = (Dictionary<TKey, TValue>)dictionaries[typeKey];
            if (dict == null) {
                dict = new Dictionary<TKey, TValue>();
                dictionaries[typeKey] = dict;
            }
            dict[key] = value;
        }

        public void Clear()
        {
            foreach (var dict in dictionaries) {
                ((IDictionary)dict)?.Clear();
            }
        }
    }
}
