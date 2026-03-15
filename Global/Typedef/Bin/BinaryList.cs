using System;
using System.Collections.Generic;
using System.IO;

// Represent a List of binaries

public static class BinaryList
{
// Read List size

private static int ReadListSize(Stream reader, RepeatCountFlags bitsLength)
{

return bitsLength switch
{
RepeatCountFlags.UInt8 => reader.ReadByte(), 
RepeatCountFlags.UInt16 => reader.ReadInt16(),
RepeatCountFlags.VarInt32 => reader.ReadVarInt(),
RepeatCountFlags.UInt64 => (int)reader.ReadInt64(),
RepeatCountFlags.VarInt64 => (int)reader.ReadVarInt64(),
_ => reader.ReadInt32()
};

}

// Read string by bits length

private static string ReadStr(Stream reader, StrLenType bitsLength)
{

NativeString owner = bitsLength switch
{
StrLenType.UInt8 => reader.ReadStringByLen8(), 
StrLenType.UInt16 => reader.ReadStringByLen16(),
StrLenType.UInt32 => reader.ReadStringByLen32(),
StrLenType.VarInt32 => reader.ReadStringByVarLen(),
StrLenType.UInt64 => reader.ReadStringByLen64(),
StrLenType.VarInt64 => reader.ReadStringByVarLen64(),
_ => reader.ReadCString()
};

string rawStr = owner.ToString();
owner.Dispose();

return rawStr;
}

// Read strings

public static List<string> ReadStrings(Stream reader, StrLenType strType, int count)
{

if(count < 0)
return null;

List<string> strList = new(count);

for(int i = 0; i < count; i++)
{
string str = ReadStr(reader, strType);

strList.Add(str);
}

return strList;
}

// Read Strings list

public static List<string> ReadStrings(Stream reader, RepeatCountFlags lstFlags, StrLenType strType)
{
int count = ReadListSize(reader, lstFlags);

return ReadStrings(reader, strType, count);
}

// Read objects

public static List<T> ReadObjects<T>(Stream reader, IBinarySerializer<T> serializer, int count)
                                     where T : class
{

if(count < 0)
return null;

List<T> list = new(count);

for(int i = 0; i < count; i++)
{
var obj = serializer.ReadBin(reader);

list.Add(obj);
}

return list;
}

// Read list of objects

public static List<T> ReadObjects<T>(Stream reader, IBinarySerializer<T> serializer) where T : class
{
int count = reader.ReadInt32();

return ReadObjects(reader, serializer, count);
}

// Write List size

private static void WriteListSize(Stream writer, int listSize, RepeatCountFlags bitsLength)
{

switch(bitsLength)
{
case RepeatCountFlags.UInt8:
writer.WriteByte( (byte)listSize); 
break;

case RepeatCountFlags.UInt16:
writer.WriteUInt16( (ushort)listSize); 
break;

case RepeatCountFlags.VarInt32:
writer.WriteVarInt(listSize); 
break;

case RepeatCountFlags.UInt64:
writer.WriteInt64(listSize); 
break;

case RepeatCountFlags.VarInt64:
writer.WriteVarInt64(listSize); 
break;

default:
writer.WriteInt32(listSize); 
break;
}

}

// Write string to Stream by type

private static void WriteStr(Stream writer, ReadOnlySpan<char> str, StrLenType bitsLength)
{

switch(bitsLength)
{
case StrLenType.UInt8:
writer.WriteStringByLen8(str); 
break;

case StrLenType.UInt16:
writer.WriteStringByLen16(str); 
break;

case StrLenType.UInt32:
writer.WriteStringByLen32(str); 
break;

case StrLenType.VarInt32:
writer.WriteStringByVarLen(str); 
break;

case StrLenType.UInt64:
writer.WriteStringByLen64(str); 
break;

case StrLenType.VarInt64:
writer.WriteStringByVarLen64(str); 
break;

default:
writer.WriteCString(str);
break;
}

}

// Write strings

public static void WriteStrings(Stream writer, List<string> list, StrLenType strType, int count)
{

for(int i = 0; i < count; i++)
WriteStr(writer, list[i], strType);

}

// Write Strings list

public static void WriteStrings(Stream writer, List<string> list, RepeatCountFlags lstFlags,
                                StrLenType strType)
{
int count = list?.Count ?? -1;

WriteListSize(writer, count, lstFlags);
WriteStrings(writer, list, strType, count);
}

// Write list of objects

public static void WriteObjects<T>(Stream writer, List<T> list, IBinarySerializer<T> serializer,
                                   bool includeLength = true)
                                   where T : class
{
int count = list?.Count ?? -1;

if(count < 0)
return;

if(includeLength)
writer.WriteInt32(count);

for(int i = 0; i < count; i++)
serializer.WriteBin(writer, list[i] );

}

}