using System;
using System.Collections.Generic;

/// <summary> Represents a Interface that Allows Reading and Updating fields without Reflection </summary>

public abstract class FieldMgr<T> : FieldReader<T> where T : class
{
// Store Field Logic for Setting Values

protected readonly Dictionary<int, Action<string>> _fieldSetters = new();

// Register Setter

protected void RegisterSetter(int index, Action<string> setter) => _fieldSetters[index] = setter;

// Setup Setters

protected abstract void InitSetters();

// Init Getter/Setter Logic

public virtual void Init()
{
InitGetters();
InitSetters();
}

}