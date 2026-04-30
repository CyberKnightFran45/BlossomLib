using System;

/// <summary> Represents a Limit that an Input should follow. </summary>
/// <param name="min"> The Minimum Range of the Limit. </param>
/// <param name="max"> The Maximum Range of the Limit. </param>

public class Limit<T>(T min, T max) where T : struct, IComparable<T>
{
/// <summary> Gets the Minimum Value of a Limit. </summary>

public T MinValue{ get; set; } = min;

/// <summary> Gets the Maximum Value of a Limit. </summary>

public T MaxValue{ get; set; } = max;

/// <summary> Creates a new Instance of the <c>Limit</c> Record. </summary>

public Limit() : this(default, default) { }

/// <summary> Creates a new Instance of the <c>Limit</c> Record with the given Range. </summary>

/// <param name="sourceRange"> The Range to be Applied. </param>

public Limit(T range) : this(range, range) { }

/// <summary> Checks if a Parameter is inside the Specified Range. </summary>

/// <param name="target"> The Parameter to be Analyzed. </param>

public bool IsParamInRange(T target)
{
return target.CompareTo(MinValue) >= 0 && target.CompareTo(MaxValue) <= 0;
}

/// <summary> Checks and adjusts a Parameter to be within the Specified Range. </summary>

/// <param name="target"> The Parameter to be Analyzed. </param>

public T CheckParamRange(T target) => IsParamInRange(target) ? target : MinValue;

/// <summary> Gets the Range of a Limit Instance based on the Type. </summary>

public static Limit<T> GetRange()
{

return typeof(T).Name switch
{
"Boolean" => new Limit<T>((T)(object)false, (T)(object)true),
"Byte" => new Limit<T>((T)(object)byte.MinValue, (T)(object)byte.MaxValue),
"SByte" => new Limit<T>((T)(object)sbyte.MinValue, (T)(object)sbyte.MaxValue),
"Int16" => new Limit<T>((T)(object)short.MinValue, (T)(object)short.MaxValue),
"UInt16" => new Limit<T>((T)(object)ushort.MinValue, (T)(object)ushort.MaxValue),
"Int32" => new Limit<T>((T)(object)int.MinValue, (T)(object)int.MaxValue),
"UInt32" => new Limit<T>((T)(object)uint.MinValue, (T)(object)uint.MaxValue),
"Int64" => new Limit<T>((T)(object)long.MinValue, (T)(object)long.MaxValue),
"UInt64" => new Limit<T>((T)(object)ulong.MinValue, (T)(object)ulong.MaxValue),
"Single" => new Limit<T>((T)(object)float.MinValue, (T)(object)float.MaxValue),
"Double" => new Limit<T>((T)(object)double.MinValue, (T)(object)double.MaxValue),
"DateTime" => new Limit<T>((T)(object)DateTime.MinValue, (T)(object)DateTime.MaxValue),
_ => new Limit<T>()
};

}

}