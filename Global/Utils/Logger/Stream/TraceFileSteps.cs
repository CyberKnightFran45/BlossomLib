// Helper for logging files operation

public static class TraceFileSteps
{
// Get display action

private static string GetActionName(string baseName, bool logInSize, long inSize)
{

if(!logInSize)
return baseName;

string displaySize = SizeT.FormatSize(inSize);

return $"{baseName} ({displaySize})";
}

// Transform files

public static void Run(TraceContext ctx, string inputPath, string outputPath, 
                       string operationName, StreamTransform transform,
					   bool logInSize = true, bool logOutSize = true)
{
ctx.LogInSize = logInSize;
ctx.LogOutSize = logOutSize;

TraceLogger.WriteActionStart("Opening files...");

using var inFile = FileManager.OpenRead(inputPath);
using var outFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

var inSize = inFile.Length;
ctx.InputSize = inSize;

string actionName = GetActionName(operationName, logInSize, inSize);
TraceLogger.WriteActionStart(actionName);

transform(inFile, outFile, ctx);

TraceLogger.WriteActionEnd();

ctx.OutputSize = outFile.Length;
}

// Compare files

public static void Run(TraceContext ctx, string src, string dest, string diff,
                       string operationName, StreamCompare compare,
					   bool logOutSize = false)
{
ctx.LogOutSize = logOutSize;

TraceLogger.WriteActionStart("Opening files...");

using var srcFile = FileManager.OpenRead(src);
using var destFile = FileManager.OpenWrite(dest);
using var diffStream = FileManager.OpenWrite(diff);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart(operationName);
compare(srcFile, destFile, diffStream, ctx);

TraceLogger.WriteActionEnd();

ctx.OutputSize = diffStream.Length;
}

}