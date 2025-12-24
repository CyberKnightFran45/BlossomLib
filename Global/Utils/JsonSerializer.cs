using NetSerializer = System.Text.Json.JsonSerializer;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Newtonsoft.Json.Linq;

/// <summary> Initializes serializing Functions for JSON Data. </summary>

public static partial class JsonSerializer
{
// Create new JsonSerializerOptions Instance (Context freezes options)

public static JsonSerializerOptions Options => new()
{
NumberHandling = JsonNumberHandling.Strict,
PropertyNameCaseInsensitive = true,
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
ReadCommentHandling = JsonCommentHandling.Skip,
AllowTrailingCommas = false,
WriteIndented = true,
Converters = { new JsonStringEnumConverter() },
Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
PropertyNamingPolicy = null
};

private static readonly JsonSerializerOptions _options = Options;

/** <summary> Cleans a JSON String by removing unnecessary characters and formatting it. </summary>

<param name="sourceStr"> The String to be cleaned. </param> */

public static void CleanJsonString(ref string sourceStr)
{
InputHelper.RemoveLiteralChars(ref sourceStr);

sourceStr = sourceStr.Replace("\"{", "{").Replace("}\"", "}");

sourceStr = JsonPropRegex().Replace(sourceStr, "\"$1\"");
sourceStr = JsonArrayRegex().Replace(sourceStr, "\"$1\"");
}

// Evluate Str Type

private static object EvaluateString(string str)
{
CleanJsonString(ref str);

if(IsSpecialString(str) )
return str;

else if(IsJson(str) )
return ParseJson(str);

else if(IsJsonArray(str) )
return ParseArray(str);

return str;
}

// Process Tokens

private static object ParseJElement(JsonElement element)
{

return element.ValueKind switch
{
JsonValueKind.Object => ParseJson(element.GetRawText() ),
JsonValueKind.Array => ParseArray(element.GetRawText() ),
JsonValueKind.String => EvaluateString(element.GetString() ),
JsonValueKind.Number => element.GetDecimal(),
JsonValueKind.True or JsonValueKind.False  => element.GetBoolean(),
_ => null
};

}

// Parse Json as Expando

public static dynamic ParseExpando(string str)
{
var jsonData = JToken.Parse(str);

if(jsonData.Type == JTokenType.Array)
return ExpandObjPlugin.ConvertJArray(jsonData.ToObject<JArray>() );

return ExpandObjPlugin.ToExpandoObject(jsonData.ToObject<JObject>() );
}

/// <summary> Parses a JSON String into a Dictionary. </summary>
/// <param name="jsonStr">The string to parse</param>
/// <returns>The object parsed as a dictionary </returns>

public static Dictionary<string, object> ParseJson(string jsonStr)
{
CleanJsonString(ref jsonStr);

using var jsonDoc = JsonDocument.Parse(jsonStr);
Dictionary<string, object> result = new();

foreach(var property in jsonDoc.RootElement.EnumerateObject() )
result.Add(property.Name, ParseJElement(property.Value) );

return result;
}

/// <summary> Parses a JSON String into a List. </summary>
/// <param name="jsonStr">The string to parse</param>
/// <returns>The object parsed as a list</returns>

public static List<object> ParseArray(string jsonStr)
{
CleanJsonString(ref jsonStr);

using var jsonDoc = JsonDocument.Parse(jsonStr);
List<object> result = new();

foreach(var element in jsonDoc.RootElement.EnumerateArray() )
result.Add(ParseJElement(element) );

return result;
}

/// <summary> Checks if a String is a valid JSON Object. </summary>
/// <param name="str"> The String to check. </param>
/// <returns> True if the String is a valid JSON Object, otherwise false. </returns>

public static bool IsJson(string str)
{
CleanJsonString(ref str);

try
{
using var jsonDoc = JsonDocument.Parse(str);

return jsonDoc.RootElement.ValueKind == JsonValueKind.Object;
}

catch(JsonException)
{
return false;
}

}

/// <summary> Checks if a String is a special JSON String (e.g., "[name]"). </summary>
/// <param name="str"> The String to check. </param>
/// <returns> True if the String is a special JSON String, otherwise false. </returns>

public static bool IsSpecialString(string str) => SpecialStrRegex().IsMatch(str);

/// <summary> Checks if a String is a valid JSON Array. </summary>
/// <param name="str">The String to check</param>
/// <returns>True if the String is an JSON Array, false otherwise</returns>

public static bool IsJsonArray(string str)
{
InputHelper.RemoveLiteralChars(ref str);

try
{
using var jsonDoc = JsonDocument.Parse(str);

return jsonDoc.RootElement.ValueKind == JsonValueKind.Array;
}

catch(JsonException)
{
return false;
}

}

/** <summary> Serializes a Object as a JSON String. </summary>

<returns> The Object serialized. </returns> */

public static string SerializeObject<T>(T obj, JsonSerializerContext context = null)
{

if(obj == null)
return "";

var typeInfo = context?.GetTypeInfo(typeof(T) );

if(typeInfo == null)
return NetSerializer.Serialize(obj, _options);

return NetSerializer.Serialize(obj, typeInfo);
}

/// <summary> Serializes a Object to a Stream. </summary>

public static void SerializeObject<T>(T obj, Stream writer, JsonSerializerContext context = null)
{

if(obj == null)
return;

var typeInfo = context?.GetTypeInfo(typeof(T) );

if(typeInfo == null)
NetSerializer.Serialize(writer, obj, _options);

else
NetSerializer.Serialize(writer, obj, typeInfo);

}

/** <summary> Deserializes a JSON String as a Object. </summary>

<param name = "json"> The String to Deserialize. </param>

<returns> The deserialized Object. </returns> */

public static T DeserializeObject<T>(string json, JsonSerializerContext context = null)
{

if(string.IsNullOrWhiteSpace(json) )
return default;

var typeInfo = context?.GetTypeInfo(typeof(T) );

if(typeInfo == null)
return NetSerializer.Deserialize<T>(json, _options);

return (T)NetSerializer.Deserialize(json, typeInfo);
}

/** <summary> Deserializes a JSON String as a Object. </summary>

<param name = "json"> The String to Deserialize. </param>

<returns> The deserialized Object. </returns> */

public static T DeserializeObject<T>(Stream reader, JsonSerializerContext context = null)
{
var typeInfo = context?.GetTypeInfo(typeof(T) );

if(typeInfo == null)
return NetSerializer.Deserialize<T>(reader, _options);

return (T)NetSerializer.Deserialize(reader, typeInfo);
}

[GeneratedRegex(@"(?<![""\\])(?<=\{|\s|,)([a-zA-Z_][a-zA-Z0-9_]*)(?=\s*:)") ]

private static partial Regex JsonPropRegex();

[GeneratedRegex(@"(?<=\[|\s)(\w+)(?=,|\s|\])") ]

private static partial Regex JsonArrayRegex();

[GeneratedRegex(@"^\[[a-zA-Z][a-zA-Z0-9_]*\]$") ]

private static partial Regex SpecialStrRegex();
}