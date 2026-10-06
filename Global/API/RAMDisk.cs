using System;
using System.IO;
using System.Linq;

// Allows writting files to a RAM Disk (by the moment only supports imdisk.exe)

public static class RAMDisk
{
// Disk overhead (NTFS)

private const long NTFS_OVERHEAD = SizeT.ONE_MEGABYTE * 16;

// Overhead per file

private const long OVERHEAD_PER_FILE = SizeT.ONE_KILOBYTE * 4; 

// Compute Disk Size (NTFS)

private static int ComputeDiskSize(int fileCount, long totalBytes, double marginFactor)
{
long fileOverhead = fileCount * OVERHEAD_PER_FILE;
var extraMargin = (long)(totalBytes * marginFactor);

long diskSize = totalBytes + NTFS_OVERHEAD + fileOverhead + extraMargin;
long diskSizeMB = diskSize / SizeT.ONE_MEGABYTE;

return diskSizeMB > int.MaxValue ? int.MaxValue : (int)diskSizeMB;
}

// Get free drive

private static char GetFreeDriveLetter()
{
var drives = Directory.GetLogicalDrives();
var used = drives.Select(d => char.ToUpperInvariant(d[0] ) ).ToHashSet();

for(char c = 'Z'; c >= 'D'; c--)
{

if(!used.Contains(c) )
return c;

}

throw new InvalidOperationException("No free drive letters available.");
}

// Redirect output to RAM Disk if posible

public static void TryRedirect(ref string outDir, int fileCount, long totalBytes,
                               RAMDiskOptions options = null)
{
options ??= new();

bool useRamDisk = options.Enabled && PlatformHelper.IsWindows && ImDiskHelper.IsInstalled;

if(!useRamDisk)
return;

TraceLogger.WriteInfo("ImDisk detected, operation will be redirected to RAM disk");

string originalOut = outDir;

try
{
char drive = options.DriveLetter ?? GetFreeDriveLetter();
int diskSizeMB = ComputeDiskSize(fileCount, totalBytes, options.ExtraMarginFactor);

TraceLogger.WriteActionStart($"Mounting \"{drive}:\"... ({diskSizeMB} MB)");
ImDiskHelper.CreateRamDisk(diskSizeMB, drive, options.FileSystem, options.Label, options.AllowExpand);

string oldRoot = Path.GetPathRoot(outDir)?.ToUpperInvariant();
string fixedOut = outDir.ToUpperInvariant();

if(oldRoot != null && fixedOut.StartsWith(oldRoot) )
{
outDir = $"{drive}:" + outDir[oldRoot.Length..];

PathHelper.CheckDuplicatedPath(ref outDir);
}

}

catch
{
TraceLogger.WriteError("Failed to mount RAM disk, normal disk will be used instead.");

outDir = originalOut;
}

TraceLogger.WriteActionEnd();

if(outDir != originalOut)
TraceLogger.WriteInfo($"Output is now redirected to: {outDir}");

}

}