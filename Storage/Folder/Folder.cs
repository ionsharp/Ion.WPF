using Ion.Analysis;
using Ion.Data;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ion.Storage;

[Image(Images.Folder), Name("Folder"), Serializable]
[Styles.Object(MemberViewType = MemberViewType.Tab)]
public sealed record class Folder(string path) : Container(ItemType.Folder, FileOrigin.Local, path)
{
    private enum Category { Files, Folders }

    [TabView(View = View.Main)]
    private enum Tab
    {
        [TabStyle(Image = Images.Numbers)]
        Summary
    }

    #region Constants

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;

    #endregion

    #region Properties

    [Styles.Number(0, int.MaxValue, 1,
        Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Characters { get => Get(0); private set => Set(value); }

    [Styles.Number(0, int.MaxValue, 1,
        Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Lines { get => Get(0); private set => Set(value); }

    [Styles.Number(0, int.MaxValue, 1,
        Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Words { get => Get(0); private set => Set(value); }

    [Group(Category.Files)]
    [Style(Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Files { get => Get(0); private set => Set(value); }

    [Group(Category.Folders)]
    [Style(Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Folders { get => Get(0); private set => Set(value); }

    [Group(Category.Files)]
    [Style(Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int HiddenFiles { get => Get(0); private set => Set(value); }

    [Group(Category.Folders)]
    [Style(Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int HiddenFolders { get => Get(0); private set => Set(value); }

    [Group(Category.Files)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeShort))]
    public DateTime? OldestFile { get => Get<DateTime?>(); private set => Set(value); }

    [Group(Category.Folders)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeShort))]
    public DateTime? OldestFolder { get => Get<DateTime?>(); private set => Set(value); }

    [Group(Category.Files)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeShort))]
    public DateTime? NewestFile { get => Get<DateTime?>(); private set => Set(value); }

    [Group(Category.Folders)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeShort))]
    public DateTime? NewestFolder { get => Get<DateTime?>(); private set => Set(value); }

    [Group(Category.Files)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public long LargestFile { get => Get(0L); private set => Set(value); }

    [Group(Category.Files)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public long SmallestFile { get => Get(0L); private set => Set(value); }

    [Group(Category.Folders)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public long LargestFolder { get => Get(0L); private set => Set(value); }

    [Group(Category.Folders)]
    [Styles.Text(Tab = Tab.Summary,
        CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public long SmallestFolder { get => Get(0L); private set => Set(value); }

    [Group(Category.Files)]
    [Style(Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int ReadOnlyFiles { get => Get(0); private set => Set(value); }

    [Group(Category.Folders)]
    [Style(Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int ReadOnlyFolders { get => Get(0); private set => Set(value); }

    #endregion
    #region Folder

    #endregion

    #region Methods

    private void RefreshContent(string path, ref int index, CancellationToken token)
    {
        if (index > 0)
        {
            Folders++;
            Try.Do(() =>
            {
                var folderInfo = new DirectoryInfo(path);
                Dispatch.Do(() =>
                {
                    if (NewestFolder is null || folderInfo.CreationTime > NewestFolder.Value)
                        NewestFolder = folderInfo.CreationTime;

                    if (OldestFolder is null || folderInfo.CreationTime < OldestFolder.Value)
                        OldestFolder = folderInfo.CreationTime;

                    if (XItemPath.IsHidden(path))
                        HiddenFolders++;

                    if (XItemPath.IsReadOnly(path))
                        ReadOnlyFolders++;
                });
            },
            e => Log.Write(e));
        }

        IEnumerable<string> files = default;
        Try.Do(() => files = GetFiles(path), e => Log.Write(e));

        double count = files?.Count() ?? 0;
        if (count > 0)
        {
            foreach (var i in files)
            {
                if (token.IsCancellationRequested) return;

                FileInfo fileInfo = null;
                var fileText = string.Empty;

                bool hidden = false; bool readOnly = false;
                Result result = Try.Do(() =>
                {
                    hidden = XItemPath.IsHidden(i); readOnly = XItemPath.IsReadOnly(i);

                    fileInfo = new FileInfo(i);
                    fileText = File.ReadAllText(i, System.Text.Encoding.Unicode);
                },
                e => Log.Write(e));

                Dispatch.Do(() =>
                {
                    Files++;

                    if (hidden)
                        HiddenFiles++;

                    if (readOnly)
                        ReadOnlyFiles++;

                    if (NewestFile is null || fileInfo.CreationTime > NewestFile)
                        NewestFile = fileInfo.CreationTime;

                    if (OldestFile is null || fileInfo.CreationTime < OldestFile)
                        OldestFile = fileInfo.CreationTime;

                    if (fileInfo.Length > LargestFile)
                        LargestFile = fileInfo.Length;

                    if (SmallestFile == 0 || fileInfo.Length < SmallestFile)
                        SmallestFile = fileInfo.Length;

                    Characters += fileText.Length; Size += fileInfo.Length;
                    Try.Do(() =>
                    {
                        Lines
                            += fileText.GetLineCount();
                        Words
                            += fileText.GetWordCount();
                    });
                });
            }
        }

        IEnumerable<string> folders = default;
        Try.Do(() => folders = GetFolders(path), e => Log.Write(e));
        if (folders?.Count() > 0)
        {
            foreach (var i in folders)
            {
                if (token.IsCancellationRequested) return;

                index++;
                RefreshContent(i, ref index, token);
            }
        }
    }

    async protected override Task RefreshContent(CancellationToken token)
    {
        RefreshingContent = true;

        Files = 0; Folders = 0;
        Characters = 0; Lines = 0; Size = 0; Words = 0;
        NewestFile = NewestFolder = OldestFile = OldestFolder = null;
        HiddenFiles = HiddenFolders = ReadOnlyFiles = ReadOnlyFolders = 0;
        LargestFile = SmallestFile = 0;

        int index = 0;
        await Task.Run(() => RefreshContent(Path, ref index, token), token);

        RefreshingContent = false;
    }

    #endregion

    #region Methods / Static

    /// <summary>
    /// Gets a new path based on the given path.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <param name="nameFormat">How to format the name (not including extension).</param>
    /// <returns>A new path based on the old path.</returns>
    public static string ClonePath(string folderPath, string nameFormat = FilePath.DefaultCloneFormat) => FilePath.CloneName(folderPath, nameFormat, i => Exists(i));

    ///

    public static void Create(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            return;

        if (directoryPath.Length < MAX_PATH)
        {
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }
        }
        else
        {
            var paths = GetAllPathsFromPath(GetWin32LongPath(directoryPath));
            foreach (var item in paths)
            {
                if (!LongExists(item))
                {
                    var ok = CreateDirectory(item, IntPtr.Zero);
                    if (!ok)
                    {
                        ThrowWin32Exception();
                    }
                }
            }
        }
    }

    public static async Task<Result> TryCreate(string directoryPath)
    {
        Result result = null;
        await Task.Run(() =>
        {
            try
            {
                Create(directoryPath);
                result = new Success();
            }
            catch (Exception e)
            {
                result = new Error(e);
            }
        });
        return result;
    }

    ///

    private static void Delete(IEnumerable<string> directories)
    {
        foreach (var directory in directories)
        {
            var files = GetFiles(directory, SearchOption.TopDirectoryOnly);
            foreach (string i in files)
                File.Delete(i);

            directories = GetFolders(directory, SearchOption.TopDirectoryOnly);
            Delete(directories);

            if (!RemoveDirectory(GetWin32LongPath(directory)))
                ThrowWin32Exception();
        }
    }

    public static void Delete(string path, bool recursive = false)
    {
        if (path.Length < MAX_PATH)
        {
            Directory.Delete(path, recursive);
        }
        else
        {
            if (!recursive)
            {
                bool ok = RemoveDirectory(GetWin32LongPath(path));
                if (!ok) ThrowWin32Exception();
            }
            else
            {
                var longPath = GetWin32LongPath(path);

                var files = GetFiles(longPath, SearchOption.TopDirectoryOnly);
                foreach (string i in files)
                    File.Delete(i);

                Delete([longPath]);
            }
        }
    }

    ///

    /// <summary>Multiple slashes and periods return <see langword="false"/>!</summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool Exists(string path)
    {
        if (path is null)
            return false;

        if (path.OnlyContains('/') || path.OnlyContains('\\') || path.OnlyContains('.'))
            return false;

        if (path.Length < MAX_PATH)
            return Directory.Exists(path);

        return LongExists(GetWin32LongPath(path));
    }

    ///

    /// <summary>Gets the path with actual casing by querying each parent folder in the path. Performance is poor due to multiple queries!</summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetActualPath(string path)
    {
        IEnumerable<string> folders;

        var last = path;
        var current = System.IO.Path.GetDirectoryName(path);

        var result = string.Empty;
        while (true)
        {
            folders = null;
            try
            {
                folders = Directory.EnumerateDirectories(current);
            }
            catch { }

            //This will always happen once we reach top-most folder (the associated drive)!
            if (folders is null || !folders.Any())
                break;

            foreach (var i in folders)
            {
                if (i.ToLower().Equals(last.ToLower()))
                {
                    var name = System.IO.Path.GetFileName(i);
                    result = result.IsEmpty() ? name : $@"{name}\{result}";
                    last = current;
                    break;
                }
            }
            current = System.IO.Path.GetDirectoryName(current);
        }

        var driveName = string.Empty;
        foreach (var i in Drive.Get())
        {
            if (path.ToLower().StartsWith(i.Name.ToLower()))
                driveName = i.Name;
        }
        return $@"{driveName}{result}";
    }

    ///

    public static IEnumerable<string> GetFiles(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (path == FilePath.Root)
            yield break;

        var searchPattern = "*";

        var files = new List<string>();
        var directories = new List<string> { path };

        if (searchOption == SearchOption.AllDirectories)
        {
            //Add all the subpaths
            directories.AddRange(GetFolders(path, SearchOption.AllDirectories));
        }

        foreach (var i in directories)
        {
            IntPtr findHandle = default;
            try
            {
                findHandle = FindFirstFile(System.IO.Path.Combine(GetWin32LongPath(i), searchPattern), out WIN32_FIND_DATA findData);
                if (findHandle != new IntPtr(-1))
                {
                    do
                    {
                        if ((findData.dwFileAttributes & FileAttributes.Directory) == 0)
                        {
                            string filename = System.IO.Path.Combine(i, findData.cFileName);
                            files.Add(GetCleanPath(filename));
                        }
                    } while (FindNextFile(findHandle, out findData));
                    FindClose(findHandle);
                }
            }
            catch (Exception)
            {
                if (findHandle != null)
                    FindClose(findHandle);

                throw;
            }
        }
        foreach (var i in files)
        {
            if (System.IO.Path.GetFileName(i).ToLower() == "desktop.ini")
            {
                //This file is used by Windows to determine how a folder is displayed in Windows Explorer. Ignore it!
                continue;
            }
            yield return i;
        }
    }

    public static IEnumerable<string> GetFolders(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (path == FilePath.Root)
            return [];

        var result = new List<string>();
        InternalGetDirectories(path, "*", searchOption, ref result);
        return [.. result];
    }

    public static IEnumerable<string> GetItems(string path)
    {
        foreach (var i in GetFolders(path))
            yield return i;

        foreach (var i in GetFiles(path))
            yield return i;
    }

    ///

    public static string GetParent(string folderPath)
    {
        var result = new DirectoryInfo(folderPath);
        return result.Parent is null ? FilePath.Root : result.Parent.FullName;
    }

    async public static Task<long> GetSize(string folderPath, CancellationToken token)
    {
        long result = 0;
        await Task.Run(async () =>
        {
            var files = Enumerable.Empty<string>();
            Try.Do(() => files = GetFiles(folderPath));
            foreach (var i in files)
            {
                if (token.IsCancellationRequested)
                    return;

                Try.Do(() =>
                {
                    var fileInfo = new FileInfo(i);
                    result += fileInfo.Length;
                });
            }

            var folders = Enumerable.Empty<string>();
            Try.Do(() => folders = GetFolders(folderPath));
            foreach (var i in folders)
            {
                if (token.IsCancellationRequested)
                    return;

                result += await GetSize(i, token);
            }
        }, token);
        return result;
    }

    ///

    public static void Move(string source, string destination)
    {
        if (source.Length < MAX_PATH || destination.Length < MAX_PATH)
        {
            Directory.Move(source, destination);
        }
        else if (!MoveFileW(GetWin32LongPath(source), GetWin32LongPath(destination)))
            ThrowWin32Exception();
    }

    #endregion

    #region External

    [DllImport("shell32.dll")]
    public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    private static string Combine(string path1, string path2)
    {
        return path1.TrimEnd('\\') + "\\" + path2.TrimStart('\\').TrimEnd('.');
    }

    private static List<string> GetAllPathsFromPath(string path)
    {
        bool unc = false;
        var prefix = @"\\?\";
        if (path.StartsWith(prefix + @"UNC\"))
        {
            unc = true;
        }
        var split = path.Split('\\');
        int i = unc ? 6 : 4;
        var list = new List<string>();
        var txt = "";

        for (int a = 0; a < i; a++)
        {
            if (a > 0) txt += "\\";
            txt += split[a];
        }
        for (; i < split.Length; i++)
        {
            txt = Combine(txt, split[i]);
            list.Add(txt);
        }

        return list;
    }

    private static string GetCleanPath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\")) return @"\\" + path[8..];
        if (path.StartsWith(@"\\?\")) return path[4..];
        return path;
    }

    private static string GetWin32LongPath(string path)
    {

        if (path.StartsWith(@"\\?\")) return path;

        var newpath = path;
        if (newpath.StartsWith("\\"))
        {
            newpath = @"\\?\UNC\" + newpath[2..];
        }
        else if (newpath.Contains(":"))
        {
            newpath = @"\\?\" + newpath;
        }
        else
        {
            var currdir = Environment.CurrentDirectory;
            newpath = Combine(currdir, newpath);
            while (newpath.Contains("\\.\\")) newpath = newpath.Replace("\\.\\", "\\");
            newpath = @"\\?\" + newpath;
        }
        return newpath.TrimEnd('.');
    }

    private static void InternalGetDirectories(string path, string searchPattern, System.IO.SearchOption searchOption, ref List<string> dirs)
    {
        IntPtr findHandle = default;

        try
        {
            findHandle = FindFirstFile(System.IO.Path.Combine(GetWin32LongPath(path), searchPattern), out WIN32_FIND_DATA findData);
            if (findHandle != new IntPtr(-1))
            {
                do
                {
                    if ((findData.dwFileAttributes & System.IO.FileAttributes.Directory) != 0)
                    {
                        if (findData.cFileName != "." && findData.cFileName != "..")
                        {
                            string subdirectory = System.IO.Path.Combine(path, findData.cFileName);
                            dirs.Add(GetCleanPath(subdirectory));
                            if (searchOption == SearchOption.AllDirectories)
                            {
                                InternalGetDirectories(subdirectory, searchPattern, searchOption, ref dirs);
                            }
                        }
                    }
                } while (FindNextFile(findHandle, out findData));
                FindClose(findHandle);
            }
            else
            {
                ThrowWin32Exception();
            }
        }
        catch (Exception)
        {
            if (findHandle != null)
                FindClose(findHandle);

            throw;
        }
    }

    private static bool LongExists(string path)
    {
        var attr = GetFileAttributesW(path);
        return (attr != INVALID_FILE_ATTRIBUTES && ((attr & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY));
    }

    [DebuggerStepThrough]
    private static void ThrowWin32Exception()
    {
        int code = Marshal.GetLastWin32Error();
        if (code != 0)
        {
            throw new System.ComponentModel.Win32Exception(code);
        }
    }

    #endregion
}