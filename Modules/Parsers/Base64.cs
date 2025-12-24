using Parser64 = System.Buffers.Text.Base64;

using static FileManager;
using System;
using System.IO;
using System.Buffers;

namespace BlossomLib.Modules.Parsers
{
/// <summary> Initializes Base64 Parse Tasks for Strings. </summary>

public static class Base64
{
// Get Encoded Len

public static long GetEncodedLength(long len) => (len + 2) / 3 * 4;

// Get Encoded Len (Utf8 variant)

public static int GetEncodedLengthUtf8(int len) => Parser64.GetMaxEncodedToUtf8Length(len);

// Get Padding required

private static int GetPadding(int dataLen)
{
int remainder = dataLen % 4;

return remainder == 0 ? 0 : 4 - remainder;
}

// Check Base64 String (Standar)

public static bool IsStandar(ReadOnlySpan<char> str, out NativeMemoryOwner<byte> output)
{
output = new();

if(str.Length % 4 != 0)
return false;

long outputLen = GetDecodedLength(str);
output = new(outputLen);

var rawBytes = output.AsSpan();

if(!Convert.TryFromBase64Chars(str, rawBytes, out int written) )
return false;

output.Realloc(written);

return true;
}

// Check if a Base64 String has readable chars (Standar)

public static bool IsStandarLike(ReadOnlySpan<char> str, out string output)
{
output = string.Empty;

bool isBase64 = IsStandar(str, out var bOwner);

if(!isBase64)
{
bOwner.Dispose();

return false;
}

try
{
using var plainText = BinaryHelper.GetNativeString(bOwner.AsSpan(), EncodingType.UTF8);
bOwner.Dispose();

foreach(char c in plainText)
{
bool isJunk = !char.IsLetterOrDigit(c) && !char.IsPunctuation(c) && !char.IsWhiteSpace(c);

if(isJunk)
return false;

}

output = new(plainText);

return true;
}

catch
{
return false;
}

}

// Check Base64 String (Web-Safe)

public static bool IsWebSafe(ReadOnlySpan<char> str, out NativeMemoryOwner<byte> output)
{
using var wOwner = ToStandar(str);

return IsStandar(wOwner.AsSpan(), out output);
}

// Check if Base64 String has readable chars (Web-Safe)

public static bool IsUrlLike(ReadOnlySpan<char> str, out string output)
{
using var wOwner = ToStandar(str);

return IsStandarLike(wOwner.AsSpan(), out output);
}

// Convert Base64 String to Web Safe (Internal)

private static void ToWebSafe(ref Span<char> input)
{
int len = input.Length;
int trim = len;

for(int i = 0; i < len; i++)
{
ref char c = ref input[i];

if(c == '+')
c = '-';

else if(c == '/')
c = '_';

else if(c == '=' && trim == len)
trim = i;

}

input = input[..trim];
}

/** <summary> Encodes an Array of Bytes as a Base64 String. </summary>

<param name = "input"> The Bytes to Encode. </param>
<param name = "isWebSafe"> Determines if the string will be Generated as a Web Safe. </param>

<returns> The Base64 String. </returns> */

public static NativeString EncodeBytes(ReadOnlySpan<byte> input, bool isWebSafe)
{
long outputLen = GetEncodedLength(input.Length);

NativeString base64Str = new(outputLen);
var base64Span = base64Str.AsSpan();

_ = Convert.TryToBase64Chars(input, base64Span, out int charsWritten);
base64Span = base64Span[.. charsWritten];

if(isWebSafe)
ToWebSafe(ref base64Span);

base64Str.Realloc(charsWritten);

return base64Str;
}

// Convert Base64 Bytes to WebSafe (UTF8 variant)

private static void ToWebSafeUtf8(ref Span<byte> input)
{
int trim = input.Length;

for(int i = 0; i < input.Length; i++)
{
ref byte c = ref input[i];

if(c == 0x2B)
c = 0x2D; // '+' --> '-'

else if(c == 0x2F)
c = 0x5F; // '/' --> '_'

else if(c == 0x3D && trim == input.Length)
trim = i; // Remove '='

}

input = input[..trim];
}

// Encode base64 from bytes directly by using UTF8

public static NativeMemoryOwner<byte> EncodeBytesUtf8(ReadOnlySpan<byte> input, bool isWebSafe)
{
int outputLen = GetEncodedLengthUtf8(input.Length);

NativeMemoryOwner<byte> output = new(outputLen);
var outBuffer = output.AsSpan();

_ = Parser64.EncodeToUtf8(input, outBuffer, out _, out int written);
outBuffer = outBuffer[.. written];

if(isWebSafe)
ToWebSafeUtf8(ref outBuffer);

output.Realloc( (ulong)written);

return output;
}

/// <summary> Encodes a Stream of Base64 Data and Writes the Result to another Stream. </summary>
/// <param name="input">The Stream containing the Data to Encode.</param>
/// <param name="output">The Stream where the Encoded Data will be Written.</param>
/// <param name="isWebSafe">Determines if the Base64 Data is Web Safe.</param>
/// <param name="maxBytes">The maximum number of bytes to be processed (-1 for unlimited).</param>
/// <param name="progressCallback">Optional callback for reporting progress.</param>

public static void EncodeStream(Stream input, Stream output, bool isWebSafe,
long maxBytes = -1, Action<long, long> progressCallback = null)
{
NativeMemoryOwner<byte> t(ReadOnlySpan<byte> data) => EncodeBytesUtf8(data, isWebSafe);

Process(input, output, t, maxBytes, progressCallback);
}

/** <summary> Encodes a File by using Base64. </summary>

<param name = "inputPath"> The Path to the File to Encode. </param>
<param name = "outputPath"> The Path where the Encoded File will be Saved. </param>
<param name = "isWebSafe"> Determines if the String will be Generated as Web Safe. </param>
<param name = "progressCallback"> Optional callback for reporting progress. </param> */

public static void EncodeFile(string inputPath, string outputPath, bool isWebSafe,
Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("Base64 Encoding Started");

try
{
string base64Flags = isWebSafe ? "Web-Safe" : "Standar";
TraceLogger.WriteDebug($"{inputPath} --> {outputPath} ({base64Flags} Base64)");

TraceLogger.WriteActionStart("Opening files...");

using FileStream inFile = OpenRead(inputPath);
using FileStream outFile = OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

string fileSize = SizeT.FormatSize(inFile.Length);
TraceLogger.WriteActionStart($"Encoding data... ({fileSize})");

EncodeStream(inFile, outFile, isWebSafe, -1, progressCallback);
TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Encode file");
}

TraceLogger.WriteLine("Base64 Encoding Finished");

var outSize = GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)}", false);
}

// Convert Base64 String to Standar (Internal)

private static NativeMemoryOwner<char> ToStandar(ReadOnlySpan<char> target)
{
int dataLen = target.Length;

while(dataLen > 0 && target[dataLen - 1] == '=')
dataLen--; // Remove existing padding

int padding = GetPadding(dataLen);
int totaLen = dataLen + padding;

NativeMemoryOwner<char> str = new(totaLen);
str.CopyFrom(target, 0, dataLen);

for(int i = 0; i < dataLen; i++)
{
ref char c = ref str[i];

if(c == '-')
c = '+';

else if(c == '_')
c = '/';

}

if(padding > 0)
str.Fill('=', dataLen, padding);

return str;
}


// Get Decoded Length of Base64 String (Internal)

private static int GetDecodedLength(ReadOnlySpan<char> base64)
{
int length = base64.Length;
int padding = 0;

if(length >= 2)
{

if(base64[length - 1] == '=')
padding++;

if(base64[length - 2] == '=')
padding++;
  
}

else if(length >= 1 && base64[length - 1] == '=')
padding++;

return (length * 3 / 4) - padding;
}

/** <summary> Decodes a Base64 String as an Array of Bytes. </summary>

<param name = "input"> The String to Decode. </param>
<param name = "isWebSafe"> Determines if the string was Generated as WebSafe. </param>

<returns> The Bytes Decoded. </returns> */

public static NativeMemoryOwner<byte> DecodeString(ReadOnlySpan<char> input, bool isWebSafe)
{

if(isWebSafe)
{
using var wOwner = ToStandar(input);

return DecodeString(wOwner.AsSpan(), false);
}

int outputLen = GetDecodedLength(input);
NativeMemoryOwner<byte> decodedBytes = new(outputLen);

_ = Convert.TryFromBase64Chars(input, decodedBytes.AsSpan(), out int written);
decodedBytes.Realloc( (ulong)written);

return decodedBytes;
}

// Convert Web-Safe Base64 to Standar (UTF8 variant)

private static NativeMemoryOwner<byte> ToStandarUtf8(ReadOnlySpan<byte> target)
{
int dataLen = target.Length;

while(dataLen > 0 && target[dataLen - 1] == 0x3D)
dataLen--; // Remove existing padding

int padding = GetPadding(dataLen);
int totaLen = dataLen + padding;

NativeMemoryOwner<byte> bytes = new(totaLen);
bytes.CopyFrom(target, 0, dataLen);

for(int i = 0; i < dataLen; i++)
{
ref byte b = ref bytes[i];

if(b == 0x2D)
b = 0x2B; // '-' --> '+'

else if(b == 0x5F)
b = 0x2F; // '_' --> '/'

}

if(padding > 0)
bytes.Fill(0x3D, dataLen, padding);

return bytes;
}

// Decode Base64 from bytes, directly by using UTF8

public static NativeMemoryOwner<byte> DecodeUtf8Bytes(ReadOnlySpan<byte> input, bool isWebSafe)
{

if(isWebSafe)
{
using var wOwner = ToStandarUtf8(input);

return DecodeUtf8Bytes(wOwner.AsSpan(), false);
}

var outputLen = Parser64.GetMaxDecodedFromUtf8Length(input.Length);
NativeMemoryOwner<byte> output = new(outputLen);

_ = Parser64.DecodeFromUtf8(input, output.AsSpan(), out _, out int written);
output.Realloc( (ulong)written);

return output;
}

/// <summary> Decodes a Stream of Base64 Data and Writes the Result to another Stream. </summary>
/// <param name="input">The Stream containing the Base64 Data to Decode.</param>
/// <param name="output">The Stream where the Decoded Data will be Written.</param>
/// <param name="isWebSafe">Determines if the Base64 Data is Web Safe.</param>
/// <param name="maxBytes">The maximum number of bytes to be processed (-1 for unlimited).</param>
/// <param name="progressCallback">Optional callback for reporting progress.</param>

public static void DecodeStream(Stream input, Stream output, bool isWebSafe,
long maxBytes = -1, Action<long, long> progressCallback = null)
{
NativeMemoryOwner<byte> t(ReadOnlySpan<byte> data) => DecodeUtf8Bytes(data, isWebSafe);

Process(input, output, t, maxBytes, progressCallback);
}

/** <summary> Decodes a File by using Base64. </summary>

<param name = "inputPath"> The Path to the File to Decode. </param>
<param name = "outputPath"> The Path where the Decoded File will be Saved. </param>
<param name = "isWebSafe"> Determines if the string was Generated as WebSafe. </param>
<param name = "progressCallback"> Optional callback for reporting progress. </param> */

public static void DecodeFile(string inputPath, string outputPath, bool isWebSafe,
Action<long, long> progressCallback = null)
{
TraceLogger.Init();
TraceLogger.WriteLine("Base64 Decoding Started");

try
{
string base64Flags = isWebSafe ? "Web-Safe" : "Standar";
TraceLogger.WriteDebug($"{inputPath} --> {outputPath} ({base64Flags} Base64)");

TraceLogger.WriteActionStart("Opening files...");

using FileStream inFile = OpenRead(inputPath);
using FileStream outFile = OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

string fileSize = SizeT.FormatSize(inFile.Length);
TraceLogger.WriteActionStart($"Decoding data... ({fileSize})");

DecodeStream(inFile, outFile, isWebSafe, -1, progressCallback);
TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Decode file");
}

TraceLogger.WriteLine("Base64 Decoding Finished");

var outSize = GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)}", false);
}

}

}