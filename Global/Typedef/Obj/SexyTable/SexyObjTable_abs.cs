using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary> Represents a Table of Objects from the same Type. </summary>

public abstract class SexyObjTable<T> where T : class
{
/// <summary> Json comment </summary>

[JsonPropertyName("#comment") ]

public string Comment{ get; set; }

/// <summary> File version </summary>

[JsonPropertyName("version") ]

public uint Version{ get; set; } = 1;

/// <summary> List of objects </summary>

[JsonPropertyName("objects") ]

public List<T> Objects{ get; set; }

// Check for null Fields

public abstract void CheckObjs();
}