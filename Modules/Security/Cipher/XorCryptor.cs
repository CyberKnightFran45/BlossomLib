using System;
using System.IO;

namespace BlossomLib.Modules.Security
{
/// <summary> Cipher data with Exclusive-OR (XOR) </summary>

public static class XorCryptor
{
/** <summary> Ciphers an Array of Bytes by using the XOR Algorithm. </summary>

<remarks> Passing an Array of plain Bytes to this Method, will output the <c>Encrypted</c> bytes; 
otherwise, the <c>Decrypted</c> bytes. </remarks>

<param name = "input"> The Bytes to Cipher. </param>
<param name = "key"> The Cipher Key. </param>

<returns> The Ciphered Data. </returns> */

public static NativeMemoryOwner<byte> CipherData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key)
{

if(input.IsEmpty || key.IsEmpty)
return new();

int length = input.Length;
NativeMemoryOwner<byte> cipheredData = new(length);

int keyLength = key.Length;

for(int i = 0; i < length; i++)
cipheredData[i] = (byte)(input[i] ^ key[i % keyLength] );

return cipheredData;
}

/** <summary> Ciphers a Stream by using the XOR Algorithm. </summary>

<param name = "input"> The Stream to Cipher. </param>
<param name = "output"> The Stream where the Ciphered Data will be Written. </param>
<param name = "key"> The Cipher Key. </param>
<param name = "maxBytes"> The Maximum Number of Bytes to Cipher. </param> 
<param name = "progress"> An optional Callback to report Progress. </param> */

public static void CipherStream(Stream input, Stream output, ReadOnlySpan<byte> key,
                                long maxBytes = -1,
                                ProgressCallback progress = null)
{
static NativeMemoryOwner<byte> t(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key) => CipherData(data, key);

FileManager.Process(input, output, key, t, maxBytes, progress);
}

// Cipher internal

private static void CipherInternal(TraceContext ctx, string inputPath, string outputPath, byte[] key,
                                   ProgressCallback progress)
{

TraceFileSteps.Run(ctx,
                   inputPath,
				   outputPath,
                   "Ciphering data...",
				   (i, o, _) => CipherStream(i, o, key, -1, progress)
);

}

/** <summary> Ciphers a File by using the XOR Algorithm. </summary>

<param name = "inputPath"> The Path where the File to Cipher is Located. </param>
<param name = "outputPath"> The Location where the Ciphered File will be Saved. </param>
<param name = "key"> The Cipher Key. </param>
<param name = "progress"> An optional Callback to report Progress. </param> */

public static void CipherFile(string inputPath, string outputPath, byte[] key,
                              ProgressCallback progress = null)
{

TraceExecutor.Run("Xor Cipher",
                  ctx => CipherInternal(ctx, inputPath, outputPath, key, progress),
                  ("InputPath", inputPath),
                  ("OutputPath", outputPath)
);

}

}

}