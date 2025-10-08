using Ion.Core;
using System;
using System.IO;

namespace Ion.Storage;

/// <summary>Lightweight version of <see cref="System.IO.FileSystemInfo"/>.</summary>
public record class ItemProperties : Model
{
    public DateTime Created { get => Get<DateTime>(); private set => Set(value); }

    public string Path { get => Get(""); private set => Set(value); }

    public DateTime LastAccessed { get => Get<DateTime>(); private set => Set(value); }

    public DateTime LastModified { get => Get<DateTime>(); private set => Set(value); }

    public long Size { get => Get<long>(); private set => Set(value); }

    private ItemProperties() : base() { }

    public ItemProperties(string path) : this()
    {
        var type = XItemPath.GetType(path);
        FileSystemInfo result;
        switch (type)
        {
            case ItemType.Drive:
                var driveInfo = new DriveInfo(path);

                Path = path;
                Size = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;
                return;

            case ItemType.File:
            case ItemType.Shortcut:
                result = new FileInfo(path);
                break;

            case ItemType.Folder:
                result = new DirectoryInfo(path);
                break;

            default: throw new NotSupportedException();
        }

        if (result is not null)
        {
            Created
                = result.CreationTime;
            LastAccessed
                = result.LastAccessTime;
            LastModified
                = result.LastWriteTime;
            Path
                = result.FullName;

            if (result is FileInfo fileInfo)
                Size = fileInfo.Length;
        }
    }

    public static ItemProperty Compare(ItemProperties a, ItemProperties b)
    {
        var result = ItemProperty.None;

        if (a.LastAccessed != b.LastAccessed)
            result |= ItemProperty.Accessed;

        if (a.Created != b.Created)
            result |= ItemProperty.Created;

        if (a.LastModified != b.LastModified)
            result |= ItemProperty.Modified;

        if (System.IO.Path.GetFileNameWithoutExtension(a.Path).ToLower() != System.IO.Path.GetFileNameWithoutExtension(b.Path).ToLower())
            result |= ItemProperty.Name;

        if (a.Size != b.Size)
            result |= ItemProperty.Size;

        if (System.IO.Path.GetExtension(a.Path).ToLower() != System.IO.Path.GetExtension(b.Path).ToLower())
            result |= ItemProperty.Type;

        return result;
    }
}