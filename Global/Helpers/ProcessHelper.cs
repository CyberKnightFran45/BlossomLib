using System.Diagnostics;

public static class ProcessHelper
{
// Create New Process

public static Process CreateNew(string fName, string args = "", bool redirectOutput = true, 
bool useShell = false, bool noWindow = true)
{
Process process = new();

process.StartInfo.FileName = fName;
process.StartInfo.Arguments = args;
process.StartInfo.RedirectStandardOutput = redirectOutput;
process.StartInfo.UseShellExecute = useShell;
process.StartInfo.CreateNoWindow = noWindow;
		
return process;
}
// Create and Start a new Process

public static Process StartNew(string fName, string args = "", bool redirectOutput = true, 
bool useShell = false, bool noWindow = true)
{
Process process = CreateNew(fName, args, redirectOutput, useShell, noWindow);

process.Start();

return process;
}

}