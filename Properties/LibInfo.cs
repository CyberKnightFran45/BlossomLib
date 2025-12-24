using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Threading;

/// <summary> Serves as Reference to the Environment of this DLL. </summary> 

public static class LibInfo
{
/// <summary> The Assembly that is being currently Executed. </summary>

public static Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

/// <summary> The Location of the Current Assembly. </summary>

public static string AssemblyLocation => AppContext.BaseDirectory;

/// <summary> The Operating System where the Program is being Executed. </summary>

public static OperatingSystem OS => Environment.OSVersion;

/// <summary> The Directory which the Current Dll is Located. </summary>

public static string CurrentDllDirectory => Path.GetDirectoryName(AssemblyLocation);

/** <summary> Gets or Sets the Culture which the Program should use. </summary>
<returns> The Current <c>CultureInfo</c>. </returns> */

public static CultureInfo CurrentCulture
{

get => Thread.CurrentThread.CurrentCulture;
set => Thread.CurrentThread.CurrentCulture = value;

}

}