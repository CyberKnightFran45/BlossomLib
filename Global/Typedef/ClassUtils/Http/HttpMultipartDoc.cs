using System.IO;

/// <summary> Represents a class that can be Serialized as Http Multipart </summary>

public abstract class HttpMultipartDoc<T> : HttpDoc<T> where T : class
{
// Read Form

public override void ReadForm(Stream reader)
{
var fields = HttpMultipartHelper.ReadContent(reader);

Read(fields);
}

// Write Form

public override void WriteForm(Stream writer)
{
static void f(Stream w, string name, object val, bool _) => HttpMultipartHelper.WriteContent(w, name, val);

Write(writer, f);

HttpMultipartHelper.WriteFooter(writer);
}

}