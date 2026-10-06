using System;

namespace BlossomLib.Modules.Security
{
/** <summary> Cipher data with Corrected Block TEA (XXTEA) </summary>

<remarks> Authors: <c>David Wheeler</c>, <c>Roger Needham</c> and <c>Ma Xiaoyun</c> </remarks> **/

public static class XXTeaCryptor
{
/// <summary> Constant for XXTEA Encryption </summary>
    
private const uint DELTA = 0x9E3779B9;

// Derive X

private static uint MX(uint sum, uint y, uint z, int p, uint e, ReadOnlySpan<uint> k)
{
int pe = (int)(p & 3 ^ e);

return (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[pe] ^ z);
}

// Get Q Factor

private static int Q(int n) => 6 + 52 / (n + 1);

// Get E Factor

private static uint E(uint sum) => sum >> 2 & 3;

// Validate Key length

private static void ThrowIfInvalidKey(ReadOnlySpan<byte> key)
{
	
if(key.Length != 16)
throw new ArgumentException("XXTEA key must be 16 bytes long.", nameof(key) );

}

// Get decoded length

private static long GetDecodedLength(ReadOnlySpan<uint> data, bool includeLength)
{
long n = data.Length * 4;

if(includeLength)
{
int m = (int)data[^1];
n -= 4;

if(m < n - 3 || m > n)
throw new Exception("Conversion error: data must be a multiple of 4 bytes");

n = m;
}

return n;
}

/** <summary> Converts bytes to an array of uints </summary>

<param name="data"> The data to convert. </param>
<param name="includeLength"> Wheter to append original block length to resulting Array. </param>

<returns> The resulting Array. </returns> */

private static NativeMemoryOwner<uint> ToUInts(ReadOnlySpan<byte> data, bool includeLength)
{
int length = data.Length;
var n = (int)SizeT.GetBlockCount(length, 4);

int outputLen = includeLength ? n + 1 : n;
NativeMemoryOwner<uint> result = new(outputLen);

if(includeLength)
result[n] = (uint)length;

for(int i = 0; i < length; i++)
result[i >> 2] |= (uint)data[i] << ( (i & 3) << 3);

return result;
}

/** <summary> Converts an array of uints into bytes </summary>

<param name="data"> The data to convert. </param>
<param name="includeLength"> Wheter to append original block length to resulting Array. </param>
<param name="destination"> The destination Span to write the bytes. </param>

<returns> The number of bytes written. </returns> */

private static int FromUInts(ReadOnlySpan<uint> data, bool includeLength, Span<byte> destination)
{
long n = GetDecodedLength(data, includeLength);

if (destination.Length < (int)n)
throw new ArgumentException("Destination span is too small.", nameof(destination));

for(int i = 0; i < n; i++) 
destination[i] = (byte)(data[i >> 2] >> ( (i & 3) << 3) );

return (int)n;
}

// Convert array of uint to byte ptr

private static NativeBuffer FromUInts(ReadOnlySpan<uint> data, bool includeLength)
{
long n = GetDecodedLength(data, includeLength);

NativeBuffer result = new(n);
FromUInts(data, includeLength, result.AsSpan() );

return result;
}

// Core Encryption logic

private static NativeMemoryOwner<uint> EncryptCore(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> key)
{
ThrowIfInvalidKey(key);

NativeMemoryOwner<uint> v = ToUInts(plaintext, true);
using NativeMemoryOwner<uint> k = ToUInts(key, false);	

var n = (int)v.Size - 1;
uint z = v[n], y, sum = 0, e;

int p, q = Q(n);

for(int i = 0; i < q; i++)
{
sum += DELTA;
e = E(sum);

for(p = 0; p < n; p++)
{
y = v[p + 1];
z = v[p] += MX(sum, y, z, p, e, k);
}

y = v[0];
z = v[n] += MX(sum, y, z, p, e, k);
}

return v;
}

/** <summary> Encrypts bytes directly into the provided destination Span. </summary> */

public static int EncryptData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key, Span<byte> destination)
{
using var v = EncryptCore(input, key);

return FromUInts(v, false, destination);
}

/** <summary> Encrypts an Array of Bytes by using the XXTEA Algorithm. </summary> */

public static NativeBuffer EncryptData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key)
{
using var v = EncryptCore(input, key);

return FromUInts(v, false);
}

// Core Decryption logic

private static NativeMemoryOwner<uint> DecryptCore(ReadOnlySpan<byte> ciphertext, ReadOnlySpan<byte> key)
{
ThrowIfInvalidKey(key);

NativeMemoryOwner<uint> v = ToUInts(ciphertext, false);
using NativeMemoryOwner<uint> k = ToUInts(key, false);

var n = (int)v.Size - 1;
uint z, y = v[0], sum = (uint)(Q(n) * DELTA), e;

int p;

while(sum != 0)
{
e = E(sum);

for(p = n; p > 0; p--)
{
z = v[p - 1];
y = v[p] -= MX(sum, y, z, p, e, k);
}

z = v[n];
y = v[0] -= MX(sum, y, z, p, e, k);

sum -= DELTA;
}

return v;
}

/** <summary> Decrypts bytes directly into the provided destination Span. </summary> */

public static int DecryptData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key, Span<byte> destination)
{
using var v = DecryptCore(input, key);

return FromUInts(v, true, destination);
}

/** <summary> Decrypts an Array of Bytes by using the XXTEA Algorithm. </summary> */

public static NativeBuffer DecryptData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key)
{
using var v = DecryptCore(input, key);

return FromUInts(v, true);
}

}

}