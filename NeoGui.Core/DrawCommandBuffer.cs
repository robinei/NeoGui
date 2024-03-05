namespace NeoGui.Core;

using System;

public class DrawCommandBuffer {
    private DrawCommand[] commands = new DrawCommand[128];
    
    public int Count { get; private set; }

    public void Add(DrawCommand cmd) {
        if (Count == commands.Length) {
            Array.Resize(ref commands, commands.Length * 2);
        }
        commands[Count++] = cmd;
    }

    public void Clear() {
        Count = 0;
    }

    public ref DrawCommand this[int index] => ref commands[index];

    public bool HasEqualCommands(DrawCommandBuffer other) {
        if (Count !=  other.Count) {
            return false;
        }
        for (var i = 0; i < Count; ++i) {
            if (!DrawCommand.AreEqual(ref commands[i], ref other.commands[i])) {
                return false;
            }
        }
        return true;
    }
}
