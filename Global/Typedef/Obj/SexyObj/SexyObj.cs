using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

/// <summary> Represents a generic Object in the SexyFramework. </summary>

public class SexyObj
{
/// <summary> Json comment </summary>

[JsonPropertyName("#comment") ]

public string Comment{ get; set; }

/// <summary> Object uid </summary>

[JsonPropertyName("uid") ]

public string Uid{ get; set; }
    
/// <summary> Aliases for this object (Optional) </summary>

[JsonPropertyName("aliases") ]

public List<string> Aliases{ get; set; }

/// <summary> Object class name </summary>

[JsonPropertyName("objclass") ]

public string ObjClass{ get; set; }

/// <summary> Set of properties and values for this object </summary>

[JsonPropertyName("objdata") ]

public ExpandoObject ObjData{ get; set; }

// ctor

public SexyObj()
{
ObjClass = "MyClassTemplate";
ObjData = new();
}

// ctor 2

public SexyObj(string comment, List<string> aliases, string objClass)
{
Comment = comment;

Aliases = aliases;
ObjClass = objClass;
}

// ctor 3

public SexyObj(string comment, List<string> aliases, string objClass, ExpandoObject objData)
{
Comment = comment;
Aliases = aliases;

ObjClass = objClass;
ObjData = objData;
}

/** <summary> Reads a JSON Object. </summary>

<param name = "sourcePath"> The Path where to Read the Obj </param>

<returns> The Obj Read. </returns> */

public static SexyObj Read(string sourcePath)
{

if(!File.Exists(sourcePath) )
throw new FileNotFoundException($"Missing file: {sourcePath}");

string rawJson = File.ReadAllText(sourcePath);
var jObject = JObject.Parse(rawJson); // Alternative for Unk JSON Struct

var comment = (string)jObject["#comment"];
var aliases = jObject["aliases"]?.ToObject<List<string>>();

var objClass = jObject["objclass"]?.ToObject<string>();

var rawObj = jObject["objdata"]?.ToObject<JObject>();
var objData = ExpandObjPlugin.ToExpandoObject(rawObj);

return new(comment, aliases, objClass, objData);
}

}