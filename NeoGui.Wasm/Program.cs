namespace NeoGui.Wasm;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NeoGui.Core;
using NeoGui.Toolkit;


public class Program {
    private static IJSInProcessRuntime jsRuntime = null!;
    private static NeoGuiContext context = null!;
    private static readonly InputState input = new();
    private static bool inputInited;

    private class NeoGuiDelegate(IJSInProcessRuntime jsRuntime) : INeoGuiDelegate {
        public Vec2 TextSize(string text, int fontId) {
            return jsRuntime.Invoke<Vec2>("measureTextSize", text);
        }

        public void DrawDot(Vec3 p, Color? c = null) {
        }
    }

    private static async Task Main(string[] args) {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        var host = builder.Build();
        jsRuntime = (IJSInProcessRuntime)host.Services.GetRequiredService<IJSRuntime>();
        FixJsonDeserialization();
        context = new NeoGuiContext(new NeoGuiDelegate(jsRuntime));
        var size = context.Delegate.TextSize("foo", 0);
        Console.WriteLine($"size: {size.X}x{size.Y}");
        await host.RunAsync();
    }

    private static void FixJsonDeserialization() {
        var property = typeof(JSRuntime).GetProperty("JsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var options = (JsonSerializerOptions)Convert.ChangeType(property.GetValue(jsRuntime, null), typeof(JsonSerializerOptions))!;
        options.IncludeFields = true;
    }

    [JSInvokable]
    public static void OnPointerEvent(float clientX, float clientY, int pointerId, int down) {
        if (down >= 0 && pointerId > 0) {
            input.MouseButtonDown[pointerId-1] = down != 0;
        }
        input.MousePos.X = clientX;
        input.MousePos.Y = clientY;
    }

    [JSInvokable]
    public static void OnKeyEvent(int keyCode, int down) {
        KeyboardKey? key = keyCode switch {
            9 => KeyboardKey.Tab,
            13 => KeyboardKey.Enter,
            _ => null,
        };
        if (key != null) {
            input.KeyDown[(int)key.Value] = (byte)down;
        }
    }

    [JSInvokable]
    public static void DoFrame(int windowWidth, int windowHeight, double time) {
        input.Time = time;
        if (!inputInited) {
            inputInited = true;
            context.Input.SetNewState(input);
        }
        context.Input.SetNewState(input);

        TestUi.DoUi(context, windowWidth, windowHeight);

        foreach (var buffer in context.DirtyDrawCommandBuffers) {
            for (var i = 0; i < buffer.Count; ++i) {
                ref var command = ref buffer[i];
                switch (command.Type) {
                case DrawCommandType.SetClipRect: {
                    var r = command.SetClipRect.ClipRect;
                    jsRuntime.InvokeVoid("setClipRect", r.X, r.Y, r.Width, r.Height);
                    break;
                }
                case DrawCommandType.SetTransform: {
                    command.SetTransform.Transform.ToMatrix(out Mat4 m);
                    var s = command.SetTransform.Transform.Scale;
                    jsRuntime.InvokeVoid("setTransform", m.M11, m.M21, m.M12, m.M22, m.M14, m.M24);
                    break;
                }
                case DrawCommandType.SolidRect: {
                    var r = command.SolidRect.Rect;
                    var c = command.SolidRect.Color;
                    jsRuntime.InvokeVoid("drawSolidRect", r.X, r.Y, r.Width, r.Height, c.R, c.G, c.B, c.A);
                    break;
                }
                case DrawCommandType.TexturedRect:
                    break;
                case DrawCommandType.Text: {
                    var p = command.Text.Vec2;
                    var c = command.Text.Color;
                    var text = context.GetInternedString(command.Text.StringId);
                    var size = context.GetTextSize(text, command.Text.StringId, command.Text.FontId);
                    p.Y += size.Y;
                    jsRuntime.InvokeVoid("drawText", text, p.X, p.Y, c.R, c.G, c.B, c.A);
                    break;
                }
                default:
                    Debug.Assert(false);
                    break;
                }
            }
        }
    }
}