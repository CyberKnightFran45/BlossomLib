using System;
using System.IO;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes Exclusive-OR (XOR) Ciphering Functions for Files. </summary>

public static class XorCryptor
{
/** <summary> Ciphers an Array of Bytes by using the XOR Algorithm. </summary>

<remarks> Passing an Array of plain Bytes to this Method, will Output the encrypted Bytes; 
otherwise, the decrypted Bytes. </remarks>

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
<param name = "progressCallback"> An optional Callback to report Progress. </param>
*/

public static void CipherStream(Stream input, Stream output, ReadOnlySpan<byte> key,
long maxBytes = -1, Action<long, long> progressCallback = null)
{
static NativeMemoryOwner<byte> t(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key) => CipherData(data, key);

FileManager.Process(input, output, key, t, maxBytes, progressCallback);
}

/** <summary> Ciphers a File by using the XOR Algorithm. </summary>

<param name = "inputPath"> The Path where the File to Cipher is Located. </param>
<param name = "outputPath"> The Location where the Ciphered File will be Saved. </param>
<param name = "key"> The Cipher Key. </param>
<param name = "progressCallback"> An optional Callback to report Progress. </param> */

public static void CipherFile(string inputPath, string outputPath, ReadOnlySpan<byte> key,
Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("Xor Cipher Started");

try
{
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");
TraceLogger.WriteActionStart("Opening files...");

using FileStream inFile = FileManager.OpenRead(inputPath);
using FileStream outFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

string fileSize = SizeT.FormatSize(inFile.Length);
TraceLogger.WriteActionStart($"Ciphering data... ({fileSize})");

CipherStream(inFile, outFile, key, -1, progressCallback);
TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Cipher data");
}

TraceLogger.Write("Xor Cipher Finished");
}

}

}