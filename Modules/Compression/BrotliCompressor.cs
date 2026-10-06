using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Compress files by using the Brotli algorithm. </summary>

public static class BrotliCompressor
{
// Get gzip stream

private static BrotliStream GetCompressor(Stream stream, CompressionLevel level) => new(stream, level, true);

// Compress stream

public static void CompressStream(Stream input, Stream output, CompressionLevel level,
                                  long maxBytes = -1, ProgressCallback progress = null)
{
NetCompressionHelper.CompressStream(input, output, level, GetCompressor, maxBytes, progress);
}

/** <summary> Compress file by using Brotli </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to compressed file. </param> */

public static void CompressFile(string inputPath, string outputPath, CompressionLevel level,
                                ProgressCallback progress = null)
{

NetCompressionHelper.CompressFile("Brotli Compression", inputPath, outputPath, ".brotli",
                                  level, GetCompressor, progress);

}

// Get decompressor

private static BrotliStream GetDecompressor(Stream stream) => new(stream, CompressionMode.Decompress, true);

// Decompress stream

public static void DecompressStream(Stream input, Stream output,
                                    long maxBytes = -1,
                                    ProgressCallback progress = null)
{
NetCompressionHelper.DecompressStream(input, output, GetDecompressor, maxBytes, progress);
}

/** <summary> Decompress file by using Brotli </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to decompressed file. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  ProgressCallback progress = null)
{

NetCompressionHelper.DecompressFile("Brotli Decompression", inputPath, outputPath,
                                    GetDecompressor, progress);

}

}

}