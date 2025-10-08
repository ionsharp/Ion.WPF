using System;

namespace Ion.Storage;

public class RemotePath() : object(), IExist
{
    public virtual bool Exists(ItemType target, string path)
    {
        if (path == FilePath.Root)
            return false;

        try
        {
            if (target == (ItemType.File | ItemType.Folder))
                return File.Exists(path) || Folder.Exists(path);

            if (target == ItemType.File)
                return File.Exists(path);

            if (target == ItemType.Folder)
                return Folder.Exists(path);

            throw new InvalidOperationException();
        }
        catch { return false; }
    }
}