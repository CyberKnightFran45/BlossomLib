using System;
using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Ciphers data with DES and converts it to Hex. </summary>

public static class XDes
{
// Encrypt Bytes to Hex

public static NativeMemoryOwner<char> Encrypt(byte[] data, byte[] key, byte[] iv,
                                              CipherMode mode = CipherMode.CBC,
                                              PaddingMode padding = PaddingMode.PKCS7)
{
var encryptedBytes = DESCryptor.Encrypt(data, key, iv, mode, padding);

return BinaryHelper.ToHex(encryptedBytes, StringCase.Upper);
}

// Decrypt Hex Bytes

public static byte[] Decrypt(ReadOnlySpan<char> data, byte[] key, byte[] iv,
                             CipherMode mode = CipherMode.CBC,
                             PaddingMode padding = PaddingMode.PKCS7)
{
using var xOwner = BinaryHelper.FromHex(data);
var encryptedBytes = xOwner.ToArray();

return DESCryptor.Decrypt(encryptedBytes, key, iv, mode, padding);
}

}

}