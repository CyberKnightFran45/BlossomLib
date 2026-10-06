using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

/// <summary> Represents a Table of different Objects. </summary>

public class SexyObjTable
{
/// <summary> Json comment </summary>

[JsonPropertyName("#comment") ]

public string Comment{ get; set; }

/// <summary> File version </summary>

[JsonPropertyName("version") ]

public uint Version{ get; set; } = 1;

/// <summary> List of objects </summary>

[JsonPropertyName("objects") ]

public List<SexyObj> Objects{ get; set; }

// ctor

public SexyObjTable()
{
Objects = new();
}

// ctor 2

public SexyObjTable(List<SexyObj> objs)
{
Objects = objs;
}

// ctor 3

public SexyObjTable(string comment, uint ver, List<SexyObj> objs)
{
Comment = comment;

Version = ver;
Objects = objs;
}

// Check for null Fields

protected virtual void CheckForNullFields() => Objects ??= new();

// Check current Instance

public void CheckObjs() => CheckForNullFields();

/** <summary> Reads a JSON Table. </summary>

<param name = "sourcePath"> The Path where to Read the Table (default is already Set). </param>

<returns> The Table Read. </returns> */

public static SexyObjTable Read(string sourcePath)
{

if(!File.Exists(sourcePath) )
throw new FileNotFoundException($"Missing file: {sourcePath}");

string rawJson = File.ReadAllText(sourcePath);
var jObject = JObject.Parse(rawJson); // Alternative for Unk JSON Struct

var comment = (string)jObject["#comment"];

var ver = (uint?)jObject["version"] ?? 1;
var objs = jObject["objects"]?.ToObject<List<SexyObj>>();

return new(comment, ver, objs);
}

}