using System.IO;

// Delegate for stream transforming

public delegate void StreamTransform(Stream input, Stream output, TraceContext ctx);

// Delegate for stream comparing

public delegate void StreamCompare(Stream a, Stream b, Stream diff, TraceContext ctx);