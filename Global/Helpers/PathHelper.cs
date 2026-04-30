using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Core;

/// <summary> Initializes some Functions for Building or Editing access Paths. </summary>

public static partial class PathHelper
{
/** <summary> Adds an Extension to the End of a Path. </summary>

<param name = "sourcePath"> The Path to be Modified. </param>

<returns> A Path with the new Extension. </returns> */

public static void AddExtension(ref string sourcePath, string ext)
{

if(string.IsNullOrEmpty(sourcePath) || ext.Equals(Path.GetExtension(sourcePath), 
StringComparison.OrdinalIgnoreCase) )
return;

sourcePath += ext;

CheckDuplicatedPath(ref sourcePath);
}

/** <summary> Checks if the Path is a Relative Path or not. </summary>

<param name = "targetPath"> The Path Defined by User. </param>  */

public static void AlignPathWithAppDir(ref string targetPath)
{

if(string.IsNullOrEmpty(targetPath) || Path.IsPathRooted(targetPath) )
return;

targetPath = Path.Combine(AppContext.BaseDirectory, targetPath);
}

/** <summary> Builds a new Path from a Directory with the Specified Params. </summary>

<param name = "parentPath"> The Parent Path (must be a Directory name). </param>
<param name = "filePath"> The File Path to Use as a Reference Name. </param>
<param name = "pathSuffix"> A Suffix to Add to the File Name (this is Optional). </param>

<returns> The new Path Built. </returns> */

public static string BuildPathFromDir(string parentPath, string filePath, string ext,
string suffix = null)
{
EnsurePathExists(parentPath);

string fileName = Path.GetFileNameWithoutExtension(filePath);
string basePath = Path.Combine(parentPath, fileName);

if(string.IsNullOrEmpty(suffix) )
return basePath + ext;

return $"{basePath}_{suffix}{ext}";
}

/** <summary> Changes the Extension from a given Path. </summary>

<param name = "sourcePath"> The Path to be Modified. </param>

<returns> A Path with the new Extension. </returns> */

public static void ChangeExtension(ref string sourcePath, string ext)
{

if(string.IsNullOrEmpty(sourcePath) || Path.GetExtension(sourcePath) == ext)
return;

sourcePath = Path.ChangeExtension(sourcePath, ext);

CheckDuplicatedPath(ref sourcePath);
}

/** <summary> Checks if a Path is already been used. </summary>

<param name = "targetPath"> The Path to be Analized. </param>

<returns> The Path Validated. </returns> */

public static void CheckDuplicatedPath(ref string targetPath)
{

if(!Directory.Exists(targetPath) && !File.Exists(targetPath) )
return;

string rootPath = Path.GetDirectoryName(targetPath);
string name = Path.GetFileName(targetPath);

string extension = string.Empty;

if(File.Exists(targetPath) )
{
extension = Path.GetExtension(targetPath);
name = Path.GetFileNameWithoutExtension(targetPath);
}

int copyIndex = 1;
var match = DuplicatedPathRegex().Match(name);

if(match.Success)
{
name = match.Groups[1].Value.Trim();
copyIndex = int.Parse(match.Groups[2].Value) + 1;
}

string newPath = targetPath;

while(Directory.Exists(newPath) || File.Exists(newPath))
{
newPath = Path.Combine(rootPath, $"{name} ({copyIndex}){extension}");
copyIndex++;
}

targetPath = newPath;
}

/** <summary> Checks if the Path provided refers to an Existing FileSystem or not. </summary>

<param name = "sourcePath"> The Path to be Analized. </param> */

public static void EnsurePathExists(string sourcePath, bool? forFiles = null)
{

if(!string.IsNullOrWhiteSpace(sourcePath) && !Path.Exists(sourcePath) )
CreateFileSystem(sourcePath, forFiles);

}


/** <summary> Creates a FileSystem (a File or a Folder) according to the given Path Type. </summary>

<param name = "targetPath"> The Path where the FileSystem will be Created. </param> */

public static void CreateFileSystem(string targetPath, bool? isFile = null)
{
isFile ??= Path.HasExtension(targetPath);

if(isFile.Value)
{
string parentDir = Path.GetDirectoryName(targetPath);

if(!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir) )
Directory.CreateDirectory(parentDir);

using var newFile = File.Create(targetPath);
}

else
Directory.CreateDirectory(targetPath);

}

// Delete End Separator

public static void DeleteEndPathSeparator(ref string str)
{
char t = str[^1];

if (t == '/' || t == '\\')
str = str[0..^1];

}

// Create filter (Core)

private static Func<string, bool> CreateFilter(Func<string, string> selector,
                                               HashSet<string> includeList,
                                               HashSet<string> excludeList,
                                               string wildcard = "*")
{

return input =>
{
string v = selector(input);

if(includeList.Contains(wildcard) )
return excludeList.Count == 0 || !excludeList.Contains(v, StringComparer.OrdinalIgnoreCase);

if(includeList.Count == 0)
return excludeList.Count == 0 || !excludeList.Contains(v, StringComparer.OrdinalIgnoreCase);

return includeList.Contains(v) && !excludeList.Contains(v);
};

}

// Defines a names filter for files

private static Func<string, bool> NamesFilter(HashSet<string> includeList, HashSet<string> excludeList)
{
return CreateFilter(path => Path.GetFileNameWithoutExtension(path), includeList, excludeList);
}

// Defines an extension filter for files

private static Func<string, bool> ExtensionFilter(HashSet<string> includeList, HashSet<string> excludeList)
{
return CreateFilter(path => Path.GetExtension(path), includeList, excludeList, ".*");
}

public static Func<string, bool> CreateFileFilter(HashSet<string> names,
                                                  HashSet<string> extensions,
                                                  HashSet<string> namesToExclude = null,
                                                  HashSet<string> extToExclude = null)
{
namesToExclude ??= new();
extToExclude ??= new();

var nameFilter = NamesFilter(names, namesToExclude);
var extFilter = ExtensionFilter(extensions, extToExclude);

bool useNameFilter = names.Count > 0 || namesToExclude.Count > 0;
bool useExtFilter = extensions.Count > 0 || extToExclude.Count > 0;

return path => (!useNameFilter || nameFilter(path) ) && (!useExtFilter || extFilter(path) );
}

/** <summary> Filters a list of files by Name and Extension </summary>

<param name = "src"> The list </param>
<param name = "extensions"> The extensions </param>

<returns> The Filtered List </returns> */

public static IEnumerable<string> FilterFiles(IEnumerable<string> src,
                                              HashSet<string> names,
                                              HashSet<string> extensions,
                                              HashSet<string> namesToExclude = null,
                                              HashSet<string> extToExclude = null)
{

if(src is null)
return [];

var filesFilter = CreateFileFilter(names, extensions, namesToExclude, extToExclude);

return src.Where(filesFilter);
}

/** <summary> Filters a Path from User's Input. </summary>

<param name = "targetPath"> The Path to be Filtered. </param> */

public static void FilterPath(ref string targetPath)
{

if(string.IsNullOrEmpty(targetPath) )
return;

string validStr = targetPath;
string filteredPath = string.Empty;

char[] invalidPathChars = InputHelper.GetInvalidChars(false);

for(int i = 0; i < invalidPathChars.Length; i++)
{

if(validStr.Contains(invalidPathChars[i] ) )
{
filteredPath = validStr.Replace(invalidPathChars[i].ToString(), string.Empty);
validStr = filteredPath;
}

filteredPath = validStr;
}

targetPath = filteredPath.Replace("\"", string.Empty);
}

// Normalize separators

public static void NormalizePath(ref string targetPath)
{

if(PlatformHelper.IsWindows)
targetPath = targetPath.Replace('/', '\\').Replace(" \\", "\\");

else
targetPath = targetPath.Replace('\\', '/');

}

// Denormalize separators

public static void DenormalizePath(ref string targetPath)
{
targetPath = targetPath.Replace('\\', '/');
}

// Combine Paths from diff OS

public static string SafeCombine(params string[] paths)
{

if(paths is null || paths.Length == 0)
return string.Empty;

for(int i = 0; i < paths.Length; i++)
NormalizePath(ref paths[i]);

string combined = Path.Combine(paths);
DenormalizePath(ref combined);

return combined;
}

// Get Path to Downloads Folder

public static string GetDownloadsFolder()
{
string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

return Path.Combine(userProfile, "Downloads");
}

/** <summary> Removes the Extension from a given Path. </summary>

<param name = "sourcePath"> The Path to be Modified. </param> */

public static void RemoveExtension(ref string sourcePath)
{

if(string.IsNullOrEmpty(sourcePath) )
return;

string ext = Path.GetExtension(sourcePath);

if(string.IsNullOrEmpty(ext) )
return;

int lengthDiff = sourcePath.Length - ext.Length;
sourcePath = sourcePath[..lengthDiff];

CheckDuplicatedPath(ref sourcePath);
}

[GeneratedRegex(@"^(.*)\((\d+)\)$")]

private static partial Regex DuplicatedPathRegex();
}