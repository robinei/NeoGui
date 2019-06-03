namespace NeoGui.Core
{
    /// <summary>
    /// Proxy used to access the state of an Element. Used as argument to OnRemoved handlers
    /// which are invoked after an Element has disappeared, and thus no longer exists
    /// (so it cannot itself be used to get to the state).
    /// </summary>
    public struct ElementStateProxy
    {
        private readonly long stateId;
        private readonly ValueStorage<StateDomain.StateKeys, long> stateStorage;

        internal ElementStateProxy(long stateId, ValueStorage<StateDomain.StateKeys, long> stateStorage)
        {
            this.stateId = stateId;
            this.stateStorage = stateStorage;
        }

        public bool HasState<TState>() => stateStorage.HasValue<TState>(stateId);
        public bool TryGetState<TState>(out TState value) => stateStorage.TryGetValue(stateId, out value);
        public TState GetState<TState>(TState defaultValue = default) => stateStorage.GetValue(stateId, defaultValue);
        public TState GetOrCreateState<TState>() where TState: new() => stateStorage.GetOrCreateValue<TState>(stateId);
        public void SetState<TState>(TState value) => stateStorage.SetValue(stateId, value);
    }
}
