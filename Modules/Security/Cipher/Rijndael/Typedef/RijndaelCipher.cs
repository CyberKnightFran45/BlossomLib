using System;
using System.Security.Cryptography;

namespace BlossomLib.Modules.Security
{
// Encrypt/Decrypt Data with Rijndael by using Pointers

public sealed class RijndaelCipher : IDisposable
{
// Fields

private readonly RijndaelEngine _engine;
private readonly int blockBytes;
private readonly RijndaelMode _mode;
private readonly RijndaelPadding _padding;
private readonly NativeMemoryOwner<byte> _iv;
private bool disposed;

// ctor

public RijndaelCipher(RijndaelBlockSize blockSize, ReadOnlySpan<byte> key,
                      RijndaelMode mode = RijndaelMode.CBC,
                      RijndaelPadding padding = RijndaelPadding.ZeroPadding,
                      ReadOnlySpan<byte> iv = default)
{
var blockSizeBits = (int)blockSize;

_engine = new(blockSizeBits, key);
blockBytes = blockSizeBits / 8;

_mode = mode;
_padding = padding;

_iv = new(blockBytes);

if(!iv.IsEmpty)
{

if(iv.Length != blockBytes)
throw new ArgumentException("IV length must equal block size", nameof(iv));

_iv.CopyFrom(iv);
}

else
RandomNumberGenerator.Fill(_iv.AsSpan() );

}

// Encrypt data

private NativeMemoryOwner<byte> Encrypt(ReadOnlySpan<byte> data)
{
int paddedLen = ApplyPaddingLength(data.Length);

NativeMemoryOwner<byte> encOwner = new(paddedLen);
var cryptoBuffer = encOwner.AsSpan();

data.CopyTo(cryptoBuffer);
ApplyPadding(cryptoBuffer, data.Length);

Span<byte> feedback = stackalloc byte[blockBytes];
_iv.CopyTo(feedback);

Span<byte> counter = stackalloc byte[blockBytes];

for(int offset = 0; offset < paddedLen; offset += blockBytes)
{
var plain = cryptoBuffer.Slice(offset, blockBytes);
var outBlock = cryptoBuffer.Slice(offset, blockBytes);

switch(_mode)
{
case RijndaelMode.ECB:
_engine.EncryptBlock(plain, outBlock);
break;

case RijndaelMode.SIC: // CTR
_iv.CopyTo(counter);

IncrementCounter(counter, offset / blockBytes);
_engine.EncryptBlock(counter, outBlock);

for(int i = 0; i < blockBytes; i++)
outBlock[i] ^= plain[i];

break;

case RijndaelMode.CFB:
_engine.EncryptBlock(feedback, outBlock);

for(int i = 0; i < blockBytes; i++)
outBlock[i] ^= plain[i];

outBlock.CopyTo(feedback);
break;

case RijndaelMode.OFB:
_engine.EncryptBlock(feedback, outBlock);

for(int i = 0; i < blockBytes; i++)
outBlock[i] ^= plain[i];

outBlock.CopyTo(feedback);
break;

default: // CBC

for(int i = 0; i < blockBytes; i++)
feedback[i] ^= plain[i];

_engine.EncryptBlock(feedback, outBlock);
outBlock.CopyTo(feedback);
break;
}

}

return encOwner;
}

// Decrypt data

private NativeMemoryOwner<byte> Decrypt(ReadOnlySpan<byte> data)
{

if(data.Length % blockBytes != 0)
throw new ArgumentException("Ciphertext length must be multiple of block size", nameof(data));

NativeMemoryOwner<byte> decOwner = new(data.Length);
var plainBuffer = decOwner.AsSpan();

Span<byte> feedback = stackalloc byte[blockBytes];
_iv.CopyTo(feedback);

Span<byte> counter = stackalloc byte[blockBytes];
Span<byte> tmp = stackalloc byte[blockBytes];

for(int offset = 0; offset < data.Length; offset += blockBytes)
{
var inBlock = data.Slice(offset, blockBytes);
var outBlock = plainBuffer.Slice(offset, blockBytes);

switch(_mode)
{
case RijndaelMode.ECB:
_engine.DecryptBlock(inBlock, outBlock);
break;

case RijndaelMode.SIC: // Also known as CTR
_iv.CopyTo(counter);

IncrementCounter(counter, offset / blockBytes);
_engine.EncryptBlock(counter, tmp);

for(int i = 0; i < blockBytes; i++)
outBlock[i] = (byte)(tmp[i] ^ inBlock[i]);

break;

case RijndaelMode.CFB:
_engine.EncryptBlock(feedback, tmp);

for(int i = 0; i < blockBytes; i++)
outBlock[i] = (byte)(tmp[i] ^ inBlock[i]);

inBlock.CopyTo(feedback);
break;

case RijndaelMode.OFB:
_engine.EncryptBlock(feedback, tmp);

for(int i = 0; i < blockBytes; i++)
{
outBlock[i] = (byte)(tmp[i] ^ inBlock[i]);
feedback[i] = tmp[i];
}

break;

default: // CBC
_engine.DecryptBlock(inBlock, tmp);

for(int i = 0; i < blockBytes; i++)
outBlock[i] = (byte)(tmp[i] ^ feedback[i]);

inBlock.CopyTo(feedback);
break;
}

}

var realLen = (ulong)RemovePadding(plainBuffer);
decOwner.Realloc(realLen);

return decOwner;
}

// Cipher data

public NativeMemoryOwner<byte> Cipher(ReadOnlySpan<byte> data, bool forEncryption)
{
return forEncryption ? Encrypt(data) : Decrypt(data);
}

#region Padding


// Get Padded Len
 
private int ApplyPaddingLength(int len)
{
int pad = blockBytes - (len % blockBytes);

if(pad == 0 && _padding != RijndaelPadding.ZeroPadding)
pad = blockBytes;

return len + pad;
}

// Pad Data

private void ApplyPadding(Span<byte> buf, int dataLen)
{
int padLen = buf.Length - dataLen;

if (padLen == 0) return;

switch(_padding)
{
case RijndaelPadding.Pkcs7:

for(int i = dataLen; i < buf.Length; i++)
buf[i] = (byte)padLen;

break;

case RijndaelPadding.ISO_7816d4:
buf[dataLen] = 0x80;

if(padLen > 1)
buf.Slice(dataLen + 1, padLen - 1).Clear();

break;

case RijndaelPadding.X923:

if(padLen > 1)
buf.Slice(dataLen, padLen - 1).Clear();

buf[buf.Length - 1] = (byte)padLen;
break;

case RijndaelPadding.Tbc:
byte last = dataLen > 0 ? buf[dataLen - 1] : (byte)0xFF;

byte fill = (last & 1) == 0 ? (byte)0xFF : (byte)0x00;

for(int i = dataLen; i < buf.Length; i++)
buf[i] = fill;

break;

default: // ZeroPadding
buf.Slice(dataLen).Clear();
break;
}

}

// Get Original Length without Padding

private int RemovePadding(Span<byte> buf)
{

if(buf.Length == 0)
return 0;

switch (_padding)
{
case RijndaelPadding.Pkcs7:
int pad = buf[buf.Length - 1];

for(int i = buf.Length - pad; i < buf.Length; i++)

if(buf[i] != pad)
throw new CryptographicException("Invalid PKCS7 padding");

return buf.Length - pad;

case RijndaelPadding.ISO_7816d4:
int iISO = buf.Length - 1;

while(iISO >= 0 && buf[iISO] == 0)
iISO--;

if(iISO < 0 || buf[iISO] != 0x80)
throw new CryptographicException("Invalid ISO7816-4 padding");

return iISO;

case RijndaelPadding.X923:
int xPad = buf[buf.Length - 1];

for(int j = buf.Length - xPad; j < buf.Length - 1; j++)

if (buf[j] != 0)
throw new CryptographicException("Invalid X9.23 padding");

return buf.Length - xPad;

case RijndaelPadding.Tbc:
byte lastByte = buf[buf.Length - 1];

int jTbc = buf.Length - 1;

while (jTbc >= 0 && buf[jTbc] == lastByte)
jTbc--;

return jTbc + 1;

default: // ZeroPadding
int iZero = buf.Length;

while(iZero > 0 && buf[iZero - 1] == 0)
iZero--;

return iZero;
}

}
 
#endregion

private static void IncrementCounter(Span<byte> counter, int blocks)
{
int len = counter.Length;

for(int b = 0; b < blocks; b++)
{

for (int i = len - 1; i >= 0; i--)
{
counter[i]++;

if(counter[i] != 0)
break;

}

}

}

// Release memory

public void Dispose()
{
	
if(!disposed)
{
_iv.Dispose();

disposed = true;
}

}

}

}