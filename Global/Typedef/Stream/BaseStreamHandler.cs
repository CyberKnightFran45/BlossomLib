using System;
using System.IO;

/// <summary> Base class for handling common stream operations. </summary>

public class BaseStreamHandler : IDisposable
{
protected Stream BaseStream;

private readonly bool LeaveOpen;

public long Length => BaseStream.Length;

public long Position => BaseStream.Position;

public bool IsClosed => BaseStream == null || !BaseStream.CanRead && !BaseStream.CanWrite;

public bool IsMemoryStream => BaseStream is MemoryStream;

protected BaseStreamHandler(bool leaveOpen = false)
{
BaseStream = new ChunkedMemoryStream();
LeaveOpen = leaveOpen;
}

protected BaseStreamHandler(Stream source, bool leaveOpen = false)
{
BaseStream = source ?? new ChunkedMemoryStream();
LeaveOpen = leaveOpen;
}

/// <summary> Closes the stream and releases all the Resources consumed by it. </summary>

public void Close() => Dispose(true);

/// <summary> Releases all the Resources consumed by the Stream. </summary>

public void Dispose()
{
Dispose(true);
GC.SuppressFinalize(this);
}

/// <summary> Releases all the Resources consumed by the Stream. </summary>

/// <param name="disposing">Determines if all the Resources should be Discarded.</param>

protected void Dispose(bool disposing)
{

if(disposing && !IsClosed)
{

if(LeaveOpen)
BaseStream.Flush();

else
BaseStream.Close();

}

}

/// <summary> Clears all buffers for this stream and causes any buffered data to be written 
/// to the underlying device. </summary>

public void Flush() => BaseStream.Flush();

/// <summary> Changes the current position of the stream. </summary>

public long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

/// <summary> Changes the Length of the stream. </summary>

public void SetLength(long length) => BaseStream.SetLength(length);

public static implicit operator Stream(BaseStreamHandler a) => a.BaseStream;
}