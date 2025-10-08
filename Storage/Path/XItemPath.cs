using Ion.Analysis;
using Ion.Windows;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ion.Storage;

[Extend<FilePath>]
public static class XItemPath
{
    /// <see cref="Region.Method.Private"/>

    /// <summary>
    /// Gets the final path of the given item if moved to the given folder with the (optional) new name.
    /// </summary>
    /// <param name="i">The item to move.</param>
    /// <param name="targetFolderPath">The folder where the item is moved to.</param>
    /// <param name="targetName">What to rename the item to (optional).</param>
    /// <returns></returns>
    private static string GetTargetFilePath(string path, string targetFolderPath, string targetName)
    {
        var type = GetType(path);

        if (type == ItemType.Drive)
            throw new NotSupportedException();

        string result = null;

        //Various scenarios are addressed differently for files and folders. Further analysis may be necessary to identify potential others.

        if (type == ItemType.File)
        {
            //Both paths must exist
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            if (!Folder.Exists(targetFolderPath))
                throw new DirectoryNotFoundException(targetFolderPath);

            //The folder path must end with \ when comparing
            var a = path;
            var b = targetFolderPath.EndsWith(@"\") ? targetFolderPath : $@"{targetFolderPath}\";

            //Both paths must be lower when comparing
            a = a.ToLower();
            b = b.ToLower();

            /*
            1.  Not okay!
                C:\Folder a\Folder b\File x.png
                C:\Folder a\Folder b\
                
                Result:
                C:\Folder a\Folder b\File x.png
            */

            var c = Path.GetDirectoryName(a).ToLower();
            c = c.EndsWith(@"\") ? c : $@"{c}\";

            //If the parent path of <a> (c) equals path <b>
            if (b == c)
            {
                //If the file isn't getting renamed
                if (targetName.IsEmpty() || targetName.ToLower() == Path.GetFileName(a).ToLower())
                    throw new InvalidOperationException();
            }

            /*
            1.  Okay!
                C:\Folder a\Folder b\File x.png
                D:\
                
                Result:
                D:\File x.png

            2.  Okay!
                C:\Folder a\Folder b\File x.png
                C:\Folder a\
                
                Result:
                C:\Folder a\File x.png
                
            3.  Okay!
                C:\Folder a\Folder b\File x.png
                C:\Folder a\Folder b\Folder c\
                
                Result:
                C:\Folder a\Folder b\Folder c\File x.png
            */

            //Preserve original casing by using input variables
            result = $@"{targetFolderPath.TrimEnd('\\')}\{(targetName.IsEmpty() ? Path.GetFileName(path) : targetName)}";

            //If a file with that path already exists (to do: Consider overwriting it)
            if (File.Exists(result))
                throw new InvalidOperationException();
        }

        else if (type == ItemType.Folder)
        {
            //Both paths must exist
            if (!Folder.Exists(path))
                throw new DirectoryNotFoundException(path);

            if (!Folder.Exists(targetFolderPath))
                throw new DirectoryNotFoundException(targetFolderPath);

            //Both paths must end with \ when comparing
            var a = path.EndsWith(@"\") ? path : $@"{path}\";
            var b = targetFolderPath.EndsWith(@"\") ? targetFolderPath : $@"{targetFolderPath}\";

            //Both paths must be lower when comparing
            a = a.ToLower();
            b = b.ToLower();

            /*
            Path <b> cannot start with path <a>

            1.  Not okay!
                C:\Folder a\Folder b\Folder x\
                C:\Folder a\Folder b\Folder x\
                                
                Result:
                C:\Folder a\Folder b\Folder x\Folder x\

            2.  Not okay!
                C:\Folder a\Folder b\Folder x\
                C:\Folder a\Folder b\Folder x\Folder c\
                
                Result:
                C:\Folder a\Folder b\Folder x\Folder c\Folder x\
            */

            if (b.StartsWith(a))
                throw new InvalidOperationException();

            /*

            Path <b> cannot equal parent path of <a> (unless renaming!)

            3.  Not okay!
                C:\Folder a\Folder b\Folder x
                C:\Folder a\Folder b

                Result:
                C:\Folder a\Folder b\Folder x
            */

            var c = Path.GetDirectoryName(a).ToLower();
            c = c.EndsWith(@"\") ? c : $@"{c}\";

            //If the parent path of <a> equals path <b>
            if (b == c)
            {
                //If the folder isn't getting renamed
                if (targetName.IsEmpty() || targetName.ToLower() == Path.GetFileName(a).ToLower())
                    throw new InvalidOperationException();
            }

            /*
            1.  Okay!
                C:\Folder a\Folder b\Folder x\
                C:\
                    
                Result:
                C:\Folder x\

            2.  Okay!
                C:\Folder a\Folder b\Folder x\
                C:\Folder a\
                    
                Result:
                C:\Folder a\Folder x\

            3.  Okay!
                C:\Folder a\Folder b\Folder x\
                D:\Folder z\

                Result:
                D:\Folder z\Folder x\

            If none of the "Not okay" scenarios apply, an "Okay" scenario is assumed to...
            */

            //Preserve original casing by using input variables
            result = $@"{targetFolderPath.TrimEnd('\\')}\{(targetName.IsEmpty() ? Path.GetFileName(path) : targetName)}";

            //If a folder with that path already exists (to do: Consider merging contents)
            if (Folder.Exists(result))
                throw new InvalidOperationException();
        }

        return result;
    }

    /// <see cref="Region.Method.Public"/>
    #region

    public static void Copy(this FilePath sourceFilePath, string targetFolderPath)
    {
        var targetFilePath = sourceFilePath.ToString().Replace(Path.GetDirectoryName(sourceFilePath), targetFolderPath);

        var type = GetType(sourceFilePath);
        if (type == ItemType.Drive) return;

        try
        {
            switch (type)
            {
                case ItemType.File:
                case ItemType.Shortcut:
                    if (sourceFilePath == targetFilePath)
                        targetFilePath = File.ClonePath(sourceFilePath);

                    FileSystem.CopyFile(sourceFilePath, targetFilePath, UIOption.AllDialogs);
                    break;

                case ItemType.Folder:
                    if (sourceFilePath == targetFilePath)
                        targetFilePath = Folder.ClonePath(sourceFilePath);

                    FileSystem.CopyDirectory(sourceFilePath, targetFilePath, UIOption.AllDialogs);
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Write(new Error(e));
        }

        if (File.Exists(sourceFilePath))
            FileSystem.CopyFile(sourceFilePath, targetFilePath, UIOption.AllDialogs);

        if (Folder.Exists(sourceFilePath))
            FileSystem.CopyDirectory(sourceFilePath, targetFilePath, UIOption.AllDialogs);
    }

    public static string GetFriendlyDescription(this FilePath path)
    {
        var result = "";
        if (path.ToString().EndsWith(@":\"))
        {
            foreach (var i in Drive.Get())
            {
                if (i.Name.ToLower() == path.ToString().ToLower())
                {
                    result = $"{new FileSize(i.AvailableFreeSpace).ToString(FileSizeFormat.BinaryUsingSI)} free of {new FileSize(i.TotalSize).ToString(FileSizeFormat.BinaryUsingSI)}";
                    break;
                }
            }
        }
        else
        {
            result = Folder.Exists(path)
                ? "Folder"
                : !path.ToString().IsEmpty()
                ? ShellProperties.GetDescription(path)
                : path;
        }

        return result.IsEmpty() ? Path.GetFileNameWithoutExtension(path) : result;
    }

    public static string GetFriendlyName(this FilePath path)
    {
        if (path.ToString() is null)
            return string.Empty;

        if (path == FilePath.Root)
            return FilePath.RootName;

        if (path.ToString().EndsWith(@":\"))
        {
            var volumeLabel = Drive.Get().FirstOrDefault(i => i.Name == path)?.VolumeLabel;
            return volumeLabel != null ? $"{volumeLabel} ({path.ToString().TrimEnd('\\')})" : path;
        }

        var result = Path.GetFileName(path);
        return result.IsEmpty() ? path : result;
    }

    public static Item GetItem(this FilePath path, bool refresh = false)
    {
        Item result = default;
        switch (GetType(path))
        {
            case ItemType.Drive:
                result = new Drive(path);
                break;
            case ItemType.File:
                result = new File(path);
                break;
            case ItemType.Folder:
                result = new Folder(path);
                break;
            case ItemType.Shortcut:
                result = new Shortcut(path);
                break;
        }
        if (refresh)
            result.Refresh();

        return result;
    }

    public static ItemType GetType(this FilePath path)
    {
        if (path == FilePath.Root)
            return ItemType.Root;

        if (path.ToString()?.EndsWith(@":") == true || path.ToString()?.EndsWith(@":\") == true)
            return ItemType.Drive;

        if (Folder.Exists(path))
            return ItemType.Folder;

        if (File.Exists(path))
        {
            if (Shortcut.Is(path))
                return ItemType.Shortcut;

            return ItemType.File;
        }
        return ItemType.Nothing;
    }

    public static bool IsHidden(this FilePath path)
    {
        foreach (var i in Drive.Get())
        {
            if (i.Name == path)
                return false;
        }
        if (File.Exists(path))
        {
            var file = new File(path);
            file.Refresh();
            return file.IsHidden;
        }
        else if (Folder.Exists(path))
        {
            var folder = new Folder(path);
            folder.Refresh();
            return folder.IsHidden;
        }
        return false;
    }

    public static bool IsReadOnly(this FilePath path)
    {
        foreach (var i in Drive.Get())
        {
            if (i.Name == path)
                return false;
        }
        if (File.Exists(path))
        {
            var file = new File(path);
            file.Refresh();
            return file.IsReadOnly;
        }
        else if (Folder.Exists(path))
        {
            var folder = new Folder(path);
            folder.Refresh();
            return folder.IsReadOnly;
        }
        return false;
    }

    public static bool IsVisible(this FilePath path)
        => !IsHidden(path);

    public static Result Move(this FilePath sourceFilePath, string targetFolderPath, string targetFileName)
    {
        try
        {
            var targetFilePath = GetTargetFilePath(sourceFilePath, targetFolderPath, targetFileName);
            switch (GetType(sourceFilePath))
            {
                case ItemType.Drive:
                    return new Error(new InvalidOperationException());

                case ItemType.File:
                case ItemType.Shortcut:
                    FileSystem.MoveFile(sourceFilePath, targetFilePath, UIOption.AllDialogs);
                    break;

                case ItemType.Folder:
                    FileSystem.MoveDirectory(sourceFilePath, targetFilePath, UIOption.AllDialogs);
                    break;
            }
            return new Success();
        }
        catch (Exception e)
        {
            Log.Write(new Error(e));
            return new Error(e);
        }
    }

    public static Result OpenInWindowsExplorer(this FilePath path)
        => File.TryOpen("explorer.exe", path);

    public static void Recycle(IEnumerable<string> paths, RecycleOption option = RecycleOption.SendToRecycleBin)
    {
        foreach (var i in paths)
            Recycle(i, option);
    }

    public static void Recycle(this FilePath path, RecycleOption option = RecycleOption.SendToRecycleBin)
    {
        try
        {
            if (Directory.Exists(path))
                FileSystem.DeleteDirectory(path, UIOption.AllDialogs, option);

            else if (System.IO.File.Exists(path))
                FileSystem.DeleteFile(path, UIOption.AllDialogs, option);
        }
        catch { }
    }

    public static Result ShowInWindowsExplorer(this FilePath path)
        => File.TryOpen("explorer.exe", @"/select, {0}".F(path));

    #endregion
}