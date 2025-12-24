using System;
using System.IO;

/// <summary> Useful Tasks for Handling Memory </summary>

public static class MemoryManager
{
// Ensure Size Limit by using Linear func

private static int SizeConstraint(long size, int min, int max)
{

if(size >= max)
return max;

else if(size <= min)
return (int)size;

var ratio = (double)size / max;
double bufferSize = min + (max - min) * ratio;

return (int)Math.Min(bufferSize, size);
}

// Get Optimal BlockSize (Internal)

private static int GetBlockFactor(long streamSize)
{
const int MIN_BLOCK_SIZE = SizeT.ONE_MEGABYTE * 64;  // 64 MB
const int MAX_BLOCK_SIZE = SizeT.ONE_MEGABYTE * 256; // 256 MB

return SizeConstraint(streamSize, MIN_BLOCK_SIZE, MAX_BLOCK_SIZE);
}

/// <summary> Gets the amount of bytes to Process in Blocks. </summary> </summary>
/// <param name="targetStream">The stream for which the buffer size is being calculated.</param>
/// <returns>An integer representing the optimal block size in bytes.</returns>

public static int GetBlockSize(Stream targetStream)
{
long streamSize;

try
{
streamSize = targetStream.Length; // May throw Exception
}

catch
{
streamSize = SizeT.ONE_KILOBYTE * 4;
}

return GetBlockFactor(streamSize);
}

/// <summary> Gets the amount of bytes to Process in Blocks. </summary>
/// <param name="filePath">The path to the file for which the block size is being calculated.</param>
/// <returns>An integer representing the optimal block size in bytes.</returns>

public static int GetBlockSize(string filePath)
{
long streamSize;

try
{
streamSize = FileManager.GetFileSize(filePath); // May throw Exception
}

catch
{
streamSize = SizeT.ONE_KILOBYTE * 4;
}

return GetBlockFactor(streamSize);
}

// Get Size factor for Stream

private static int GetSizeFactor(long streamSize)
{
const int MIN_BUFFER_SIZE = 4 * 1024;     // 4 KB
const int MAX_BUFFER_SIZE = 1024 * 1024;  // 1 MB

return SizeConstraint(streamSize, MIN_BUFFER_SIZE, MAX_BUFFER_SIZE);
}

/// <summary> Gets the Buffer Size for a Stream without Exceeding Available Memory.
/// This method calculates the optimal buffer size based on the available RAM and the size of the target stream.
/// </summary>
/// <param name="targetStream">The stream for which the buffer size is being calculated.</param>
/// <returns>An integer representing the optimal buffer size in bytes.</returns>

public static int GetBufferSize(Stream targetStream)
{
long streamSize;

try
{
streamSize = targetStream.Length; // May throw Exception
}

catch
{
streamSize = SizeT.ONE_KILOBYTE * 4;
}

return GetSizeFactor(streamSize);
}

/// <summary> Gets the Buffer Size for a File without Exceeding Available Memory.
/// This method calculates the optimal buffer size based on the available RAM and the size of the target file.
/// </summary>
/// <param name="filePath">The path to the file for which the buffer size is being calculated.</param>
/// <returns>An integer representing the optimal buffer size in bytes.</returns>

public static int GetBufferSize(string filePath)
{
long streamSize;

try
{
streamSize = FileManager.GetFileSize(filePath); // May throw Exception
}

catch
{
streamSize = SizeT.ONE_KILOBYTE * 4;
}

return GetSizeFactor(streamSize);
}

// Get Size factor for JSON

private static int GetJFactor(long streamSize)
{
const int MIN_JSON_SIZE = 4 * 1024;           // 4 KB min
const int MAX_JSON_SIZE = 64 * 1024 * 1024;  // 64 MB max

return SizeConstraint(streamSize, MIN_JSON_SIZE, MAX_JSON_SIZE);
}

/// <summary> Gets the Buffer Size for a JSON Stream. </summary>
/// <param name="targetStream">The stream for which the buffer size is being calculated.</param>
/// <returns>An integer representing the optimal buffer size in bytes.</returns>

public static int GetJsonSize(Stream targetStream)
{
long streamSize;

try
{
streamSize = targetStream.Length; // May throw Exception
}

catch
{
streamSize = SizeT.ONE_KILOBYTE * 4;
}

return GetJFactor(streamSize);
}

}