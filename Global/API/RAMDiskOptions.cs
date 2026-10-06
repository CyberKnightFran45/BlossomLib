public sealed class RAMDiskOptions
{
// If RAM disk is enabled or not

public bool Enabled{ get; init; } = true;

// Drive letter for disk (null for auto)

public char? DriveLetter{ get; init; } = 'R';

// File system used

public string FileSystem{ get; init; } = "NTFS";

// Disk label

public string Label{ get; init; } = "RamDisk";

// Try expand disk if already exists

public bool AllowExpand{ get; init; } = true;

// Extra margin over space calculated

public double ExtraMarginFactor { get; init; } = 0.5;
}