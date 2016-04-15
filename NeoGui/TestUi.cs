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
            public int NumButtons = 3;
        }

        private class TestCount
        {
            public int Value;
            public string StringValue;
        }

        private static bool panelVisible = true;

        private static bool switchValue;

        public static void DoUi(NeoGuiContext ui, float windowWidth, float windowHeight)
        {
            ui.BeginFrame();

            var root = ui.Root;
            root.Rect = new Rect(0, 0, windowWidth, windowHeight);
            Panel.AddProps(root);

            var toggleButton = TextButton.Create(root, "Toggle", e => {
                panelVisible = !panelVisible;
            });
            toggleButton.Rect = new Rect(70, 40, 100, 30);

            if (panelVisible) {
                var panel = Panel.Create(root, Color.LightGray);
                panel.AttachStateHolder();
                panel.Rect = new Rect(70, 80, 500, 600);
                panel.ClipContent = true;
                
                var state = panel.GetOrCreateState<TestState>();

                var tabButton0 = TextButton.Create(panel, "Tab 0", e => {
                    e.FindState<TestState>().ActiveTab = 0;
                });
                tabButton0.Enabled = state.ActiveTab != 0;
                tabButton0.Rect = new Rect(0, 0, 100, 30);

                var tabButton1 = TextButton.Create(panel, "Tab 1", e => {
                    e.FindState<TestState>().ActiveTab = 1;
                });
                tabButton1.Enabled = state.ActiveTab != 1;
                tabButton1.Rect = new Rect(101, 0, 100, 30);
                
                if (state.ActiveTab == 0) {
                    var tab0 = Element.Create(panel);
                    tab0.Rect = new Rect(0, 30, 300, 550);

                    var titleLabel = Label.Create(tab0, "This is tab 0", Color.Black);
                    titleLabel.Rect = new Rect(10, 10, 100, 30);

                    var addButton = TextButton.Create(tab0, "Add", e => {
                        e.FindState<TestState>().NumButtons++;
                    });
                    addButton.Rect = new Rect(150, 10, 100, 30);

                    for (var i = 0; i < state.NumButtons; ++i) {
                        var button = TextButton.Create(tab0, "Ok", e => {
                            var s = e.GetOrCreateState<TestCount>();
                            s.StringValue = $"count: {++s.Value}";
                        });
                        button.Rect = new Rect(10, 50 + i * 40, 100 + (float)Math.Sin(ui.Input.Time * 3 + i * 0.1f) * 30, 30);

                        var countString = button.GetOrCreateState<TestCount>().StringValue;
                        var countLabel = Label.Create(tab0, countString, Color.Black);
                        countLabel.Rect = new Rect(170, 50 + i * 40, 100, 30);
                    }
                } else {
                    var tab1 = Element.Create(panel);
                    tab1.Rect = new Rect(0, 30, 300, 550);

                    var titleLabel = Label.Create(tab1, "This is tab 1", Color.Black);
                    titleLabel.Rect = new Rect(10, 10, 100, 30);
                    
                    
                    var outerScrollArea = ScrollArea.Create(tab1);
                    outerScrollArea.Rect = new Rect(10, 40, 300, 300);
                    Panel.AddProps(outerScrollArea, Color.LightGray);

                    var outerContentPanel = ScrollArea.GetContentPanel(outerScrollArea);
                    outerContentPanel.Size = new Vec2(500, 500);
                    Panel.AddProps(outerContentPanel, new Color(240, 240, 240));


                    var scrollArea = ScrollArea.Create(outerContentPanel);
                    scrollArea.Rect = new Rect(50, 50, 200, 200);
                    Panel.AddProps(scrollArea, new Color(240, 240, 240));

                    var contentPanel = ScrollArea.GetContentPanel(scrollArea);
                    contentPanel.Size = new Vec2(250, 250);
                    Panel.AddProps(contentPanel, new Color(220, 220, 220));

                    var label = Label.Create(contentPanel, "Drag me", Color.Black);
                    label.Rect = new Rect(10, 10, 100, 30);

                    var button = TextButton.Create(contentPanel, "Hello", e => Debug.WriteLine("Hello"));
                    button.Rect = new Rect(10, 50, 100, 30);
                    
                    var toggleLabel = Label.Create(contentPanel, "Switch me:", Color.Black);
                    toggleLabel.Rect = new Rect(10, 100, 70, 20);
                    var toggle = ToggleSwitch.Create(contentPanel, switchValue, e => switchValue = !switchValue);
                    toggle.Pos = new Vec2(100, 102);
                }
            }

            ui.EndFrame();
        }
    }
}