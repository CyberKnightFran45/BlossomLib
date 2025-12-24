using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

/// <summary> Represents a Table of different Objects. </summary>

public class SexyObjTable
{
/** <summary> Gets or Sets a Comment for this Table. </summary>
<returns> The Json Comment. </returns> */

[JsonPropertyName("#comment") ]

public string Comment{ get; set; }

/** <summary> Gets or Sets the Version of the Table. </summary>
<returns> The Version (Default is 1). </returns> */

[JsonPropertyName("version") ]

public uint Version{ get; set; } = 1;

/** <summary> Gets or Sets a List of Objects for this Table. </summary>
<returns> The List of Objects. </returns> */

[JsonPropertyName("objects") ]

public List<SexyObj> Objects{ get; set; } = new();

// ctor

public SexyObjTable()
{
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
PathHelper.EnsurePathExists(Path.GetDirectoryName(sourcePath) );

if(!File.Exists(sourcePath) || FileManager.FileIsEmpty(sourcePath) )
return null;

var jObject = JObject.Parse(File.ReadAllText(sourcePath) ); // Alternative for Unk JSON Struct

SexyObjTable targetObj = new()
{
Comment = (string)jObject["#comment"],
Version = (uint?)jObject["version"] ?? 1,
Objects = jObject["objects"]?.ToObject<List<SexyObj>>() ?? new()
};

return targetObj;
}

}

/// <summary> Represents a Table of Objects from the same Type. </summary>

public abstract class SexyObjTable<T> where T : class
{
/** <summary> Gets or Sets a Comment for this Table. </summary>
<returns> The Json Comment. </returns> */

[JsonPropertyName("#comment") ]

public string Comment{ get; set; }

/** <summary> Gets or Sets the Version of the Table. </summary>
<returns> The Version (Default is 1). </returns> */

[JsonPropertyName("version") ]

public uint Version{ get; set; } = 1;

/** <summary> Gets or Sets a List of Objects for this Table. </summary>
<returns> The List of Objects. </returns> */

[JsonPropertyName("objects") ]

public List<T> Objects{ get; set; } = new();

// Check for null Fields

public abstract void CheckObjs();
}