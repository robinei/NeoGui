using System;
using System.Diagnostics;
using NeoGui.Core;
using NeoGui.Toolkit;

namespace NeoGui
{
    public static class TestUi
    {
        private class TestState
        {
            public int ActiveTab = 1;
            public int NumButtons = 10;
        }

        private class TestCount
        {
            public int Value;
            public string StringValue;
        }

        private static bool panelVisible = true;
        private static StateDomain panelStateDomain;

        private static bool switchValue;

        private static readonly object PanelKey = new object();
        private static readonly object ListKey = new object();
        private static readonly object Tab1Key = new object();
        private static readonly object Tab2Key = new object();

        public static void DoUi(NeoGuiContext ui, float windowWidth, float windowHeight)
        {
            ui.BeginFrame();

            if (panelStateDomain == null) {
                panelStateDomain = ui.CreateStateDomain();
            }

            var root = ui.Root;
            root.Rect = new Rect(0, 0, windowWidth, windowHeight);
            Panel.AddProps(root);

            var toggleButton = TextButton.Create(root, "Toggle", e => {
                panelVisible = !panelVisible;
                if (!panelVisible) {
                    panelStateDomain.Reset();
                }
            });
            toggleButton.Scale = new Vec3(0.5f, 0.5f, 0);
            toggleButton.Rect = new Rect(70, 40, 100, 30);

            if (panelVisible) {
                var panel = Panel.Create(root, Color.LightGray, PanelKey, panelStateDomain);
                panel.Rect = new Rect(70, 80, 400, 600);
                panel.ClipContent = true;
                
                var state = panel.GetOrCreateState<TestState>();

                var tabButton0 = TextButton.Create(panel, "Tab 0", e => {
                    e.FindState<TestState>().ActiveTab = 0;
                });
                tabButton0.Disabled = state.ActiveTab == 0;
                tabButton0.Rect = new Rect(0, 0, 100, 30);

                var tabButton1 = TextButton.Create(panel, "Tab 1", e => {
                    e.FindState<TestState>().ActiveTab = 1;
                });
                tabButton1.Disabled = state.ActiveTab == 1;
                tabButton1.Rect = new Rect(101, 0, 100, 30);
                
                if (state.ActiveTab == 0) {
                    var tab0 = Element.Create(panel, Tab1Key);
                    tab0.Rect = new Rect(0, 30, 300, 550);

                    var titleLabel = Label.Create(tab0, "This is tab 0");
                    titleLabel.Pos = new Vec2(10, 10);
                    var buttonCountLabel = Label.Create(tab0, "Button count: " + state.NumButtons);
                    buttonCountLabel.Pos = new Vec2(10, 40);

                    var addButton = TextButton.Create(tab0, "Add 1", e => {
                        e.FindState<TestState>().NumButtons++;
                    });
                    addButton.Rect = new Rect(150, 10, 100, 30);
                    var addButton2 = TextButton.Create(tab0, "Add 100", e => {
                        e.FindState<TestState>().NumButtons += 100;
                    });
                    addButton2.Rect = new Rect(260, 10, 100, 30);

                    var buttonScroller = ScrollArea.Create(tab0, ScrollAreaFlags.BounceY | ScrollAreaFlags.FillX);
                    var buttonContent = ScrollArea.GetContentPanel(buttonScroller);
                    Panel.AddProps(buttonScroller, new Color(200, 200, 200));
                    buttonScroller.Rect = new Rect(10, 70, 250, 400);
                    StackLayout.AddProps(buttonContent);
                    for (var i = 0; i < state.NumButtons; ++i) {
                        var row = Element.Create(buttonContent);
                        row.Height = 30;

                        var button = TextButton.Create(row, "Ok", e => {
                            var s = e.GetOrCreateState<TestCount>();
                            s.StringValue = $"count: {++s.Value}";
                        });
                        button.Size = new Vec2(100 + (float)Math.Sin(ui.Input.Time * 3 + i * 0.1f) * 30, 30);

                        var buttonCount = button.GetOrCreateState<TestCount>();
                        var countString = buttonCount.StringValue;
                        var countLabel = Label.Create(row, countString);
                        countLabel.Rect = new Rect(170, 0, 100, 30);
                        countLabel.SizeToFit = false;
                    }
                } else {
                    var tab1 = Element.Create(panel, Tab2Key);
                    tab1.Rect = new Rect(0, 30, 300, 550);

                    var titleLabel = Label.Create(tab1, "This is tab 1");
                    titleLabel.Pos = new Vec2(10, 10);
                    
                    
                    var outerScrollArea = ScrollArea.Create(tab1);
                    outerScrollArea.Rect = new Rect(10, 40, 300, 300);
                    Panel.AddProps(outerScrollArea, new Color(182, 182, 182));

                    var outerContentPanel = ScrollArea.GetContentPanel(outerScrollArea);
                    outerContentPanel.Size = new Vec2(500, 500);
                    Panel.AddProps(outerContentPanel, new Color(240, 240, 240));


                    var scrollArea = ScrollArea.Create(outerContentPanel);
                    scrollArea.Rect = new Rect(50, 50, 200, 200);
                    Panel.AddProps(scrollArea, new Color(230, 230, 230));

                    var contentPanel = ScrollArea.GetContentPanel(scrollArea);
                    contentPanel.Size = new Vec2(250, 250);
                    Panel.AddProps(contentPanel, new Color(220, 220, 220));

                    var label = Label.Create(contentPanel, "Drag me");
                    label.Pos = new Vec2(10, 10);

                    var button = TextButton.Create(contentPanel, "Hello", e => Debug.WriteLine("Hello"));
                    button.Rect = new Rect(10, 50, 100, 30);
                    //button.Pivot = new Vec3(0, 0, 10);
                    button.Rotation = Quat.FromAxisAngle(new Vec3(0, 1, 0).Normalized, (float)ui.Input.Time);
                    
                    var toggleLabel = Label.Create(contentPanel, "Toggle me:");
                    toggleLabel.Pos = new Vec2(10, 100);
                    var toggle = ToggleSwitch.Create(contentPanel, switchValue, e => switchValue = !switchValue);
                    toggle.Pos = new Vec2(100, 102);
                    toggle.OnInserted(e => Debug.WriteLine("switch inserted"));
                }
            }

            var virtualList = VirtualList.Create(root, 50, 40.0f, (parent, index) => {
                var label = Label.Create(parent, "Row " + index, alignment: (TextAlignment)(index % 3));
                return label;
            }, ListKey);
            virtualList.Rect = new Rect(550, 80, 100, 600);
            Panel.AddProps(ScrollArea.GetContentPanel(virtualList), new Color(240, 240, 240));

            ui.EndFrame();
        }
    }
}