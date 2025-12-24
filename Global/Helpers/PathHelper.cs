using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

/** <summary> Creates a Filter from a List of Extensions. </summary>

<param name = "includeList"> The Extensions to Include. </param>
<param name = "excludeList"> The Extensions to Exclude. </param>

<returns> The Extensions Filter. </returns> */

private static Func<string, bool> CreateExtFilter(HashSet<string> includeList, HashSet<string> excludeList)
{

return extFilter => 
{
string fileExt = Path.GetExtension(extFilter);

if(includeList.Contains(".*") )   
return excludeList.Count == 0 || !excludeList.Contains(fileExt, StringComparer.OrdinalIgnoreCase);

else if(includeList.Count == 0 && excludeList.Count > 0)
return !excludeList.Contains(fileExt, StringComparer.OrdinalIgnoreCase);

return includeList.Contains(fileExt, StringComparer.OrdinalIgnoreCase) &&
!excludeList.Contains(fileExt, StringComparer.OrdinalIgnoreCase);

};

}

/** <summary> Creates a Filter from a Specific Names List. </summary>

<returns> The Names Filter. </returns> */

private static Func<string, bool> CreateNamesFilter(HashSet<string> includeList, HashSet<string> excludeList)
{

return namesFilter => 
{
string fileName = Path.GetFileNameWithoutExtension(namesFilter);

if(includeList.Contains("*") )
return excludeList.Count == 0 || !excludeList.Contains(fileName, StringComparer.OrdinalIgnoreCase);

else if(includeList.Count == 0 && excludeList.Count > 0)
return !excludeList.Contains(fileName, StringComparer.OrdinalIgnoreCase);

return includeList.Contains(fileName, StringComparer.OrdinalIgnoreCase) &&
!excludeList.Contains(fileName, StringComparer.OrdinalIgnoreCase); 
};

}

/** <summary> Creates a Extensions Filter from a Dir Length. </summary>

<returns> The Extensions Filter. </returns> */

private static Func<string, bool> CreateDirFilter(int dirLength)
{
return dirFilter => dirLength < 0 || Directory.GetFileSystemEntries(dirFilter).Length == dirLength;
}

// Delete End Separator

public static void DeleteEndPathSeparator(ref string str)
{
char t = str[^1];

if (t == '/' || t == '\\')
str = str[0..^1];

}

/** <summary> Filters a List of Files by a Specific Name and a Specific Extension,
which can be in Lowercase or in Uppercase. </summary>

<param name = "sourceList"> The Files List to be Filtered. </param>
<param name = "specificExtensions"> A List of Specific Extensions used for Filtering the Files. </param>

<returns> The Filtered Files List. </returns> */

public static void FilterFiles(ref IEnumerable<string> sourceFiles, HashSet<string> names,
HashSet<string> extensions, HashSet<string> namesToExclude = null, HashSet<string> extToExclude = null)
{

if(sourceFiles == null || (names.Count > 0 && extensions.Count > 0) )
return;

namesToExclude ??= new();
extToExclude ??= new();

var namesFilter = CreateNamesFilter(names, namesToExclude);
var extFilter = CreateExtFilter(extensions, extToExclude);

HashSet<string> added = new(StringComparer.OrdinalIgnoreCase);

var filtered = Enumerable.Empty<string>();

if(extensions.Count > 0)
{
filtered = filtered.Concat(sourceFiles.Where(extFilter)
.Where(file =>
{
var name = Path.GetFileNameWithoutExtension(file);
return !namesToExclude.Contains(name, StringComparer.OrdinalIgnoreCase) && added.Add(file);
}));
}

if(names.Count > 0)
filtered = filtered.Concat(sourceFiles.Where(namesFilter).Where(added.Add) );

sourceFiles = filtered;
}

/** <summary> Filters a List of Folders by a Specific Name and by Content Length. </summary>

<param name = "sourceList"> The Folders List to be Filtered. </param>

<returns> The Filtered Dirs. </returns> */

public static void FilterDirs(ref IEnumerable<string> sourceDirs, HashSet<string> names, int maxLength,
HashSet<string> namesToExclude = null)
{

if(sourceDirs == null || (names.Count > 0 && maxLength < 0) )
return;

namesToExclude ??= new();

var namesFilter = CreateNamesFilter(names, namesToExclude);
var lengthFilter = CreateDirFilter(maxLength);

HashSet<string> added = new(StringComparer.OrdinalIgnoreCase);

var filtered = Enumerable.Empty<string>();

if(names.Count > 0)
filtered = filtered.Concat(sourceDirs.Where(namesFilter).Where(added.Add) );

if(maxLength > 0)
filtered = filtered.Concat(sourceDirs.Where(lengthFilter).Where(added.Add));

sourceDirs = filtered;
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

if(paths == null || paths.Length == 0)
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