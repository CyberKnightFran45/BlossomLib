using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Compress files by using the GZip algorithm. </summary>

public static class GZipCompressor
{
// Get gzip stream

private static GZipStream GetCompressor(Stream stream, CompressionLevel level) => new(stream, level, true);

// Compress stream

public static void CompressStream(Stream input, Stream output, CompressionLevel level,
                                  long maxBytes = -1, ProgressCallback progress = null)
{
NetCompressionHelper.CompressStream(input, output, level, GetCompressor, maxBytes, progress);
}

/** <summary> Compress file by using GZip </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to compressed file. </param> */

public static void CompressFile(string inputPath, string outputPath, CompressionLevel level,
                                ProgressCallback progress = null)
{

NetCompressionHelper.CompressFile("GZip Compression", inputPath, outputPath, ".gz",
                                  level, GetCompressor, progress);

}

// Get decompressor

private static GZipStream GetDecompressor(Stream stream) => new(stream, CompressionMode.Decompress, true);

// Decompress stream

public static void DecompressStream(Stream input, Stream output,
                                    long maxBytes = -1,
                                    ProgressCallback progress = null)
{
NetCompressionHelper.DecompressStream(input, output, GetDecompressor, maxBytes, progress);
}

/** <summary> Decompress file by using GZip </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to decompressed file. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  ProgressCallback progress = null)
{

NetCompressionHelper.DecompressFile("GZip Decompression", inputPath, outputPath,
                                    GetDecompressor, progress);

}

}

}