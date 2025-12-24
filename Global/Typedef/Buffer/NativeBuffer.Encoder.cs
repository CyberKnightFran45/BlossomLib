using System;
using System.Runtime.CompilerServices;

/// <summary> Represents a buffer that supports Binary operations </summary>

public sealed partial class NativeBuffer : NativeMemoryOwner<byte>
{
/// <summary> Initializes a empty <see cref="NativeBuffer"/>  </summary>

public NativeBuffer()
{
}

/** <summary> Initializes a new <see cref="NativeBuffer"/> with n bytes. </summary>

<param name="chars"> The amount of bytes. </param> **/
	
public NativeBuffer(long bytes) : base(bytes)
{
}

/** <summary> Initializes a new <see cref="NativeBuffer"/> with the specified length. </summary>

<param name="length"> The string length </param> **/

public NativeBuffer(ulong length) : base(length)
{
}

// Set boolean

public void SetBool(ulong index, bool v)
{
this[index] = (byte)(v ? 1u : 0u);
}

public void SetBool(long index, bool v) => SetBool(ClampIdx(index), v);

// Set char (8-bits)

public void SetChar8(ulong index, char c) => this[index] = (byte)c;

public void SetChar8(long index, char c) => SetChar8(ClampIdx(index), c);

// Set char (16-bits)

public void SetChar16(ulong index, char c, Endianness endian = default)
{
var view = AsSpan(index, 2);

BinaryHelper.WriteChar16(c, view, endian);
}

public void SetChar16(long index, char c, Endianness endian = default)
{
SetChar16(ClampIdx(index), c, endian);
}

// Set int8

public void SetInt8(ulong index, sbyte v) => this[index] = (byte)v;

public void SetInt8(long index, sbyte v) => SetInt8(ClampIdx(index), v);

// Set uint8

public void SetUInt8(ulong index, byte b) => this[index] = b;

public void SetUInt8(long index, byte b) => SetUInt8(ClampIdx(index), b);

// Set int16

public void SetInt16(ulong index, short v, Endianness endian = default)
{
var view = AsSpan(index, 2);

BinaryHelper.WriteInt16(v, view, endian);
}

public void SetInt16(long index, short v, Endianness endian = default)
{
SetInt16(ClampIdx(index), v, endian);
}

// Set uint16

public void SetUInt16(ulong index, ushort v, Endianness endian = default)
{
var view = AsSpan(index, 2);

BinaryHelper.WriteUInt16(v, view, endian);
}

public void SetUInt16(long index, ushort v, Endianness endian = default)
{
SetUInt16(ClampIdx(index), v, endian);
}

// Set int24

public void SetInt24(ulong index, int v, Endianness endian = default)
{
var view = AsSpan(index, 3);

BinaryHelper.WriteInt24(v, view, endian);
}

public void SetInt24(long index, int v, Endianness endian = default)
{
SetInt24(ClampIdx(index), v, endian);
}

// Set uint24

public void SetUInt24(ulong index, uint v, Endianness endian = default)
{
var view = AsSpan(index, 3);

BinaryHelper.WriteUInt24(v, view, endian);
}

public void SetUInt24(long index, uint v, Endianness endian = default)
{
SetUInt24(ClampIdx(index), v, endian);
}

// Set int32

public void SetInt32(ulong index, int v, Endianness endian = default)
{
var view = AsSpan(index, 4);

BinaryHelper.WriteInt32(v, view, endian);
}

public void SetInt32(long index, int v, Endianness endian = default)
{
SetInt32(ClampIdx(index), v, endian);
}

// Set uint32

public void SetUInt32(ulong index, uint v, Endianness endian = default)
{
var view = AsSpan(index, 4);

BinaryHelper.WriteUInt32(v, view, endian);
}

public void SetUInt32(long index, uint v, Endianness endian = default)
{
SetUInt32(ClampIdx(index), v, endian);
}

// Set int64

public void SetInt64(ulong index, long v, Endianness endian = default)
{
var view = AsSpan(index, 8);

BinaryHelper.WriteInt64(v, view, endian);
}

public void SetInt64(long index, long v, Endianness endian = default)
{
SetInt64(ClampIdx(index), v, endian);
}

// Set uint64

public void SetUInt64(ulong index, ulong v, Endianness endian = default)
{
var view = AsSpan(index, 8);

BinaryHelper.WriteUInt64(v, view, endian);
}

public void SetUInt64(long index, ulong v, Endianness endian = default)
{
SetUInt64(ClampIdx(index), v, endian);
}

// Set int128

public void SetInt128(ulong index, Int128 v, Endianness endian = default)
{
var view = AsSpan(index, 16);

BinaryHelper.WriteInt128(v, view, endian);
}

public void SetInt128(long index, Int128 v, Endianness endian = default)
{
SetInt128(ClampIdx(index), v, endian);
}

// Set uint128

public void SetUInt128(ulong index, UInt128 v, Endianness endian = default)
{
var view = AsSpan(index, 16);

BinaryHelper.WriteUInt128(v, view, endian);
}

public void SetUInt128(long index, UInt128 v, Endianness endian = default)
{
SetUInt128(ClampIdx(index), v, endian);
}

// Set byte and advance one pos

private void SetNextByte(byte b, ref ulong pos)
{
SetUInt8(pos, b);

pos++;
}

// Set VarInt

public void SetVarInt(ulong index, int v, out int bytesWritten)
{
ulong pos = index;

BinaryHelper.EncodeVarInt(b => SetNextByte(b, ref pos), v, out bytesWritten);
}

public void SetVarInt(long index, int v, out int bytesWritten)
{
SetVarInt(ClampIdx(index), v, out bytesWritten);
}

// Set VarInt64

public void SetVarInt64(ulong index, long v, out int bytesWritten)
{
ulong pos = index;

BinaryHelper.EncodeVarInt64(b => SetNextByte(b, ref pos), v, out bytesWritten);
}

public void SetVarInt64(long index, long v, out int bytesWritten)
{
SetVarInt64(ClampIdx(index), v, out bytesWritten);
}

// Set ZigZag int

public void SetZigZag(ulong index, int v)
{
int zigZag = BinaryHelper.EncodeZigZag(v);

SetVarInt(index, zigZag, out _);
}

public void SetZigZag(long index, int v)
{
SetZigZag(ClampIdx(index), v);
}

// Set ZigZag long

public void SetZigZag64(ulong index, long v)
{
long zigZag = BinaryHelper.EncodeZigZag64(v);

SetVarInt64(index, zigZag, out _);
}

public void SetZigZag64(long index, long v)
{
SetZigZag64(ClampIdx(index), v);
}

// Set half

public void SetHalf(ulong index, Half v, Endianness endian = default)
{
var view = AsSpan(index, 2);

BinaryHelper.WriteHalf(v, view, endian);
}

public void SetHalf(long index, Half v, Endianness endian = default)
{
SetHalf(ClampIdx(index), v, endian);
}

// Set float

public void SetFloat(ulong index, float v, Endianness endian = default)
{
var view = AsSpan(index, 4);

BinaryHelper.WriteFloat(v, view, endian);
}

public void SetFloat(long index, float v, Endianness endian = default)
{
SetFloat(ClampIdx(index), v, endian);
}

// Set double

public void SetDouble(ulong index, double v, Endianness endian = default)
{
var view = AsSpan(index, 8);

BinaryHelper.WriteDouble(v, view, endian);
}

public void SetDouble(long index, double v, Endianness endian = default)
{
SetDouble(ClampIdx(index), v, endian);
}

// Set string

public void SetString(ulong index, ReadOnlySpan<char> str, out ulong bytesWritten,
                      EncodingType encoding = EncodingType.UTF8)
{
bytesWritten = 0;

if(str.IsEmpty)
return;

using var rawBytes = BinaryHelper.GetNativeBytes(str, encoding);
bytesWritten = rawBytes.Size;

CopyFrom(rawBytes, index);
}

public void SetString(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetString(ClampIdx(index), str, out _, encoding);
}

// Set string prefixed by int8 length

public void SetStringByLen8(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
{
SetUInt8(index, 0x00);
return;
}

var rawLen = (byte)BinaryHelper.GetEncodedLength(str, encoding);
SetUInt8(index, rawLen);

SetString(index + 1, str, out _, encoding);
}

public void SetStringByLen8(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetStringByLen8(ClampIdx(index), str, encoding);
}

// Set string prefixed by int16 length

public void SetStringByLen16(ulong index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8,
                             Endianness endian = default)
{

if(str.IsEmpty)
{
SetUInt16(index, 0);
return;
}

var rawLen = (ushort)BinaryHelper.GetEncodedLength(str, encoding);
SetUInt16(index, rawLen, endian);

SetString(index + 2, str, out _, encoding);
}

public void SetStringByLen16(long index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8,
                             Endianness endian = default)
{
SetStringByLen16(ClampIdx(index), str, encoding, endian);
}

// Set string prefixed by int32 length

public void SetStringByLen32(ulong index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8,
                             Endianness endian = default)
{

if(str.IsEmpty)
{
SetUInt32(index, 0);
return;
}

int rawLen = BinaryHelper.GetEncodedLength(str, encoding);
SetInt32(index, rawLen, endian);

SetString(index + 4, str, out _, encoding);
}

public void SetStringByLen32(long index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8,
                             Endianness endian = default)
{
SetStringByLen32(ClampIdx(index), str, encoding, endian);
}

// Set string prefixed by int64 length

public void SetStringByLen64(ulong index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8,
                             Endianness endian = default)
{

if(str.IsEmpty)
{
SetUInt64(index, 0);
return;
}

long rawLen = BinaryHelper.GetEncodedLength(str, encoding);
SetInt64(index, rawLen, endian);

SetString(index + 8, str, out _, encoding);
}

public void SetStringByLen64(long index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8,
                             Endianness endian = default)
{
SetStringByLen64(ClampIdx(index), str, encoding, endian);
}

// Set string prefixed by varint length

public void SetStringByVarLen(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
{
SetVarInt(index, 0, out _);
return;
}

int rawLen = BinaryHelper.GetEncodedLength(str, encoding);
SetVarInt(index, rawLen, out int bytesWritten);

ulong strIndex = index + (ulong)bytesWritten;
SetString(strIndex, str, out _, encoding);
}

public void SetStringByVarLen(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetStringByVarLen(ClampIdx(index), str, encoding);
}

// Set string prefixed by varint64 length

public void SetStringByVarLen64(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
{
SetVarInt64(index, 0, out _);
return;
}

long rawLen = BinaryHelper.GetEncodedLength(str, encoding);
SetVarInt64(index, rawLen, out int bytesWritten);

ulong strIndex = index + (ulong)bytesWritten;
SetString(strIndex, str, out _, encoding);
}

public void SetStringByVarLen64(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetStringByVarLen64(ClampIdx(index), str, encoding);
}

// Set C-string

public void SetCString(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetString(index, str, out var bytesWritten, encoding);

SetChar8(index + bytesWritten, '\0');
}

public void SetCString(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetCString(ClampIdx(index), str, encoding);
}

// Get line

public void SetLine(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetString(index, str, out var bytesWritten, encoding);

SetString(index + bytesWritten, Environment.NewLine, out _, encoding);
}

public void SetLine(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
SetLine(ClampIdx(index), str, encoding);
}

}