using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.IO;

namespace BlossomLib.Modules.Compression
{
/// <summary> Compress files by using the BZip2 algorithm. </summary>

public static class BZip2Compressor
{
// Compress Stream

public static void CompressStream(Stream input, Stream output, int blockSize,
                                  long maxBytes = -1,
                                  ProgressCallback progress = null)
{
using BZip2OutputStream compressor = new(output, blockSize);
compressor.IsStreamOwner = false;

FileManager.Process(input, compressor, maxBytes, progress);
}

// Compress transform

private static void CompressTransform(Stream input, Stream output, TraceContext ctx, int blockSize,
									  ProgressCallback progress)
{
ctx.ShowRatio = true;

CompressStream(input, output, blockSize, -1, progress);
}

// Compress internal

private static void CompressInternal(TraceContext ctx, string inputPath, string outputPath, int blockSize,
                                     ProgressCallback progress)
{
PathHelper.AddExtension(ref outputPath, ".bz2");

TraceFileSteps.Run(ctx,
                   inputPath,
                   outputPath,
                   "Compressing data...",
                   (i, o, c) => CompressTransform(i, o, c, blockSize, progress)
);

}

/** <summary> Compress file by using BZip2 </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to compressed file. </param> */

public static void CompressFile(string inputPath, string outputPath, int blockSize,
                                ProgressCallback progress = null)
{

TraceExecutor.Run("BZip2 Compression",
                  ctx => CompressInternal(ctx, inputPath, outputPath, blockSize, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath),
                  ("BlockSize", blockSize)

);

}

// Decompress stream

public static void DecompressStream(Stream input, Stream output,
                                    long maxBytes = -1,
                                    ProgressCallback progress = null)
{
using BZip2InputStream decompressor = new(input);
decompressor.IsStreamOwner = false;

FileManager.Process(decompressor, output, maxBytes, progress);
}

// Decompress internal

private static void DecompressInternal(TraceContext ctx, string inputPath, string outputPath,
                                       ProgressCallback progress)
{

TraceFileSteps.Run(ctx,
                   inputPath,
                   outputPath,
                   "Decompressing data...",
                   (i, o, _) => DecompressStream(i, o, -1, progress)
);

}

/** <summary> Decompress file by using BZip2 </summary>

<param name = "inputPath"> Path to target file. </param>
<param name = "outputPath"> Path to decompressed file. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  ProgressCallback progress = null)
{
TraceExecutor.Run("BZip2 Decompression",
                  ctx => DecompressInternal(ctx, inputPath, outputPath, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath)

);

}

}

}