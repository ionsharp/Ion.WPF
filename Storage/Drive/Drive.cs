using Ion.Data;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ion.Storage;

[Image(Images.Drive)]
[Name("Drive")]
[Styles.Object(MemberViewType = MemberViewType.Tab)]
public sealed record class Drive : Container
{
    private enum Category { Attributes, Size }

    [TabView(View = View.Main)]
    private enum Tab { }

    /// <see cref="Region.Property.Public"/>
    #region

    public string Format { get => Get(""); set => Set(value); }

    [Group(Category.Size), Name("Available free space")]
    [Styles.Text(CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public long AvailableSize { get => Get(0L); set => Set(value); }

    [Group(Category.Size), Name("Total size")]
    [Styles.Text(CanEdit = false,
        ValueConvert = typeof(ConvertFileSize),
        ValueConvertParameter = FileSizeFormat.BinaryUsingSI)]
    public long TotalSize { get => Get(0L); set => Set(value); }

    #endregion

    /// <see cref="Region.Property.Public.Override"/>
    #region

    public override DateTime Created { get => base.Created; set => base.Created = value; }

    [Group(Category.Attributes), Name("Hidden")]
    [Style(CanEdit = false)]
    public override bool IsHidden
    {
        get => base.IsHidden;
        set => base.IsHidden = value;
    }

    [Group(Category.Attributes), Name("ReadOnly")]
    [Style(CanEdit = false)]
    public override bool IsReadOnly
    {
        get => base.IsReadOnly;
        set => base.IsReadOnly = value;
    }

    public override DateTime LastAccessed { get => base.LastAccessed; set => base.LastAccessed = value; }

    public override DateTime LastModified { get => base.LastModified; set => base.LastModified = value; }

    public override long Size { get => base.Size; set => base.Size = value; }

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public Drive(DriveInfo driveInfo) : base(ItemType.Drive, FileOrigin.Local, driveInfo.Name)
    {
        Format
            = driveInfo.DriveFormat;

        AvailableSize
            = driveInfo.AvailableFreeSpace;
        TotalSize
            = driveInfo.TotalSize;
    }

    public Drive(string path) : base(ItemType.Drive, FileOrigin.Local, path) { }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    public override FileSystemInfo Read() => null;

    public static bool Exists(string path)
        => Drive.Get().Any(i => i.Name.ToLower() == path.ToLower());

    public static IEnumerable<DriveInfo> Get()
    {
        foreach (var i in DriveInfo.GetDrives())
        {
            if (i.IsReady)
                yield return i;
        }
    }

    public static IEnumerable<string> GetRemovable()
    {
        var drives = DriveInfo.GetDrives();
        if (drives?.Length > 0)
        {
            foreach (var i in drives)
            {
                if (i.DriveType == DriveType.Removable)
                    yield return i.Name;
            }
        }
    }

    #endregion
}