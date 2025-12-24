using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes 3-DES Functions for Bytes. </summary>

public static class TripleDESCryptor
{
// Init 3-DES Algorithm

private static TripleDES Init(CipherMode mode, PaddingMode padding, byte[] key, byte[] iv)
{
var alg = TripleDES.Create();

alg.Mode = mode;
alg.Padding = padding;
alg.Key = key;

if(mode != CipherMode.ECB && iv is not null)
alg.IV = iv;

return alg;
}

// Encrypt Bytes

public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv = null,
CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.None)
{
using var alg = Init(mode, padding, key, iv);
var encryptor = alg.CreateEncryptor();

return encryptor.TransformFinalBlock(data, 0, data.Length);
}

// Decrypt Bytes

public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv = null,
CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.None)
{
using var alg = Init(mode, padding, key, iv);
var decryptor = alg.CreateDecryptor();

return decryptor.TransformFinalBlock(data, 0, data.Length);
}

}

}