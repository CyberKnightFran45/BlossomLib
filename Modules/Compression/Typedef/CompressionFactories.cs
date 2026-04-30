using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
// Compress stream factory

public delegate Stream CompressFactory(Stream output, CompressionLevel level);

// Decompress stream factory

public delegate Stream DecompressFactory(Stream input);
}