namespace NeoGui;

using System;
using System.Diagnostics;

public static class Program {
    [STAThread]
    static void Main() {
        Trace.Listeners.Add(new ConsoleTraceListener());
        using var game = new Game();
        game.Run();
    }
}
