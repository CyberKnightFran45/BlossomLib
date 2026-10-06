using System;
using System.Linq;
using System.Collections.Generic;

/// <summary> Helper used for making Signatures from inherited Class fields. </summary>

public abstract class SignHelper<T> where T : class
{
// Map fields to Digest

protected readonly Dictionary<int, Func<string>> _targetFields = new();

// Register field

public void AddTarget(int index, Func<string> getter) => _targetFields[index] = getter;

// Get Signature field

protected Func<string> _getSignature;

// Set Signature field

protected Action<string> _setSignature;

// Init Getter

public void InitGetter(Func<string> getter) => _getSignature = getter;

// Init Setter

public void InitSetter(Action<string> setter) => _setSignature = setter;

/** <summary> Concatenates all the registered Fields to Produce the raw content for signing. </summary>

<returns> Raw concatenated string from getters. </returns> */

protected virtual NativeMemoryOwner<char> GetContent()
{

if(_targetFields.Count == 0)
return new();

var values = _targetFields.Select(kvp => kvp.Value.Invoke() );
int totaLen = values.Sum(v => v.Length);

NativeMemoryOwner<char> buffer = new(totaLen);
int pos = 0;

foreach(var str in values)
{
buffer.CopyFrom(str, pos);

pos += str.Length;
}

buffer.Realloc(pos);

return buffer;
}

/** <summary> Concatenates all the registered field getters to Produce the raw content for signing. </summary>

<returns> Raw concatenated string from getters. </returns> */

protected abstract NativeMemoryOwner<char> Sign();

/// <summary> Checks the signature against the current raw content and updates Sign if necessary. </summary>

public void CheckSign()
{
string oldSign = _getSignature();

using var sOwner = Sign();
string newSign = new(sOwner.AsSpan() );

if(string.IsNullOrWhiteSpace(oldSign) || oldSign != newSign)
_setSignature(newSign);

}

}