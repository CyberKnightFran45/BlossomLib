using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/** <summary> Represents a owner for unmanaged types using native memory allocation. </summary>

<typeparam name="T"> The unmanaged type of the memory owner. </typeparam> **/

public unsafe class NativeMemoryOwner<T> : IDisposable where T : unmanaged
{
/// <summary> Pointer to the allocated memory. </summary>

protected T* _ptr;

/// <summary> Size of the allocated memory in elements of T </summary>

protected ulong _size;

/// <summary> Indicates whether the memory has been disposed. </summary>

private bool _disposed;

/// <summary> Gets the size of the allocated memory. </summary>

public ulong Size
{

get
{
ThrowIfDisposed();

return _size;
}

}

/// <summary> Initializes a empty <see cref="NativeMemoryOwner{T}"/>  </summary>

public NativeMemoryOwner()
{
}

/** <summary> Initializes a new <see cref="NativeMemoryOwner{T}"/> with the specified length. </summary>

<param name="len"> The length of the memory to allocate in elements of T </param> **/

public NativeMemoryOwner(ulong length)
{

if(length == 0)
return;

_disposed = false;

ulong capped = CapToMaxAllocatable(length);
var bytes = (nuint)(capped * (ulong)sizeof(T) );

_ptr = (T*)NativeMemory.Alloc(bytes);
_size = capped;
}

/** <summary> Initializes a new <see cref="NativeMemoryOwner{T}"/> with n bytes. </summary>

<param name="bytes"> The length of the memory to allocate in bytes. </param> **/

public NativeMemoryOwner(long bytes) : this(ClampIdx(bytes) )
{
}

// Destructor

~NativeMemoryOwner() => Dispose(false);

// Convert Index from long to ulong

[MethodImpl(MethodImplOptions.AggressiveInlining)]

protected static ulong ClampIdx(long index)
{
return index >= 0 ? (ulong)index : 0;
}

// Checks if this Owner is empty or not

public bool IsEmpty() => _ptr == null || _size == 0;

// Get max allocatable size based on platform's arquitecture (32 or 64-bits)

private static ulong CapToMaxAllocatable(ulong requested)
{
var maxBytes = (ulong)nuint.MaxValue;
ulong maxElements = maxBytes / (ulong)sizeof(T);

return Math.Min(requested, maxElements);
}

// Throw Exception if memory is disposed

private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, GetType() );

#region ==========  MEMORY OPERATIONS  ==========

// Copy (inner)

private void CopyCore(NativeMemoryOwner<T> src, NativeMemoryOwner<T> dst,
                      ulong srcOffset, ulong dstOffset, ulong count)
{

if(src._size == 0 || count == 0)
return;

src.ThrowIfDisposed();
dst.ThrowIfDisposed();

ArgumentOutOfRangeException.ThrowIfGreaterThan(srcOffset, src._size);
ArgumentOutOfRangeException.ThrowIfGreaterThan(dstOffset, dst._size);

ulong availableSrc = src._size - srcOffset;

if(count > availableSrc)
count = availableSrc;

ulong required = dstOffset + count;

if(required > _size)
dst.Realloc(required);

void* srcPtr = src._ptr + srcOffset;
void* dstPtr = dst._ptr + dstOffset;

ulong bytes = count * (ulong)sizeof(T);

Buffer.MemoryCopy(srcPtr, dstPtr, bytes, bytes);
}

/// <summary> Copy all elements from a Owner to this one </summary>

public void CopyFrom(NativeMemoryOwner<T> src) => CopyFrom(src, 0, 0, src._size);

/// <summary> Copy elements from a Owner to this one, at the specified pos </summary>

public void CopyFrom(NativeMemoryOwner<T> src, ulong dstOffset) => CopyFrom(src, 0, dstOffset, src._size);

// Copy (signed)

public void CopyFrom(NativeMemoryOwner<T> src, long dstOffset)
{
CopyFrom(src, ClampIdx(dstOffset) );
}

/// <summary> Copy elements from a Owner to this one, at the specified pos </summary>

public void CopyFrom(NativeMemoryOwner<T> src, ulong dstOffset, ulong count)
{
CopyFrom(src, 0, dstOffset, count);
}

// Copy (signed)

public void CopyFrom(NativeMemoryOwner<T> src, long dstOffset, long count)
{
CopyFrom(src, 0, dstOffset, count);
}

/// <summary> Copy elements from a Owner at the specified pos to this one,
///           starting from the given offset. </summary>

public void CopyFrom(NativeMemoryOwner<T> src, ulong srcOffset, ulong dstOffset, ulong count)
{
CopyCore(src, this, srcOffset, dstOffset, count);
}

// Copy (signed)

public void CopyFrom(NativeMemoryOwner<T> src, long srcOffset, long dstOffset, long count)
{
var srcOffset64 = ClampIdx(srcOffset);
var dstOffset64 = ClampIdx(dstOffset);
var count64 = ClampIdx(count);

CopyFrom(src, srcOffset64, dstOffset64, count64);
}

/// <summary> Copy all elements from a Span to this Owner </summary>

public void CopyFrom(ReadOnlySpan<T> src) => CopyFrom(src, 0, 0, src.Length);

/// <summary> Copy elements from a Span to this Owner, at the specified pos </summary>

public void CopyFrom(ReadOnlySpan<T> src, ulong dstOffset) => CopyFrom(src, 0, dstOffset, src.Length);

// Copy (signed)

public void CopyFrom(ReadOnlySpan<T> src, long dstOffset) => CopyFrom(src, 0, dstOffset, src.Length);

/// <summary> Copy elements from Span to this Owner at the specified pos,
///           starting at the given offset </summary>

public void CopyFrom(ReadOnlySpan<T> src, ulong dstOffset, int count) => CopyFrom(src, 0, dstOffset, count);

// Copy (signed)

public void CopyFrom(ReadOnlySpan<T> src, long dstOffset, int count) => CopyFrom(src, 0, dstOffset, count);

/// <summary> Copy elements from a Span to this Owner, starting at the given offset
///           and copying count items. </summary>

public void CopyFrom(ReadOnlySpan<T> src, int srcOffset, ulong dstOffset, int count)
{

if(count == 0 || src.Length == 0)
return;

ThrowIfDisposed();

srcOffset = srcOffset < 0 ? 0 : srcOffset;

ArgumentOutOfRangeException.ThrowIfGreaterThan(srcOffset, src.Length);
ArgumentOutOfRangeException.ThrowIfGreaterThan(dstOffset, _size);

int availableSrc = src.Length - srcOffset;

var count64 = count > availableSrc ? (ulong)availableSrc : ClampIdx(count);
ulong required = dstOffset + count64;

if(required > _size)
Realloc(required);

var view = src[srcOffset ..];
ref T first = ref MemoryMarshal.GetReference(view);

void* srcPtr = Unsafe.AsPointer(ref first);
void* dstPtr = _ptr + dstOffset;

ulong bytes = count64 * (ulong)sizeof(T);

Buffer.MemoryCopy(srcPtr, dstPtr, bytes, bytes);
}

// Copy (signed)

public void CopyFrom(ReadOnlySpan<T> src, int srcOffset, long dstOffset, int count)
{
CopyFrom(src, srcOffset, ClampIdx(dstOffset), count);
}

/// <summary> Copy all elements from this Owner to another </summary>

public void CopyTo(NativeMemoryOwner<T> dst) => CopyTo(dst, 0, 0, dst._size);

/// <summary> Copy elements from this Owner to another, starting at the given offset </summary>

public void CopyTo(NativeMemoryOwner<T> dst, ulong srcOffset) => CopyTo(dst, srcOffset, 0, dst._size);

// CopyTo (signed)

public void CopyTo(NativeMemoryOwner<T> dst, long srcOffset)
{
CopyTo(dst, ClampIdx(srcOffset) );
}

/// <summary> Copy elements from this Owner to another starting at the given offset </summary>

public void CopyTo(NativeMemoryOwner<T> dst, ulong srcOffset, ulong count) => CopyTo(dst, srcOffset, 0, count);

// CopyTo (signed)

public void CopyTo(NativeMemoryOwner<T> dst, long srcOffset, long count) => CopyTo(dst, srcOffset, 0, count);

/// <summary> Copy elements from this Owner to another in the specified pos,
///           starting at the given offset </summary>

public void CopyTo(NativeMemoryOwner<T> dst, ulong srcOffset, ulong dstOffset, ulong count)
{
CopyCore(this, dst, srcOffset, dstOffset, count);
}

// CopyTo (signed)

public void CopyTo(NativeMemoryOwner<T> dst, long srcOffset, long dstOffset, long count)
{
var srcOffset64 = ClampIdx(srcOffset);
var dstOffset64 = ClampIdx(dstOffset);
var count64 = ClampIdx(count);

CopyTo(dst, srcOffset64, dstOffset64, count64);
}

/// <summary> Copy elements from this Owner to a Span </summary>

public void CopyTo(Span<T> dst) => CopyTo(dst, 0, 0, dst.Length);

/// <summary> Copy elements from this Owner to a Span starting at the given offset </summary>

public void CopyTo(Span<T> dst, ulong srcOffset) => CopyTo(dst, srcOffset, 0, dst.Length);

// CopyTo (signed)

public void CopyTo(Span<T> dst, long srcOffset) => CopyTo(dst, srcOffset, dst.Length);

/// <summary> Copy elements from this Owner to a Span starting at srcOffset </summary>

public void CopyTo(Span<T> dst, ulong srcOffset, int count) => CopyTo(dst, srcOffset, 0, count);

// CopyTo (signed)

public void CopyTo(Span<T> dst, long srcOffset, int count) => CopyTo(dst, srcOffset, 0, count);

/// <summary> Copy elements from  this Owner to a Span, starting at srcOffset
///            and copying count items. </summary>

public void CopyTo(Span<T> dst, ulong srcOffset, int dstOffset, int count)
{

if(count <= 0 || dst.IsEmpty || _size == 0)
return;

ThrowIfDisposed();

srcOffset = srcOffset < 0 ? 0 : srcOffset;
ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(srcOffset, _size);

ulong availableSrc = _size - srcOffset;
int availableDst = dst.Length - dstOffset;

var count64 = Math.Min(availableSrc, (ulong)Math.Min(availableDst, count) );
ulong bytes = count64 * (ulong)sizeof(T);

fixed(T* dstPtr = dst)
{
void* srcPtr = _ptr + srcOffset;

Buffer.MemoryCopy(srcPtr, dstPtr + dstOffset, bytes, bytes);
}

}

// CopyTo (signed)

public void CopyTo(Span<T> dst, long srcOffset, int dstOffset, int count)
{
CopyTo(dst, ClampIdx(srcOffset), dstOffset, count);
}

/// <summary> Move elements in-place (memmove semantics) </summary>

public void Move(ulong srcOffset, ulong dstOffset, ulong count)
{
bool isEmpty = _size == 0 || count == 0 || srcOffset == dstOffset;
bool invalidMem = _disposed || _ptr == null;

bool outsideBounds = srcOffset >= _size || dstOffset >= _size;

if(isEmpty || invalidMem || outsideBounds)
return;

ulong maxCount = _size - Math.Max(srcOffset, dstOffset);

if(count > maxCount)
count = maxCount;

var src = (byte*)(_ptr + srcOffset);
var dst = (byte*)(_ptr + dstOffset);

var bytes = (nuint)(count * (ulong)sizeof(T) );

// forward copy

if(dst < src)
{
        
for(nuint i = 0; i < bytes; i++)
dst[i] = src[i];

}

// backward copy

else
{
        
for(nuint i = bytes; i-- > 0;)
dst[i] = src[i];

}

}

// memmove (signed)

public void Move(long srcOffset, long dstOffset, long count)
{
var srcOffset64 = ClampIdx(srcOffset);
var dstOffset64 = ClampIdx(dstOffset);
var count64 = ClampIdx(count);

Move(srcOffset64, dstOffset64, count64);
}

// Fill (signed)

public void Fill(T v, long start, long count)
{
var start64 = ClampIdx(start);
var count64 = ClampIdx(count);

Fill(v, start64, count64);
}

/// <summary> Fill all elements with v </summary>

public void Fill(T v, ulong start = 0, ulong count = 0)
{

if(_disposed || _ptr == null || _size == 0)
return;

ulong max = _size - start;
count = count == 0 ? max : Math.Min(count, max);

for(ulong i = 0; i < count; i++)
_ptr[start + i] = v;

}

/// <summary> Clears the allocated memory by setting all bytes to zero. </summary>

public void Clear()
{

if(_disposed || _ptr == null || _size == 0)
return;

var bytes = (nuint)(_size * (ulong)sizeof(T) );
NativeMemory.Clear(_ptr, bytes);
}

/** <summary> Reallocates the memory to a new size. </summary>

<remarks> If the pointer is <c>null</c>, it allocates new memory. </remarks>

<param name="n"> The new size in elements of T </param> **/

public void Realloc(ulong n)
{
var maxAlloc = CapToMaxAllocatable(n);
var sizeInBytes = (nuint)(maxAlloc * (ulong)sizeof(T) );

if(_ptr == null)
_ptr = (T*)NativeMemory.Alloc(sizeInBytes);

else
_ptr = (T*)NativeMemory.Realloc(_ptr, sizeInBytes);

_size = maxAlloc;
}

/// <summary> Realloc (signed) </summary>

public void Realloc(long n) => Realloc(ClampIdx(n) );

#endregion


#region ==========  ACCESORS ==========

// Access element by index

public ref T this[long index] => ref this[ClampIdx(index) ];

// Access element by long index

public ref T this[ulong index]
{

get
{
ThrowIfDisposed();

if(_ptr == null || index >= _size)
throw new IndexOutOfRangeException();

return ref _ptr[index];
}

}

// Get enumerator

public NativeEnumerator<T> GetEnumerator()
{
ThrowIfDisposed();

return new(AsSpan() );
}

// Get element at pos X

public T ElementAt(ulong index)
{
ThrowIfDisposed();

if(_ptr == null || index >= _size)
throw new IndexOutOfRangeException();

return _ptr[index];
}

// ElementAt (signed)

public T ElementAt(long index) => ElementAt(ClampIdx(index) );

// Get first ocurrence

public long IndexOf(T val) => IndexOf(val, 0);

// Get first ocurrence

public long IndexOf(T val, ulong startOffset)
{
var view = MemoryMarshal.CreateSpan(ref val, 1);

return IndexOf(view, startOffset);
}

// IndexOf (signed)

public long IndexOf(T val, long startOffset)
{
return IndexOf(val, ClampIdx(startOffset) );
}

// Get first ocurrence as a Sequence

public long IndexOf(ReadOnlySpan<T> val) => IndexOf(val, 0);

// Get first ocurrence starting from a given Offset

public long IndexOf(ReadOnlySpan<T> val, ulong startOffset)
{

if(val.IsEmpty || _size == 0 || startOffset >= _size)
return -1;

const ulong MAX_CHUNK_SIZE = int.MaxValue; // 2 GB

var needleLength = (ulong)val.Length;

if(_size - startOffset < needleLength)
return -1;

ulong maxStart = _size - needleLength;
ulong offset = startOffset;

while(offset <= maxStart)
{
ulong remaining = _size - offset;
var windowLength = Math.Min(MAX_CHUNK_SIZE, remaining);

if(windowLength < needleLength)
break;

var view = GetView(offset, (int)windowLength);
int localIndex = view.IndexOf(val);

if(localIndex >= 0)
return (long)(offset + (ulong)localIndex);

offset += windowLength - needleLength + 1;
}

return -1;
}

// IndexOf (signed)

public long IndexOf(ReadOnlySpan<T> val, long startOffset)
{
return IndexOf(val, ClampIdx(startOffset) );
}

/** <summary> Gets a whole view from the memory. </summary>

<returns> A ReadOnlySpan with the data </returns> **/

public ReadOnlySpan<T> GetView() => GetView(0, -1);

/** <summary> Gets a view over the allocated memory starting from the specified offset </summary>

<param name="offset"> View offset </param>

<returns> A ReadOnlySpan with the data </returns> **/

public ReadOnlySpan<T> GetView(ulong offset) => GetView(offset, -1);

// GetView (signed)

public ReadOnlySpan<T> GetView(long offset) => GetView(offset, -1);

/** <summary> Gets a view over the allocated memory starting from the specified offset
              and with the desired length. </summary>

<param name="offset"> View offset </param>
<param name="length"> View size </param>

<returns> A ReadOnlySpan with the data </returns> **/

public ReadOnlySpan<T> GetView(ulong offset, int length)
{

if(_ptr == null || _size == 0 || offset >= _size)
return [];

ulong maxLength = _size - offset;

if(length < 0 || (ulong)length > maxLength)
length = (int)Math.Min(maxLength, int.MaxValue);

return new(_ptr + offset, length);
}

// GetView (signed)

public ReadOnlySpan<T> GetView(long offset, int length)
{
return GetView(ClampIdx(offset), length);
}

/** <summary> Creates a span over the whole memory. </summary>

<returns> An Span representing memory data </returns> **/

public Span<T> AsSpan() => AsSpan(0, -1);

/** <summary> Creates a span over the allocated memory starting from the specified offset </summary>

<param name="offset"> Offset to memory </param>

<returns> An Span representing memory data </returns> **/

public Span<T> AsSpan(ulong offset) => AsSpan(offset, -1);

// AsSpan (signed)

public Span<T> AsSpan(long offset) => AsSpan(offset, -1);

/** <summary> Creates a span over the allocated memory starting from the specified offset
              and with the desired length. </summary>

<param name="offset"> Offset to memory </param>
<param name="length"> View size </param>

<returns> An Span representing memory data </returns> **/

public Span<T> AsSpan(ulong offset, int length)
{

if(_ptr == null || _size == 0 || offset >= _size)
return [];

ulong maxLength = _size - offset;

if(length < 0 || (ulong)length > maxLength)
length = (int)Math.Min(maxLength, int.MaxValue);

return new(_ptr + offset, length);
}

// AsSpan (signed)

public Span<T> AsSpan(long offset, int length)
{
return AsSpan(ClampIdx(offset), length);
}

/** <summary> Creates an Array from the whole memory. </summary>

<returns> An array representing memory data </returns> **/

public T[] ToArray() => ToArray(0, -1);

/** <summary> Converts the allocated memory to a managed Array, 
              starting from the specified offset. </summary>

<param name="offset"> Offset to memory </param>

<returns> An array representing memory data </returns> **/

public T[] ToArray(ulong offset) => ToArray(offset, -1);

// ToArray (signed)

public T[] ToArray(long offset) => ToArray(offset, -1);

/** <summary> Converts the allocated memory to a managed Array, starting from the specified offset
              and with the desired length. </summary>

<param name="offset"> Offset to memory </param>
<param name="length"> Array size </param>

<returns> An array representing memory data </returns> **/

public T[] ToArray(ulong offset, int length)
{
Span<T> view = AsSpan(offset, length);
var array = new T[view.Length];

view.CopyTo(array);

return array;
}

// ToArray (signed)

public T[] ToArray(long offset, int length)
{
return ToArray(ClampIdx(offset), length);
}

// Convert to Array directly

public static implicit operator T[](NativeMemoryOwner<T> owner) => owner?.ToArray();

#endregion

// Dispose

public void Dispose()
{
Dispose(true);

GC.SuppressFinalize(this);
}

// Dispose (inner)

private void Dispose(bool disposing)
{

if(_disposed)
return;

if(_ptr != null)
{
NativeMemory.Free(_ptr);

_ptr = null;
_size = 0;
}

_disposed = true;
}

}