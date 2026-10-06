
using System.IO;

/// <summary> Represents a signed HTTP document in Multipart format </summary>
public abstract class HttpMultipartSignedDoc<T> : HttpMultipartDoc<T> where T : class
{
// Helper used for making signs

protected SignHelper<T> _signer;

// Init field logic

public override void Init()
{
base.Init();

SetupDigest();
}

// Write Form

public override void WriteForm(Stream writer)
{
CheckSign();

base.WriteForm(writer);
}

// Check Sign

public void CheckSign() => _signer.CheckSign();

// Setup Digest

protected abstract void SetupDigest();
}