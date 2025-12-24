using System;
using System.IO;

public static class StreamPlugin
{
#region ==========  READER  ==========

// Read data as Ptr

public static NativeBuffer ReadPtr(this Stream reader)
{
long remaining = reader.Length - reader.Position;

return reader.ReadPtr(remaining);
}

// Read data as Ptr

public static NativeBuffer ReadPtr(this Stream reader, long count)
{
const int CHUNK_SIZE = int.MaxValue;

var toRead = (ulong)count;
NativeBuffer memOwner = new(toRead);

ulong totalRead = 0;

while(totalRead < toRead)
{
int blockSize = (int)Math.Min(CHUNK_SIZE, toRead - totalRead);

var buffer = memOwner.AsSpan(totalRead, blockSize);
reader.ReadExactly(buffer);

totalRead += (ulong)blockSize;
}

return memOwner;
}

// Read bool

public static bool ReadBool(this Stream reader) => reader.ReadByte() != 0;

// Read char (8-bits)

public static char ReadChar8(this Stream reader) => (char)reader.ReadUInt8();

// Read char (16-bits)

public static char ReadChar16(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
reader.ReadExactly(buffer);

return BinaryHelper.ReadChar16(buffer, endian);
}

// Read int8

public static sbyte ReadInt8(this Stream reader) => (sbyte)reader.ReadUInt8();

// Read uint8

public static byte ReadUInt8(this Stream reader)
{
int raw = reader.ReadByte();

if(raw == -1)
throw new EndOfStreamException();

return (byte)raw;
}

// Read int16

public static short ReadInt16(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
reader.ReadExactly(buffer);

return BinaryHelper.ReadInt16(buffer, endian);
}

// Read uint16

public static ushort ReadUInt16(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
reader.ReadExactly(buffer);

return BinaryHelper.ReadUInt16(buffer, endian);
}

// Read int24

public static int ReadInt24(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[3];
reader.ReadExactly(buffer);

return BinaryHelper.ReadInt24(buffer, endian);
}

// Read uint24

public static uint ReadUInt24(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[3];
reader.ReadExactly(buffer);

return BinaryHelper.ReadUInt24(buffer, endian);
}

// Read int32

public static int ReadInt32(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[4];
reader.ReadExactly(buffer);

return BinaryHelper.ReadInt32(buffer, endian);
}

// Read uint32

public static uint ReadUInt32(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[4];
reader.ReadExactly(buffer);

return BinaryHelper.ReadUInt32(buffer, endian);
}

// Read int64

public static long ReadInt64(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[8];
reader.ReadExactly(buffer);

return BinaryHelper.ReadInt64(buffer, endian);
}

// Read uint64

public static ulong ReadUInt64(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[8];
reader.ReadExactly(buffer);

return BinaryHelper.ReadUInt64(buffer, endian);
}

// Read int128

public static Int128 ReadInt128(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[16];
reader.ReadExactly(buffer);

return BinaryHelper.ReadInt128(buffer, endian);
}

// Read uint128

public static UInt128 ReadUInt128(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[16];
reader.ReadExactly(buffer);

return BinaryHelper.ReadUInt128(buffer, endian);
}

// Read varint

public static int ReadVarInt(this Stream reader)
{
return BinaryHelper.DecodeVarInt( () => reader.ReadUInt8(), out _);
}

// Read unsigned varint

public static uint ReadVarUInt(this Stream reader) => (uint)reader.ReadVarInt();

// Read varint64

public static long ReadVarInt64(this Stream reader)
{
return BinaryHelper.DecodeVarInt64( () => reader.ReadUInt8(), out _);
}

// Read unsigned varInt64

public static ulong ReadVarUInt64(this Stream reader) => (ulong)reader.ReadVarInt64();

// Read ZigZag int

public static int ReadZigZag(this Stream reader)
{
uint v = reader.ReadVarUInt();

return BinaryHelper.DecodeZigZag(v);
}

// Read ZigZag long

public static long ReadZigZag64(this Stream reader)
{
ulong v = reader.ReadVarUInt64();

return BinaryHelper.DecodeZigZag64(v);
}

// Read half

public static Half ReadHalf(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
reader.ReadExactly(buffer);

return BinaryHelper.ReadHalf(buffer, endian);
}

// Read float

public static float ReadFloat(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[4];
reader.ReadExactly(buffer);

return BinaryHelper.ReadFloat(buffer, endian);
}

// Read double

public static double ReadDouble(this Stream reader, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[8];
reader.ReadExactly(buffer);

return BinaryHelper.ReadDouble(buffer, endian);
}

// Read whole file as string

public static NativeString ReadString(this Stream reader, EncodingType encoding = EncodingType.UTF8)
{
long remaining = reader.Length - reader.Position;

return reader.ReadString(remaining, encoding);
}

// Read string 

public static NativeString ReadString(this Stream reader, long strLen,
                                      EncodingType encoding = EncodingType.UTF8)
{

if(strLen <= SizeT.MAX_STACK)
{
Span<byte> buffer = stackalloc byte[(int)strLen];
reader.ReadExactly(buffer);

return BinaryHelper.GetNativeString(buffer, encoding);
}

using var rawBytes = reader.ReadPtr(strLen);

return BinaryHelper.GetNativeString(rawBytes.AsSpan(), encoding);
}

// Read string prefixed by int8 length

public static NativeString ReadStringByLen8(this Stream reader, EncodingType encoding = EncodingType.UTF8)
{
byte strLen = reader.ReadUInt8();

return reader.ReadString(strLen, encoding);
}

// Read string prefixed by int16 length

public static NativeString ReadStringByLen16(this Stream reader, EncodingType encoding = EncodingType.UTF8,
                                             Endianness endian = default)
{
ushort strLen = reader.ReadUInt16(endian);

return reader.ReadString(strLen, encoding);
}

// Read string prefixed by int32 length

public static NativeString ReadStringByLen32(this Stream reader, EncodingType encoding = EncodingType.UTF8,
                                             Endianness endian = default)
{
uint strLen = reader.ReadUInt32(endian);

return reader.ReadString(strLen, encoding);
}

// Read string prefixed by int64 length

public static NativeString ReadStringByLen64(this Stream reader, EncodingType encoding = EncodingType.UTF8,
                                             Endianness endian = default)
{
long strLen = reader.ReadInt64(endian);

return reader.ReadString(strLen, encoding);
}

// Read string prefixed by varint length

public static NativeString ReadStringByVarLen(this Stream reader, EncodingType encoding = EncodingType.UTF8)
{
int strLen = reader.ReadVarInt();

return reader.ReadString(strLen, encoding);
}

// Read string prefixed by varint64 length

public static NativeString ReadStringByVarLen64(this Stream reader, EncodingType encoding = EncodingType.UTF8)
{
long strLen = reader.ReadVarInt64();

return reader.ReadString(strLen, encoding);
}

// Read string until '\0' is reached

public static NativeString ReadCString(this Stream reader, EncodingType encoding = EncodingType.UTF8)
{
using NativeMemoryOwner<byte> buffer = new(256);
int length = 0;

while(true)
{
int raw = reader.ReadByte();

if(raw == -1)
break;

var b = (byte)raw;

if(b == 0x00)
break;

if(length >= (int)buffer.Size)
buffer.Realloc(buffer.Size * 2);

buffer[length++] = b;	
}

return BinaryHelper.GetNativeString(buffer.AsSpan(0, length), encoding);
}

// Read line

public static NativeString ReadLine(this Stream reader, EncodingType encoding = EncodingType.UTF8)
{
using NativeMemoryOwner<byte> buffer = new(512);
int length = 0;

while(true)
{
int raw = reader.ReadByte();

if(raw == -1)
return null;

var b = (byte)raw;

if(b == 0x0D) // '\r'
{
int peek = reader.PeekByte();

if(peek == 0x0A) // Handle '\r\n' as '\n'
reader.ReadByte();

break;
}

else if(b == 0x0A) // '\n'
break;

if(length >= (int)buffer.Size)
buffer.Realloc(buffer.Size * 2);

buffer[length++] = b;	
}

return BinaryHelper.GetNativeString(buffer.AsSpan(0, length), encoding);
}

#endregion


#region ==========  WRITER  =========

// Write bool

public static void WriteBool(this Stream writer, bool v)
{
var b = (byte)(v ? 1u : 0u);

writer.WriteByte(b);
}

// Write char (8-bits)

public static void WriteChar8(this Stream writer, char c) => writer.WriteByte( (byte)c);

// Write char (16-bits)

public static void WriteChar16(this Stream writer, char c, Endianness endian = default)
{
writer.WriteUInt16(c, endian);
}

// Write sbyte

public static void WriteInt8(this Stream writer, sbyte v) => writer.WriteByte( (byte)v);

// Write short

public static void WriteInt16(this Stream writer, short v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
BinaryHelper.WriteInt16(v, buffer, endian);

writer.Write(buffer);
}

// Write ushort

public static void WriteUInt16(this Stream writer, ushort v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
BinaryHelper.WriteUInt16(v, buffer, endian);

writer.Write(buffer);
}

// Write int24

public static void WriteInt24(this Stream writer, int v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[3];
BinaryHelper.WriteInt24(v, buffer, endian);

writer.Write(buffer);
}

// Write uint24

public static void WriteUInt24(this Stream writer, uint v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[3];
BinaryHelper.WriteUInt24(v, buffer, endian);

writer.Write(buffer);
}

// Write int32

public static void WriteInt32(this Stream writer, int v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[4];
BinaryHelper.WriteInt32(v, buffer, endian);

writer.Write(buffer);
}

// Write uint32

public static void WriteUInt32(this Stream writer, uint v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[4];

BinaryHelper.WriteUInt32(v, buffer, endian);
writer.Write(buffer);
}

// Write int64

public static void WriteInt64(this Stream writer, long v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[8];
BinaryHelper.WriteInt64(v, buffer, endian);

writer.Write(buffer);
}

// Write uint6

public static void WriteUInt64(this Stream writer, ulong v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[8];
BinaryHelper.WriteUInt64(v, buffer, endian);

writer.Write(buffer);
}

// Write int128

public static void WriteInt128(this Stream writer, Int128 v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[16];
BinaryHelper.WriteInt128(v, buffer, endian);

writer.Write(buffer);
}

// Write uint128

public static void WriteUInt128(this Stream writer, UInt128 v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[16];
BinaryHelper.WriteUInt128(v, buffer, endian);

writer.Write(buffer);
}

// Write varint

public static void WriteVarInt(this Stream writer, int v)
{
BinaryHelper.EncodeVarInt(writer.WriteByte, v, out _);
}

// Write unsigned varint

public static void WriteVarUInt(this Stream writer, uint v) => writer.WriteVarInt( (int)v);

// Write varint64

public static void WriteVarInt64(this Stream writer, long v)
{
BinaryHelper.EncodeVarInt64(writer.WriteByte, v, out _);
}

// Write unsigned varint64

public static void WriteVarUInt64(this Stream writer, ulong v) => writer.WriteVarInt64( (long)v);

// Write ZigZag int

public static void WriteZigZag32(this Stream writer, int v)
{
int zigZag = BinaryHelper.EncodeZigZag(v);

writer.WriteVarInt(zigZag);
}

// Write ZigZag long

public static void WriteZigZag64(this Stream writer, long v)
{
long zigZag = BinaryHelper.EncodeZigZag64(v);

writer.WriteVarInt64(zigZag);
}

// Write half

public static void WriteHalf(this Stream writer, Half v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[2];
BinaryHelper.WriteHalf(v, buffer, endian);

writer.Write(buffer);
}

// Write float

public static void WriteFloat(this Stream writer, float v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[4];
BinaryHelper.WriteFloat(v, buffer, endian);

writer.Write(buffer);
}

// Write double

public static void WriteDouble(this Stream writer, double v, Endianness endian = default)
{
Span<byte> buffer = stackalloc byte[8];
BinaryHelper.WriteDouble(v, buffer, endian);

writer.Write(buffer);
}

// Write string

public static void WriteString(this Stream writer, ReadOnlySpan<char> str,
                               EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
return;

using var buffer = BinaryHelper.GetNativeBytes(str, encoding);

writer.Write(buffer.AsSpan() );
}

// Write string prefixed by uint8 length

public static void WriteStringByLen8(this Stream writer, ReadOnlySpan<char> str,
                                     EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
{
writer.WriteByte(0x00);
return;
}

var rawLen = (byte)BinaryHelper.GetEncodedLength(str, encoding);
writer.WriteByte(rawLen);

writer.WriteString(str, encoding);
}

// Write string prefixed by uint16 length

public static void WriteStringByLen16(this Stream writer, ReadOnlySpan<char> str,
                                       EncodingType encoding = EncodingType.UTF8,
									   Endianness endian = default)
{

if(str.IsEmpty)
{
writer.WriteUInt16(0);
return;
}

var rawLen = (ushort)BinaryHelper.GetEncodedLength(str, encoding);
writer.WriteUInt16(rawLen, endian);

writer.WriteString(str, encoding);
}

// Write string prefixed by uint32 length

public static void WriteStringByLen32(this Stream writer, ReadOnlySpan<char> str,
                                      EncodingType encoding = EncodingType.UTF8,
                                      Endianness endian = default)
{

if(str.IsEmpty)
{
writer.WriteUInt32(0);
return;
}

int rawLen = BinaryHelper.GetEncodedLength(str, encoding);
writer.WriteInt32(rawLen, endian);

writer.WriteString(str, encoding);
}

// Write string prefixed by uint64 length

public static void WriteStringByLen64(this Stream writer, ReadOnlySpan<char> str,
                                      EncodingType encoding = EncodingType.UTF8,
                                      Endianness endian = default)
{

if(str.IsEmpty)
{
writer.WriteUInt64(0);
return;
}

long rawLen = BinaryHelper.GetEncodedLength(str, encoding);
writer.WriteInt64(rawLen, endian);

writer.WriteString(str, encoding);
}

// Write string prefixed by varint length

public static void WriteStringByVarLen(this Stream writer, ReadOnlySpan<char> str,
                                       EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
{
writer.WriteVarInt(0);
return;
}

int rawLen = BinaryHelper.GetEncodedLength(str, encoding);
writer.WriteVarInt(rawLen);

writer.WriteString(str, encoding);
}

// Write string prefixed by varint64 length

public static void WriteStringByVarLen64(this Stream writer, ReadOnlySpan<char> str,
                                         EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
{
writer.WriteVarInt64(0);
return;
}

long rawLen = BinaryHelper.GetEncodedLength(str, encoding);
writer.WriteVarInt64(rawLen);

writer.WriteString(str, encoding);
}

// Write c-string

public static void WriteCString(this Stream writer, ReadOnlySpan<char> str,
                                EncodingType encoding = EncodingType.UTF8)
{
writer.WriteString(str, encoding);

writer.WriteChar8('\0');
}

// Write line

public static void WriteLine(this Stream writer, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8)
{
writer.WriteString(str, encoding);

writer.WriteString(Environment.NewLine, encoding);
}

#endregion


#region ==========  HELPER  =========

// Fill stream buffer

public static void Fill(this Stream writer, int count, byte padding = 0x00)
{

if(count <= 0)
return;

if(count == 1)
{
writer.WriteByte(padding);

return;
}

Span<byte> temp = stackalloc byte[1024];
temp.Fill(padding);

while(count > 0)
{
int chunk = Math.Min(count, temp.Length);
writer.Write(temp[..chunk]);

count -= chunk;
}

}

// Align stream

public static void Align(this Stream writer, int alignment, byte padding = 0x00)
{

if(alignment <= 0)
return;

if(!writer.CanSeek)
throw new NotSupportedException("Align requires a seekable stream");

var required = (int)SizeT.GetPadding(writer.Position, alignment);

writer.Fill(required, padding);
}

// Peek byte

public static int PeekByte(this Stream stream)
{

if(!stream.CanSeek)
throw new NotSupportedException("PeekByte requires a seekable stream");

long pos = stream.Position;
int b = stream.ReadByte();

if(b != -1)
stream.Position = pos;

return b;
}

#endregion
}