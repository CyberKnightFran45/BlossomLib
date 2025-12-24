using System;
using System.IO;

public sealed class XorStream : Stream
{
private readonly Stream BaseStream;

private readonly byte _key;
private readonly bool _leaveOpen;

public XorStream(byte key)
{
BaseStream = new MemoryStream();

_key = key;
_leaveOpen = false;
}

public XorStream(Stream source, byte key, bool leaveOpen = false)
{
BaseStream = source ?? new MemoryStream();

_key = key;
_leaveOpen = leaveOpen;
}

public static XorStream OpenRead(string path, byte key)
{
return new(FileManager.OpenRead(path), key);
}

public static XorStream OpenWrite(string path, byte key)
{
return new(FileManager.OpenWrite(path), key);
}

public override bool CanRead => BaseStream.CanRead;

public override bool CanSeek => BaseStream.CanSeek;

public override bool CanWrite => BaseStream.CanWrite;

public override long Length => BaseStream.Length;

public override long Position
{
get => BaseStream.Position;
set => BaseStream.Position = value;
}

public override void Flush() => BaseStream.Flush();

public override int Read(Span<byte> buffer)
{
int bytesRead = BaseStream.Read(buffer);
Xor(buffer[..bytesRead] );

return bytesRead;
}

public override int Read(byte[] buffer, int offset, int count)
{
return Read(buffer.AsSpan(offset, count) );
}

public override int ReadByte()
{
int b = BaseStream.ReadByte();

if(b == -1)
return -1;

return b ^ _key;
}

public new void ReadExactly(Span<byte> buffer)
{
int totalRead = 0;

while(totalRead < buffer.Length)
{
int read = Read(buffer[totalRead..] );

if(read == 0)
throw new EndOfStreamException();

totalRead += read;
}

}

public override void Write(ReadOnlySpan<byte> buffer)
{
int bufferSize = buffer.Length;

if(bufferSize <= SizeT.MAX_STACK)
{
Span<byte> xkSpan = stackalloc byte[bufferSize];
buffer.CopyTo(xkSpan);

Xor(xkSpan);
BaseStream.Write(xkSpan);

return;
}

using NativeMemoryOwner<byte> xOwner = new(bufferSize);
var xSpan = xOwner.AsSpan();

buffer.CopyTo(xSpan);
Xor(xSpan);

BaseStream.Write(xSpan);
}

public override void Write(byte[] buffer, int offset, int count)
{
Write(buffer.AsSpan(offset, count) );
}

public override void WriteByte(byte val)
{
byte xor8 = (byte)(val ^ _key);

BaseStream.WriteByte(xor8);
}

public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

public override void SetLength(long length) => BaseStream.SetLength(length);

protected override void Dispose(bool disposing)
{

if(disposing)
{

if(_leaveOpen)
BaseStream.Flush();

else
BaseStream.Dispose();
}

base.Dispose(disposing);
}

private void Xor(Span<byte> v)
{
for (int i = 0; i < v.Length; i++)
v[i] ^= _key;
}

}