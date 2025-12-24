using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.IO;

namespace BlossomLib.Modules.Compression
{
/// <summary> Initializes Compression Tasks for Files by using the BZip2 algorithm. </summary>

public static class BZip2Compressor
{
// Get BZip2 Stream

public static void CompressStream(Stream input, Stream output, int blockSize, long maxBytes = -1,
                                  Action<long, long> progressCallback = null)
{
using BZip2OutputStream bz2Stream = new(output, blockSize);

bz2Stream.IsStreamOwner = false;

FileManager.Process(input, bz2Stream, maxBytes, progressCallback);
}

/** <summary> Compresses the Contents of a File by using BZip2 Compression. </summary>

<param name = "inputPath"> The Access Path where the File to be Compressed is Located. </param>
<param name = "outputPath"> The Location where the Compressed File will be Saved. </param> */

public static void CompressFile(string inputPath, string outputPath, int blockSize,
                                Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("BZip2 Compression Started");

long originalSize = 0;

try
{
PathHelper.AddExtension(ref outputPath, ".bz2");
TraceLogger.WriteDebug($"{inputPath} --> {outputPath} (Level: {blockSize})");

TraceLogger.WriteActionStart("Opening files...");

using var inFile = FileManager.OpenRead(inputPath);
using var outFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

originalSize = inFile.Length;
string fileSize = SizeT.FormatSize(originalSize);

TraceLogger.WriteActionStart($"Compressing data... ({fileSize})");
CompressStream(inFile, outFile, blockSize, -1, progressCallback);

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Compress file");
}

TraceLogger.WriteLine("BZip2 Compression Finished");

var outSize = FileManager.GetFileSize(outputPath);
string sizeCompressed = SizeT.FormatSize(outSize);

var ratio = (double)outSize / originalSize;

TraceLogger.WriteInfo($"Output Size: {sizeCompressed} (Ratio: {ratio:P2})", false);
}

// Get Plain Stream

public static void DecompressStream(Stream input, Stream output, long maxBytes = -1,
                                    Action<long, long> progressCallback = null)
{
using BZip2InputStream decompressionStream = new(input);

decompressionStream.IsStreamOwner = false;

FileManager.Process(decompressionStream, output, maxBytes, progressCallback);
}

/** <summary> Decompresses the Contents of a File using BZip2 Compression. </summary>

<param name = "inputPath"> The Path where the File to be Decompressed is Located. </param>
<param name = "outputPath"> The Location where the Decompressed File will be Saved. </param> */

public static void DecompressFile(string inputPath, string outputPath,
                                  Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("BZip2 Decompression Started");

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

TraceLogger.WriteLine("BZip2 Decompression Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)}", false);
}

}

}