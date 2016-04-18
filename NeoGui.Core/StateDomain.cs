using System;
using System.Collections.Generic;

namespace NeoGui.Core
{
    public class StateDomain : IDisposable
    {
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

        internal long NewStateIdForKey(object key)
        {
            Entry entry;
            if (!entries.TryGetValue(key, out entry)) {
                entry.KeyId = ++context.KeyIdCounter;
            }
            entry.Counter = 0;
            entries[key] = entry;
            return Util.TwoIntsToLong(entry.KeyId, entry.Counter);
        }

        internal long GetStateIdForKey(object key)
        {
            Entry entry;
            if (!entries.TryGetValue(key, out entry)) {
                entry.KeyId = ++context.KeyIdCounter;
            }
            ++entry.Counter;
            entries[key] = entry;
            return Util.TwoIntsToLong(entry.KeyId, entry.Counter);
        }

        public void Reset()
        {
            entries.Clear();
            Storage.Clear();
        }

        public void Dispose()
        {
            Reset();
            context.ReuseStateDomain(this);
        }
    }
}
