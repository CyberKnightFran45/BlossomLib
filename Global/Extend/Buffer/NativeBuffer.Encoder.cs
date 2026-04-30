using System;

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

// Set bool (16-bits)

public void SetBool16(ulong index, bool v)
{
var u = (ushort)(v ? 1u : 0u);

SetUInt16(index, u);
}

public void SetBool16(long index, bool v) => SetBool16(ClampIdx(index), v);

// Set bool (32-bits)

public void SetBool32(ulong index, bool v) => SetUInt32(index, v ? 1u : 0u);

public void SetBool32(long index, bool v) => SetBool32(ClampIdx(index), v);

// Set bool (64-bits)

public void SetBool64(ulong index, bool v) => SetUInt64(index, v ? 1u : 0u);

public void SetBool64(long index, bool v) => SetBool64(ClampIdx(index), v);

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

// Set char (by Encoding)

public int SetChar(ulong index, char c, EncodingType encodeFlags)
{
var buffer = AsSpan(index, 4); // UTF-8 worst case

return BinaryHelper.WriteChar(c, buffer, encodeFlags);
}

public int SetChar(long index, char c, EncodingType encodeFlags)
{
return SetChar(ClampIdx(index), c, encodeFlags);
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

public int SetVarInt(ulong index, uint v)
{
ulong pos = index;

return BinaryHelper.EncodeVarInt(b => SetNextByte(b, ref pos), v);
}

public int SetVarInt(long index, uint v) => SetVarInt(ClampIdx(index), v);

// Set VarInt64

public int SetVarInt64(ulong index, ulong v)
{
ulong pos = index;

return BinaryHelper.EncodeVarInt64(b => SetNextByte(b, ref pos), v);
}

public int SetVarInt64(long index, ulong v) => SetVarInt64(ClampIdx(index), v);

// Set ZigZag int

public int SetZigZag(ulong index, int v)
{
uint zigZag = BinaryHelper.EncodeZigZag(v);

return SetVarInt(index, zigZag);
}

public int SetZigZag(long index, int v) => SetZigZag(ClampIdx(index), v);

// Set ZigZag long

public int SetZigZag64(ulong index, long v)
{
ulong zigZag = BinaryHelper.EncodeZigZag64(v);

return SetVarInt64(index, zigZag);
}

public int SetZigZag64(long index, long v) => SetZigZag64(ClampIdx(index), v);

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

// Set UNIX Time (32-bits)

public void SetUnixTime32(ulong index, DateTime dateTime)
{
var view = AsSpan(index, 4);

BinaryHelper.WriteUnixTime32(dateTime, view);
}

public void SetUnixTime32(long index, DateTime dateTime)
{
SetUnixTime32(ClampIdx(index), dateTime);
}

// Set UNIX Time (64-bits)

public void SetUnixTime64(ulong index, DateTime dateTime)
{
var view = AsSpan(index, 8);

BinaryHelper.WriteUnixTime64(dateTime, view);
}

public void SetUnixTime64(long index, DateTime dateTime)
{
SetUnixTime64(ClampIdx(index), dateTime);
}

// Set string

public ulong SetString(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
return 0;

using var rawBytes = BinaryHelper.GetNativeBytes(str, encoding);
ulong bytesWritten = rawBytes.Size;

CopyFrom(rawBytes, index);

return bytesWritten;
}

public ulong SetString(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
return SetString(ClampIdx(index), str, encoding);
}

// Set string prefixed by int8 length

public ulong SetStringByLen8(ulong index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8)
{
ulong rawLen = 0;

if(!str.IsEmpty)
rawLen = SetString(index + 1, str, encoding);

SetUInt8(index, (byte)rawLen);

return rawLen + 1;
}

public ulong SetStringByLen8(long index, ReadOnlySpan<char> str,
                             EncodingType encoding = EncodingType.UTF8)
{
return SetStringByLen8(ClampIdx(index), str, encoding);
}

// Set string prefixed by int16 length

public ulong SetStringByLen16(ulong index, ReadOnlySpan<char> str,
                              EncodingType encoding = EncodingType.UTF8,
                              Endianness endian = default)
{
ulong rawLen = 0;

if(!str.IsEmpty)
rawLen = SetString(index + 2, str, encoding);

SetUInt16(index, (ushort)rawLen, endian);

return rawLen + 2;
}

public ulong SetStringByLen16(long index, ReadOnlySpan<char> str,
                              EncodingType encoding = EncodingType.UTF8,
                              Endianness endian = default)
{
return SetStringByLen16(ClampIdx(index), str, encoding, endian);
}

// Set string prefixed by int32 length

public ulong SetStringByLen32(ulong index, ReadOnlySpan<char> str,
                              EncodingType encoding = EncodingType.UTF8,
                              Endianness endian = default)
{
ulong rawLen = 0;

if(!str.IsEmpty)
rawLen = SetString(index + 4, str, encoding);

SetUInt32(index, (uint)rawLen, endian);

return rawLen + 4;
}

public ulong SetStringByLen32(long index, ReadOnlySpan<char> str,
                              EncodingType encoding = EncodingType.UTF8,
                              Endianness endian = default)
{
return SetStringByLen32(ClampIdx(index), str, encoding, endian);
}

// Set string prefixed by int64 length

public ulong SetStringByLen64(ulong index, ReadOnlySpan<char> str,
                              EncodingType encoding = EncodingType.UTF8,
                              Endianness endian = default)
{
ulong rawLen = 0;

if(!str.IsEmpty)
rawLen = SetString(index + 8, str, encoding);

SetUInt64(index, rawLen, endian);

return rawLen + 8;
}

public ulong SetStringByLen64(long index, ReadOnlySpan<char> str,
                              EncodingType encoding = EncodingType.UTF8,
                              Endianness endian = default)
{
return SetStringByLen64(ClampIdx(index), str, encoding, endian);
}

// Set string prefixed by varint length

public ulong SetStringByVarLen(ulong index, ReadOnlySpan<char> str,
                               EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
return (ulong)SetVarInt(index, 0);

var rawLen = (uint)BinaryHelper.GetEncodedLength(str, encoding);
var varLen = (ulong)SetVarInt(index, rawLen);

ulong strIndex = index + varLen;
SetString(strIndex, str, encoding);

return (ulong)rawLen + varLen;
}

public ulong SetStringByVarLen(long index, ReadOnlySpan<char> str,
                               EncodingType encoding = EncodingType.UTF8)
{
return SetStringByVarLen(ClampIdx(index), str, encoding);
}

// Set string prefixed by varint64 length

public ulong SetStringByVarLen64(ulong index, ReadOnlySpan<char> str,
                                 EncodingType encoding = EncodingType.UTF8)
{

if(str.IsEmpty)
return (ulong)SetVarInt64(index, 0);

var rawLen = (ulong)BinaryHelper.GetEncodedLength(str, encoding);
var varLen = (ulong)SetVarInt64(index, rawLen);

ulong strIndex = index + varLen;
SetString(strIndex, str, encoding);

return (ulong)rawLen + varLen;
}

public ulong SetStringByVarLen64(long index, ReadOnlySpan<char> str,
                                 EncodingType encoding = EncodingType.UTF8)
{
return SetStringByVarLen64(ClampIdx(index), str, encoding);
}

// Set C-string

public ulong SetCString(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
ulong rawBytes = SetString(index, str, encoding);
SetUInt8(index + rawBytes, 0x00);

return rawBytes + 1;
}

public ulong SetCString(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
return SetCString(ClampIdx(index), str, encoding);
}

// Get line

public ulong SetLine(ulong index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
ulong rawBytes = SetString(index, str, encoding);
ulong separatorLen = SetString(index + rawBytes, Environment.NewLine, encoding);

return rawBytes + separatorLen;
}

public ulong SetLine(long index, ReadOnlySpan<char> str, EncodingType encoding = EncodingType.UTF8)
{
return SetLine(ClampIdx(index), str, encoding);
}

}