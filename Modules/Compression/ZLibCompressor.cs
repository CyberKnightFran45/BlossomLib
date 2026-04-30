using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Compress files by using the ZLib algorithm. </summary>

public static class ZLibCompressor
{
// Get gzip stream

private static ZLibStream GetCompressor(Stream stream, CompressionLevel level) => new(stream, level, true);

// Compress stream

public static void CompressStream(Stream input, Stream output, CompressionLevel level,
                                  long maxBytes = -1, ProgressCallback progress = null)
{
NetCompressionHelper.CompressStream(input, output, level, GetCompressor, maxBytes, progress);
}

/** <summary> Compress file by using ZLib </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to compressed file. </param> */

public static void CompressFile(string inputPath, string outputPath, CompressionLevel level,
                                ProgressCallback progress = null)
{

NetCompressionHelper.CompressFile("ZLib Compression", inputPath, outputPath, ".zlib",
                                  level, GetCompressor, progress);

}

// Get decompressor

private static ZLibStream GetDecompressor(Stream stream) => new(stream, CompressionMode.Decompress, true);

// Decompress stream

public static void DecompressStream(Stream input, Stream output,
                                    long maxBytes = -1,
                                    ProgressCallback progress = null)
{
NetCompressionHelper.DecompressStream(input, output, GetDecompressor, maxBytes, progress);
}

/** <summary> Decompress file by using ZLib </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to decompressed file. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  ProgressCallback progress = null)
{

NetCompressionHelper.DecompressFile("ZLib Decompression", inputPath, outputPath,
                                    GetDecompressor, progress);

}

}

}