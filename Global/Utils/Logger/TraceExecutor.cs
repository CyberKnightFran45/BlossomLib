using System;
using System.IO;

// Executes an action with logging enabled

public static class TraceExecutor
{
// Run action

public static void Run(string operationName, Action<TraceContext> action,
                       params (string Name, object Value)[] args)
{
TraceLogger.WriteLine($"{operationName} started");
TraceLogger.WriteLine();

if(args.Length > 0)
{
DebugArgs(args);

TraceLogger.WriteLine();
}

TraceContext context = new();

try
{
action(context);
}

catch(Exception ex)
{
TraceLogger.WriteError(ex, $"{operationName} failed");
}

TraceLogger.WriteLine($"{operationName} finished");

if(context.LogOutSize)
LogSize(context);

}

// Log size

private static void LogSize(TraceContext context)
{
var displaySize = SizeT.FormatSize(context.OutputSize);

if(context.ShowRatio)
{
var ratio = (double)context.OutputSize / context.InputSize;

TraceLogger.WriteInfo($"Output Size: {displaySize} (Ratio: {ratio:P2})", false);
}

else
TraceLogger.WriteInfo($"Output Size: {displaySize}", false);

}

// Log params

private static void DebugArgs( (string Name, object Value)[] args)
{
TraceLogger.WriteLine("==============  Arguments  ==============");
TraceLogger.WriteLine();

for(int i = 0; i < args.Length; i++)
{
var arg = args[i];

TraceLogger.WriteLine($"[{i}] {arg.Name} = {FormatValue(arg.Value)}");
}

TraceLogger.WriteLine();

TraceLogger.WriteLine("====================================");
TraceLogger.WriteLine();
}

// Format value

private static string FormatValue(object val)
{

return val switch
{
null => "<null>",
string s => FormatString(s),
bool b => b ? "true" : "false",
Enum e => $"{e} ({Convert.ToInt32(e)})",
_ => val.ToString() ?? "?"
};

}

// Format string, including paths

private static string FormatString(string str)
{

if(str.Contains('\\') || str.Contains('/') )
return ShortPath(str);

if(str.Length > 100)
return str[.. 100] + "...";

return str;
}

// Shorten path

private static string ShortPath(string path)
{

try
{
string baseDir = Environment.CurrentDirectory;

return Path.GetRelativePath(baseDir, path);
}
	
catch
{
return path;
}

}

}
