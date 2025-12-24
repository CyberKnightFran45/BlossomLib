using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// Allows reading JSON Streams in blocks for faster performance

public unsafe class NativeJsonReader : BaseStreamHandler
{
private readonly int _bufferSize;
private NativeMemoryOwner<byte> _buffer;

private long _bytesInBuffer;
private long _consumed;

private JsonReaderState _readerState;
private JsonTokenType _currentTokenType;

private object _currentValue;
private string _currentPropertyName;

private readonly Stack<JsonTokenType> _stack = new();
private bool _isRootObjectClosing;

public JsonTokenType CurrentTokenType => _currentTokenType;
public string CurrentPropertyName => _currentPropertyName;

public bool IsJsonEnd => _isRootObjectClosing;

public bool IsInsideObject => _stack.Count > 0 && _stack.Peek() == JsonTokenType.StartObject;

public bool IsInsideArray => _stack.Count > 0 && _stack.Peek() == JsonTokenType.StartArray;

// ctor

public NativeJsonReader(Stream baseStream, bool leaveOpen = false)
: base(baseStream, leaveOpen)
{
_bufferSize = MemoryManager.GetJsonSize(baseStream);

_buffer = new(_bufferSize);
_readerState = default;
}

// Updates reader State

private void UpdateState(ref Utf8JsonReader reader)
{
_consumed += reader.BytesConsumed;

_readerState = reader.CurrentState;
_currentTokenType = reader.TokenType;

// Read Json value

_currentValue = _currentTokenType switch
{
JsonTokenType.PropertyName => reader.GetString(),
JsonTokenType.String => reader.GetString(),
JsonTokenType.Number => reader.TryGetInt64(out long i64) ? i64 : reader.GetDouble(),
JsonTokenType.True => true,
JsonTokenType.False => false,
_ => null
};

if(_currentTokenType == JsonTokenType.PropertyName)
_currentPropertyName = (string)_currentValue;

// Sub-nodes control

switch(_currentTokenType)
{
case JsonTokenType.StartObject:

case JsonTokenType.StartArray:
_stack.Push(_currentTokenType);

_isRootObjectClosing = false;
break;

case JsonTokenType.EndObject:

if(_stack.Count == 0 || _stack.Pop() != JsonTokenType.StartObject)
throw new JsonException("Invalid JSON: unexpected '}'");

_isRootObjectClosing = _stack.Count == 0;
break;

case JsonTokenType.EndArray:

if(_stack.Count == 0 || _stack.Pop() != JsonTokenType.StartArray)
throw new JsonException("Invalid JSON: unexpected ']'");

_isRootObjectClosing = false;
break;

default:
_isRootObjectClosing = false;
break;
}

}

// Read next Token

public bool ReadToken()
{

while(true)
{

if(_consumed < _bytesInBuffer && TryRead(false))
return true;

long remaining = _bytesInBuffer - _consumed;

if(remaining > 0 && _consumed > 0)
_buffer.Move((ulong)_consumed, 0, (ulong)remaining);

_bytesInBuffer = remaining;
_consumed = 0;

var chunkSize = (int)(_bufferSize - _bytesInBuffer);
Span<byte> rawJson = _buffer.AsSpan( (ulong)_bytesInBuffer, chunkSize);

int bytesRead = BaseStream.Read(rawJson);

if(bytesRead == 0)
{

if(_bytesInBuffer == 0)
return false;

if(TryRead(true) )
return true;

throw new JsonException("JSON is incomplete or improperly terminated.");
}

_bytesInBuffer += bytesRead;
}

}

// Read Raw Bytes as JSON (in blocks)

private bool TryRead(bool isFinalBlock)
{
var newChunkSize = (int)(_bytesInBuffer - _consumed);
var span = _buffer.AsSpan((ulong)_consumed, newChunkSize);

Utf8JsonReader reader = new(span, isFinalBlock, _readerState);

if(!reader.Read() )
{
_readerState = reader.CurrentState;

return false;
}

UpdateState(ref reader);

return true;
}

// Get BaseReader without consuming Tokens

private Utf8JsonReader PeekReader()
{
int remaining = (int)(_bytesInBuffer - _consumed);
var view = _buffer.AsSpan( (ulong)_consumed, remaining);

return new(view, true, _readerState);
}

// Count Array Elements

public int CountArrayElements()
{
var preview = PeekReader();

if(preview.TokenType != JsonTokenType.StartArray)
throw new JsonException("Reader is not positioned at StartArray.");

int count = 0;
int targetDepth = preview.CurrentDepth;

while(preview.Read() )
{

if(preview.TokenType == JsonTokenType.EndArray && preview.CurrentDepth == targetDepth)
break;

switch(preview.TokenType)
{
case JsonTokenType.StartObject:

case JsonTokenType.StartArray:
count++;

int depth = preview.CurrentDepth;

while(preview.Read() && preview.CurrentDepth > depth)
{
// Skip inner elements
}

break;

case JsonTokenType.PropertyName:

case JsonTokenType.Comment:
break; // Ignore PropertyNames and Comments

default:
count++;
break;
}

}

return count;
}

// Check token type

private void EnsureToken(params JsonTokenType[] expected)
{
if(!expected.Contains(_currentTokenType) )
	
throw new InvalidOperationException($"Invalid token '{_currentTokenType}', expected: {string.Join(", ", expected)}.");
}

// Cast token as string

public string GetString()
{
EnsureToken(JsonTokenType.String);

return _currentValue as string ?? throw new InvalidOperationException("Current token is not a string.");
}

// Cast token as int

public int GetInt32()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToInt32(_currentValue);
}

// Cast token as long

public long GetInt64()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToInt64(_currentValue);
}

// Cast token as double

public double GetDouble()
{
EnsureToken(JsonTokenType.Number);

return Convert.ToDouble(_currentValue);
}

// Cast token as bool

public bool GetBoolean()
{
EnsureToken(JsonTokenType.True, JsonTokenType.False);

return (bool)_currentValue!;
}

// Check if Token is Null

public bool IsNull() => _currentTokenType == JsonTokenType.Null;

// Open from Disk

public static NativeJsonReader Open(string path) => new(FileManager.OpenRead(path));

// Check remaining struct

public void ValidateStructure()
{

if(_stack.Count != 0)
throw new JsonException("JSON structure is unbalanced (open arrays/objects remain).");

}

}