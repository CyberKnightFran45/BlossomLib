using System;

namespace BlossomLib.Modules.Security
{
/// <summary> Cipher data with Rijndael </summary>

public static class RijndaelCryptor
{
/** <summary> Ciphers the Data specified with the provided Key. </summary>

<param name = "input"> The Bytes to Cipher. </param>
<param name = "key"> The Cipher Key. </param>
<param name = "iv"> The Initialization Vector (null if ECB). </param>
<param name = "forEncryption"> Determines if the Data should be Encrypted or not. </param>
<param name = "mode"> The expected BlockCipher Name (Default is CBC). </param>
<param name = "paddingType"> The Index of the BlockCipherPadding (Default is ZeroPadding). </param>

<returns> The Data Ciphered. </returns> */

public static NativeBuffer CipherData(ReadOnlySpan<byte> input, 
                                      ReadOnlySpan<byte> key,
                                      bool forEncryption,
									  ReadOnlySpan<byte> iv = default,
									  RijndaelBlockSize blockSize = RijndaelBlockSize.SIZE_16,
									  RijndaelMode mode = RijndaelMode.CBC,
									  RijndaelPadding padding = RijndaelPadding.ZeroPadding)
{
blockSize = Enum.IsDefined(blockSize) ? blockSize : RijndaelBlockSize.SIZE_16;

using RijndaelCipher cryptoEngine = new(blockSize, key, mode, padding, iv);

return cryptoEngine.Cipher(input, forEncryption);
}

/** <summary> Ciphers the Data specified into the provided destination Span. </summary>

<param name = "input"> The Bytes to Cipher. </param>
<param name = "destination"> The destination Span to write the ciphered bytes. </param>
<param name = "key"> The Cipher Key. </param>
<param name = "forEncryption"> Determines if the Data should be Encrypted or not. </param>
<param name = "iv"> The Initialization Vector (null if ECB). </param>
<param name = "blockSize"> The expected Block Size (Default is SIZE_16). </param>
<param name = "mode"> The expected BlockCipher Name (Default is CBC). </param>
<param name = "padding"> The BlockCipher Padding (Default is ZeroPadding). </param>

<returns> The number of bytes written to the destination span. </returns> */

public static int CipherData(ReadOnlySpan<byte> input,
                             Span<byte> destination,
                             ReadOnlySpan<byte> key,
                             bool forEncryption,
                             ReadOnlySpan<byte> iv = default,
                             RijndaelBlockSize blockSize = RijndaelBlockSize.SIZE_16,
                             RijndaelMode mode = RijndaelMode.CBC,
                             RijndaelPadding padding = RijndaelPadding.ZeroPadding)
{
blockSize = Enum.IsDefined(blockSize) ? blockSize : RijndaelBlockSize.SIZE_16;

using RijndaelCipher cryptoEngine = new(blockSize, key, mode, padding, iv);

return cryptoEngine.Cipher(input, destination, forEncryption);
}

}

}