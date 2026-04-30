using Parser64 = BlossomLib.Modules.Parsers.Base64;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Buffers;

/** <summary> Optimized JSON stream reader that processes JSON in blocks for better performance.

Handles multi-segment tokens and maintains proper position tracking. </summary> **/

public unsafe class NativeJsonReader : BaseStreamHandler
{
// Buffer

private readonly int _bufferSize;

private readonly NativeMemoryOwner<byte> _buffer;

private long _bytesInBuffer;

private long _consumed;

private long _globalBytePosition;

// Token state

private JsonReaderState _readerState;

private JsonTokenType _currentTokenType;

// Current property/value

private string _currentPropertyName;

private string _currentValue;

private long _tokenGlobalStart;

// Root level

private readonly Stack<JsonTokenType> _stack = new();

private bool _isRootObjectClosing;

/// <summary> Current token type </summary>

public JsonTokenType CurrentTokenType => _currentTokenType;

/// <summary> Indicates if json end is reached or not </summary>

public bool IsJsonEnd => _isRootObjectClosing;

/// <summary> Indicates if reader is inside a child object </summary>

public bool IsInsideObject => _stack.Count > 0 && _stack.Peek() == JsonTokenType.StartObject;

/// <summary> Indicates if reader is inside an array </summary>

public bool IsInsideArray => _stack.Count > 0 && _stack.Peek() == JsonTokenType.StartArray;

/// <summary> Global token position within stream </summary>

public long TokenGlobalPosition => _tokenGlobalStart;

/// <summary> Total bytes consumed by reader </summary>

public long BytesConsumed => _globalBytePosition + _consumed;

// Decimal chars (UTF-8)

private static readonly char[] DECIMAL_CHARS = [ '.', 'e', 'E' ];

// ctor

public NativeJsonReader(Stream baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen)
{
ArgumentNullException.ThrowIfNull(baseStream);

_bufferSize = MemoryManager.GetJsonSize(baseStream);
_buffer = new(_bufferSize);

_readerState = default;
}

// Read raw number

private static string GetRawNumber(ref Utf8JsonReader reader)
{

if(reader.HasValueSequence)
return Encoding.UTF8.GetString(reader.ValueSequence.ToArray() );

return Encoding.UTF8.GetString(reader.ValueSpan);
}

// Get json value as raw string

private static string GetJsonValue(ref Utf8JsonReader reader, JsonTokenType type)
{

return type switch
{
JsonTokenType.String => reader.GetString(),
JsonTokenType.Number => GetRawNumber(ref reader),
_ => null,
};

}

// Sub-nodes control

private void StackNodes()
{

switch(_currentTokenType)
{
case JsonTokenType.StartObject:

case JsonTokenType.StartArray:
_stack.Push(_currentTokenType);

_isRootObjectClosing = false;
break;

case JsonTokenType.EndObject:

if(_stack.Count == 0 || _stack.Pop() != JsonTokenType.StartObject)
throw new JsonException("Invalid JSON: unexpected '}' without matching opening '{'");

_isRootObjectClosing = _stack.Count == 0;
break;

case JsonTokenType.EndArray:

if(_stack.Count == 0 || _stack.Pop() != JsonTokenType.StartArray)
throw new JsonException("Invalid JSON: unexpected ']' without matching opening '['");

_isRootObjectClosing = _stack.Count == 0;
break;

default:
_isRootObjectClosing = false;
break;
}

}

// Updates reader State

private void UpdateState(ref Utf8JsonReader reader)
{
_tokenGlobalStart = _globalBytePosition + _consumed + reader.TokenStartIndex;

_readerState = reader.CurrentState;
_currentTokenType = reader.TokenType;

if(_currentTokenType == JsonTokenType.PropertyName)
_currentPropertyName = reader.GetString();

else
_currentValue = GetJsonValue(ref reader, _currentTokenType);

StackNodes();
}

/// <summary> Read next token from JSON Stream </summary>

public bool ReadToken()
{

while(true)
{

if(_consumed < _bytesInBuffer && TryRead(false) )
return true;

long remaining = _bytesInBuffer - _consumed;

if(remaining > 0 && _consumed > 0)
_buffer.Move(_consumed, 0, remaining);

_globalBytePosition += _consumed;
_bytesInBuffer = remaining;

_consumed = 0;

var chunkSize = (int)(_bufferSize - _bytesInBuffer);
Span<byte> rawJson = _buffer.AsSpan(_bytesInBuffer, chunkSize);

int bytesRead = BaseStream.Read(rawJson);

if(bytesRead == 0)
{

if(_bytesInBuffer == 0)
return false;

if(TryRead(true) )
return true;

throw new JsonException("JSON is incomplete or improperly terminated");
}

_bytesInBuffer += bytesRead;
}

}

// Read Raw Bytes as JSON (in blocks)

private bool TryRead(bool isFinalBlock)
{
var newChunkSize = (int)(_bytesInBuffer - _consumed);

if(newChunkSize <= 0)
return false;

var span = _buffer.AsSpan( (ulong)_consumed, newChunkSize);

Utf8JsonReader reader = new(span, isFinalBlock, _readerState);

if(!reader.Read() )
{
_readerState = reader.CurrentState;

return false;
}

long consumedInReader = reader.BytesConsumed;
UpdateState(ref reader);

_consumed += consumedInReader;

return true;
}

// Check token type

private void EnsureToken(params JsonTokenType[] expected)
{

if(!expected.Contains(_currentTokenType) )
{
string displayExpected = string.Join(" | ", expected);
string msg = $"Invalid token: {_currentTokenType} @ {_tokenGlobalStart} (Expected: {displayExpected})";

throw new InvalidOperationException(msg);
}

}

/// <summary> Get current property name </summary>

public string GetPropertyName()
{

if(_currentTokenType != JsonTokenType.PropertyName)
throw new InvalidOperationException($"Current token is not a PropertyName: {_currentTokenType}");

return _currentPropertyName;
}

/// <summary> Get raw value as text </summary>

public string GetRawValue() =>  _currentValue;

/// <summary> Cast current token as a <c>boolean</c> </summary>

public bool GetBoolean()
{
EnsureToken(JsonTokenType.True, JsonTokenType.False);

return _currentTokenType == JsonTokenType.True;
}

/// <summary> Cast current token as a <c>sbyte</c> </summary>

public sbyte GetInt8()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToSByte(_currentValue);
}

/// <summary> Cast current token as a <c>short</c> </summary>

public short GetInt16()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToInt16(_currentValue);
}

/// <summary> Cast current token as an <c>int</c> </summary>

public int GetInt32()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToInt32(_currentValue);
}

/// <summary> Cast current token as a <c>long</c> </summary>

public long GetInt64()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToInt64(_currentValue);
}

/// <summary> Cast current token as a <c>byte</c> </summary>

public byte GetUInt8()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToByte(_currentValue);
}

/// <summary> Cast current token as a <c>ushort</c> </summary>

public ushort GetUInt16()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToUInt16(_currentValue);
}

/// <summary> Cast current token as a <c>uint</c> </summary>

public uint GetUInt32()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToUInt32(_currentValue);
}

/// <summary> Cast current token as a <c>ulong</c> </summary>

public ulong GetUInt64()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToUInt64(_currentValue);
}

/// <summary> Cast current token as a <c>float</c> </summary>

public float GetFloat()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToSingle(_currentValue);
}

/// <summary> Cast current token as a <c>double</c> </summary>

public double GetDouble()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToDouble(_currentValue);
}

/// <summary> Cast current token as a <c>decimal</c> </summary>

public decimal GetDecimal()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToDecimal(_currentValue);
}

/// <summary> Cast current token as a <c>string</c> </summary>

public string GetString()
{
EnsureToken(JsonTokenType.String);

return _currentValue;
}

/// <summary> Cast current token as a <c>DateTime</c> </summary>

public DateTime GetDateTime()
{
EnsureToken(JsonTokenType.String);

return DateTime.Parse(_currentValue);
}

/// <summary> Cast current token as a <c>DateTimeOffset</c> </summary>

public DateTimeOffset GetDateTimeOffset()
{
EnsureToken(JsonTokenType.String);

return DateTimeOffset.Parse(_currentValue);
}

/// <summary> Cast current token as a <c>Guid</c> </summary>

public Guid GetGuid()
{
EnsureToken(JsonTokenType.String);

return Guid.Parse(_currentValue);
}

/// <summary> Convert current token from base64 to raw bytes </summary>

public NativeMemoryOwner<byte> GetBytesFromBase64(bool isWebSafe = false)
{
EnsureToken(JsonTokenType.String);

return Parser64.DecodeString(_currentValue, isWebSafe);
}

/// <summary> Check if current token is <c>null</c> </summary>

public bool IsNull() => _currentTokenType == JsonTokenType.Null;

/// <summary> Check if current token is a PropertyName </summary>

public bool IsPropertyName => _currentTokenType == JsonTokenType.PropertyName;

/// <summary> Check if current node is a decimal number </summary>

public bool IsDecimal()
{

if(_currentTokenType != JsonTokenType.Number)
return false;

return _currentValue.IndexOfAny(DECIMAL_CHARS) >= 0;
}

/// <summary> Check if current node is a negative number </summary>

public bool IsNegativeNumber()
{

if(_currentTokenType != JsonTokenType.Number)
return false;

return _currentValue.Length > 0 && _currentValue[0] == '-';
}

/// <summary> Validates if json struct is balanced (no objects/arrays unterminated) </summary>

public void ValidateStructure()
{

if(_stack.Count != 0)
{
var openTypes = string.Join(", ", _stack.Select(t => t.ToString() ) );

throw new JsonException($"JSON structure is unbalanced @ {BytesConsumed}. Open tokens: {openTypes}");
}

}

/// <summary> Validates that json is fully processed </summary>

public bool IsComplete => _isRootObjectClosing && _stack.Count == 0;

// Release resources

protected override void Dispose(bool disposing)
{

if(disposing)
{
_buffer?.Dispose();
_stack?.Clear(); 
}

base.Dispose(disposing);
}

}