using System;
using System.IO;

namespace BlossomLib.Modules.Security
{
/// <summary> Digest data by using MD5 </summary>

public static class Md5Digest
{
/// <summary> Gets the hash string of the input bytes using MD5. </summary>
/// <param name="input">The bytes to cipher</param>
/// <param name="strCase">The string case to use for the output</param>
/// <returns>A MemoryOwner containing the hash string</returns>

public static NativeMemoryOwner<char> GetString(ReadOnlySpan<byte> input, StringCase strCase = default)
{
return GenericDigest.GetString(input, "MD5", strCase);
}

/// <summary> Gets the hash string of the input stream using MD5. </summary>
/// <param name="input">The stream to cipher</param>
/// <param name="strCase">The string case to use for the output</param>

public static NativeMemoryOwner<char> GetString(Stream input, StringCase strCase = default)
{
return GenericDigest.GetString(input, "MD5", strCase);
}

// Hash stream

private static void HashStream(Stream input, Stream output, TraceContext ctx)
{
using var digest = GetString(input);

output.WriteString(digest.AsSpan() );
}

// Hash internal

private static void HashInternal(TraceContext ctx, string srcPath, string destPath)
{
TraceFileSteps.Run(ctx, srcPath, destPath, "Computing digest...", HashStream, true, false);
}

// Compute md5 hash

public static void HashFile(string srcPath, string destPath)
{

TraceExecutor.Run("MD5 Digest",
                  ctx => HashInternal(ctx, srcPath, destPath),
                  ("InputPath", srcPath),
                  ("OutputPath", destPath)
);

}

}

}