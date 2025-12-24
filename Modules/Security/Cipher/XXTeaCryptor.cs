using System;

namespace BlossomLib.Modules.Security
{
/** <summary> Initializes Corrected Block TEA (XXTEA) Ciphering for Bytes. </summary>

<remarks> Authors: <c>David Wheeler</c>, <c>Roger Needham</c> and <c>Ma Xiaoyun</c> </remarks> */

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

<returns> The resulting Array. </returns> */

private static NativeMemoryOwner<byte> FromUInts(ReadOnlySpan<uint> data, bool includeLength)
{
long n = data.Length << 2;

if(includeLength)
{
int m = (int)data[^1];
n -= 4;

if(m < n - 3 || m > n)
throw new Exception("Error on Decryption: data must be a Multiple of 4 bytes");

n = m;
}

NativeMemoryOwner<byte> result = new(n);

for(int i = 0; i < n; i++) 
result[i] = (byte)(data[i >> 2] >> ( (i & 3) << 3) );

return result;
}

/** <summary> Encrypts an Array of Bytes by using the XXTEA Algorithm. </summary>

<returns> The Data Encrypted. </returns> */

public static NativeMemoryOwner<byte> EncryptData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key)
{

if(key.Length != 16)
throw new ArgumentException("XXTEA key must be 16 bytes long.", nameof(key) );

using NativeMemoryOwner<uint> v = ToUInts(input, true);
var n = (int)v.Size - 1;

using NativeMemoryOwner<uint> k = ToUInts(key, false);

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

return FromUInts(v, false);
}

/** <summary> Decrypts an Array of Bytes by using the XXTEA Algorithm. </summary>

<param name = "input"> The Bytes to Decrypt. </param>
<param name = "key"> The Cipher Key. </param>

<returns> The Data Decrypted. </returns> */

public static NativeMemoryOwner<byte> DecryptData(ReadOnlySpan<byte> input, ReadOnlySpan<byte> key)
{

if(key.Length != 16)
throw new ArgumentException("XXTEA key must be 16 bytes long.", nameof(key) );

using NativeMemoryOwner<uint> v = ToUInts(input, false);
var n = (int)v.Size - 1;

using NativeMemoryOwner<uint> k = ToUInts(key, false);

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

return FromUInts(v, true);
}

}

}