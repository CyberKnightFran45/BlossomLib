using System;

// Native Enumerator passed by ref

public ref struct NativeEnumerator<T> where T : unmanaged
{
private readonly Span<T> _span;

private int _index;

// ctor

public NativeEnumerator(Span<T> span)
{
_span = span;
_index = -1;
}

// Current element

public readonly T Current => _span[_index];

// Move to next element

public bool MoveNext() => ++_index < _span.Length;
}