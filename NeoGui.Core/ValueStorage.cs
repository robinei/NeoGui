using System;
using System.Collections;
using System.Collections.Generic;

namespace NeoGui.Core
{
    public class ValueStorage<TTypeCategory>
    {
        private object[] dictionaries = new object[64];

        public bool HasValue<TValue>(int elemIndex)
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            if (typeKey >= dictionaries.Length) {
                return false;
            }
            var dict = (Dictionary<int, TValue>)dictionaries[typeKey];
            return dict != null && dict.ContainsKey(elemIndex);
        }

        public TValue GetValue<TValue>(int elemIndex, bool create)
            where TValue: new()
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            while (typeKey >= dictionaries.Length) {
                if (!create) {
                    return default(TValue);
                }
                Array.Resize(ref dictionaries, dictionaries.Length * 2);
            }
            var dict = (Dictionary<int, TValue>)dictionaries[typeKey];
            if (dict == null) {
                if (!create) {
                    return default(TValue);
                }
                dict = new Dictionary<int, TValue>();
                dictionaries[typeKey] = dict;
            }
            TValue value;
            if (dict.TryGetValue(elemIndex, out value)) {
                return value;
            }
            if (!create) {
                return default(TValue);
            }
            value = new TValue();
            dict[elemIndex] = value;
            return value;
        }

        public void SetValue<TValue>(int elemIndex, TValue value)
        {
            var typeKey = TypeKeys<TTypeCategory, TValue>.Key;
            if (typeKey >= dictionaries.Length) {
                Array.Resize(ref dictionaries, dictionaries.Length * 2);
            }
            var dict = (Dictionary<int, TValue>)dictionaries[typeKey];
            if (dict == null) {
                dict = new Dictionary<int, TValue>();
                dictionaries[typeKey] = dict;
            }
            dict[elemIndex] = value;
        }

        public void Clear()
        {
            foreach (var dict in dictionaries) {
                ((IDictionary)dict)?.Clear();
            }
        }
    }
}
