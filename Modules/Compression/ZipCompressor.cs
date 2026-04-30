using System.IO;
using System.IO.Compression;

namespace BlossomLib.Modules.Compression
{
/// <summary> Initializes Compression for Files by using the Zip algorithm. </summary>

public static class ZipCompressor
{
// Adapt progress

private static ProgressCallback Adapt(FileProgress callback, string fileName)
{

if(callback is null)
return null;

return (current, total) => callback?.Invoke(fileName, current, total);
}

// Add file to ZipContainer

private static void AddFile(ZipArchive archive, string fullPath, string entryName,
                            CompressionLevel level, ProgressCallback progress)
{
entryName = entryName.Replace('\\', '/'); // Normalize

ZipArchiveEntry entry = archive.CreateEntry(entryName, level);

using var input = FileManager.OpenRead(fullPath);
using var entryStream = entry.Open();

FileManager.Process(input, entryStream, -1, progress);
}

public static void CompressStream(string input, Stream output, CompressionLevel level,
                                  FileProgress progress = null)
{
using ZipArchive archive = new(output, ZipArchiveMode.Create, true);

if(File.Exists(input) )
{
string fileName = Path.GetFileName(input);
var fileProgress = Adapt(progress, fileName);

AddFile(archive, input, fileName, level, fileProgress);
}

else if(Directory.Exists(input) )
{
var files = Directory.EnumerateFiles(input, "*.*", SearchOption.AllDirectories);

foreach(var path in files)
{
string relative = Path.GetRelativePath(input, path);
var fileProgress = Adapt(progress, relative);

AddFile(archive, path, relative, level, fileProgress);
}

}

else
throw new FileNotFoundException("Input path not found.", input);

}

// Compress internal

private static void CompressInternal(string inputPath, string outputFile, CompressionLevel level,
                                     FileProgress progress)
{
PathHelper.AddExtension(ref outputFile, ".zip");

TraceLogger.WriteActionStart("Compressing files...");

using var outFile = FileManager.OpenWrite(outputFile);
CompressStream(inputPath, outFile, level, progress);

TraceLogger.WriteActionEnd();
}

// Compress file or folder to ZIP


public static void Compress(string inputPath, string outputFile, CompressionLevel level,
                            FileProgress progress = null)
{
	
TraceExecutor.Run("Zip Compression",
                  ctx => CompressInternal(inputPath, outputFile, level, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputFile),
                  ("CompressionLevel", level)
);

}

// Decompress stream

public static void DecompressStream(Stream input, string outputDir,
                                    FileProgress progress = null)
{
using ZipArchive archive = new(input, ZipArchiveMode.Read, true);
Directory.CreateDirectory(outputDir);

foreach(var entry in archive.Entries)
{
string fullPath = Path.GetFullPath(Path.Combine(outputDir, entry.FullName) );

if(!fullPath.StartsWith(Path.GetFullPath(outputDir) ) )
continue; // skip entry outside target dir

if(string.IsNullOrEmpty(entry.Name) )
{
Directory.CreateDirectory(fullPath);

continue;
}

string rootDir = Path.GetDirectoryName(fullPath);

if(!string.IsNullOrEmpty(rootDir) )
Directory.CreateDirectory(rootDir);

using var entryStream = entry.Open();
using var output = FileManager.OpenWrite(fullPath);

var fileProgress = Adapt(progress, entry.FullName);
FileManager.Process(entryStream, output, -1, fileProgress);

File.SetLastWriteTimeUtc(fullPath, entry.LastWriteTime.UtcDateTime);
}

}

// Decompress internal

private static void DecompressInternal(string inputPath, string outputDir, FileProgress progress)
{
TraceLogger.WriteActionStart("Extracting files...");

using var inFile = FileManager.OpenRead(inputPath);
DecompressStream(inFile, outputDir, progress);

TraceLogger.WriteActionEnd();
}

// Decompress zip

public static void Decompress(string inputPath, string outputDir,
                              FileProgress progress = null)
{

TraceExecutor.Run("Zip Extraction",
                  ctx => DecompressInternal(inputPath, outputDir, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputDir)
);

}

}

}