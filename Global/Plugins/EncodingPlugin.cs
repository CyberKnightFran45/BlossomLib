using System.Collections.Generic;
using System.Text;

public static class EncodingPlugin
{
// Cached Encodings

private static readonly Dictionary<EncodingType, Encoding> _cache = new();

// Register provider only once

static EncodingPlugin()
{
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
}

/** <summary> Gets the corresponding Encoding from an EncodingType enum. </summary>
<remarks> Falls back to ASCII if the encoding is unsupported. </remarks>

<param name="type">Encoding type (as enum)</param>

/// <returns> <c>Encoding</c> instance </returns> */

public static Encoding GetEncoding(this EncodingType type)
{

if (_cache.TryGetValue(type, out var encoding) )
return encoding;

try
{
var newEncoding = Encoding.GetEncoding( (int)type);
_cache[type] = newEncoding;

return newEncoding;
}

catch
{
return Encoding.ASCII;
}

}

}