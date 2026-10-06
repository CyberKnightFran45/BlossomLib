// Span based bit reader

using System;

public ref struct BitReader
{
private readonly ReadOnlySpan<byte> _source;

private int _bitPosition;

public BitReader(ReadOnlySpan<byte> source)
{
_source = source;

_bitPosition = 0;
}

// Read bits

public int ReadBits(int count)
{
uint val = 0;

for(int i = 0; i < count; i++)
{
int byteIndex = _bitPosition >> 3;
int bitIndex = _bitPosition & 7;

if( (uint)byteIndex >= (uint)_source.Length)
throw new ArgumentOutOfRangeException(nameof(count) );

val |= (uint)( (_source[byteIndex] >> bitIndex) & 1) << i;

_bitPosition++;
}

return (int)val;
}

}