using System;
using System.IO;

/// <summary> Read-only Stream adapter over a pointer. </summary>

public unsafe sealed class ReadOnlyUnmanagedStream : Stream
{
// pointer

private readonly byte* _ptr;

// length

private readonly long _length;

// position

private long _position;

// ptr

public ReadOnlyUnmanagedStream(void* ptr, long length)
{
_ptr = (byte*)ptr;
_length = length;
}

// Properties

public override bool CanRead => true;

public override bool CanSeek => true;

public override bool CanWrite => false;

public override long Length => _length;

public override long Position
{

get => _position;
set => Seek(value, SeekOrigin.Begin);

}

// Read bytes

public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count) );

// Read to span

public override int Read(Span<byte> buffer)
{
long remaining = _length - _position;

if(remaining <= 0)
return 0;

var count = (int)Math.Min((long)buffer.Length, remaining);

ReadOnlySpan<byte> view = new(_ptr + (nint)_position, count);
view.CopyTo(buffer[.. count] );

_position += count;

return count;
}

// Read byte

public override int ReadByte()
{

if(_position >= _length)
return -1;

return _ptr[ (nint)_position];
}

// Seek

public override long Seek(long offset, SeekOrigin origin)
{

long next = origin switch
{
SeekOrigin.Begin => offset,
SeekOrigin.Current => _position + offset,
SeekOrigin.End => _length + offset,
_ => throw new ArgumentOutOfRangeException(nameof(origin))
};

if(next < 0 || next > _length)
throw new IOException("Attempted to seek outside the span.");

_position = next;

return _position;
}

public override void Flush()
{ 
// no-op
}

// Set length

public override void SetLength(long value) => throw new NotSupportedException();

// Write bytes

public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

// Write span

public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();
}