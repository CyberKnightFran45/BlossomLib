using System;

// Native decoder

public sealed partial class NativeBuffer : NativeMemoryOwner<byte>
{
// Get boolean

public bool GetBool(ulong index) => GetUInt8(index) != 0;

public bool GetBool(long index) => GetBool(ClampIdx(index) );

// Get char (8-bits)

public char GetChar8(ulong index) => (char)GetUInt8(index);

public char GetChar8(long index) => GetChar8(ClampIdx(index) );

// Get char (16-bits)

public char GetChar16(ulong index, Endianness endian = default)
{
var view = GetView(index, 2);

return BinaryHelper.ReadChar16(view, endian);
}

public char GetChar16(long index, Endianness endian = default) => GetChar16(ClampIdx(index), endian);

// Get int8

public sbyte GetInt8(ulong index) => (sbyte)GetUInt8(index);

public sbyte GetInt8(long index) => GetInt8(ClampIdx(index) );

// Get uint8

public byte GetUInt8(ulong index) => ElementAt(index);

public byte GetUInt8(long index) => ElementAt(index);

// Get int16

public short GetInt16(ulong index, Endianness endian = default)
{
var view = GetView(index, 2);

return BinaryHelper.ReadInt16(view, endian);
}

public short GetInt16(long index, Endianness endian = default) => GetInt16(ClampIdx(index), endian);

// Get uint16

public ushort GetUInt16(ulong index, Endianness endian = default)
{
var view = GetView(index, 2);

return BinaryHelper.ReadUInt16(view, endian);
}

public ushort GetUInt16(long index, Endianness endian = default) => GetUInt16(ClampIdx(index), endian);

// Get int24

public int GetInt24(ulong index, Endianness endian = default)
{
var view = GetView(index, 3);

return BinaryHelper.ReadInt24(view, endian);
}

public int GetInt24(long index, Endianness endian = default) => GetInt24(ClampIdx(index), endian);

// Get uint24

public uint GetUInt24(ulong index, Endianness endian = default)
{
var view = GetView(index, 3);

return BinaryHelper.ReadUInt24(view, endian);
}

public uint GetUInt24(long index, Endianness endian = default) => GetUInt24(ClampIdx(index), endian);

// Get int32

public int GetInt32(ulong index, Endianness endian = default)
{
var view = GetView(index, 4);

return BinaryHelper.ReadInt32(view, endian);
}

public int GetInt32(long index, Endianness endian = default) => GetInt32(ClampIdx(index), endian);

// Get uint32

public uint GetUInt32(ulong index, Endianness endian = default)
{
var view = GetView(index, 4);

return BinaryHelper.ReadUInt32(view, endian);
}

public uint GetUInt32(long index, Endianness endian = default) => GetUInt32(ClampIdx(index), endian);

// Get int64

public long GetInt64(ulong index, Endianness endian = default)
{
var view = GetView(index, 8);

return BinaryHelper.ReadInt64(view, endian);
}

public long GetInt64(long index, Endianness endian = default) => GetInt64(ClampIdx(index), endian);

// Get uint64

public ulong GetUInt64(ulong index, Endianness endian = default)
{
var view = GetView(index, 8);

return BinaryHelper.ReadUInt64(view, endian);
}

public ulong GetUInt64(long index, Endianness endian = default) => GetUInt64(ClampIdx(index), endian);

// Get int128

public Int128 GetInt128(ulong index, Endianness endian = default)
{
var view = GetView(index, 16);

return BinaryHelper.ReadInt128(view, endian);
}

public Int128 GetInt128(long index, Endianness endian = default) => GetInt128(ClampIdx(index), endian);

// Get uint128

public UInt128 GetUInt128(ulong index, Endianness endian = default)
{
var view = GetView(index, 16);

return BinaryHelper.ReadUInt128(view, endian);
}

public UInt128 GetUInt128(long index, Endianness endian = default) => GetUInt128(ClampIdx(index), endian);

// Advance one byte

private byte GetNextByte(ref ulong pos)
{
var b = GetUInt8(pos);
pos++;

return b;
}

// Get varint

public int GetVarInt(ulong index, out int bytesRead)
{
ulong pos = index;

return BinaryHelper.DecodeVarInt( () => GetNextByte(ref pos), out bytesRead);
}

public int GetVarInt(long index, out int bytesRead) => GetVarInt(ClampIdx(index), out bytesRead);

// Get unsigned varint

public uint GetVarUInt(ulong index, out int bytesRead) => (uint)GetVarInt(index, out bytesRead);

public uint GetVarUInt(long index, out int bytesRead) => (uint)GetVarInt(index, out bytesRead);

// Read varint64

public long GetVarInt64(ulong index, out int bytesRead)
{
ulong pos = index;

return BinaryHelper.DecodeVarInt64( () => GetNextByte(ref pos), out bytesRead);
}

public long GetVarInt64(long index, out int bytesRead) => GetVarInt64(ClampIdx(index), out bytesRead);

// Get unsigned varint64

public ulong GetVarUInt64(ulong index, out int bytesRead) => (ulong)GetVarInt64(index, out bytesRead);

public ulong GetVarUInt64(long index, out int bytesRead) => (ulong)GetVarInt64(index, out bytesRead);

// Get ZigZag int

public int GetZigZag(ulong index)
{
uint v = GetVarUInt(index, out _);

return BinaryHelper.DecodeZigZag(v);
}

public int GetZigZag(long index) => GetZigZag(ClampIdx(index) );

// Get ZigZag long

public long GetZigZag64(ulong index)
{
ulong v = GetVarUInt64(index, out _);

return BinaryHelper.DecodeZigZag64(v);
}

public long GetZigZag64(long index) => GetZigZag64(ClampIdx(index) );

// Get half

public Half GetHalf(ulong index, Endianness endian = default)
{
var view = GetView(index, 2);

return BinaryHelper.ReadHalf(view, endian);
}

public Half GetHalf(long index, Endianness endian = default) => GetHalf(ClampIdx(index), endian);

// Get float

public float GetFloat(ulong index, Endianness endian = default)
{
var view = GetView(index, 4);

return BinaryHelper.ReadFloat(view, endian);
}

public float GetFloat(long index, Endianness endian = default) => GetFloat(ClampIdx(index), endian);

// Get double

public double GetDouble(ulong index, Endianness endian = default)
{
var view = GetView(index, 8);

return BinaryHelper.ReadDouble(view, endian);
}

public double GetDouble(long index, Endianness endian = default) => GetDouble(ClampIdx(index), endian);

// Get string

public NativeString GetString(ulong index, EncodingType encoding = EncodingType.UTF8)
{
return GetString(index, -1, encoding);
}

public NativeString GetString(long index, EncodingType encoding = EncodingType.UTF8)
{
return GetString(index, -1, encoding);
}

// Get string with n chars

public NativeString GetString(ulong index, int strLen, EncodingType encoding = EncodingType.UTF8)
{
var view = GetView(index, strLen);

return BinaryHelper.GetNativeString(view, encoding);
}

public NativeString GetString(long index, int strLen, EncodingType encoding = EncodingType.UTF8)
{
return GetString(ClampIdx(index), strLen, encoding);
}

// Get string prefixed by int8 length

public NativeString GetStringByLen8(ulong index, EncodingType encoding = EncodingType.UTF8)
{
byte strLen = GetUInt8(index);

return GetString(index + 1, strLen, encoding);
}

public NativeString GetStringByLen8(long index, EncodingType encoding = EncodingType.UTF8)
{
return GetStringByLen8(ClampIdx(index), encoding);
}

// Get string prefixed by int16 length

public NativeString GetStringByLen16(ulong index, EncodingType encoding = EncodingType.UTF8,
                                     Endianness endian = default)
{
ushort strLen = GetUInt16(index, endian);

return GetString(index + 2, strLen, encoding);
}

public NativeString GetStringByLen16(long index, EncodingType encoding = EncodingType.UTF8,
                                     Endianness endian = default)
{
return GetStringByLen16(ClampIdx(index), encoding, endian);
}

// Get string prefixed by int32 length

public NativeString GetStringByLen32(ulong index, EncodingType encoding = EncodingType.UTF8,
                                     Endianness endian = default)
{
int strLen = GetInt32(index, endian);

return GetString(index + 4, strLen, encoding);
}

public NativeString GetStringByLen32(long index, EncodingType encoding = EncodingType.UTF8,
                                     Endianness endian = default)
{
return GetStringByLen32(ClampIdx(index), encoding, endian);
}

// Get string prefixed by int64 length

public NativeString GetStringByLen64(ulong index, EncodingType encoding = EncodingType.UTF8,
                                     Endianness endian = default)
{
long rawLen = GetInt64(index, endian);
var strLen = (int)Math.Min(rawLen, int.MaxValue);

return GetString(index + 8, strLen, encoding);
}

public NativeString GetStringByLen64(long index, EncodingType encoding = EncodingType.UTF8,
                                     Endianness endian = default)
{
return GetStringByLen64(ClampIdx(index), encoding, endian);
}

// Get string prefixed by varint length

public NativeString GetStringByVarLen(ulong index, EncodingType encoding = EncodingType.UTF8)
{
int strLen = GetVarInt(index, out int bytesRead);
ulong strIndex = index + (ulong)bytesRead;

return GetString(strIndex, strLen, encoding);
}

public NativeString GetStringByVarLen(long index, EncodingType encoding = EncodingType.UTF8)
{
return GetStringByVarLen(ClampIdx(index), encoding);
}

// Get string prefixed by varint64 length

public NativeString GetStringByVarLen64(ulong index, EncodingType encoding = EncodingType.UTF8)
{
long rawLen = GetVarInt64(index, out int bytesRead);
var strLen = (int)Math.Min(rawLen, int.MaxValue);

ulong strIndex = index + (ulong)bytesRead;

return GetString(strIndex, strLen, encoding);
}

public NativeString GetStringByVarLen64(long index, EncodingType encoding = EncodingType.UTF8)
{
return GetStringByVarLen64(ClampIdx(index), encoding);
}

// Get C-string

public NativeString GetCString(ulong index, EncodingType encoding = EncodingType.UTF8)
{
ulong finalPos = index;

while(true)
{
byte b = GetUInt8(finalPos);

if(b == 0x00)
break;

finalPos++;
}

ulong rawLen = finalPos - index;
var strLen = (int)Math.Min(rawLen, int.MaxValue);

return GetString(index, strLen, encoding);
}

public NativeString GetCString(long index, EncodingType encoding = EncodingType.UTF8)
{
return GetCString(ClampIdx(index), encoding);
}

// Get line

public NativeString GetLine(ulong index, EncodingType encoding = EncodingType.UTF8)
{
ulong pos = index;
int strLen = 0;

while(true)
{
byte b = GetUInt8(pos);

if(b == 0x0D) // '\r'
{
byte peek = GetUInt8(pos + 1);

if(peek == 0x0A) // Handle '\r\n'
pos += 2;
            
else
pos++;

break;
}

else if (b == 0x0A) // '\n'
{
pos++;

break;
}

pos++;
strLen++;
}

return GetString(index, strLen, encoding);
}

public NativeString GetLine(long index, EncodingType encoding = EncodingType.UTF8)
{
return GetLine(ClampIdx(index), encoding);
}

}