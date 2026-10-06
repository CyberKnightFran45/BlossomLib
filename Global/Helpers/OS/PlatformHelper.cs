using System.Runtime.InteropServices;

public static class PlatformHelper
{
// Check if current platform is Windows

public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

// Check if current platform is Linux

public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

// Check if current platform is MacOS

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