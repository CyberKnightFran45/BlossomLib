using System.Runtime.InteropServices;

public static class PlatformHelper
{
public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

// Execute Commmand for Linux or OSX

public static string ExecuteXCommand(string command)
{
using var process = ProcessHelper.StartNew("/bin/bash", $"-c \"{command}\"");

string output = process.StandardOutput.ReadToEnd();
process.WaitForExit();

return output.Trim();
}

}