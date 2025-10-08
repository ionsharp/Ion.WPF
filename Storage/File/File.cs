using Ion.Analysis;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ion.Storage;

[Image(Images.File)]
[Name("File")]
[Serializable]
[Styles.Object(MemberViewType = MemberViewType.Tab)]
public record class File(string path) : Item(ItemType.File, FileOrigin.Local, path)
{
    private enum Category { Content }

    [TabView(View = View.Main)]
    private enum Tab
    {
        [TabStyle(Image = Images.Exif)]
        Exif,
        [TabStyle(Image = Images.Numbers)]
        Summary,
    }

    public const string DefaultExtension = "data";

    #region Properties

    [Group(Category.Content)]
    [Styles.Number(0, int.MaxValue, 1,
        Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Characters { get => Get(0); private set => Set(value); }

    [Group(Category.Content)]
    [Styles.Number(0, int.MaxValue, 1,
        Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Lines { get => Get(0); private set => Set(value); }

    [Group(Category.Content)]
    [Styles.Number(0, int.MaxValue, 1,
        Tab = Tab.Summary,
        CanEdit = false,
        ValueFormat = NumberFormat.Default)]
    public int Words { get => Get(0); private set => Set(value); }

    [Styles.Path(Template.PathFile,
        CanEdit = false,
        Pin = Sides.LeftOrTop)]
    public override string Path
    {
        get => base.Path; set => base.Path = value;
    }

    #endregion

    #region File

    #endregion

    #region Methods

    public sealed override FileSystemInfo Read() => new FileInfo(Path);

    async protected override Task RefreshContent(CancellationToken token)
    {
        RefreshingContent = true;

        Characters = 0; Lines = 0; Size = 0; Words = 0;
        await Task.Run(() =>
        {
            FileInfo fileInfo = null;
            var fileText = string.Empty;

            var result = Try.Do(() =>
            {
                fileInfo = new FileInfo(Path);
                fileText = ReadAllText(Path, Encoding.Unicode);
            },
            e => Log.Write(e));

            Dispatch.Do(() =>
            {
                Characters += fileText.Length; Size += fileInfo.Length;
                Try.Do(() => { Lines += fileText.GetLineCount(); Words += fileText.GetWordCount(); }, e => Log.Write(e));
            });
        },
        token);

        RefreshingContent = false;
    }

    #endregion

    #region Methods / Static

    public static void AppendAllText(string path, string contents)
    {
        AppendAllText(path, contents, Encoding.Default);
    }

    public static void AppendAllText(string path, string contents, Encoding encoding)
    {
        if (path.Length < MAX_PATH)
        {
            System.IO.File.AppendAllText(path, contents, encoding);
        }
        else
        {
            var fileHandle = CreateFileForAppend(GetWin32LongPath(path));
            using var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Write);
            var bytes = encoding.GetBytes(contents);
            fs.Position = fs.Length;
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    ///

    public static FileAttributes Attributes(string path)
    {
        if (path.Length < MAX_PATH)
        {
            return System.IO.File.GetAttributes(path);
        }
        else
        {
            var longFilename = GetWin32LongPath(path);
            return (FileAttributes)GetFileAttributesW(longFilename);
        }
    }

    public static void AddAttribute(string path, FileAttributes input)
    {
        if (path.Length < MAX_PATH)
        {
            System.IO.File.SetAttributes(path, System.IO.File.GetAttributes(path) | input);
        }
        else
        {
            var longFilename = GetWin32LongPath(path);
            SetFileAttributesW(longFilename, (int)(Attributes(path) | input));
        }
    }

    public static void RemoveAttribute(string path, FileAttributes input)
    {
        FileAttributes current;
        if (path.Length < MAX_PATH)
        {
            current = System.IO.File.GetAttributes(path);
            current &= ~input;
            System.IO.File.SetAttributes(path, current);
        }
        else
        {
            current = Attributes(path);
            current &= ~input;
            var longFilename = GetWin32LongPath(path);
            SetFileAttributesW(longFilename, (int)current);
        }
    }

    ///

    /// <summary>
    /// Gets a new path based on the given path.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <param name="nameFormat">How to format the name (not including extension).</param>
    /// <returns>A new path based on the old path.</returns>
    public static string ClonePath(string path, string nameFormat = FilePath.DefaultCloneFormat) => FilePath.CloneName(path, nameFormat, i => Exists(i));

    ///

    public static void Copy(string source, string target, bool overwrite = false)
    {
        if (source.Length < MAX_PATH && (target.Length < MAX_PATH))
        {
            System.IO.File.Copy(source, target, overwrite);
        }
        else
        {
            var ok = CopyFileW(GetWin32LongPath(source), GetWin32LongPath(target), !overwrite);
            if (!ok) ThrowWin32Exception();
        }
    }

    public static void Delete(string path)
    {
        if (path.Length < MAX_PATH) System.IO.File.Delete(path);
        else
        {
            bool ok = DeleteFileW(GetWin32LongPath(path));
            if (!ok) ThrowWin32Exception();
        }
    }

    public static bool Exists(string path)
    {
        if (path is null)
            return false;

        if (path.Length < MAX_PATH)
            return System.IO.File.Exists(path);

        var attribute = GetFileAttributesW(GetWin32LongPath(path));
        return (attribute != INVALID_FILE_ATTRIBUTES && ((attribute & FILE_ATTRIBUTE_ARCHIVE) == FILE_ATTRIBUTE_ARCHIVE));
    }

    ///

    public static DateTime GetLastAccessed(string path)
    {
        long cTime = 0, aTime = 0, mTime = 0;
        using var handle = GetFileHandleWithWrite(path);
        GetFileTime(handle, ref cTime, ref aTime, ref mTime);
        return DateTime.FromFileTimeUtc(aTime);
    }

    public static void SetLastAccessed(string path, DateTime value)
    {
        long cTime = 0, aTime = 0, mTime = 0;
        using var handle = GetFileHandleWithWrite(path);
        GetFileTime(handle, ref cTime, ref aTime, ref mTime);

        var fileTime = value.ToFileTimeUtc();
        if (!SetFileTime(handle, ref cTime, ref fileTime, ref mTime))
        {
            throw new Win32Exception();
        }
    }

    public static DateTime GetCreated(string path)
    {
        long cTime = 0, aTime = 0, mTime = 0;
        using var handle = GetFileHandleWithWrite(path);
        GetFileTime(handle, ref cTime, ref aTime, ref mTime);
        return DateTime.FromFileTimeUtc(cTime);
    }

    public static void SetCreated(string path, DateTime value)
    {
        long cTime = 0, aTime = 0, mTime = 0;
        using var handle = GetFileHandleWithWrite(path);
        GetFileTime(handle, ref cTime, ref aTime, ref mTime);
        var fileTime = value.ToFileTimeUtc();
        if (!SetFileTime(handle, ref fileTime, ref aTime, ref mTime))
        {
            throw new Win32Exception();
        }
    }

    public static DateTime GetModified(string path)
    {
        long cTime = 0, aTime = 0, mTime = 0;
        using var handle = GetFileHandleWithWrite(path);
        GetFileTime(handle, ref cTime, ref aTime, ref mTime);
        return DateTime.FromFileTimeUtc(mTime);
    }

    public static void SetModified(string path, DateTime value)
    {
        long cTime = 0, aTime = 0, mTime = 0;
        using var handle = GetFileHandleWithWrite(path);
        GetFileTime(handle, ref cTime, ref aTime, ref mTime);

        var fileTime = value.ToFileTimeUtc();
        if (!SetFileTime(handle, ref cTime, ref aTime, ref fileTime))
        {
            throw new Win32Exception();
        }
    }

    ///

    public static FileStream GetFileStream(string filename, FileAccess access = FileAccess.Read)
    {
        var longName = GetWin32LongPath(filename);

        SafeFileHandle hfile;
        if (access == FileAccess.Write)
        {
            hfile = CreateFile(longName, (int)(FILE_GENERIC_READ | FILE_GENERIC_WRITE | FILE_WRITE_ATTRIBUTES), FILE_SHARE_NONE, IntPtr.Zero, CREATE_NEW, 0, IntPtr.Zero);
        }
        else hfile = CreateFile(longName, (int)FILE_GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

        if (hfile.IsInvalid)
            ThrowWin32Exception();

        return new FileStream(hfile, access);
    }

    public static int GetIndex(string path)
    {
        int i = 0;
        foreach (var j in Folder.GetFiles(System.IO.Path.GetDirectoryName(path)))
        {
            if (j == path)
                return i;

            i++;
        }
        return -1;
    }

    public static ImageSource GetThumbnail(string path)
    {
        BitmapMetadata meta = null;

        double angle = 0;
        var orientation = ExifOrientations.None;
        BitmapSource result;
        try
        {
            //Attempt of creation of the thumbnail via Bitmap frame: very fast and very inexpensive memory!
            var frame = BitmapFrame.Create(new Uri(path), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            if (frame.Thumbnail is null) // failure, attempts with BitmapImage (slower and expensive in memory)
            {
                MemoryStream ms = new();
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                ms.Write(bytes, 0, bytes.Length);

                var image = new BitmapImage()
                {
                    CacheOption = BitmapCacheOption.None,
                    CreateOptions = BitmapCreateOptions.DelayCreation,
                    DecodePixelHeight = 256
                };

                image.BeginInit();
                image.StreamSource = ms;
                image.EndInit();

                if (image.CanFreeze)
                    image.Freeze(); //To avoid memory leak 

                result = image;
            }
            else
            {
                //Get the image meta
                meta = frame.Metadata as BitmapMetadata;
                result = frame.Thumbnail;
            }

            if ((meta != null) && (result != null)) //si on a des meta, tentative de récupération de l'orientation
            {
                if (meta.GetQuery("/app1/ifd/{ushort=274}") != null) orientation = (ExifOrientations)Enum.Parse(typeof(ExifOrientations), meta.GetQuery("/app1/ifd/{ushort=274}").ToString());
                switch (orientation)
                {
                    case ExifOrientations.Rotate90:
                        angle = -90;
                        break;
                    case ExifOrientations.Rotate180:
                        angle = 180;
                        break;
                    case ExifOrientations.Rotate270:
                        angle = 90;
                        break;
                }
                if (angle != 0) // we have to rotate the image
                {
                    result = new TransformedBitmap(result.Clone(), new RotateTransform(angle));
                    result.Freeze();
                }
            }
        }
        catch
        {
            return null;
        }
        return result;
    }

    ///

    public static void Hide(string filePath, bool hide)
    {
        if (hide)
        {
            AddAttribute(filePath, FileAttributes.Hidden);
            return;
        }
        RemoveAttribute(filePath, FileAttributes.Hidden);
    }

    public static void Move(string source, string target)
    {
        if (source.Length < MAX_PATH && (target.Length < MAX_PATH)) System.IO.File.Move(source, target);
        else
        {
            var ok = MoveFileW(GetWin32LongPath(source), GetWin32LongPath(target));
            if (!ok) ThrowWin32Exception();
        }
    }

    public static Result TryOpen(string filePath, string arguments = null) => Try.Do(() => Process.Start(filePath, arguments));

    ///

    public static byte[] ReadAllBytes(string path)
    {
        if (path.Length < MAX_PATH) return System.IO.File.ReadAllBytes(path);
        var fileHandle = GetFileHandle(GetWin32LongPath(path));

        using var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Read);
        var data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        return data;
    }

    public static string[] ReadAllLines(string path, Encoding encoding)
    {
        if (path.Length < MAX_PATH) { return System.IO.File.ReadAllLines(path, encoding); }
        var fileHandle = GetFileHandle(GetWin32LongPath(path));

        using var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Read);
        var data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        var str = encoding.GetString(data);
        if (str.Contains("\r")) return str.Split(new[] { "\r\n" }, StringSplitOptions.None);
        return str.Split('\n');
    }

    public static string ReadAllText(string path, Encoding encoding)
    {
        if (path.Length < MAX_PATH) { return System.IO.File.ReadAllText(path, encoding); }
        var fileHandle = GetFileHandle(GetWin32LongPath(path));

        using var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Read);
        var data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        return encoding.GetString(data);
    }

    ///

    public static void WriteAllBytes(string path, byte[] bytes)
    {
        if (path.Length < MAX_PATH)
        {
            System.IO.File.WriteAllBytes(path, bytes);
        }
        else
        {
            var fileHandle = CreateFileForWrite(GetWin32LongPath(path));

            using var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Write);
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    public static void WriteAllText(string path, string contents)
    {
        WriteAllText(path, contents, Encoding.Default);
    }

    public static void WriteAllText(string path, string contents, Encoding encoding)
    {
        if (path.Length < MAX_PATH)
        {
            System.IO.File.WriteAllText(path, contents, encoding);
        }
        else
        {
            var fileHandle = CreateFileForWrite(GetWin32LongPath(path));

            using var fs = new System.IO.FileStream(fileHandle, System.IO.FileAccess.Write);
            var bytes = encoding.GetBytes(contents);
            fs.Write(bytes, 0, bytes.Length);
        }
    }

    #endregion

    #region External

    private static string Combine(string path1, string path2)
    {
        return path1.TrimEnd('\\') + "\\" + path2.TrimStart('\\').TrimEnd('.'); ;
    }

    ///

    private static SafeFileHandle CreateFileForAppend(string filename)
    {
        if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
        SafeFileHandle hfile = CreateFile(filename, (int)FILE_GENERIC_WRITE, FILE_SHARE_NONE, IntPtr.Zero, CREATE_NEW, 0, IntPtr.Zero);
        if (hfile.IsInvalid)
        {
            hfile = CreateFile(filename, (int)FILE_GENERIC_WRITE, FILE_SHARE_NONE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (hfile.IsInvalid) ThrowWin32Exception();
        }
        return hfile;
    }

    private static SafeFileHandle CreateFileForWrite(string filename)
    {
        if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
        SafeFileHandle hfile = CreateFile(filename, (int)FILE_GENERIC_WRITE, FILE_SHARE_NONE, IntPtr.Zero, CREATE_ALWAYS, 0, IntPtr.Zero);
        if (hfile.IsInvalid) ThrowWin32Exception();
        return hfile;
    }

    ///

    internal static SafeFileHandle GetFileHandle(string filename)
    {
        if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
        SafeFileHandle hfile = CreateFile(filename, (int)FILE_GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        if (hfile.IsInvalid) ThrowWin32Exception();
        return hfile;
    }

    internal static SafeFileHandle GetFileHandleWithWrite(string filename)
    {
        if (filename.Length >= MAX_PATH) filename = GetWin32LongPath(filename);
        SafeFileHandle hfile = CreateFile(filename, (int)(FILE_GENERIC_READ | FILE_GENERIC_WRITE | FILE_WRITE_ATTRIBUTES), FILE_SHARE_NONE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
        if (hfile.IsInvalid) ThrowWin32Exception();
        return hfile;
    }

    ///

    private static string GetWin32LongPath(string path)
    {
        if (path.StartsWith(@"\\?\")) return path;

        if (path.StartsWith("\\"))
        {
            path = @"\\?\UNC\" + path[2..];
        }
        else if (path.Contains(":"))
        {
            path = @"\\?\" + path;
        }
        else
        {
            var currdir = Environment.CurrentDirectory;
            path = Combine(currdir, path);
            while (path.Contains("\\.\\")) path = path.Replace("\\.\\", "\\");
            path = @"\\?\" + path;
        }
        return path.TrimEnd('.'); ;
    }

    [DebuggerStepThrough]
    private static void ThrowWin32Exception()
    {
        var code = Marshal.GetLastWin32Error();

        if (code != 0)
            throw new Win32Exception(code);
    }

    #endregion
}