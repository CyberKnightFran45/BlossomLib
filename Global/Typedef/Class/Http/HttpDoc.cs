using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary> Represents a class that can be Serialized as Http Multipart </summary>

public abstract class HttpDoc<T> : FieldMgr<T> where T : class
{
// Map Http Fields with their Getter/Setter Index

protected Dictionary<string, int> _httpMap = new();

// Register Http Field

protected void RegisterField(string name, int index) => _httpMap[name] = index;

// Setup Http Fields

protected abstract void SetupFields();

// Read fields

protected void Read(MatchCollection fields)
{

foreach(Match field in fields)
{
string fieldName = field.Groups["name"].Value.Trim();

string rawValue = field.Groups["value"].Value.Trim();
string fieldValue = Uri.UnescapeDataString(rawValue);

bool isNameReg = _httpMap.TryGetValue(fieldName, out int fieldIndex);
bool isSetterReg = _fieldSetters.TryGetValue(fieldIndex, out var setter);

if(isNameReg && isSetterReg)
setter(fieldValue);

}

}

// Read Form

public abstract void ReadForm(Stream reader);

// Write Fields

protected void Write(Stream writer, Action<Stream, string, object, bool> writeFunc)
{
bool isFirst = true;

foreach(var kvp in _httpMap)
{
string fieldName = kvp.Key;
int fieldIndex = kvp.Value;

if(_fieldGetters.TryGetValue(fieldIndex, out var getter) )
{
string fieldValue = getter();
writeFunc(writer, fieldName, fieldValue, isFirst);

isFirst = false;
}

}

}

// Write Form

public abstract void WriteForm(Stream writer);

// Init All

public override void Init()
{
base.Init();

SetupFields();
}

}