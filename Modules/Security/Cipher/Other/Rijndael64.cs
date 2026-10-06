using BlossomLib.Modules.Parsers;
using System;

namespace BlossomLib.Modules.Security
{
/// <summary> Ciphers Data with Rijndael + Base64 (Web-safe) </summary>

public static class Rijndael64
{
// Encrypts some Bytes and Encode them with Base64

public static NativeString Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key,
                                   ReadOnlySpan<byte> iv = default,
								   RijndaelBlockSize blockSize = RijndaelBlockSize.SIZE_16,
								   RijndaelMode mode = RijndaelMode.CBC,
								   RijndaelPadding padding = RijndaelPadding.ZeroPadding)
{
using var encOwner = RijndaelCryptor.CipherData(data, key, true, iv, blockSize, mode, padding);

return Base64.EncodeBytes(encOwner.AsSpan(), true);
}

// Decodes some Base64 Bytes, then Decrypt data

public static NativeMemoryOwner<byte> Decrypt(ReadOnlySpan<char> data, ReadOnlySpan<byte> key,
											  ReadOnlySpan<byte> iv = default,
											  RijndaelBlockSize blockSize = RijndaelBlockSize.SIZE_16,
                                              RijndaelMode mode = RijndaelMode.CBC,
                                              RijndaelPadding padding = RijndaelPadding.ZeroPadding)
{
using var decOwner = Base64.DecodeString(data, true);

return RijndaelCryptor.CipherData(decOwner.AsSpan(), key, false, iv, blockSize, mode, padding);
}

}

}