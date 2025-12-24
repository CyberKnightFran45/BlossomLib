using System;
using System.IO;

namespace BlossomLib.Modules.Security
{
/// <summary> Provides CRC-32 Calculation for Checksums. </summary>

public static class Crc32
{
// Precomputed table

private static readonly uint[] Table = new uint[256];

// Init table

static Crc32()
{
const uint POLY = 0xEDB88320;

for(uint i = 0; i < 256; i++)
{
uint crc = i;

for(int j = 0; j < 8; j++)
crc = (crc & 1) != 0 ? (crc >> 1) ^ POLY : crc >> 1;
                
Table[i] = crc;
}

}

/** <summary> Gets the Checksum of an Array of Bytes by using the Crc32 Algorithm. </summary>

<param name = "data"> The Bytes where the Checksum will be Obtained from. </param>

<returns> The Crc32 Checksum. </returns> */

public static uint Compute(ReadOnlySpan<byte> data) => Compute(data, 0xFFFFFFFF);

/** <summary> Gets the CRC-32 checksum with an initial value. </summary>

<param name = "data"> The Bytes where the Checksum will be Obtained from. </param>
<param name = "initial"> Initial CRC Value </param>

<returns> The Crc32 Checksum. </returns> */

public static uint Compute(ReadOnlySpan<byte> data, uint initial)
{
uint crc = initial;

int length = data.Length;
int i = 0;

// Process 64-bits

while(i + 8 <= length)
{
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
}

// Process 32-bits

while(i + 4 <= length)
{
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
}

// Process 16-bits

while(i + 2 <= length)
{
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);
}

// Process last byte

while(i < length)
crc = Table[(crc ^ data[i++]) & 0xFF] ^ (crc >> 8);

return ~crc;
}

/** <summary> Gets the Checksum of a Stream by using the Crc32 Algorithm. </summary>

<param name = "input"> The Stream where the Checksum will be Obtained from. </param>

<returns> The Adler32 Checksum. </returns> */

public static uint Compute(Stream input)
{
int blockSize = MemoryManager.GetBlockSize(input);

using NativeMemoryOwner<byte> bOwner = new(blockSize);
var buffer = bOwner.AsSpan();

uint checksum = 0;
int bytesRead;

while( (bytesRead = input.Read(buffer) ) > 0)
checksum = Compute(buffer[..bytesRead], checksum);

return checksum;
}

}

}