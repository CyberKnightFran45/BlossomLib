using System;
using System.Text;

// Convert between uint and String

public static class String32
{
// Encoding used

private static readonly Encoding encoding = EncodeHelper.GetEncoding(EncodingType.ISO_8859_1);

// Endianness

private const Endianness ENDIAN = Endianness.BigEndian;

// Convert string to uint

public static uint ToInt(ReadOnlySpan<char> v)
{

if(v.Length > 4)
v = v[.. 4];

Span<byte> buffer = stackalloc byte[4];
buffer.Clear();

encoding.GetBytes(v, buffer);

return BinaryHelper.ReadUInt32(buffer, ENDIAN);
}

// Convert uint to string

public static string FromInt(uint v)
{

if(v == 0)
return string.Empty;

Span<byte> buffer = stackalloc byte[4];
BinaryHelper.WriteUInt32(v, buffer, ENDIAN);

return encoding.GetString(buffer).TrimEnd('\0');
}

}