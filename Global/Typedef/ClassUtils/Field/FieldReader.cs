using System;
using System.Collections.Generic;

/// <summary> Represents a Interface that Allows Reading fields without Reflection </summary>

public abstract class FieldReader<T> where T : class
{
// Store Field Logic for Getting Values

protected readonly Dictionary<int, Func<string>> _fieldGetters = new();

// Register Getter

protected void RegisterGetter(int index, Func<string> getter) => _fieldGetters[index] = getter;

// Register Random (Default)

protected void RegisterGetter(int index)
{
static string getter() => "";

RegisterGetter(index, getter);
}

// Register Random Getter

protected void RegisterGetterRnd(int index)
{
string randomStr = InputHelper.GenRandomStr(10);
string getter() => randomStr;

RegisterGetter(index, getter);
}

// Setup Getters

protected abstract void InitGetters();
}