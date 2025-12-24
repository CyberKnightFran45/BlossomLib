using System;

namespace BlossomLib.Modules.Security
{
/// <summary> Initializes Rijndael Ciphering for Files. </summary>

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

public static NativeMemoryOwner<byte> CipherData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key,
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

}

}