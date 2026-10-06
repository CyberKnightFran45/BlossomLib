using System;
using System.Security.Cryptography;
using System.Text;
using BlossomLib.Modules.Security;

/// <summary> Initializes Handling Functions for Parameters that are used
/// on Data Encryption or Decryption. </summary>

public static class CryptoParams
{
/** <summary> Checks if the Size of the given Data Block meets the expected Range. </summary>

<param name = "data"> The Data Block to be Validated. </param> 
<param name = "minLength"> The Minimum Length of the Block. </param>
<param name = "maxLength"> The Maximum Length of the Block. </param>

<returns> The Block Size. </returns> */

private static int GetBlockSize(ReadOnlySpan<byte> data, int minLength, int maxLength)
{
return Math.Clamp(data.Length, minLength, maxLength);
}

/** <summary> Checks if the provided Block meets the expected Size. </summary>

<remarks> In case it doesn't meet the expected Size, a similar one will be generated instead. </remarks>

<param name = "data"> The Data Block to be Validated. </param>
<param name = "expectedLength"> The Expected Length of the Block. </param>
 */

private static void CheckBlockSize(ref byte[] data, Limit<int> expectedLength)
{
int blockLen = GetBlockSize(data, expectedLength.MinValue, expectedLength.MaxValue);

Array.Resize(ref data, blockLen);
}

/** <summary> Generates a derived Key from an Existing one, by Performing some Iterations. </summary>

<param name = "key"> The Cipher Key to Derive. </param>
<param name = "deriveKeys"> Indicates whether to derive keys or not. </param>
<param name = "expectedKeySize"> The Key Size Expected. </param>
<param name = "salt"> The Salt Value used for Reinforcing the Cipher Key. </param>
<param name = "hashType"> The Hash to be used. </param>
<param name = "iterations"> The Number of Iterations to Perform. </param>

<returns> The derived Cipher Key. </returns> */

public static void KeySchedule(ref byte[] key, bool deriveKeys, int expectedKeySize,
byte[] salt = null, string hashType = null, uint? iterations = null)
{
KeySchedule(ref key, deriveKeys, salt, hashType, iterations, new(expectedKeySize) );
}

/** <summary> Generates a derived Key from an Existing one, by Performing some Iterations. </summary>

<param name = "key"> The Cipher Key to Derive. </param>
<param name = "salt"> The Salt Value used for Reinforcing the Cipher Key. </param>
<param name = "hashType"> The Hash to be used. </param>
<param name = "iterations"> The Number of Iterations to Perform. </param>
<param name = "expectedLength"> The Expected Length of the Key. </param>
<param name = "deriveKeys"> Indicates whether to derive keys or not. </param>

<returns> The derived Cipher Key. </returns> */

public static void KeySchedule(ref byte[] key, bool deriveKeys, byte[] salt = null,
string hashType = "", uint? iterations = null, Limit<int> expectedLength = null)
{
expectedLength ??= new(1, Array.MaxLength);

CheckBlockSize(ref key, expectedLength);

if(deriveKeys)
{
hashType = string.IsNullOrEmpty(hashType) ? "MD5" : hashType;
salt ??= Encoding.UTF8.GetBytes("<Enter a SaltValue>");

var hashAlgName = GenericDigest.GetHashAlgName(hashType);
int derivedKeySize = GetBlockSize(key, expectedLength.MinValue, expectedLength.MaxValue);

key = Rfc2898DeriveBytes.Pbkdf2(key, salt, (int)(iterations ?? 1), hashAlgName, derivedKeySize);
}

}

/** <summary> Initializes a Vector from a Cipher Key. </summary>

<param name = "key"> The Cipher Key to use. </param>
<param name = "length"> The Length of the Vector to be Generated. </param>
<param name = "startIndex"> Specifies where the Vector should Start Copying the Bytes from the Key. </param>

<returns> The IV that was Generated. </returns> */

public static byte[] InitVector(ReadOnlySpan<byte> key, int length, int startIndex = 0)
{
Limit<int> sizeRange = new(length);

return InitVector(key, sizeRange, startIndex);
}

/** <summary> Initializes a Vector from a Cipher Key. </summary>

<param name = "key"> The Cipher Key to use. </param>
<param name = "expectedLength"> The Vector Size Expected. </param>
<param name = "startIndex"> Specifies where the Vector should Start Copying the Bytes from the Key. </param>

<returns> The IV that was Generated. </returns> */

public static byte[] InitVector(ReadOnlySpan<byte> key, Limit<int> expectedLength, int startIndex = 0)
{
int vectorSize = GetBlockSize(key, expectedLength.MinValue, expectedLength.MaxValue);
Span<byte> iv = stackalloc byte[vectorSize];

startIndex = Math.Clamp(startIndex, 0, vectorSize - 1);
key.Slice(startIndex, vectorSize).CopyTo(iv);

if(key.Length == vectorSize)
iv.Reverse();

return iv.ToArray();
}

}