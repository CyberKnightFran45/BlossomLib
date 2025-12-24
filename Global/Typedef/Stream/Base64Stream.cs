using System;
using System.IO;
using BlossomLib.Modules.Parsers;

public sealed class Base64Stream : Stream
{
private readonly Stream _baseStream;
private readonly bool _webSafe;

private readonly NativeMemoryOwner<byte> _rawBuffer;
private ulong _rawBufferLen;

private readonly NativeMemoryOwner<byte> _encodedBuffer;

private ulong _position;
private readonly ulong _length;

private bool _disposed = false;

public Base64Stream(Stream baseStream, bool webSafe, ulong bufferSize = 8192)
{
_baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
_webSafe = webSafe;

_rawBuffer = new(bufferSize);
_rawBufferLen = 0;

_encodedBuffer = new(Base64.GetEncodedLengthUtf8((int)bufferSize));

_position = 0;

if(baseStream.CanSeek)
_length = (ulong)baseStream.Length;
}

public override bool CanRead => _baseStream.CanRead;
public override bool CanWrite => _baseStream.CanRead;
public override bool CanSeek => _baseStream.CanSeek;

public override long Length
{
get
{
if (_length > long.MaxValue)
throw new IOException("Length too large to represent as long.");

return (long)_length;
}
}

public override long Position
{
get
{

if(_position > long.MaxValue)
throw new IOException("Position too large to represent as long.");

return (long)_position;
}

set
{

if(!_baseStream.CanSeek)
throw new NotSupportedException();

ArgumentOutOfRangeException.ThrowIfNegative(value);

Position64 = (ulong)value;
}

}

public ulong Position64
{
get => _position;

set
{

if(!_baseStream.CanSeek)
throw new NotSupportedException();

_position = value;

_rawBufferLen = 0;
_baseStream.Position = 0; // Reset base stream position for simplicity
}

}

public override void Flush() => FlushFinalBlock();


private void FlushRawBuffer()
{

if(_rawBufferLen == 0)
return;

using var encoded = Base64.EncodeBytesUtf8(_rawBuffer.AsSpan(0, (int)_rawBufferLen), _webSafe);

_baseStream.Write(encoded.AsSpan() );
_baseStream.Flush();

_rawBufferLen = 0;
}

public void FlushFinalBlock()
{
FlushRawBuffer();

_baseStream.Flush();
}

public override void Write(byte[] buffer, int offset, int count)
{
Write(buffer.AsSpan(offset, count) );
}

public override void Write(ReadOnlySpan<byte> buffer)
{

if(!CanWrite)
throw new NotSupportedException();

int totalWritten = 0;

while(!buffer.IsEmpty)
{
var spaceLeft = (int)(_rawBuffer.Size - _rawBufferLen);
int toCopy = Math.Min(buffer.Length, spaceLeft);

_rawBuffer.CopyFrom(buffer, _rawBufferLen, toCopy);
_rawBufferLen += (ulong)toCopy;

buffer = buffer[toCopy ..];

if(_rawBufferLen == _rawBuffer.Size)
FlushRawBuffer();

totalWritten += toCopy;
}

_position += (ulong)totalWritten;
}

public override int Read(byte[] buffer, int offset, int count)
{
return Read(buffer.AsSpan(offset, count) );
}

public override int Read(Span<byte> buffer)
{

if(!CanRead)
throw new NotSupportedException();

if(_rawBufferLen == 0)
{
int readEncoded = _baseStream.Read(_encodedBuffer.AsSpan() );

if(readEncoded == 0)
return 0;

using var decoded = Base64.DecodeUtf8Bytes(_encodedBuffer.AsSpan(0, readEncoded), _webSafe);
var decodedLen = (int)decoded.Size;

int toCopy = Math.Min(decodedLen, buffer.Length);
decoded.CopyTo(buffer, 0, toCopy);

int leftover = decodedLen - toCopy;

if(leftover > 0)
{
_rawBufferLen = (ulong)leftover;

decoded.CopyTo(_rawBuffer, (ulong)toCopy);
}

else
_rawBufferLen = 0;

_position += (ulong)toCopy;

return toCopy;
}

else
{
var toCopy = Math.Min(_rawBufferLen, (ulong)buffer.Length);
_rawBuffer.CopyTo(buffer, 0, (int)toCopy);

ulong remaining = _rawBufferLen - toCopy;

if(remaining > 0)
_rawBuffer.Move(toCopy, 0, remaining);

_rawBufferLen = remaining;
_position += toCopy;

return (int)toCopy;
}

}

public override long Seek(long offset, SeekOrigin origin)
{

if(!CanSeek)
throw new NotSupportedException();

ulong newPos = origin switch
{
SeekOrigin.Begin => (ulong)offset,
SeekOrigin.Current => _position + (ulong)offset,
SeekOrigin.End => _length + (ulong)offset,
_ => throw new ArgumentOutOfRangeException(nameof(origin) ),
};

Position64 = newPos;

return(long)_position;
}

public override void SetLength(long value) => throw new NotSupportedException();

	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				FlushFinalBlock();

				_rawBuffer.Dispose();
				_encodedBuffer.Dispose();
				_baseStream.Dispose();
			}
			_disposed = true;
		}

		base.Dispose(disposing);
	}

public ReadOnlySpan<byte> GetView(ulong offset = 0, int size = -1)
{
return _rawBuffer.AsSpan(offset, size);
}

// Get Base64 Data

public ReadOnlySpan<byte> GetView64(ulong offset = 0, int size = -1)
{
return _encodedBuffer.AsSpan(offset, size);
}

}