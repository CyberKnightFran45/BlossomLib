using System;
using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Initializes Compression for Files by using the Zip algorithm. </summary>

public static class ZipCompressor
{

private static Action<long, long> WriteProgress(string fileName, Action<string, long, long> callback)
{
return (written, total) => callback?.Invoke(fileName, written, total);
}

private static void AddFile(ZipArchive archive, string fullPath, string entryName,
                            CompressionLevel level, Action<long, long> progress)
{
entryName = entryName.Replace('\\', '/'); // Normalize
ZipArchiveEntry entry = archive.CreateEntry(entryName, level);

using var input = FileManager.OpenRead(fullPath);
using Stream entryStream = entry.Open();

FileManager.Process(input, entryStream, -1, progress);
}

public static void CompressStream(string input, Stream output, CompressionLevel level,
                                  Action<string, long, long> progressCallback = null)
{
using ZipArchive archive = new(output, ZipArchiveMode.Create, true);

if(File.Exists(input) )
{
string fileName = Path.GetFileName(input);
var progress = WriteProgress(fileName, progressCallback);

AddFile(archive, input, fileName, level, progress);
}

else if(Directory.Exists(input) )
{
var files = Directory.EnumerateFiles(input, "*.*", SearchOption.AllDirectories);

foreach(var file in files)
{
string relative = Path.GetRelativePath(input, file);
var progress = WriteProgress(relative, progressCallback);

AddFile(archive, file, relative, level, progress);
}

}

}

public static void Compress(string inputPath, string outputFile, CompressionLevel level,
                            Action<string, long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("Zip Compression Started");

try
{
PathHelper.AddExtension(ref outputFile, ".zip");
TraceLogger.WriteDebug($"{inputPath} --> {outputFile}");

TraceLogger.WriteActionStart("Opening file...");
using var output = FileManager.OpenWrite(outputFile);

TraceLogger.WriteActionEnd();
TraceLogger.WriteActionStart("Compressing files...");

CompressStream(inputPath, output, level, progressCallback);
TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Compress file");
}

TraceLogger.WriteLine("Zip Compression Finished");

var outSize = FileManager.GetFileSize(outputFile);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)}", false);
}

public static void DecompressStream(Stream input, string outputDir,
                                    Action<string, long, long> progressCallback = null)
{
using ZipArchive archive = new(input, ZipArchiveMode.Read, true);
Directory.CreateDirectory(outputDir);

foreach(var entry in archive.Entries)
{
string fullPath = Path.Combine(outputDir, entry.FullName);

if(string.IsNullOrEmpty(entry.Name) )
{
Directory.CreateDirectory(fullPath);
continue;
}

Directory.CreateDirectory(Path.GetDirectoryName(fullPath) );

using Stream entryStream = entry.Open();
using FileStream output = FileManager.OpenWrite(fullPath);

var progress = WriteProgress(entry.FullName, progressCallback);
FileManager.Process(entryStream, output, -1, progress);

File.SetLastWriteTimeUtc(fullPath, entry.LastWriteTime.UtcDateTime);
}

}

public static void Decompress(string inputPath, string outputDir,
                              Action<string, long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("Zip Decompression Started");

try
{
TraceLogger.WriteDebug($"{inputPath} --> {outputDir}");
TraceLogger.WriteActionStart("Opening file...");

using var input = FileManager.OpenRead(inputPath);
TraceLogger.WriteActionEnd();

string fileSize = SizeT.FormatSize(input.Length);
TraceLogger.WriteActionStart($"Decompressing data... ({fileSize})");

DecompressStream(input, outputDir, progressCallback);
TraceLogger.WriteActionEnd();
}

catch (Exception error)
{
TraceLogger.WriteError(error, "Failed to Decompress file");
}

TraceLogger.Write("Zip Decompression Finished");
}

}

}