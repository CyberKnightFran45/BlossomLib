using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;

public static partial class ExpandObjPlugin
{
// Change Object to its proper Type

private static Dictionary<string, object> ConvertToCorrectTypes(IDictionary<string, object> dictionary)
{
Dictionary<string, object> result = new();

foreach(var kvp in dictionary)      
result[kvp.Key] = ConvertValue(kvp.Value);

return result;
}

// Convert a obj to a C# Value

private static object ConvertValue(object value)
{

if(value == null)
return null;

else if(value.Equals("") )
return string.Empty;

else if(value is ExpandoObject expando)
return ConvertToCorrectTypes(expando);

else if(value is IDictionary<string, object> dict)     
return ConvertToCorrectTypes(dict);

else if(value is IList<object> list)
return list.Select(ConvertValue).ToList();

else if(value is Array array)
{
var convertedArray = Array.CreateInstance(array.GetType().GetElementType(), array.Length);

for(int i = 0; i < array.Length; i++)      
convertedArray.SetValue(ConvertValue(array.GetValue(i) ), i);
        
return convertedArray;
}

else if(value is string str)
{
JsonSerializer.CleanJsonString(ref str);

if(int.TryParse(str, out int intValue) )
return intValue;

else if(float.TryParse(str, out float floatValue) )
return floatValue;

else if(bool.TryParse(str, out bool boolValue) )
return boolValue;

else if(JsonSerializer.IsSpecialString(str) )
return str;

else if(JsonSerializer.IsJson(str) )
return JsonSerializer.ParseJson(str);

else if(JsonSerializer.IsJsonArray(str) )
return JsonSerializer.ParseArray(str);

}

return value;
}

// Convert Dictionary to ExpandoObject

public static ExpandoObject ToExpandoObject(IDictionary<string, object> dictionary)
{

if(dictionary == null)
return null;

ExpandoObject expando = new();
var expandoDict = (IDictionary<string, object>)expando;

foreach(var kvp in dictionary)       
expandoDict[kvp.Key] = kvp.Value;

return expando;
}

// Parse Json Array

public static List<object> ConvertJArray(JArray array)
{
List<object> list = new();

foreach (var item in array)
{

if(item is JObject)
list.Add(ToExpandoObject(item as JObject) );
            
else if (item is JArray)
list.Add(ConvertJArray(item as JArray) );
            
else
list.Add(item.ToObject<object>() );
     
}

return list;
}

// Parse JSON Object

public static ExpandoObject ToExpandoObject(JObject jObject)
{

if(jObject == null)
return null;

var expandoObject = new ExpandoObject() as IDictionary<string, object>;

foreach(var kvp in jObject)
{
var value = kvp.Value;

if(value is JObject)
expandoObject[kvp.Key] = ToExpandoObject(value as JObject);
            
else if (value is JArray array)
expandoObject[kvp.Key] = ConvertJArray(array);
         
else
expandoObject[kvp.Key] = value.ToObject<object>();
         
}

return (ExpandoObject)expandoObject;
}

// Correct fields using Strict Typing in obj

public static ExpandoObject CorrectTypes(this ExpandoObject obj)
{
var dictionary = (IDictionary<string, object>)obj;

var correctedDict = ConvertToCorrectTypes(dictionary);

return ToExpandoObject(correctedDict);
}

// Compare two ExpandoObjects

public static bool Compare(this ExpandoObject oldObj, ExpandoObject newObj, bool getObjChanges,
bool getAddedProps, out ExpandoObject diff)
{
diff = new();

var alterDict = (IDictionary<string, object>)diff;

var oldDict = (IDictionary<string, object>)oldObj;
var newDict = (IDictionary<string, object>)newObj;

HashSet<string> keys = new(oldDict.Keys.Concat(newDict.Keys) );
bool hasDiff = false;

foreach(var key in keys)
{
bool hasOldValue = oldDict.TryGetValue(key, out object oldValue);
bool hasNewValue = newDict.TryGetValue(key, out object newValue);

if(hasOldValue && hasNewValue && getObjChanges)
{

if(oldValue is IEnumerable<object> listA && newValue is IEnumerable<object> listB)
{

if(!CollectionsEqual(listA, listB) )
{
var diffList = GetCollectionDiff(listA, listB);

if(!IsNullOrEmptyCollection(diffList) )
{
alterDict[key] = diffList;
hasDiff = true;
}

}

}

else if(oldValue is IDictionary<string, object> dictA && newValue is IDictionary<string, object> dictB)
{

if(!DictionariesEqual(dictA, dictB, out var dictDiff) && !IsNullOrEmptyCollection(dictDiff) )
{
alterDict[key] = dictDiff;
hasDiff = true;
}

}

else if(!Equals(oldValue, newValue) )
{
alterDict[key] = newValue;
hasDiff = true;
}

}

else if(hasNewValue && !hasOldValue && getAddedProps)
{
alterDict[key] = newValue;
hasDiff = true;
}

}

return hasDiff;
}

// Compare two Collections

private static bool CollectionsEqual(IEnumerable<object> first, IEnumerable<object> second)
{
var firstList = first.ToList();
var secondList = second.ToList();

if(firstList.Count != secondList.Count)
return false;

for(int i = 0; i < firstList.Count; i++)
{

if(firstList[i] is IEnumerable<object> listA && secondList[i] is IEnumerable<object> listB)
return CollectionsEqual(listA, listB);

else if(!Equals(firstList[i], secondList[i] ) )       
return false;

}

return true;
}

// Get Collection Diff

private static List<object> GetCollectionDiff(IEnumerable<object> oldCollection, IEnumerable<object> newCollection)
{
var oldList = oldCollection.ToList();
var newList = newCollection.ToList();

List<object> diffList = new();

HashSet<object> oldSet = new(oldList);
HashSet<object> newSet = new(newList);

foreach(var item in newSet)
{

if(!oldSet.Contains(item) )
diffList.Add(item);
 
}

return diffList;
}

// Compare two Dictionaries

private static bool DictionariesEqual(IDictionary<string, object> first, IDictionary<string, object> second,
out ExpandoObject diff)
{
diff = new();

var alterDict = (IDictionary<string, object>)diff;
HashSet<string> keys = new(first.Keys.Concat(second.Keys) );

bool hasDiff = false;

foreach(var key in keys)
{
bool hasFirstValue = first.TryGetValue(key, out object firstValue);
bool hasSecondValue = second.TryGetValue(key, out object secondValue);

if(hasFirstValue && hasSecondValue && !Equals(firstValue, secondValue) )
{
alterDict[key] = secondValue;
hasDiff = true;
}

else if(hasSecondValue && !hasFirstValue)
{
alterDict[key] = secondValue;
hasDiff = true;
}

}

return hasDiff;
}

// Check if an Obj is Null or if its a Empty Collection

private static bool IsNullOrEmptyCollection(object value)
{

if(value == null)
return true;

else if(value is IEnumerable<object> list)
return !list.Any(); 

else if(value is IDictionary<string, object> dict)
return dict.Count == 0;

return false;
}

}