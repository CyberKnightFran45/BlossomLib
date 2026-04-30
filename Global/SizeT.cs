/// <summary> Represents a Table of Sizes. </summary> 

public static class SizeT
{
/// <summary> The Value of one Kilobyte (1024 Bytes). </summary>
public const int ONE_KILOBYTE = 1024;

/// <summary> The Value of one Megabyte (1024 Kilobytes). </summary>
public const int ONE_MEGABYTE = ONE_KILOBYTE * 1024;

/// <summary> The Value of one Gigabyte (1024 Megabytes). </summary>
public const long ONE_GIGABYTE = ONE_MEGABYTE * 1024;

/// <summary> The Value of one Terabyte (1024 Gigabytes). </summary>
public const long ONE_TERABYTE = ONE_GIGABYTE * 1024;

/// <summary> The Value of one Petabyte (1024 Terabytes). </summary>
public const long ONE_PETABYTE = ONE_TERABYTE * 1024;

/// <summary> The Maximum Amount of Bytes to alloc in Stack (2 KB). </summary>
public const int MAX_STACK = ONE_KILOBYTE * 2;

/// <summary> Formats a size in bytes into a human-readable string. </summary>
/// <param name="size">The size in bytes.</param>
/// <returns>A string representing the size in a human-readable format.</returns>

public static string FormatSize(long size)
{

if (size < ONE_KILOBYTE) return $"{size} Bytes";
if (size < ONE_MEGABYTE) return $"{size / (double)ONE_KILOBYTE:0.##} KB";
if (size < ONE_GIGABYTE) return $"{size / (double)ONE_MEGABYTE:0.##} MB";
if (size < ONE_TERABYTE) return $"{size / (double)ONE_GIGABYTE:0.##} GB";
if (size < ONE_PETABYTE) return $"{size / (double)ONE_TERABYTE:0.##} TB";

return $"{size / (double)ONE_PETABYTE:0.##} PB";
}

/** <summary> Calculates the padded length for the data, ensuring it is a multiple of n bytes. </summary>

<returns> The padded length. </returns> */

public static long GetPaddedLen(long length, int n)
{
var remainder = length % n;

return remainder == 0 ? length : length + (n - remainder);
}

// Get padding needed

public static long GetPadding(long length, int n)
{
long paddedLen = GetPaddedLen(length, n);

return paddedLen - length;
}

// Get the number of blocks needed

public static long GetBlockCount(long length, int n)
{
long paddedLen = GetPaddedLen(length, n);

return paddedLen / n;
}

// Get Original length

public static long GetOriginaLen(long paddedLen, int n)
{
return paddedLen - GetPadding(paddedLen, n);
}

}