using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Compress files by using the Deflate algorithm. </summary>

public static class DeflateCompressor
{
// Get deflate stream

private static DeflateStream GetDeflator(Stream stream, CompressionLevel level) => new(stream, level, true);

// Compress stream

public static void CompressStream(Stream input, Stream output, CompressionLevel level,
                                  long maxBytes = -1, ProgressCallback progress = null)
{
NetCompressionHelper.CompressStream(input, output, level, GetDeflator, maxBytes, progress);
}

/** <summary> Compress file by using Deflate </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to compressed file. </param> */

public static void CompressFile(string inputPath, string outputPath, CompressionLevel level,
                                ProgressCallback progress = null)
{

NetCompressionHelper.CompressFile("Deflate Compression", inputPath, outputPath, ".dfl",
                                  level, GetDeflator, progress);

}

// Get inflator

private static DeflateStream GetInflator(Stream stream) => new(stream, CompressionMode.Decompress, true);

// Decompress stream

public static void DecompressStream(Stream input, Stream output,
                                    long maxBytes = -1,
                                    ProgressCallback progress = null)
{
NetCompressionHelper.DecompressStream(input, output, GetInflator, maxBytes, progress);
}

/** <summary> Decompress file by using Deflate </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to decompressed file. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  ProgressCallback progress = null)
{

NetCompressionHelper.DecompressFile("Deflate Decompression", inputPath, outputPath,
                                    GetInflator, progress);

}

}

}