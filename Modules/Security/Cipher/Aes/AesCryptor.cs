using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes Advanced Encryption Standar (AES) Functions. </summary>

public static class AesCryptor
{
// Init AES Algorithm

private static Aes Init(CipherMode mode, PaddingMode padding, byte[] key, byte[] iv)
{
var alg = Aes.Create();

alg.Mode = mode;
alg.Padding = padding;
alg.Key = key;

if(mode == CipherMode.ECB && iv is not null)
alg.IV = iv;

return alg;
}

// Encrypt Bytes

public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv = null,
CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.PKCS7)
{
using var alg = Init(mode, padding, key, iv);
var encryptor = alg.CreateEncryptor();

return encryptor.TransformFinalBlock(data, 0, data.Length);
}

// Decrypt Bytes

public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv = null,
CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.PKCS7)
{
using var alg = Init(mode, padding, key, iv);
var decryptor = alg.CreateDecryptor();

return decryptor.TransformFinalBlock(data, 0, data.Length);
}

}

}