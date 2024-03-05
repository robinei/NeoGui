namespace NeoGui.Core;

using System;
using System.Collections.Generic;

public class StateDomain : IDisposable {
    public void Dispose() => context.RelinquishStateDomain(this);

    private struct Entry {
        public int KeyId;
        public int Counter;
    }

    private readonly NeoGuiContext context;
    private readonly Dictionary<object, Entry> entries = [];

    internal struct StateKeys { }
    internal readonly ValueStorage<StateKeys, long> Storage = new();

    internal StateDomain(NeoGuiContext context) {
        this.context = context;
    }
    
    internal long GetStateIdForOwnKey(object key) {
        if (!entries.TryGetValue(key, out Entry entry)) {
            entry.KeyId = ++context.KeyIdCounter;
        }
        entry.Counter = 0;
        entries[key] = entry;
        return Util.TwoIntsToLong(entry.KeyId, entry.Counter);
    }
    
    internal long GetStateIdForInheritedKey(object key) {
        var entry = entries[key];
        ++entry.Counter;
        entries[key] = entry;
        return Util.TwoIntsToLong(entry.KeyId, entry.Counter);
    }

    internal void Reset() {
        entries.Clear();
        Storage.Clear();
    }
}
