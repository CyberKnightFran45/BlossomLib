using System;
using System.IO;
using System.Runtime.InteropServices;

/// <summary> Useful Tasks for Handling Memory </summary>

public static class MemoryManager
{
// Windows only

[DllImport("kernel32.dll")]

static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

// cache

private static ulong _cachedRam;

private static long _lastCheck;

// Get available RAM (Core)

private static ulong GetAvailableRamCore()
{

MEMORYSTATUSEX mem = new()
{
dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
};

if(!GlobalMemoryStatusEx(ref mem) )
return 0;

return mem.ullAvailPhys;
}

// Get available RAM

public static ulong GetAvailableRam()
{
long now = Environment.TickCount64;

if(now - _lastCheck > 100)
{
_cachedRam = GetAvailableRamCore();
_lastCheck = now;
}

return _cachedRam;
}

// Ensure Size Limit by using Linear func

private static int SizeConstraint(long size, int min, int max, double ramFactor)
{

if(size <= 0)
return min;

double ratio = (double)size / max;
double scaled = min + (max - min) * ratio;

ulong freeRam = GetAvailableRam();

var ramLimit = freeRam == 0 ? (ulong)max : (ulong)(freeRam * ramFactor);
ulong final = Math.Min( (ulong)scaled, ramLimit);

if(final < (ulong)min)
return min;

return final > int.MaxValue ? int.MaxValue : (int)final;
}

// Get Optimal BlockSize (Internal)

private static int GetBlockFactor(long streamSize)
{
const int MIN_BLOCK_SIZE = SizeT.ONE_MEGABYTE * 64;  // 64 MB
const int MAX_BLOCK_SIZE = SizeT.ONE_MEGABYTE * 256; // 256 MB

return SizeConstraint(streamSize, MIN_BLOCK_SIZE, MAX_BLOCK_SIZE, 0.15);
}

/// <summary> Gets the amount of bytes to Process in Blocks. </summary>
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

return SizeConstraint(streamSize, MIN_BUFFER_SIZE, MAX_BUFFER_SIZE, 0.05);
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

/// <summary> Gets the Buffer Size for a known content length </summary>

public static int GetBufferSize(long size) => GetSizeFactor(size);

// Get Size factor for JSON

private static int GetJFactor(long streamSize)
{
const int MIN_JSON_SIZE = 4 * 1024;            // 4 KB min
const int MAX_JSON_SIZE = 64 * 1024 * 1024;   // 64 MB max

return SizeConstraint(streamSize, MIN_JSON_SIZE, MAX_JSON_SIZE, 0.1);
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