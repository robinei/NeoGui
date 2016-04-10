using System.Diagnostics;
using NeoGui.Core;

namespace NeoGui
{
    public class ButtonBehaviorState
    {
        public bool MouseOver;
        public bool MousePressed;
    }

    public static class ButtonBehaviorExt
    {
        public static void AddButtonBehavior(this Element elem)
        {
            var state = elem.GetState<ButtonBehaviorState>(true);

            if (state.MouseOver) {
                elem.Context.Root.AddEventListener<MouseMotionEvent>(e => {
                
                }, true);
            }

            if (state.MousePressed) {
                elem.Context.Root.AddEventListener<MouseButtonEvent>(e => {

                }, true);
            }

            elem.AddEventListener<MouseMotionEvent>(e => {
                var pos = e.Target.ToLocalCoord(e.Pos);
                Debug.WriteLine("MouseMotionEvent: " + pos);
            });
            
            elem.AddEventListener<MouseButtonEvent>(e => {
                var pos = e.Target.ToLocalCoord(e.Pos);
                Debug.WriteLine("MouseButtonEvent: " + pos);
            });
        }
    }
    


    public struct TextButtonData
    {
        public string Text;
    }

    public static class TextButtonExt
    {
        public static Element CreateTextButton(this Element parent, string text)
        {
            var elem = parent.CreateElement();
            elem.AddButtonBehavior();
            elem.SetData(new TextButtonData {
                Text = text
            });
            elem.Draw = drawContext => {
                var data = drawContext.Target.GetData<TextButtonData>();
                var size = drawContext.Target.Size;
                var textSize = drawContext.TextSize(data.Text);
                drawContext.SolidRect(new Rect(size), Color.Gray);
                drawContext.Text((size - textSize) * 0.5f, data.Text, Color.White);
            };
            return elem;
        }
    }

    public static class TestUi
    {
        public static void DoUi(NeoGuiContext ui, float windowWidth, float windowHeight)
        {
            ui.BeginFrame();

            var root = ui.Root;
            root.Rect = new Rect(0, 0, windowWidth, windowHeight);
            root.Draw = drawContext => {
                drawContext.SolidRect(new Rect(drawContext.Target.Size), Color.White);
            };

            var panel = root.CreateElement();
            panel.AttachStateHolder();
            panel.Rect = new Rect(50, 50, 300, 200);
            panel.Draw = drawContext => {
                drawContext.SolidRect(new Rect(drawContext.Target.Size), Color.LightGray);
            };

            var button = panel.CreateTextButton("Ok");
            button.Rect = new Rect(10, 10, 100, 30);

            ui.EndFrame();
        }
    }
}