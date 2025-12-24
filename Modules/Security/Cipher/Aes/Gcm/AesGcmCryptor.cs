using System;
using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes AES-GCM Functions for Bytes. </summary>

public static class AesGcmCryptor
{
/// <summary> Expected Nonce Length (in bytes). </summary>

private const int NONCE_LENGTH = 12;

// Init AES-GCM Algorithm

private static AesGcm Init(byte[] key, AesGcmTagSize tagSize, out int tagSizeInBytes)
{
tagSize = Enum.IsDefined(tagSize) ? tagSize : AesGcmTagSize.SIZE_16;

tagSizeInBytes = (int)tagSize;

return new(key, tagSizeInBytes);
}

// Gen Random Nonce

private static NativeMemoryOwner<byte> GenNonce()
{
NativeMemoryOwner<byte> nOwner = new(NONCE_LENGTH);
RandomNumberGenerator.Fill(nOwner.AsSpan() );

return nOwner;
}

// Encrypt AES-GCM with Random nonce

public static NativeMemoryOwner<byte> Encrypt(ReadOnlySpan<byte> data, byte[] key,
AesGcmTagSize tagSize = AesGcmTagSize.SIZE_16, ReadOnlySpan<byte> associatedData = default)
{
using var nOwner = GenNonce();

return Encrypt(data, key, nOwner.AsSpan(), tagSize, associatedData);
}

// Encrypt Bytes and Saves the Ciphertext along with the Nonce and Tag

public static NativeMemoryOwner<byte> Encrypt(ReadOnlySpan<byte> data, byte[] key, ReadOnlySpan<byte> nonce,
                                              AesGcmTagSize tagSize = AesGcmTagSize.SIZE_16,
                                              ReadOnlySpan<byte> associatedData = default)
{
using var alg = Init(key, tagSize, out var tagSizeInBytes);

var inputLen = data.Length;
var bufferLen = NONCE_LENGTH + inputLen + tagSizeInBytes;

NativeMemoryOwner<byte> encrypted = new(bufferLen);
encrypted.CopyFrom(nonce, 0, NONCE_LENGTH);

var cipherText = encrypted.AsSpan(NONCE_LENGTH, inputLen);
var tag = encrypted.AsSpan(NONCE_LENGTH + inputLen, tagSizeInBytes);

alg.Encrypt(nonce, data, cipherText, tag, associatedData);

return encrypted;
}

// Decrypt Bytes, by extracting the Nonce and the Tag from Message

public static NativeMemoryOwner<byte> Decrypt(ReadOnlySpan<byte> data, byte[] key,
											  AesGcmTagSize tagSize = AesGcmTagSize.SIZE_16,
                                              ReadOnlySpan<byte> associatedData = default)
{
using var alg = Init(key, tagSize, out var tagSizeInBytes);
var cipherLen = data.Length - NONCE_LENGTH - tagSizeInBytes;

Span<byte> nonce = stackalloc byte[NONCE_LENGTH];
data[.. NONCE_LENGTH].CopyTo(nonce);

NativeMemoryOwner<byte> decrypted = new(cipherLen);

var cipherText = data.Slice(NONCE_LENGTH, cipherLen);
var tag = data.Slice(NONCE_LENGTH + cipherLen, tagSizeInBytes);

alg.Decrypt(nonce, cipherText, tag, decrypted.AsSpan(), associatedData);

return decrypted;
}

}

}