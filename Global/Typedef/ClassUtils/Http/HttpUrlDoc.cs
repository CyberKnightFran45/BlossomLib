using System.IO;

/// <summary> Represents a class that can be Serialized as Http Url-encoded </summary>

public abstract class HttpUrlDoc<T> : HttpDoc<T> where T : class
{
// Read Form

public override void ReadForm(Stream reader)
{
var fields = HttpUrlHelper.ReadContent(reader);

Read(fields);
}

// Write Form

public override void WriteForm(Stream writer)
{
static void f(Stream w, string name, object val, bool first) => HttpUrlHelper.WriteContent(w, name, val, first);

Write(writer, f);
}

}