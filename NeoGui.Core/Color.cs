namespace NeoGui.Core;

public struct Color(byte r, byte g, byte b, byte a = 255) {
    public byte R = r, G = g, B = b, A = a;

    public static Color Black => new(0, 0, 0);
    public static Color White => new(255, 255, 255);
    public static Color Red => new(255, 0, 0);
    public static Color Green => new(0, 255, 0);
    public static Color Blue => new(0, 0, 255);
    public static Color DarkBlue => new(0, 0, 128);
    public static Color Yellow => new(255, 255, 0);
    public static Color Gray => new(128, 128, 128);
    public static Color LightGray => new(192, 192, 192);
    public static Color DarkGray => new(64, 64, 64);
}