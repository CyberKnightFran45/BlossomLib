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

}

}