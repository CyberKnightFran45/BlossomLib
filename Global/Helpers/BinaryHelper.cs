using System;
using System.Buffers.Binary;

/// <summary> Allows reading or writing values from Binary buffers  </summary>

public static class BinaryHelper
{
#region ==========  READER  =========

// Read char (16-bits)

public static char ReadChar16(ReadOnlySpan<byte> buffer, Endianness endian)
{
return (char)ReadUInt16(buffer, endian);
}

// Read short

public static short ReadInt16(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadInt16BigEndian(buffer);

return BinaryPrimitives.ReadInt16LittleEndian(buffer);
}

// Read ushort

public static ushort ReadUInt16(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadUInt16BigEndian(buffer);

return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
}

// Read int24

public static int ReadInt24(ReadOnlySpan<byte> buffer, Endianness endian)
{
uint v = ReadUInt24(buffer, endian);

if( (v & 0x800000) != 0) 
v |= 0xFF000000;

return (int)v;
}

// Read uint24

public static uint ReadUInt24(ReadOnlySpan<byte> buffer, Endianness endian)
{
uint b1, b2, b3;

if(endian == Endianness.BigEndian)
{
b1 = (uint)buffer[0] << 16;
b2 = (uint)buffer[1] << 8;
b3 = buffer[2];
}

else
{
b1 = buffer[0];
b2 = (uint)buffer[1] << 8;
b3 = (uint)buffer[2] << 16;
}

return b1 | b2 | b3;
}

// Read int

public static int ReadInt32(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadInt32BigEndian(buffer);

return BinaryPrimitives.ReadInt32LittleEndian(buffer);
}

// Read uint

public static uint ReadUInt32(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadUInt32BigEndian(buffer);

return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
}

// Read long

public static long ReadInt64(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadInt64BigEndian(buffer);

return BinaryPrimitives.ReadInt64LittleEndian(buffer);
}

// Read ulong

public static ulong ReadUInt64(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadUInt64BigEndian(buffer);

return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
}

// Read int128

public static Int128 ReadInt128(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadInt128BigEndian(buffer);

return BinaryPrimitives.ReadInt128LittleEndian(buffer);
}

// Read uint128

public static UInt128 ReadUInt128(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadUInt128BigEndian(buffer);

return BinaryPrimitives.ReadUInt128LittleEndian(buffer);
}

// Read half

public static Half ReadHalf(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadHalfBigEndian(buffer);

return BinaryPrimitives.ReadHalfLittleEndian(buffer);
}

// Read float

public static float ReadFloat(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadSingleBigEndian(buffer);

return BinaryPrimitives.ReadSingleLittleEndian(buffer);
}

// Read double

public static double ReadDouble(ReadOnlySpan<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
return BinaryPrimitives.ReadDoubleBigEndian(buffer);

return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
}

// Decode varint (Inner)

private static int DecodeVarInt(Func<byte> readFunc, int mask, out int bytesRead)
{
bytesRead = 0;

int result = 0;
int shift = 0;

byte b;

while(shift < mask)
{
b = readFunc();
bytesRead++;

result |= (b & 0x7F) << shift;

if( (b & 0x80) == 0)
break;

shift += 7;
}

return result;
}

// Decode varint

public static int DecodeVarInt(Func<byte> readFunc, out int bytesRead)
{
return DecodeVarInt(readFunc, 35, out bytesRead);
}

// Decode varint64

public static long DecodeVarInt64(Func<byte> readFunc, out int bytesRead)
{
return DecodeVarInt(readFunc, 70, out bytesRead);
}

// Decode VarInt as ZigZag

public static int DecodeZigZag(uint v)
{

if( (v & 0b1) == 0)
return (int)(v >> 1);

return -(int)( (v + 1) >> 1);
}

// Decode VarInt as ZigZag (64-bits)

public static long DecodeZigZag64(ulong v)
{

if( (v & 0b1) == 0)
return (long)(v >> 1);

return -(long)( (v + 1) >> 1);
}

#endregion


#region ==========  WRITER  =========

// Write char (16-bits)

public static void WriteChar16(char c, Span<byte> buffer, Endianness endian)
{
WriteUInt16(c, buffer, endian);
}

// Write short

public static void WriteInt16(short v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteInt16BigEndian(buffer, v);

else
BinaryPrimitives.WriteInt16LittleEndian(buffer, v);

}

// Write ushort

public static void WriteUInt16(ushort v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteUInt16BigEndian(buffer, v);

else
BinaryPrimitives.WriteUInt16LittleEndian(buffer, v);

}

// Write int24

public static void WriteInt24(int v, Span<byte> buffer, Endianness endian)
{
var u = (uint)(v & 0xFFFFFF);

WriteUInt24(u, buffer, endian);
}

// Write uint24

public static void WriteUInt24(uint v, Span<byte> buffer, Endianness endian)
{
byte b1, b2, b3;

if(endian == Endianness.BigEndian)
{
b1 = (byte)(v >> 16);
b2 = (byte)(v >> 8);
b3 = (byte)v;
}

else
{
b1 = (byte)v;
b2 = (byte)(v >> 8);
b3 = (byte)(v >> 16);
}

buffer[0] = b1;
buffer[1] = b2;
buffer[2] = b3;
}

// Write int

public static void WriteInt32(int v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteInt32BigEndian(buffer, v);

else
BinaryPrimitives.WriteInt32LittleEndian(buffer, v);

}

// Write uint

public static void WriteUInt32(uint v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteUInt32BigEndian(buffer, v);

else
BinaryPrimitives.WriteUInt32LittleEndian(buffer, v);

}

// Write long

public static void WriteInt64(long v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteInt64BigEndian(buffer, v);

else
BinaryPrimitives.WriteInt64LittleEndian(buffer, v);

}

// Write ulong

public static void WriteUInt64(ulong v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteUInt64BigEndian(buffer, v);

else
BinaryPrimitives.WriteUInt64LittleEndian(buffer, v);

}

// Write int128

public static void WriteInt128(Int128 v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteInt128BigEndian(buffer, v);

else
BinaryPrimitives.WriteInt128LittleEndian(buffer, v);

}

// Write uint128

public static void WriteUInt128(UInt128 v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteUInt128BigEndian(buffer, v);

else
BinaryPrimitives.WriteUInt128LittleEndian(buffer, v);

}

// Write half

public static void WriteHalf(Half v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteHalfBigEndian(buffer, v);

else
BinaryPrimitives.WriteHalfLittleEndian(buffer, v);

}

// Write float

public static void WriteFloat(float v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteSingleBigEndian(buffer, v);

else
BinaryPrimitives.WriteSingleLittleEndian(buffer, v);

}

// Write double

public static void WriteDouble(double v, Span<byte> buffer, Endianness endian)
{

if(endian == Endianness.BigEndian)
BinaryPrimitives.WriteDoubleBigEndian(buffer, v);

else
BinaryPrimitives.WriteDoubleLittleEndian(buffer, v);

}

// Encode varint (Inner)

private static void EncodeVarInt(Action<byte> writeFunc, ulong v, out int bytesWritten)
{
bytesWritten = 0;

while(v > 0x7F)
{
writeFunc( (byte)(v | 0x80) );
bytesWritten++;

v >>= 7;
}

writeFunc( (byte)v);
bytesWritten++;
}

// Encode varint

public static void EncodeVarInt(Action<byte> writeFunc, int v, out int bytesWritten)
{
EncodeVarInt(writeFunc, (ulong)v, out bytesWritten);
}

// Encode varint64

public static void EncodeVarInt64(Action<byte> writeFunc, long v, out int bytesWritten)
{
EncodeVarInt(writeFunc, (ulong)v, out bytesWritten);
}

// Encode ZigZag (inner)

private static long EncodeZigZag(long v, int mask) => (v << 1) ^ (v >> mask);

// Encode ZigZag int

public static int EncodeZigZag(int v) => (int)EncodeZigZag(v, 31);

// Encode ZigZag long

public static long EncodeZigZag64(long v) => EncodeZigZag(v, 63);

#endregion


#region ==========  CONVERTER  =========

// Get Encoded length

public static int GetEncodedLength(ReadOnlySpan<char> str, EncodingType encodeFlags)
{
var encoding = encodeFlags.GetEncoding();

return encoding.GetByteCount(str);
}

/** <summary> Converts a sequence of Chars into Bytes. </summary>

<param name = "str"> String to Convert. </param>
<param name = "encodeFlags"> Encoding to use. </param>

<returns> The Bytes converted. </returns> **/

public static byte[] GetBytes(ReadOnlySpan<char> str, EncodingType encodeFlags)
{

if(str.IsEmpty)
return [];

var encoding = encodeFlags.GetEncoding();

int byteCount = encoding.GetByteCount(str);
byte[] result = new byte[byteCount];

encoding.GetBytes(str, result);

return result;
}

/** <summary> Converts a sequence of Chars into a BytePtr. </summary>

<param name = "str"> String to Convert. </param>
<param name = "encodeFlags"> Encoding to use. </param>

<returns> The NativeBytes. </returns> */

public static NativeMemoryOwner<byte> GetNativeBytes(ReadOnlySpan<char> str, EncodingType encodeFlags)
{

if(str.IsEmpty)
return new();

var encoding = encodeFlags.GetEncoding();

int byteCount = encoding.GetByteCount(str);
NativeMemoryOwner<byte> bytes = new(byteCount);

var view = bytes.AsSpan();
encoding.GetBytes(str, view);

return bytes;
}

/** <summary> Converts a sequence of Bytes into a String. </summary>

<param name = "bytes"> Bytes to Convert. </param>
<param name = "encodeFlags> Encoding to use. </param>

<returns> The String converted </returns> */

public static string GetString(ReadOnlySpan<byte> bytes, EncodingType encodeFlags)
{

if(bytes.IsEmpty)
return string.Empty;

var encoding = encodeFlags.GetEncoding();

return encoding.GetString(bytes);
}

/** <summary> Converts a sequence of Bytes into a NativeString. </summary>

<param name = "bytes"> Bytes to Convert. </param>
<param name = "encodeFlags"> Encoding to use. </param>

<returns> The String converted. </returns> */

public static NativeString GetNativeString(ReadOnlySpan<byte> bytes, EncodingType encodeFlags)
{

if(bytes.IsEmpty)
return new();

var encoding = encodeFlags.GetEncoding();

int charCount = encoding.GetCharCount(bytes);
NativeString str = new(charCount);

var charSpan = str.AsSpan();
encoding.GetChars(bytes, charSpan);

return str;
}

// Convert int to hex Char

private static char ToHex(int v, StringCase strCase)
{
char offset = strCase == StringCase.Upper ? 'A' : 'a';

return (char)(v < 10 ? '0' + v : offset + (v - 10) );
}

/** <summary> Converts some Bytes into a hexadecimal String. </summary>

<param name = "input"> The Bytes to Convert. </param>
<param name = "strCase"> String case. </param>

<returns> The hex String. </returns> */

public static NativeString ToHex(ReadOnlySpan<byte> input, StringCase strCase)
{

if(input.IsEmpty)
return new();

int maxLen = input.Length * 2;
NativeString hex = new(maxLen);

int index = 0;

for(int i = 0; i < input.Length; i++)
{
byte b = input[i];

hex[index++] = ToHex(b >> 4, strCase);
hex[index++] = ToHex(b & 0xF, strCase);
}

hex.Realloc(index);

return hex;
}

// Convert hex char

private static byte FromHex(char c)
{

if(c >= '0' && c <= '9')
return (byte)(c - '0');

if(c >= 'A' && c <= 'F')
return (byte)(c - 'A' + 10);

if(c >= 'a' && c <= 'f')
return (byte)(c - 'a' + 10);

return 0;
}

/** <summary> Converts a hexadecimal String to a sequence of Bytes. </summary>

<param name="hexStr"> The Hexadecimal String to Convert. </param>
<param name="separator"> The Separator Character to use between Hex Bytes. </param>

<returns> A NativeMemoryOwner containing the raw Bytes. </returns> */

public static NativeMemoryOwner<byte> FromHex(ReadOnlySpan<char> hex)
{

if(hex.IsEmpty)
return new();

int maxBytes = (hex.Length + 1) / 2;
NativeMemoryOwner<byte> rawBytes = new(maxBytes);

int byteCount = 0;
int i = 0;

if(hex.Length % 2 != 0)
{
rawBytes[byteCount++] = FromHex(hex[0]);

i = 1;
}

while(i < hex.Length)
{
byte high = FromHex(hex[i]);
byte low  = FromHex(hex[i + 1]);

rawBytes[byteCount++] = (byte)((high << 4) | low);
i += 2;
}

rawBytes.Realloc(byteCount);

return rawBytes;
}

#endregion
}