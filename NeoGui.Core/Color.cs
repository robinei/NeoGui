namespace NeoGui.Core
{
    public struct Color
    {
        public byte R, G, B, A;

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        
        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Red = new Color(255, 0, 0);
        public static readonly Color Green = new Color(0, 255, 0);
        public static readonly Color Blue = new Color(0, 0, 255);
        public static readonly Color Yellow = new Color(255, 255, 0);
        public static readonly Color Gray = new Color(128, 128, 128);
        public static readonly Color LightGray = new Color(192, 192, 192);
        public static readonly Color DarkGray = new Color(64, 64, 64);
    }
}