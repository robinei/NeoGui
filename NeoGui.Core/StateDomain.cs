using System;
using System.Collections.Generic;

namespace NeoGui.Core
{
    public class StateDomain : IDisposable
    {
        public void Dispose() => context.RelinquishStateDomain(this);


        private struct Entry
        {
            public int KeyId;
            public int Counter;
        }

        private readonly NeoGuiContext context;
        private readonly Dictionary<object, Entry> entries = new Dictionary<object, Entry>();

        internal struct StateKeys { }
        internal readonly ValueStorage<StateKeys, long> Storage = new ValueStorage<StateKeys, long>();

        internal StateDomain(NeoGuiContext context)
        {
            this.context = context;
        }
        
        internal long GetStateIdForOwnKey(object key)
        {
            Entry entry;
            if (!entries.TryGetValue(key, out entry)) {
                entry.KeyId = ++context.KeyIdCounter;
            }
            entry.Counter = 0;
            entries[key] = entry;
            return Util.TwoIntsToLong(entry.KeyId, entry.Counter);
        }
        
        internal long GetStateIdForInheritedKey(object key)
        {
            var entry = entries[key];
            ++entry.Counter;
            entries[key] = entry;
            return Util.TwoIntsToLong(entry.KeyId, entry.Counter);
        }

        internal void Reset()
        {
            entries.Clear();
            Storage.Clear();
        }
    }
}
