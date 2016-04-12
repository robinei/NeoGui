namespace NeoGui.Core
{
    public abstract class Event
    {
        public NeoGuiContext Context => Target.Context;
        public Element Target { get; internal set; }

        internal bool PropagationStopped { get; set; }

        public void StopPropagation()
        {
            PropagationStopped = true;
        }
        
        public abstract bool Bubbles { get; }
    }



    public class MouseButtonEvent : Event
    {
        public override bool Bubbles => true;
        public Vec2 Pos { get; set; }
        public bool Pressed { get; set; }
        public MouseButton Button { get; set; }
    }

    public class MouseMotionEvent : Event
    {
        public override bool Bubbles => true;
        public Vec2 Pos { get; set; }
    }
}