using System;
using System.Text;

/// <summary> Provides Encoding Functions for String Manipulation. </summary>

public static class EncodeHelper
{
/** <summary> Gets a Encoding by using some flags. </summary>

<returns> The Encoding. </returns> */

public static Encoding GetEncoding(EncodingType flags) => flags.GetEncoding();

/// <summary> Checks if a String is ASCII Encoded. </summary>
/// <param name="str"> The String to Check. </param>
/// <returns> True if the String is ASCII Encoded, otherwise False. </returns>

public static bool IsASCII(ReadOnlySpan<char> str)
{

for (int i = 0; i < str.Length; i++)
{

if (str[i] > 127)
return false;

}

return true;
}

}