using System.IO;

/// <summary> Allows Reading and Writing Bits as Streams. </summary>

public sealed class BitStream : BaseStreamHandler
{
private int _bitsPosition;
private int _bitBuffer;

public BitStream(bool leaveOpen = false) : base(leaveOpen)
{
}

public BitStream(Stream source, bool leaveOpen = false) : base(source, leaveOpen)
{
}

public static BitStream OpenRead(string path) => new(FileManager.OpenRead(path), true);

public static BitStream OpenWrite(string path) => new(FileManager.OpenWrite(path), true);

public int ReadOneBit()
{
int buffer = 0;

if(_bitsPosition == 0)
{
buffer = BaseStream.ReadByte();

if(buffer == -1)
return buffer;

}

_bitsPosition = (_bitsPosition + 7) % 8;

return (buffer >> _bitsPosition) & 0b1;
}

public int ReadBits(int count)
{
int bits = 0;

for(int i = count - 1; i >= 0; i--)
{
int s = ReadOneBit();

if(s < 0)
break;

bits |= s << i;
}
return bits;
}

public void WriteOneBit(int bit)
{
_bitBuffer = (_bitBuffer << 1) | (bit & 1);
_bitsPosition++;

if(_bitsPosition == 8)
{
BaseStream.WriteByte( (byte)_bitBuffer);

_bitBuffer = 0;
_bitsPosition = 0;
}

}

public void WriteBits(int val, int count)
{
	
for (int i = count - 1; i >= 0; i--)
{
int bit = (val >> i) & 1;
WriteOneBit(bit);
}

}

}