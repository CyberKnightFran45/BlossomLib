using System;
using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Initializes Compression for Files by using the ZLib algorithm. </summary>

public static class ZLibCompressor
{
// Get ZLib Stream (Deflate with Header and Footer)

public static void CompressStream(Stream input, Stream output, CompressionLevel level, long maxBytes = -1,
                                  Action<long, long> progressCallback = null)
{
using ZLibStream zlibStream = new(output, level, true);

FileManager.Process(input, zlibStream, maxBytes, progressCallback);
}

/** <summary> Compresses the Contents of a File by using the Deflate Algorithm. </summary>

<param name = "inputPath"> The Path where the File to be Compressed is Located. </param>
<param name = "outputPath"> The Location where the Compressed File will be Saved. </param> */

public static void CompressFile(string inputPath, string outputPath, CompressionLevel level,
                                Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("ZLib Compression Started");

long originalSize = 0;

try
{
PathHelper.AddExtension(ref outputPath, ".zlib");
TraceLogger.WriteDebug($"{inputPath} --> {outputPath} (Level: {level})");

TraceLogger.WriteActionStart("Opening files...");

using var inFile = FileManager.OpenRead(inputPath);
using var outFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

originalSize = inFile.Length;
string fileSize = SizeT.FormatSize(originalSize);

TraceLogger.WriteActionStart($"Compressing data... ({fileSize})");
CompressStream(inFile, outFile, level, -1, progressCallback);

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Compress file");
}

TraceLogger.WriteLine("ZLib Compression Finished");

var outSize = FileManager.GetFileSize(outputPath);
string sizeCompressed = SizeT.FormatSize(outSize);

var ratio = (double)outSize / originalSize;
TraceLogger.WriteInfo($"Output Size: {sizeCompressed} (Ratio: {ratio:P2})", false);
}

// Get Plain Stream

public static void DecompressStream(Stream input, Stream output, long maxBytes = -1,
                                    Action<long, long> progressCallback = null)
{
using ZLibStream decompressor = new(input, CompressionMode.Decompress, true);

FileManager.Process(decompressor, output, maxBytes, progressCallback);
}

/** <summary> Decompresses the Contents of a File by using the Deflate Algorithm. </summary>

<param name = "inputPath"> The Path where the File to be Decompressed is Located. </param>
<param name = "outputPath"> The Location where the Decompressed File will be Saved. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("ZLib Decompression Started");

try
{
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");

using var inFile = FileManager.OpenRead(inputPath);
using var outFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

string fileSize = SizeT.FormatSize(inFile.Length);
TraceLogger.WriteActionStart($"Decompressing data... ({fileSize})");

DecompressStream(inFile, outFile, -1, progressCallback);
TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Decompress file");
}

TraceLogger.WriteLine("ZLib Decompression Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)}", false);
}

}

}