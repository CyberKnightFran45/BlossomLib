using System;
using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Ciphers data with 3-DES and converts it to Hex. </summary>

public static class X3Des
{
// Encrypt Bytes to Hex

public static NativeMemoryOwner<char> Encrypt(byte[] data, byte[] key,
                                              byte[] iv = null,
                                              CipherMode mode = CipherMode.ECB,
                                              PaddingMode padding = PaddingMode.None)
{
var encryptedBytes = TripleDESCryptor.Encrypt(data, key, iv, mode, padding);

return BinaryHelper.ToHex(encryptedBytes, StringCase.Upper);
}

// Decrypt Hex Bytes

public static byte[] Decrypt(ReadOnlySpan<char> data, byte[] key,
                             byte[] iv = null,
                             CipherMode mode = CipherMode.ECB,
                             PaddingMode padding = PaddingMode.None)
{
using var xOwner = BinaryHelper.FromHex(data);
var encryptedBytes = xOwner.ToArray();

return TripleDESCryptor.Decrypt(encryptedBytes, key, iv, mode, padding);
}

}

}