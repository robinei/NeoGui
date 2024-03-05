namespace NeoGui.Core;

public struct Color(byte r, byte g, byte b, byte a = 255) {
    public byte R = r, G = g, B = b, A = a;

    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color White = new(255, 255, 255);
    public static readonly Color Red = new(255, 0, 0);
    public static readonly Color Green = new(0, 255, 0);
    public static readonly Color Blue = new(0, 0, 255);
    public static readonly Color Yellow = new(255, 255, 0);
    public static readonly Color Gray = new(128, 128, 128);
    public static readonly Color LightGray = new(192, 192, 192);
    public static readonly Color DarkGray = new(64, 64, 64);
}