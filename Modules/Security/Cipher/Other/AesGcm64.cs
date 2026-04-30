using BlossomLib.Modules.Parsers;
using System;

namespace BlossomLib.Modules.Security
{
/// <summary> Ciphers Data with AES-GCM + Base64 (Web-safe) </summary>

public static class AesGcm64
{
// Encrypts AES-GCM-64 with Random Nonce

public static NativeString Encrypt(ReadOnlySpan<byte> data, byte[] key,
AesGcmTagSize tagSize = AesGcmTagSize.SIZE_16, ReadOnlySpan<byte> associatedData = default)
{
using var encOwner = AesGcmCryptor.Encrypt(data, key, tagSize, associatedData);

return Base64.EncodeBytes(encOwner.AsSpan(), true);
}

// Encrypts some Bytes with AES-GCM and Encode them with Base64

public static NativeString Encrypt(ReadOnlySpan<byte> data, byte[] key, ReadOnlySpan<byte> nonce,
AesGcmTagSize tagSize = AesGcmTagSize.SIZE_16, ReadOnlySpan<byte> associatedData = default)
{
using var encOwner = AesGcmCryptor.Encrypt(data, key, nonce, tagSize, associatedData);

return Base64.EncodeBytes(encOwner.AsSpan(), true);
}

// Decodes some Base64 Bytes, then Decrypt data

public static NativeMemoryOwner<byte> Decrypt(ReadOnlySpan<char> data, byte[] key,
AesGcmTagSize tagSize = AesGcmTagSize.SIZE_16, ReadOnlySpan<byte> associatedData = default)
{
using var decOwner = Base64.DecodeString(data, true);

return AesGcmCryptor.Decrypt(decOwner.AsSpan(), key, tagSize, associatedData);
}

}

}