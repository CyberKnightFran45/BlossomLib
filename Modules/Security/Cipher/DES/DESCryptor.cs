using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes Data Encryption Standar (DES) Functions. </summary>

public static class DESCryptor
{
// Init DES Algorithm

private static DES Init(CipherMode mode, PaddingMode padding, byte[] key, byte[] iv)
{
var alg = DES.Create();

alg.Mode = mode;
alg.Padding = padding;
alg.Key = key;

if(mode != CipherMode.ECB && iv is not null)
alg.IV = iv;

return alg;
}

// Encrypt Bytes

public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
{
using var alg = Init(mode, padding, key, iv);
var encryptor = alg.CreateEncryptor();

return encryptor.TransformFinalBlock(data, 0, data.Length);
}

// Decrypt Bytes

public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, CipherMode mode, PaddingMode padding)
{
using var alg = Init(mode, padding, key, iv);
var decryptor = alg.CreateDecryptor();

return decryptor.TransformFinalBlock(data, 0, data.Length);
}

}

}