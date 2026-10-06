using System;
using System.IO;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes Adler32 Checksum Tasks for Bytes and Streams. </summary>

public static class Adler32
{
/// <summary> The Maximum Value for the Adler32 Checksum. </summary>

private const uint MOD_ADLER = 65521;

/** <summary> Gets the Checksum of an Array of Bytes by using the Adler32 Algorithm. </summary>

<param name = "data"> The Bytes where the Checksum will be Obtained from. </param>

<returns> The Adler32 Checksum. </returns> */

public static uint Calculate(ReadOnlySpan<byte> data)
{
uint sumX = 1;
uint sumY = 0;

for (int i = 0; i < data.Length; i++)
{
sumX += data[i];
sumY += sumX;
}

sumX %= MOD_ADLER;
sumY %= MOD_ADLER;

return (sumY << 16) | sumX;
}

/** <summary> Gets the Checksum of a Stream by using the Adler32 Algorithm. </summary>

<param name = "input"> The Stream where the Checksum will be Obtained from. </param>

<returns> The Adler32 Checksum. </returns> */

public static uint Calculate(Stream input)
{
uint sumX = 1;
uint sumY = 0;

int blockSize = MemoryManager.GetBlockSize(input);
using NativeMemoryOwner<byte> bOwner = new(blockSize);

Span<byte> buffer = bOwner.AsSpan();
int bytesRead;

while( (bytesRead = input.Read(buffer) ) > 0)
{

for(int i = 0; i < bytesRead; i++)
{
sumX += buffer[i];
sumY += sumX;
}

sumX %= MOD_ADLER;
sumY %= MOD_ADLER;
}

return (sumY << 16) | sumX;
}

// Hash stream

private static void HashStream(Stream input, Stream output, TraceContext ctx)
{
uint checksum = Calculate(input);
var text = checksum.ToString("X8");

output.WriteString(text);
}

// Hash internal

private static void HashInternal(TraceContext ctx, string srcPath, string destPath)
{
TraceFileSteps.Run(ctx, srcPath, destPath, "Computing digest...", HashStream, true, false);
}

// Compute Adler32 hash

public static void HashFile(string srcPath, string destPath)
{

TraceExecutor.Run("Adler32 Checksum",
                  ctx => HashInternal(ctx, srcPath, destPath),
                  ("InputPath", srcPath),
                  ("OutputPath", destPath)
);

}

}

}