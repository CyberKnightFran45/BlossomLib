using System;
using System.IO;
using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes Digest Tasks for Files by using the .NET Providers. </summary>

public static class GenericDigest
{
// PRE-CACHING

private const string SHA1 = "SHA1";

private const string SHA256 = "SHA256";

private const string SHA384 = "SHA384";

private const string SHA512 = "SHA512";

// HMAC PRE-CACHING

private const string HMAC_SHA1 = "HMACSHA1";

private const string HMAC_SHA256 = "HMACSHA256";

private const string HMAC_SHA384 = "HMACSHA384";

private const string HMAC_SHA512 = "HMACSHA512";

/** <summary> Gets the Length an Auth Code must Have. </summary>

<returns> The Auth Code Length. </returns> */

private static int GetAuthCodeLen(HMAC hmacAlg) => hmacAlg.OutputBlockSize / 8;

/// <summary>
/// Creates a Hash Algorithm based on the given provider name.
/// If the provider name is not recognized, it defaults to MD5.
/// </summary>
/// <param name="providerName">The provider Name</param>
/// <returns>The HashAlgorithm</returns>

public static HashAlgorithm CreateAlg(ReadOnlySpan<char> providerName)
{

if(providerName.Equals(SHA1, StringComparison.OrdinalIgnoreCase) )
return System.Security.Cryptography.SHA1.Create();

else if(providerName.Equals(SHA256, StringComparison.OrdinalIgnoreCase) )
return System.Security.Cryptography.SHA256.Create();

else if(providerName.Equals(SHA384, StringComparison.OrdinalIgnoreCase) )
return System.Security.Cryptography.SHA384.Create();

else if(providerName.Equals(SHA512, StringComparison.OrdinalIgnoreCase) )
return System.Security.Cryptography.SHA512.Create();

return MD5.Create();
}

/// <summary>
///  Creates a HMAC algorithm based on the given provider name.
/// If the provider name is not recognized, it defaults to HMACMD5.
/// </summary>
/// <param name="providerName">The provider name</param>
/// <returns>The HMAC Algorithm</returns>

public static HMAC CreateHmac(ReadOnlySpan<char> providerName)
{

if(providerName.Equals(HMAC_SHA1, StringComparison.OrdinalIgnoreCase) )
return new HMACSHA1();

else if(providerName.Equals(HMAC_SHA256, StringComparison.OrdinalIgnoreCase) )
return new HMACSHA256();

else if(providerName.Equals(HMAC_SHA384, StringComparison.OrdinalIgnoreCase) )
return new HMACSHA384();

else if(providerName.Equals(HMAC_SHA512, StringComparison.OrdinalIgnoreCase) )
return new HMACSHA512();

return new HMACMD5();
}

/// <summary> Gets the HashAlgorithmName based on the given algorithm name.
/// If the algorithm name is not recognized, it defaults to MD5.
/// </summary>
/// <param name="algorithm">The algorithm name</param>
/// <returns>The HashAlgorithmName</returns>

public static HashAlgorithmName GetHashAlgName(ReadOnlySpan<char> algorithm)
{

if(algorithm.Equals(SHA1, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA1;

else if(algorithm.Equals(SHA256, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA256;

else if(algorithm.Equals(SHA384, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA384;

else if(algorithm.Equals(SHA512, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA512;

return HashAlgorithmName.MD5;
}

// HMAC Variant

public static HashAlgorithmName GetHmacAlgName(ReadOnlySpan<char> algorithm)
{

if(algorithm.EndsWith(SHA1, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA1;

else if(algorithm.EndsWith(SHA256, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA256;

else if(algorithm.EndsWith(SHA384, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA384;

else if(algorithm.EndsWith(SHA512, StringComparison.OrdinalIgnoreCase) )
return HashAlgorithmName.SHA512;

return HashAlgorithmName.MD5;
}

/// <summary> Computes the hash of the input bytes using the specified provider name. </summary>
/// <param name="input">The bytes to cipher</param>
/// <param name="providerName">The provider name</param>
/// <returns>A MemoryOwner containing the digest</returns>

public static NativeMemoryOwner<byte> ComputeBytes(ReadOnlySpan<byte> input, ReadOnlySpan<char> providerName)
{
using HashAlgorithm hashAlg = CreateAlg(providerName);

NativeMemoryOwner<byte> hOwner = new(hashAlg.HashSize / 8);
var hashBytes = hOwner.AsSpan();

_ = hashAlg.TryComputeHash(input, hashBytes, out int bytesWritten);
hashBytes = hashBytes[..bytesWritten];

return hOwner;
}

/// <summary> Gets the hash string of the input bytes using the specified provider name. </summary>
/// <param name="input">The bytes to cipher</param>
/// <param name="providerName">The provider name</param>
/// <param name="strCase">The string case to use for the output</param>
/// <returns>A MemoryOwner containing the hash string</returns>

public static NativeMemoryOwner<char> GetString(ReadOnlySpan<byte> input, ReadOnlySpan<char> providerName,
                                                StringCase strCase = default)
{
using var hOwner = ComputeBytes(input, providerName);

return BinaryHelper.ToHex(hOwner.AsSpan(), strCase);
}

/// <summary> Computes the hash of the input stream using the specified provider name. </summary>
/// <param name="input">The stream to cipher</param>
/// <param name="providerName">The provider name</param>
/// <returns>A byte array containing the digest</returns>

public static byte[] ComputeBytes(Stream input, ReadOnlySpan<char> providerName)
{
using HashAlgorithm hashAlg = CreateAlg(providerName);

return hashAlg.ComputeHash(input);
}

/// <summary> Gets the hash string of the input stream using the specified provider name. </summary>
/// <param name="input">The stream to cipher</param>
/// <param name="providerName">The provider name</param>
/// <param name="strCase">The string case to use for the output</param>

public static NativeMemoryOwner<char> GetString(Stream input, ReadOnlySpan<char> providerName,
                                                StringCase strCase = default)
{
var hashBytes = ComputeBytes(input, providerName);

return BinaryHelper.ToHex(hashBytes, strCase);
}

/// <summary> Computes the hash of the input bytes using the specified provider name and HMAC. </summary>
/// <param name="input">The bytes to cipher</param>
/// <param name="providerName">The provider name</param>
/// <param name="authCode">The authentication code for HMAC</param>

public static NativeMemoryOwner<byte> ComputeBytes(ReadOnlySpan<byte> input, ReadOnlySpan<char> providerName,
byte[] authCode)
{

if(authCode == null || authCode.Length == 0)
return ComputeBytes(input, providerName);

using HMAC hmacAlg = CreateHmac(providerName);

int authCodeLen = GetAuthCodeLen(hmacAlg);
CryptoParams.KeySchedule(ref authCode, false, authCodeLen);

hmacAlg.Key = authCode;

NativeMemoryOwner<byte> hOwner = new(hmacAlg.HashSize / 8);
var hashBytes = hOwner.AsSpan();

_ = hmacAlg.TryComputeHash(input, hashBytes, out int bytesWritten);
hashBytes = hashBytes[..bytesWritten];

return hOwner;
}

/// <summary> Gets the hash string of the input bytes using the specified provider name and HMAC. </summary>
/// <param name="input">The bytes to cipher</param>
/// <param name="providerName">The provider name</param>
/// <param name="authCode">The authentication code for HMAC</param>
/// <param name="strCase">The string case to use for the output</param>

public static NativeMemoryOwner<char> GetString(ReadOnlySpan<byte> input, ReadOnlySpan<char> providerName,
byte[] authCode, StringCase strCase = default)
{
using var hOwner = ComputeBytes(input, providerName, authCode);

var hashBytes = hOwner.AsSpan();

return BinaryHelper.ToHex(hashBytes, strCase);
}

/// <summary> Computes the hash of the input stream using the specified provider name and HMAC. </summary>
/// <param name="input">The stream to cipher</param>
/// <param name="providerName">The provider name</param>
/// <param name="authCode">The authentication code for HMAC</param>
/// <returns>A byte array containing the digest</returns>

public static byte[] ComputeBytes(Stream input, ReadOnlySpan<char> providerName, byte[] authCode)
{
using HMAC hmacAlg = CreateHmac(providerName);

int authCodeLen = GetAuthCodeLen(hmacAlg);
CryptoParams.KeySchedule(ref authCode, false, authCodeLen);

hmacAlg.Key = authCode;

return hmacAlg.ComputeHash(input);
}

// Get Hash Str from Stream (Hmac)

public static NativeMemoryOwner<char> GetString(Stream input, ReadOnlySpan<char> providerName,
byte[] authCode, StringCase strCase = default)
{
var hashBytes = ComputeBytes(input, providerName, authCode);

return BinaryHelper.ToHex(hashBytes, strCase);
}

}

}