using System;
using System.IO;
using System.Linq;

// Helper used for calling imdisk.exe

public static class ImDiskHelper
{
// Path to Program

private static readonly string _programPath = GetProgramPath();

// Get Program Path

private static string GetProgramPath()
{
var sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

return Path.Combine(sys32, "imdisk.exe");
}

// Check if ImDisk is installed

public static bool IsInstalled => File.Exists(_programPath);

// Check if Drive exists

private static bool DriveExists(char letter)
{
string root = $"{letter}:\\";
var drives = Directory.GetLogicalDrives();

return drives.Any(d => string.Equals(d, root, StringComparison.OrdinalIgnoreCase) );
}

// Format RAM Disk

public static void FormatDisk(char driveLetter, string fileSystem, string label = "")
{
string args = $"/c format {driveLetter}: /fs:{fileSystem} /v:{label} /q /y";

using var process = ProcessHelper.CreateNew("cmd.exe", args, false, true); 
process.StartInfo.Verb = "runas"; // Disk format requires admin privileges

process.Start();
process.WaitForExit();

if(process.ExitCode != 0)
throw new Exception("ImDisk: failed to format disk.");

}

// Expand RAM Disk (Core)

private static void ExpandDiskCore(char driveLetter, int addSizeMB)
{
string drive = $"{driveLetter}:";
string imdiskArgs = $"-e -s {addSizeMB}M -m {drive}";

using var proc = ProcessHelper.StartNew(_programPath, imdiskArgs);
proc.WaitForExit();

if(proc.ExitCode != 0)
throw new Exception("ImDisk: failed to expand device.");

string script = $@"select volume {driveLetter}
                   extend
                   exit";

string tempFile = Path.GetTempFileName();
File.WriteAllText(tempFile, script);

try
{
using var proc2 = ProcessHelper.StartNew("diskpart.exe", $"/s \"{tempFile}\"");

proc2.WaitForExit();

if(proc2.ExitCode != 0)
throw new Exception("diskpart: failed to update disk info.");

}

finally
{
File.Delete(tempFile);
}

}

// Expand disk

public static void ExpandDisk(char driveLetter, int requiredSizeMB)
{
DriveInfo drive = new($"{driveLetter}:");

long currentSizeMB = drive.TotalSize / SizeT.ONE_MEGABYTE;

if(currentSizeMB >= requiredSizeMB)
return;

long neededExtraMB = requiredSizeMB - currentSizeMB;

ulong freeRam = MemoryManager.GetAvailableRam();
var requiredExtraBytes = (ulong)neededExtraMB * SizeT.ONE_MEGABYTE;

if(freeRam < requiredExtraBytes)
{
TraceLogger.WriteWarn("Not enough RAM to expand disk.");

return;
}

ExpandDiskCore(driveLetter, (int)neededExtraMB);
}

// Create disk (core)

private static void CreateDiskCore(int sizeInMB, char driveLetter, string fileSystem, string label)
{
string createArgs = $"-a -s {sizeInMB}M -m {driveLetter}:";

using var process = ProcessHelper.StartNew(_programPath, createArgs);
process.WaitForExit();

if(process.ExitCode != 0)
throw new Exception("ImDisk: failed to create disk.");

FormatDisk(driveLetter, fileSystem, label);
}

// Create and format RAM Disk

public static void CreateRamDisk(int sizeInMB, char driveLetter, string fileSystem,
                                 string label = "", bool allowExpand = true)
{
ulong freeRam = MemoryManager.GetAvailableRam();
var memRequired = (ulong)sizeInMB * SizeT.ONE_MEGABYTE;

if(freeRam < memRequired)
{
TraceLogger.WriteWarn("Not enough RAM, skipping RAM disk.");

return;
}

if(DriveExists(driveLetter) && allowExpand)
ExpandDisk(driveLetter, sizeInMB);

else
CreateDiskCore(sizeInMB, driveLetter, fileSystem, label);

}

// Remove RAM Disk

public static void RemoveRamDisk(char driveLetter)
{
string removeArgs = $"-D -m {driveLetter}:";

using var process = ProcessHelper.StartNew(_programPath, removeArgs);
process.WaitForExit();

if(process.ExitCode != 0)
throw new Exception("ImDisk: failed to remove disk.");

}

}