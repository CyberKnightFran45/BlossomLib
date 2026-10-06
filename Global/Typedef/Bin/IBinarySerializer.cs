using System.IO;

// Interface for binary serializer

public interface IBinarySerializer<T> where T : class
{
// Read data

T ReadBin(Stream reader);

// Write data

void WriteBin(Stream writer, T value);
}