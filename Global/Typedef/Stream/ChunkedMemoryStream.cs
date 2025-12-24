using System;
using System.Collections.Generic;
using System.IO;

public class ChunkedMemoryStream : Stream
{
    private const int DefaultChunkSize = 8 * 1024 * 1024; // 8 MB
    private readonly List<byte[]> _chunks = new();
    private readonly int _chunkSize;

    private long _length;
    private long _position;
    private bool _disposed;

    public ChunkedMemoryStream(int chunkSize = DefaultChunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);
        _chunkSize = chunkSize;
    }

    public override bool CanRead => !_disposed;
    public override bool CanSeek => !_disposed;
    public override bool CanWrite => !_disposed;
    public override long Length
    {
        get
        {
            ThrowIfDisposed();
            return _length;
        }
    }

    public override long Position
    {
        get
        {
            ThrowIfDisposed();
            return _position;
        }
        set
        {
            ThrowIfDisposed();
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _position = value;
        }
    }

    private void EnsureCapacity(long value)
    {
        if (value < 0) throw new IOException("Stream too long");
        while (value > (long)_chunks.Count * _chunkSize)
        {
            _chunks.Add(new byte[_chunkSize]);
        }
        if (value > _length)
            _length = value;
    }

    public override void Flush()
    {
        ThrowIfDisposed();
        // No-op para memoria
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException();
        if (buffer.Length - offset < count) throw new ArgumentException();

        return Read(new Span<byte>(buffer, offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        ThrowIfDisposed();
        if (_position >= _length)
            return 0;

        int toRead = (int)Math.Min(buffer.Length, _length - _position);
        int totalRead = 0;

        while (toRead > 0)
        {
            int chunkIndex = (int)(_position / _chunkSize);
            int chunkOffset = (int)(_position % _chunkSize);

            int bytesInChunk = Math.Min(toRead, _chunkSize - chunkOffset);

            var source = new ReadOnlySpan<byte>(_chunks[chunkIndex], chunkOffset, bytesInChunk);
            source.CopyTo(buffer.Slice(totalRead));

            totalRead += bytesInChunk;
            _position += bytesInChunk;
            toRead -= bytesInChunk;
        }

        return totalRead;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException();
        if (buffer.Length - offset < count) throw new ArgumentException();

        Write(new ReadOnlySpan<byte>(buffer, offset, count));
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ThrowIfDisposed();
        long newPos = _position + buffer.Length;
        EnsureCapacity(newPos);

        int bytesWritten = 0;
        int toWrite = buffer.Length;

        while (toWrite > 0)
        {
            int chunkIndex = (int)(_position / _chunkSize);
            int chunkOffset = (int)(_position % _chunkSize);

            int bytesInChunk = Math.Min(toWrite, _chunkSize - chunkOffset);

            var dest = new Span<byte>(_chunks[chunkIndex], chunkOffset, bytesInChunk);
            buffer.Slice(bytesWritten, bytesInChunk).CopyTo(dest);

            bytesWritten += bytesInChunk;
            _position += bytesInChunk;
            toWrite -= bytesInChunk;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        ThrowIfDisposed();
        long refPos = origin switch
        {
            SeekOrigin.Begin => 0,
            SeekOrigin.Current => _position,
            SeekOrigin.End => _length,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        long newPos = refPos + offset;
        if (newPos < 0) throw new IOException("Seek before begin");

        _position = newPos;
        return _position;
    }

    public override void SetLength(long value)
    {
        ThrowIfDisposed();
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        EnsureCapacity(value);
        if (value < _length)
        {
            // Opcional: liberar buffers extra
            // Por simplicidad no implementado
        }
        _length = value;
        if (_position > value)
            _position = value;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _chunks.Clear();
            _length = 0;
            _position = 0;
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    private void ThrowIfDisposed()
    {
			ObjectDisposedException.ThrowIf(_disposed, nameof(ChunkedMemoryStream) );
    }
}