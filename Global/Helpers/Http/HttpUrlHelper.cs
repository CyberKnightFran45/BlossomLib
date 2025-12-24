using System;
using System.IO;
using System.Text.RegularExpressions;

// Handle Form Contents in the URL Format

public static partial class HttpUrlHelper
{
// Read FormContent

public static MatchCollection ReadContent(Stream reader)
{
using var fOwner = reader.ReadString();
string rawForm = fOwner.ToString();

return FormRegex().Matches(rawForm);
}

// Write FormContent

public static void WriteContent(Stream writer, string name, object val, bool isFirst = false)
{

if(!isFirst)
writer.WriteChar16('&');

string encodedVal = val == null ? string.Empty : Uri.EscapeDataString(val.ToString() );

writer.WriteString($"{name}={encodedVal}");
}

// Form Struct

[GeneratedRegex(@"(?<name>[^=&]+)=(?<value>[^&]*)", RegexOptions.Compiled) ]

private static partial Regex FormRegex();
}