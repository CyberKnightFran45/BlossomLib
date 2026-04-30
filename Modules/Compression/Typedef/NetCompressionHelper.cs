using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
// .NET Compressor helper 

public static class NetCompressionHelper
{
// Compress stream

public static void CompressStream(Stream input, Stream output,
                                  CompressionLevel level, CompressFactory factory,
                                  long maxBytes = -1, ProgressCallback progress = null)
{
using var compressor = factory(output, level);

FileManager.Process(input, compressor, maxBytes, progress);
}

// Compress transform

private static void CompressTransform(Stream input, Stream output, TraceContext ctx,
                                      CompressionLevel level, CompressFactory factory,
									  ProgressCallback progress)
{
ctx.ShowRatio = true;

CompressStream(input, output, level, factory, -1, progress);
}

// Compress internal

private static void CompressInternal(TraceContext ctx, string inputPath,
                                     string outputPath, string extension,
                                     CompressionLevel level, CompressFactory factory,
                                     ProgressCallback progress)
{
PathHelper.AddExtension(ref outputPath, extension);

TraceFileSteps.Run(ctx,
                   inputPath,
				   outputPath,
                   "Compressing data...",
				   (i, o, c) => CompressTransform(i, o, c, level, factory, progress)
);

}

// Compress file

public static void CompressFile(string operationName, string inputPath,
                                string outputPath, string extension,
                                CompressionLevel level, CompressFactory factory,
                                ProgressCallback progress = null)
{

TraceExecutor.Run(operationName,
                  ctx => CompressInternal(ctx, inputPath, outputPath, extension, level, factory, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath),
                  ("CompressionLevel", level)
);

}

// Decompress stream

public static void DecompressStream(Stream input, Stream output, DecompressFactory factory,
                                    long maxBytes = -1, ProgressCallback progress = null)
{
using var decompressor = factory(input);

FileManager.Process(decompressor, output, maxBytes, progress);
}

// Decompress internal

private static void DecompressInternal(TraceContext ctx, string inputPath, string outputPath,
                                       DecompressFactory factory, ProgressCallback progress)
{

TraceFileSteps.Run(ctx,
                   inputPath,
                   outputPath,
                   "Decompressing data...",
                   (i, o, _) => DecompressStream(i, o, factory, -1, progress)
);

}

// Decompress file

public static void DecompressFile(string operationName, string inputPath, string outputPath,
                                  DecompressFactory factory, ProgressCallback progress = null)
{

TraceExecutor.Run(operationName,
                  ctx => DecompressInternal(ctx, inputPath, outputPath, factory, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath)
);

}

}

}