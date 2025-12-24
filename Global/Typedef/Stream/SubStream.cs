using System;
using System.IO;

// Represents a View inside a Stream

public sealed class SubStream : Stream
{
private readonly Stream _baseStream;
private readonly long _start;

private readonly long _length;
private long _position;

// ctor

public SubStream(Stream baseStream, long start, long length)
{
_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));

_start = start;
_length = length;

_baseStream.Seek(start, SeekOrigin.Begin);
_position = 0;
}

public override long Length => _length;

public override long Position
{

get => _position;
set => Seek(value, SeekOrigin.Begin);

}

public override bool CanRead => _baseStream.CanRead;
public override bool CanSeek => _baseStream.CanSeek;
public override bool CanWrite => _baseStream.CanWrite;
public override bool CanTimeout => _baseStream.CanTimeout;

public override void Flush() => _baseStream.Flush();

// Seek

public override long Seek(long offset, SeekOrigin origin)
{

long newPos = origin switch
{
SeekOrigin.Current => _position + offset,
SeekOrigin.End => _length + offset,
_ => offset
};

if(newPos < 0 || newPos > _length)
throw new IOException("Attempt to seek outside SubStream boundaries.");

_position = newPos;
_baseStream.Seek(_start + _position, SeekOrigin.Begin);

return _position;
}

public override void SetLength(long value) => throw new NotSupportedException("SubStream cannot change its length.");

// Read bytes

public override int Read(Span<byte> buffer)
{
long remaining = _length - _position;

if(remaining <= 0)
return 0;

int toRead = (int)Math.Min(buffer.Length, remaining);
int read = _baseStream.Read(buffer[..toRead]);

_position += read;

return read;
}

public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count) );

/// Write bytes


public override void Write(ReadOnlySpan<byte> buffer)
{
long remaining = _length - _position;

if(buffer.Length > remaining)
throw new IOException("Attempt to write outside SubStream boundaries.");

_baseStream.Write(buffer);
_position += buffer.Length;
}

public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count) );
}