using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary> Initializes Filtering Functions for Input Values. </summary>

public static partial class InputHelper
{
/// <summary> Searches for a Number on a String. </summary>

[GeneratedRegex("([-+]?\\d*\\.?\\d+)")]

private static partial Regex NumericPattern();

/// <summary> Applies a String Case to a Span of Characters. </summary>

public static void ApplyStringCase(Span<char> target, StringCase strCase)
{

switch(strCase)
{
case StringCase.Lower:

for(int i = 0; i < target.Length; i++)
target[i] = char.ToLowerInvariant(target[i]);

break;

case StringCase.Upper:

for(int i = 0; i < target.Length; i++)
target[i] = char.ToUpperInvariant(target[i]);

break;
}

}

/** <summary> Filters a <c>DateTime</c> from user's Input. </summary>

<returns> The Filtered Value. </returns> */
 
public static DateTime FilterDate(ReadOnlySpan<char> input)
{

if(DateTime.TryParse(input, LibInfo.CurrentCulture, out var filteredDate) )
return filteredDate;

return DateTime.Now;
}

/** <summary> Filters a Name from User's Input. </summary>

<param name = "source"> The Name to be Filtered. </param>

<returns> The Filtered Name. </returns> */
 
public static NativeString FilterName(ReadOnlySpan<char> source)
{
NativeString name = new(source.Length);
var invalidChars = GetInvalidChars(true);

int count = 0;

foreach(char c in source)
{

if(Array.IndexOf(invalidChars, c) == -1)
name[count++] = c;

}

name.Realloc(count);

return name;
}

/** <summary> Extracts the Numeric Digits from user's Input. </summary>

<param name = "sourceStr"> The String to be Analized. </param>

<returns> A Sequence of Chars that represent the numerical Digits. </returns> */

private static string ExtractNumeric(string number)
{
var match = NumericPattern().Match(number);

if(match.Success)
{
Group numbers = match.Groups[1];

return numbers.Value;
}

return "0";
}

/** <summary> Parse a string as a Number of type T </summary>

<typeparam name = "T"> The Integer Type. </typeparam>
<param name = "number"> Numeric string. </param>

<returns> A Value that is Inside the Range of the expected Type. </returns> */

private static T ParseNumber<T>(string number) where T : struct
{
object parsed = Convert.ChangeType(number, typeof(T) );

return parsed == null ? default : (T)parsed;
}

/** <summary> Filters a numeric Value from user's Input. </summary>

<param name = "input"> Input to be Filtered. </param>

<returns> The Filtered Value. </returns> */

public static T FilterNumber<T>(string input) where T : struct
{
string number = ExtractNumeric(input);

return ParseNumber<T>(number);
}

// Generate Random String

public static string GenRandomStr(int length)
{
const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

return RandomNumberGenerator.GetString(ALPHABET, length);
}

/** <summary> Gets a List of Invalid Chars for FileNames or DirNames. </summary>

<param name = "isShortName"> Determines if the File/Folder name is a Name (Short Name) 
or a Path (Full Name). </param>

<returns> The Invalid Chars. </returns> */

public static char[] GetInvalidChars(bool isShortName)
{
return isShortName ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
}

/** <summary> Removes Literal Characters from a String. </summary>

<remarks> This Method is used to remove Chars such as \r, \n, \t </remarks>

<param name = "targetStr"> The String to be Processed. </param> */  

public static void RemoveLiteralChars(ref string targetStr)
{

targetStr = targetStr.Replace("\\r\\n", string.Empty)
                     .Replace("\\r", string.Empty)
                     .Replace("\\n", string.Empty)
                     .Replace("\\t", string.Empty);

}

}