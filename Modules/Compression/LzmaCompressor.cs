using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace BlossomLib.Modules.Compression
{
/// <summary> Decompresses a file by using Lzma. </summary>

public static class LzmaCompressor
{
// Lzma encoder

private static readonly Encoder encoder = new();

// Lzma decoder

private static readonly Decoder decoder = new();

// Compress

public static void Compress(Stream input, Stream output)
{
encoder.WriteCoderProperties(output);

long inSize = input.Length;

output.WriteInt64(inSize);
encoder.Code(input, output, inSize, -1, null);
}

// Compress internal

private static void CompressInternal(TraceContext ctx, string inputPath, string outputPath)
{
PathHelper.AddExtension(ref outputPath, ".lzma");

ctx.ShowRatio = true;

TraceFileSteps.Run(ctx,
                   inputPath,
				   outputPath,
                   "Compressing data...",
				   (i, o, _) => Compress(i, o)
);

}

/** <summary> Compress a file with Lzma. </summary>

<param name = "inputPath"> The Path where the File to be Compressed is Located. </param>
<param name = "outputPath"> The Location where the Compressed File will be Saved. </param> */

public static void CompressFile(string inputPath, string outputPath)
{

TraceExecutor.Run("Lzma Compression", 
                  ctx => CompressInternal(ctx, inputPath, outputPath),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath)
);

}

// Decompress

public static void Decompress(Stream input, Stream output, long inSize = -1)
{
inSize = inSize < 0 ? input.Length : inSize;

byte[] properties = new byte[5];
input.ReadExactly(properties);

long outSize = input.ReadInt64();
 
decoder.SetDecoderProperties(properties);
decoder.Code(input, output, inSize - 13, outSize, null);
}

// Decompress internal

private static void DecompressInternal(TraceContext ctx, string inputPath, string outputPath)
{
PathHelper.RemoveExtension(ref outputPath);

TraceFileSteps.Run(ctx,
                   inputPath,
				   outputPath,
                   "Decompressing data...",
				   (i, o, _) => Decompress(i, o, -1)
);

}

/** <summary> Decompresses a file with Lzma </summary>

<param name = "inputPath" > The Path where the File to be Decompressed is Located. </param>
<param name = "outputPath" > The Location where the Decompressed File will be Saved. </param> */

public static void DecompressFile(string inputPath, string outputPath)
{

TraceExecutor.Run("Lzma Decompression", 
                  ctx => DecompressInternal(ctx, inputPath, outputPath),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath)
);

}

}

}