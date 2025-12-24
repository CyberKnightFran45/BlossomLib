using BlossomLib.Modules.Parsers;
using System;
using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Ciphers Data with AES + Base64 (Web-safe) </summary>

public static class Aes64
{
// Encrypts some Bytes and Encode them with Base64

public static NativeString Encrypt(byte[] data, byte[] key, byte[] iv = null,
CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.PKCS7)
{
var encryptedData = AesCryptor.Encrypt(data, key, iv, mode, padding);

return Base64.EncodeBytes(encryptedData, true);
}

// Decodes some Base64 Bytes, then Decrypt data

public static byte[] Decrypt(ReadOnlySpan<char> data, byte[] key, byte[] iv = null,
CipherMode mode = CipherMode.ECB, PaddingMode padding = PaddingMode.PKCS7)
{
using var decOwner = Base64.DecodeString(data, true);
var decodedData = decOwner.ToArray();

return AesCryptor.Decrypt(decodedData, key, iv, mode, padding);
}

}

}