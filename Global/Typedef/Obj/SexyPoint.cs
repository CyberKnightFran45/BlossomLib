using System.IO;
using System.Text.Json.Serialization;

/// <summary> Represents a Point in the SexyFramework. </summary>

public class SexyPoint
{
[JsonPropertyName("mX") ]

public int X{ get; set; } 

[JsonPropertyName("mY") ]

public int Y{ get; set; }

public SexyPoint()
{
}

public SexyPoint(int x, int y)
{
X = x;
Y = y;
}

// Read as binary

public static SexyPoint Read(Stream reader)
{
int x = reader.ReadInt32();
int y = reader.ReadInt32();

return new(x, y);
}

// Write to binary

public void Write(Stream writer)
{
writer.WriteInt32(X);
writer.WriteInt32(Y);
}

}