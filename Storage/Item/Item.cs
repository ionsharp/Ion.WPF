using Ion.Core;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using Ion.Threading;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ion.Storage;

[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
[Serializable]
public abstract record class Item : Namable, IItemProperties
{
    private enum Category { Attributes, Size }

    [TabView(View = View.Main)]
    private enum Tab
    {
        [TabStyle(Image = Images.Properties)]
        Properties
    }

    /// <see cref="Region.Field"/>
    #region

    protected const int INVALID_FILE_ATTRIBUTES = -1;
    protected const int FILE_ATTRIBUTE_ARCHIVE = 0x20;

    protected const int FILE_READ_DATA = 0x0001;
    protected const int FILE_WRITE_DATA = 0x0002;
    protected const int FILE_APPEND_DATA = 0x0004;
    protected const int FILE_READ_EA = 0x0008;
    protected const int FILE_WRITE_EA = 0x0010;

    protected const int FILE_READ_ATTRIBUTES = 0x0080;
    protected const int FILE_WRITE_ATTRIBUTES = 0x0100;

    protected const int FILE_SHARE_NONE = 0x00000000;
    protected const int FILE_SHARE_READ = 0x00000001;

    protected const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

    protected const long FILE_GENERIC_WRITE = STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE;

    protected const long FILE_GENERIC_READ = STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE;

    protected const long READ_CONTROL = 0x00020000L;
    protected const long STANDARD_RIGHTS_READ = READ_CONTROL;
    protected const long STANDARD_RIGHTS_WRITE = READ_CONTROL;

    protected const long SYNCHRONIZE = 0x00100000L;

    protected const int CREATE_NEW = 1;
    protected const int CREATE_ALWAYS = 2;
    protected const int OPEN_EXISTING = 3;

    protected const int MAX_PATH = 260;
    protected const int MAX_ALTERNATE = 14;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    protected struct WIN32_FIND_DATA
    {
        public FileAttributes dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public uint nFileSizeHigh; //changed all to uint, otherwise you run into unexpected overflow
        public uint nFileSizeLow;  //|
        public uint dwReserved0;   //|
        public uint dwReserved1;   //v
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
        public string cAlternate;
    }

    private readonly Taskable content;

    private static readonly Dictionary<string, System.ComponentModel.PropertyChangedEventArgs> EventArgCache;

    #endregion

    /// <see cref="Region.Property.Public"/>
    #region

    [Filter(Ion.Filter.Group | Ion.Filter.Search | Ion.Filter.Sort)]
    public override string Name { get => base.Name; set => base.Name = value; }

    [Filter(Ion.Filter.Search | Ion.Filter.Sort)]
    [Styles.Text(CanEdit = false,
        Pin = Sides.LeftOrTop)]
    public virtual string Path { get => Get(""); set => Set(value); }

    public int Permissions { get => Get(0); set => Set(value); }

    public ItemProperties Properties { get => Get<ItemProperties>(); set => Set(value); }

    public bool RefreshingContent { get => Get(false); set => Set(value); }

    [Filter(Ion.Filter.Group | Ion.Filter.Sort)]
    public ItemType Type { get => Get<ItemType>(); protected set => Set(value); }

    /// <see cref="Tab.Properties"/>
    #region

    [Filter(Ion.Filter.Sort)]
    [Name("Created")]
    [Styles.Text(Tab = Tab.Properties,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        Update = true)]
    public virtual DateTime Created { get => Get<DateTime>(); set => Set(value); }

    private bool isHidden;
    [Filter(Ion.Filter.Group | Ion.Filter.Sort)]
    [Group(Category.Attributes)]
    [Name("Hidden")]
    [Style(Tab = Tab.Properties)]
    public virtual bool IsHidden
    {
        get => isHidden;
        set
        {
            Try.Do(() =>
            {
                if (value)
                    File.AddAttribute(Path, FileAttributes.Hidden);

                else File.RemoveAttribute(Path, FileAttributes.Hidden);
                isHidden = value;
            },
            e => Analysis.Log.Write(e));
            Reset(() => IsHidden);
        }
    }

    private bool isReadOnly;
    [Filter(Ion.Filter.Group | Ion.Filter.Sort)]
    [Group(Category.Attributes)]
    [Name("ReadOnly")]
    [Style(Tab = Tab.Properties)]
    public virtual bool IsReadOnly
    {
        get => isReadOnly;
        set
        {
            Try.Do(() =>
            {
                if (value)
                    File.AddAttribute(Path, FileAttributes.ReadOnly);

                else File.RemoveAttribute(Path, FileAttributes.ReadOnly);
                isReadOnly = value;
            },
            e => Analysis.Log.Write(e));
            Reset(() => IsReadOnly);
        }
    }

    [Filter(Ion.Filter.Sort)]
    [Name("Accessed")]
    [Styles.Text(Tab = Tab.Properties,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        Update = true)]
    public virtual DateTime LastAccessed { get => Get<DateTime>(); set => Set(value); }

    [Filter(Ion.Filter.Sort)]
    [Name("Modified")]
    [Styles.Text(Tab = Tab.Properties,
        CanEdit = false,
        ValueConvert = typeof(ConvertToStringTimeRelative),
        Update = true)]
    public virtual DateTime LastModified { get => Get<DateTime>(); set => Set(value); }

    [Filter(Ion.Filter.Sort)]
    [Group(Category.Size)]
    [Name("Size")]
    [Styles.Text(Tab = Tab.Properties,
        CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public virtual long Size { get => Get(0L); set => Set(value); }

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    private Item() : base() => content = new(RefreshContent, TaskStrategy.FinishAndRestart);

    protected Item(ItemType type, FileOrigin origin, string path) : this()
    {
        Type = type; Path = path;
    }

    static Item()
    {
        EventArgCache = [];
    }

    #endregion

    /// <see cref="Region.Method.Import"/>
    #region

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool CopyFileW(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern int GetFileAttributesW(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool DeleteFileW(string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool MoveFileW(string lpExistingFileName, string lpNewFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool GetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool FindClose(IntPtr hFindFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool RemoveDirectory(string path);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    protected static extern int SetFileAttributesW(string lpFileName, int fileAttributes);

    #endregion

    /// <see cref="Region.Method.Private"/>

    private void Refresh(FileSystemInfo i)
    {
        if (i is null)
            return;

        LastAccessed
            = i.LastAccessTime;
        Created
            = i.CreationTime;

        ///

        isHidden
            = this is not Drive && Path != FilePath.Root && i.Attributes.HasFlag(FileAttributes.Hidden);
        Reset(() => IsHidden);

        isReadOnly
            = i.Attributes.HasFlag(FileAttributes.ReadOnly);
        Reset(() => IsReadOnly);

        ///

        LastModified
            = i.LastWriteTime;
        Name
            = System.IO.Path.GetFileName(i.FullName);

        if (i is FileInfo)
        {
            Size = i.To<FileInfo>().Length;
        }
        else if (i is DirectoryInfo) { }

        Properties = new ItemProperties(i.FullName);
    }

    /// <see cref="Region.Method.Public"/>
    #region

    public abstract FileSystemInfo Read();

    ///

    public async Task RefreshAsync() => await RefreshAsync(Path);

    public async Task RefreshAsync(string Path) => await Task.Run(() => Refresh(Path));

    ///

    public void Refresh() => Refresh(Read());

    public void Refresh(string path)
    {
        Path = path;
        Refresh();
    }

    ///

    protected virtual Task RefreshContent(CancellationToken token) => default;

    public void RefreshContent() => _ = content.Start();

    #endregion

    /// <see cref="ICommand"/>
    #region

    private ICommand refreshContentCommand;
    [Name("Refresh content"), Image(Images.Refresh), Style(Template.ButtonDefault, Pin = Sides.RightOrBottom), VisibilityTrigger(nameof(RefreshingContent), false)]
    public ICommand RefreshContentCommand => refreshContentCommand ??= new RelayCommand(RefreshContent, () => !content.IsStarted);

    private ICommand cancelRefreshContentCommand;
    [Name("Cancel refreshing content"), Image(Images.Block), Style(Template.ButtonCancel, Pin = Sides.RightOrBottom), VisibilityTrigger(nameof(RefreshingContent), true)]
    public ICommand CancelRefreshContentCommand => cancelRefreshContentCommand ??= new RelayCommand(() => content.Cancel(), () => content.IsStarted);

    #endregion
}