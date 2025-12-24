using System;
using System.Globalization;

/// <summary> Represents a CharPtr that can be casted to a <c>string</c> </summary>

public unsafe sealed class NativeString : NativeMemoryOwner<char>
{
/// <summary> Gets the String Length. </summary>

public long Length => (long)Math.Clamp(_size, 0, long.MaxValue);

/// <summary> Initializes a empty <see cref="NativeString"/>  </summary>

public NativeString()
{
}

/** <summary> Initializes a new <see cref="NativeString"/> with n chars. </summary>

<param name="chars"> The amount of chars. </param> **/
	
public NativeString(long chars) : base(chars)
{
}

/** <summary> Initializes a new <see cref="NativeString"/> with the specified length. </summary>

<param name="length"> The string length </param> **/

public NativeString(ulong length) : base(length)
{
}

// Checks if string is null or Empty

public bool IsNullOrEmpty() => _ptr == null || _size == 0 || _ptr[0] == '\0';

// Check if char is whitespace

private static bool IsWhiteSpace(char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

// Checks if string is null or whitespace

public bool IsNullOrWhiteSpace()
{

if(IsNullOrEmpty() )
return true;

for(ulong i = 0; i < _size; i++)
{
char c = _ptr[i];

if(!IsWhiteSpace(c) )
return false;

}

return true;
}

// Remove whitespace (in-place)

public void Trim()
{

if(_ptr == null || _size == 0)
return;

ulong start = 0;
ulong end = _size;

while(start < end && IsWhiteSpace(_ptr[start]) )
start++;

while(end > start && IsWhiteSpace(_ptr[end - 1]) )
end--;

ulong newLen = end - start;

if(newLen > 0 && start > 0)
Move(start, 0, newLen);

_size = newLen;
}

/** <summary> Gets a managed String from the specified offset </summary>

<param name="offset"> String offset </param>

<returns> A managed string </returns> **/

public string Substring(ulong offset) => Substring(offset, -1);

// Substring (signed)

public string Substring(long offset) => Substring(offset, -1);

/** <summary> Gets a managed String from the specified offset and with the desired length </summary>

<param name="offset"> String offset </param>
<param name="length"> String length </param>

<returns> A managed string </returns> **/

public string Substring(ulong offset, int length)
{
var strView = GetView(offset, length);

return new(strView);
}

// Substring (signed)

public string Substring(long offset, int length)
{
var strView = GetView(offset, length);

return new(strView);
}

// Convert to UPPERCASE (in-place)

public void ToUpper()
{

for(ulong i = 0; i < _size; i++)
_ptr[i] = char.ToUpperInvariant(_ptr[i] );

}

// Convert to lowercase (in-place)

public void ToLower()
{

for(ulong i = 0; i < _size; i++)
_ptr[i] = char.ToLowerInvariant(_ptr[i] );

}

#region ==========  COMPARER  =========

// Compare NativeString with Span

public static int Compare(NativeString a, ReadOnlySpan<char> b, StringComparison cmp)
{

if(a == null && b.IsEmpty)
return 0;

if(a == null)
return -1;

if(b.IsEmpty)
return 1;

var viewA = a.GetView();

return viewA.CompareTo(b, cmp);
}

// Compare two Native Strings

public static int Compare(NativeString a, NativeString b, StringComparison cmp)
{

if(a == null && b == null)
return 0;

if(a == null)
return -1;

if(b == null)
return 1;

var viewA = a.GetView();
var viewB = b.GetView();

return viewA.CompareTo(viewB, cmp);
}

// Compare current instance with Span

public int CompareTo(ReadOnlySpan<char> other, StringComparison cmp) => Compare(this, other, cmp);

// Compare current instance with another

public int CompareTo(NativeString other, StringComparison cmp) => Compare(this, other, cmp);

// Check if this String starts with the given prefix

public bool StartsWith(ReadOnlySpan<char> prefix)
{
var view = GetView();

return view.StartsWith(prefix);
}

// Check if this String starts with the given prefix

public bool StartsWith(ReadOnlySpan<char> prefix, StringComparison cmp)
{
var view = GetView();

return view.StartsWith(prefix, cmp);
}

// Check if this String starts with the given prefix

public bool StartsWith(NativeString other)
{
var prefix = other.GetView();

return StartsWith(prefix);
}

// Check if this String starts with the given prefix

public bool StartsWith(NativeString other, StringComparison cmp)
{
var prefix = other.GetView();

return StartsWith(prefix, cmp);
}

// Check if this String ends with the given suffix

public bool EndsWith(ReadOnlySpan<char> suffix)
{
var view = GetView();

return view.EndsWith(suffix);
}

// Check if this String ends with the given suffix

public bool EndsWith(ReadOnlySpan<char> suffix, StringComparison cmp)
{
var view = GetView();

return view.EndsWith(suffix, cmp);
}

// Check if this String ends with the given suffix

public bool EndsWith(NativeString other)
{
var suffix = other.GetView();

return EndsWith(suffix);
}

// Check if this String ends with the given prefix

public bool EndsWith(NativeString other, StringComparison cmp)
{
var suffix = other.GetView();

return EndsWith(suffix, cmp);
}

#endregion

// Convert to string

public override string ToString() => Substring(0, -1);

// Direct cast

public static implicit operator string(NativeString native) => native?.ToString();
}