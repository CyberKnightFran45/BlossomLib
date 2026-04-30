using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary> Represents a concrete Object in the SexyFramework. </summary>
public abstract class SexyObj<T> where T : class
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

/// <summary> Instance of concrete type containing data </summary>

[JsonPropertyName("objdata") ]

public T ObjData{ get; set; }
}