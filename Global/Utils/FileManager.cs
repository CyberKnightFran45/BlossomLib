using System;
using System.IO;

/// <summary> Initializes Extra Functions for Files. </summary> 

public static class FileManager
{
/** <summary> Gets the Size in Bytes of an Archive. </summary>

<param name = "targetPath"> The Path to the File where the Properties will be Obtained from. </param>

<returns> The File Size. </returns> */

public static long GetFileSize(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;

/** <summary> Checks if a File is Empty or not by Analizing its Content. </summary>

<param name = "path"> The Path where the Archive to be Checked is Located. </param>

<returns> <b>true</b> if the File is Empty; otherwise, returns <b>false</b>. </returns> */

public static bool FileIsEmpty(string path) => GetFileSize(path) == 0;


/** <summary> Opens a File Stream for both, Reading and Writing. </summary>

<param name="filePath"> The Path to the File to be Opened. </param>

<returns> A FileStream with Full Access. </returns> */

public static FileStream Open(string filePath)
{
PathHelper.EnsurePathExists(filePath, true);

int bufferSize = MemoryManager.GetBufferSize(filePath);

return new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, bufferSize, false);
}

/** <summary> Opens a File Stream for Reading. </summary>

<param name="filePath"> The Path to the File to be Opened. </param>

<returns> A FileStream that can be used to Read from the File. </returns> */

public static FileStream OpenRead(string filePath)
{
int bufferSize = MemoryManager.GetBufferSize(filePath);

return new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, false);
}

/** <summary> Opens a File Stream for Writing. If the File does not exist, it will be created. </summary>

<param name="filePath"> The Path to the File to be Opened. </param>

<returns> A FileStream that can be used to Write to the File. </returns> */

public static FileStream OpenWrite(string filePath)
{
PathHelper.EnsurePathExists(filePath, true);

int bufferSize = MemoryManager.GetBufferSize(filePath);

return new(filePath, FileMode.Open, FileAccess.Write, FileShare.None, bufferSize, false);
}

/** <summary> Process the Data Contained on a Stream and Writes it to another Stream. </summary>

<param name="input"> The Stream which Contains the Data to Process. </param>
<param name="output"> The Stream where the Processed Data will be Written. </param>
<param name="maxBytes"> The maximum number of bytes to be processed (-1 for unlimited). </param>
<param name="progressCallback"> Optional callback for reporting progress </param> */

public static void Process(Stream input, Stream output, long maxBytes = -1,
                           Action<long, long> progressCallback = null)
{
long totalRead = 0;
int blockSize = MemoryManager.GetBufferSize(input);

using NativeMemoryOwner<byte> mOwner = new(blockSize);
Span<byte> buffer = mOwner.AsSpan();

while(true)
{
int toRead = blockSize;

if(maxBytes >= 0)
{
long remaining = maxBytes - totalRead;

if(remaining <= 0)
break;

toRead = (int)Math.Min(blockSize, remaining);
}

int bytesRead = input.Read(buffer[..toRead]);

if(bytesRead <= 0)
break;

output.Write(buffer[..bytesRead]);
totalRead += bytesRead;

progressCallback?.Invoke(totalRead, maxBytes < 0 ? totalRead : maxBytes);
}

output.Flush();
}

/// <summary> A delegate that defines a transformation function for processing byte data. </summary>

public delegate NativeMemoryOwner<byte> BytesTransform(ReadOnlySpan<byte> input);

/// <summary> A delegate that defines a transformation function for processing byte data
/// with an extra argument. </summary>

public delegate NativeMemoryOwner<byte> BytesTransform2(ReadOnlySpan<byte> input,
ReadOnlySpan<byte> arg1);

/** <summary> Processes the data contained in an input stream, applies a transformation function, 
and writes the transformed data to an output stream. </summary>

<param name="input"> The stream which contains the data to be processed. </param>
<param name="output"> The stream where the processed data will be written. </param>
<param name="transform"> A function that determines how the buffer should be transformed. </param>
<param name="maxBytes"> The maximum number of bytes to process (-1 for no limit). </param> 
<param name="progressCallback"> An optional callback to report progress during processing. </param> */

public static void Process(Stream input, Stream output, BytesTransform transform, long maxBytes = -1,
                           Action<long, long> progressCallback = null)
{
int blockSize = MemoryManager.GetBlockSize(input);

using NativeMemoryOwner<byte> inputOwner = new(blockSize);
Span<byte> inBuffer = inputOwner.AsSpan();

long totalRead = 0;

while(true)
{
int toRead = blockSize;

if(maxBytes >= 0)
{
long remaining = maxBytes - totalRead;

if(remaining <= 0)
break;

toRead = (int)Math.Min(remaining, blockSize);
}

int bytesRead = input.Read(inBuffer[..toRead] );

if(bytesRead <= 0)
break;

ReadOnlySpan<byte> chunk = inBuffer[..bytesRead];
using NativeMemoryOwner<byte> outputOwner = transform(chunk);

Span<byte> outBuffer = outputOwner.AsSpan();
output.Write(outBuffer);

totalRead += bytesRead;
progressCallback?.Invoke(totalRead, maxBytes < 0 ? totalRead : maxBytes);
}

output.Flush();
}

/// <summary> Processes the data contained in an input stream, 
/// applies a transformation function with an argument,
/// and writes the transformed data to an output stream. </summary>
/// <param name="input"> The stream which contains the data to be processed. </param>
/// <param name="output"> The stream where the processed data will be written. </param>
/// <param name="arg"> An argument to be passed to the transformation function. </param>
/// <param name="transform"> A function that determines how the buffer should be transformed. </param>
/// /// <param name="maxBytes"> The maximum number of bytes to process (-1 for no limit). </param>
/// <param name="progressCallback"> An optional callback to report progress during processing. </param>

public static void Process(Stream input, Stream output, ReadOnlySpan<byte> arg,
                           BytesTransform2 transform, long maxBytes = -1, 
                           Action<long, long> progressCallback = null)
{
int blockSize = MemoryManager.GetBlockSize(input);
using NativeMemoryOwner<byte> inputOwner = new(blockSize);

Span<byte> inBuffer = inputOwner.AsSpan();
long totalRead = 0;

while(true)
{
int toRead = blockSize;

if(maxBytes >= 0)
{
long remaining = maxBytes - totalRead;

if(remaining <= 0)
break;

toRead = (int)Math.Min(remaining, blockSize);
}

int bytesRead = input.Read(inBuffer[..toRead]);

if(bytesRead <= 0)
break;

ReadOnlySpan<byte> chunk = inBuffer[..bytesRead];
using NativeMemoryOwner<byte> outputOwner = transform(chunk, arg);

Span<byte> outBuffer = outputOwner.AsSpan();
output.Write(outBuffer);

totalRead += bytesRead;
progressCallback?.Invoke(totalRead, maxBytes < 0 ? totalRead : maxBytes);
}

output.Flush();
}

}