using System.IO;
using System.Text.RegularExpressions;

// Handle Form Contents in the Multipart Format

public static partial class HttpMultipartHelper
{
private const string BOUNDARY = "--_{{}}_";

private const string FORM_FIELD = "{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";

// Read FormContent

public static MatchCollection ReadContent(Stream reader)
{
using var fOwner = reader.ReadString();
string rawForm = fOwner.ToString();

return FormRegex().Matches(rawForm);
}

// Write FormContent

public static void WriteContent(Stream writer, string name, object val)
{
string node = string.Format(FORM_FIELD, BOUNDARY, name, val);

writer.WriteString(node);
}

// Write Footer

public static void WriteFooter(Stream writer) => writer.WriteString(BOUNDARY + "--");

// Form Struct

[GeneratedRegex(@"Content-Disposition:\s*form-data;\s*name=""(?<name>[^""]+)""\s*\n\s*\n(?<value>.*?)(?=\n--|\Z)",
RegexOptions.Singleline | RegexOptions.IgnoreCase) ]

private static partial Regex FormRegex();
}