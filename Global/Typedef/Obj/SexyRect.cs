using System.IO;
using System.Text.Json.Serialization;

/// <summary> Represents a Rectangle in the SexyFramework. </summary>
public class SexyRect
{
[JsonPropertyName("mX") ]
public int X{ get; set; }

[JsonPropertyName("mY") ]
public int Y{ get; set; }

[JsonPropertyName("mWidth") ]
public int Width{ get; set; }

[JsonPropertyName("mHeight") ]
public int Height{ get; set; }

public SexyRect()
{
}

public SexyRect(int x, int y, int width, int height)
{
X = x;
Y = y;
Width = width;
Height = height;
}

// Read as binary

public static SexyRect Read(Stream reader)
{
int x = reader.ReadInt32();
int y = reader.ReadInt32();
int width = reader.ReadInt32();
int height = reader.ReadInt32();

return new(x, y, width, height);
}

// Write to binary

public void Write(Stream writer)
{
writer.WriteInt32(X);
writer.WriteInt32(Y);
writer.WriteInt32(Width);
writer.WriteInt32(Height);
}

}