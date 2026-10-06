using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary> Initializes Extra Functions for Dirs. </summary>

public static class DirManager
{
/** <summary> Gets the Path of a Container. </summary>

<param name = "targetPath"> The Path where the Container will be Created. </param>
<param name = "namePrefix"> The Prefix to Add to the Beginning of the Name (Optional). </param> */

public static string GetContainerPath(string targetPath, string namePrefix = "FilesContainer")
{
string rootPath = Path.GetDirectoryName(targetPath);
string fileName = Path.GetFileNameWithoutExtension(targetPath);

string containerPath = Path.Combine(rootPath, namePrefix, fileName);
PathHelper.EnsurePathExists(containerPath, false);

return containerPath;
}

/** <summary> Gets the Name of a Directory.  </summary>

<param name = "targetPath"> The Path where the Folder to be Analized is Located. </param>

<returns> The Name of the Folder. </returns> */

public static string GetFolderName(string targetPath) => new DirectoryInfo(targetPath).Name;

/** <summary> Gets the Size of a Directory expressed in Bytes. </summary>

<param name = "targetPath"> The Path where the Directory to be Analized is Located. </param>

<returns> The Size of the Folder. </returns> */

public static long GetFolderSize(string targetPath)
{
IEnumerable<string> files = Directory.EnumerateFiles(targetPath);
long folderSize = 0;

foreach(string file in files)
{
long fileSize = FileManager.GetFileSize(file);
folderSize += fileSize;
}

IEnumerable<string> subfolders = Directory.EnumerateDirectories(targetPath);

foreach(string subfolder in subfolders)
folderSize += GetFolderSize(subfolder);

return folderSize;
}

/** <summary> Checks if a Folder is Empty or not by Analizing its Content. </summary>

<param name = "targetPath"> The Path where the Directory to be Checked is Located. </param>

<returns> <b>true</b> if the Folder is Empty; otherwise, <b>false</b>. </returns> */

public static bool FolderIsEmpty(string targetPath)
{
var files = Directory.EnumerateFiles(targetPath);

return !files.Any();
}

}