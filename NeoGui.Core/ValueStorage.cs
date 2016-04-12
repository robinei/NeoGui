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

        public TValue GetValue<TValue>(TKey key, bool create)
            where TValue: new()
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            while (typeKey >= dictionaries.Length) {
                if (!create) {
                    return default(TValue);
                }
                Array.Resize(ref dictionaries, dictionaries.Length * 2);
            }
            var dict = (Dictionary<TKey, TValue>)dictionaries[typeKey];
            if (dict == null) {
                if (!create) {
                    return default(TValue);
                }
                dict = new Dictionary<TKey, TValue>();
                dictionaries[typeKey] = dict;
            }
            TValue value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            }
            if (!create) {
                return default(TValue);
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
