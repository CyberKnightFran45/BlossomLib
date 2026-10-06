using System.IO;

/// <summary> Represents a RGBA Color in the SexyFramework. </summary>
public class SexyColor(int r, int g, int b, int a)
{
public int Red{ get; set; } = r;
public int Green{ get; set; } = g;
public int Blue{ get; set; } = b;
public int Alpha{ get; set; } = a;

public SexyColor(int r, int g, int b) : this(r, g, b, 255) {}

// Read as binary

public static SexyColor Read(Stream reader)
{
int r = reader.ReadInt32();
int g = reader.ReadInt32();
int b = reader.ReadInt32();
int a = reader.ReadInt32();

return new(r, g, b, a);
}

// Write to binary

public void Write(Stream writer)
{
writer.WriteInt32(Red);
writer.WriteInt32(Green);
writer.WriteInt32(Blue);
writer.WriteInt32(Alpha);
}

public static SexyColor operator +(SexyColor c1, SexyColor c2)
{
return new(c1.Red + c2.Red, c1.Green + c2.Green, c1.Blue + c2.Blue, c1.Alpha + c2.Alpha);
}

public static SexyColor operator -(SexyColor c1, SexyColor c2)
{
return new(c1.Red - c2.Red, c1.Green - c2.Green, c1.Blue - c2.Blue, c1.Alpha - c2.Alpha);
}

public static SexyColor operator *(SexyColor c, byte factor)
{
return new(c.Red * factor, c.Green * factor, c.Blue * factor, c.Alpha * factor);
}

public static int operator %(SexyColor c1, SexyColor c2)
{
return c1.Red * c2.Red + c1.Green * c2.Green + c1.Blue * c2.Blue + c1.Alpha * c2.Alpha;
}

}