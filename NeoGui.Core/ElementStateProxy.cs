namespace NeoGui.Core;

/// <summary>
/// Proxy used to access the state of an Element. Used as argument to OnRemoved handlers
/// which are invoked after an Element has disappeared, and thus no longer exists
/// (so it cannot itself be used to get to the state).
/// </summary>
public readonly struct ElementStateProxy
{
    private readonly ValueStorage<StateDomain.StateKeys, long> stateStorage;

    internal ElementStateProxy(NeoGuiContext context, long stateId, ValueStorage<StateDomain.StateKeys, long> stateStorage)
    {
        Context = context;
        Id = stateId;
        this.stateStorage = stateStorage;
    }

    public readonly NeoGuiContext Context;
    public readonly long Id;
    public bool HasFocus => Context.FocusId == Id;
    public bool HasState<TState>() => stateStorage.HasValue<TState>(Id);
    public bool TryGetState<TState>(out TState? value) => stateStorage.TryGetValue(Id, out value);
    public TState GetState<TState>() => stateStorage.GetValue<TState>(Id);
    public TState GetState<TState>(TState defaultValue) => stateStorage.GetValue(Id, defaultValue);
    public TState GetOrCreateState<TState>() where TState: new() => stateStorage.GetOrCreateValue<TState>(Id);
    public void SetState<TState>(TState value) => stateStorage.SetValue(Id, value);
}
