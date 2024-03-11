namespace NeoGui.Toolkit;

using System;
using System.Diagnostics;
using NeoGui.Core;

public static class TestUi {
    private class TestState {
        private int buttonCount = 10;
        private string? buttonCountText;
        
        public int ActiveTab = 1;
        public int NumButtons {
            get => buttonCount;
            set {
                buttonCount = value;
                buttonCountText = null;
            }
        }
        public string ButtonCountText => buttonCountText ??= "Button count: " +  buttonCount;
    }

    private class TestCount {
        public int Value = 0;
        public string StringValue = string.Empty;
    }

    private static bool panelVisible = true;
    private static StateDomain? panelStateDomain = null;
    private static readonly Action<Element> onTogglePanel = e => {
        panelVisible = !panelVisible;
        if (!panelVisible && panelStateDomain != null) {
            panelStateDomain.Dispose();
            panelStateDomain = null;
        }
    };

    private static bool switchValue = false;
    private static readonly Action<Element> onToggleSwitch = e => switchValue = !switchValue;

    private static readonly object PanelKey = new();
    private static readonly object ListKey = new();
    private static readonly object Tab1Key = new();
    private static readonly object Tab2Key = new();

    public static void DoUi(NeoGuiContext context, float windowWidth, float windowHeight) {
        context.BeginFrame();

        var root = context.Root
            .SetRect(0, 0, windowWidth, windowHeight)
            .AddPanelProps()
            .CreateTextButton("Toggle", onTogglePanel)
                .SetScale(new Vec3(0.5f, 0.5f, 1) + new Vec3(1, 1, 0) * (float)Math.Abs(Math.Sin(context.Input.Time)))
                .SetRect(70, 40, 100, 30)
                .Parent;

        if (panelVisible) {
            panelStateDomain ??= context.CreateStateDomain();

            var panel = root.CreatePanel(Color.LightGray, PanelKey, panelStateDomain)
                .SetRect(70, 80, 400, 600)
                .SetClipContent(true)
                .GetOrCreateState<TestState>(out var state)
                .CreateTextButton("Tab 0", e => {
                        e.FindState<TestState>().ActiveTab = 0;
                    })
                    .SetDisabled(state.ActiveTab == 0)
                    .SetRect(0, 0, 100, 30)
                    .Parent
                .CreateTextButton("Tab 1", e => {
                        e.FindState<TestState>().ActiveTab = 1;
                    })
                    .SetDisabled(state.ActiveTab == 1)
                    .SetRect(101, 0, 100, 30)
                    .Parent;
            
            if (state.ActiveTab == 0) {
                _ = panel.CreateElement(Tab1Key)
                    .SetRect(0, 30, 300, 550)
                    .CreateLabel("This is tab 0")
                        .SetPos(10, 10)
                        .Parent
                    .CreateLabel(state.ButtonCountText)
                        .SetPos(10, 40)
                        .Parent
                    .CreateTextButton("Add 1", e => {
                            e.FindState<TestState>().NumButtons++;
                        })
                        .SetRect(150, 10, 100, 30)
                        .Parent
                    .CreateTextButton("Add 100", e => {
                            e.FindState<TestState>().NumButtons += 100;
                        })
                        .SetRect(260, 10, 100, 30)
                        .Parent
                    .CreateScrollArea(ScrollAreaFlags.BounceY | ScrollAreaFlags.FillX)
                        .AddPanelProps(new Color(200, 200, 200))
                        .SetRect(10, 70, 250, 400)
                        .GetScrollAreaContentPanel()
                            .AddStackLayoutProps()
                            .Capture(out var buttonContent)
                            .Parent
                        .Parent;
                
                for (var i = 0; i < state.NumButtons; ++i) {
                    _ = buttonContent.CreateElement()
                        .SetHeight(30)
                        .CreateTextButton("Ok", e => {
                                var s = e.GetOrCreateState<TestCount>();
                                s.StringValue = $"count: {++s.Value}";
                            })
                            .SetSize(100 + (float)Math.Sin(context.Input.Time * 3 + i * 0.1f) * 30, 30)
                            .GetOrCreateState<TestCount>(out var buttonCount)
                            .Parent
                        .CreateLabel(buttonCount.StringValue)
                            .SetRect(170, 0, 100, 30)
                            .SetSizeToFit(false)
                            .Parent;
                }
            } else {
                _ = panel.CreateElement(Tab2Key)
                    .SetRect(0, 30, 300, 550)
                    .CreateLabel("This is tab 1")
                        .SetPos(10, 10)
                        .Parent
                    .CreateScrollArea()
                        .SetRect(10, 40, 300, 300)
                        .AddPanelProps(new Color(182, 182, 182))
                        .GetScrollAreaContentPanel()
                            .SetSize(500, 500)
                            .AddPanelProps(new Color(240, 240, 240))
                            .CreateScrollArea()
                                .SetRect(50, 50, 200, 200)
                                .AddPanelProps(new Color(230, 230, 230))
                                .GetScrollAreaContentPanel()
                                    .SetSize(250, 250)
                                    //.SetRotation(Quat.FromAxisAngle(new Vec3(1, 1, 1).Normalized, (float)context.Input.Time))
                                    .SetClipContent(false)
                                    .AddPanelProps(new Color(220, 220, 220))
                                    .CreateLabel("Drag me")
                                        .SetPos(10, 10)
                                        .Parent
                                    .CreateTextButton("Hello", e => Debug.WriteLine("Hello"))
                                        .SetRect(10, 50, 100, 30)
                                        //.SetPivot(0, 0, 10)
                                        .SetRotation(Quat.FromAxisAngle(new Vec3(1, 1, 1).Normalized, (float)context.Input.Time))
                                        //.SetRotation(Quat.FromAxisAngle(new Vec3(0, 0, 1).Normalized, (float)Math.Sin(context.Input.Time)))
                                        //.SetRotation(Quat.FromAxisAngle(new Vec3(0, 0, 1).Normalized, (float)Math.PI*(float)Math.Sin(context.Input.Time)))
                                        .OnDepthDescent(e => {
                                            var p0 = e.ToWorldCoord(Vec3.Zero);
                                            e.Context.Delegate.DrawDot(p0 + e.Normal * 10, Color.Yellow);
                                            e.Context.Delegate.DrawDot(p0 + e.Normal * 20, Color.Yellow);
                                            e.Context.Delegate.DrawDot(p0 + e.Normal * 30, Color.Yellow);
                                        })
                                        .Parent
                                    .CreateLabel("Toggle me:")
                                        .SetPos(10, 100)
                                        .Parent
                                    .CreateToggleSwitch(switchValue, onToggleSwitch)
                                        .SetPos(100, 102)
                                        .OnInserted(e => Debug.WriteLine("switch inserted"))
                                        .OnRemoved(e => Debug.WriteLine("switch removed"))
                                        .Parent
                                    .Parent
                                .Parent
                            .Parent
                        .Parent;
            }
        }

        _ = root.CreateVirtualList(50, 40.0f, (parent, index) => {
                return parent.CreateLabel("Row", alignment: (TextAlignment)(index % 3));
            }, ListKey)
            .SetRect(550, 80, 100, 600)
            .GetScrollAreaContentPanel()
                .AddPanelProps(new Color(240, 240, 240))
                .Parent;

        context.EndFrame();
    }
}
