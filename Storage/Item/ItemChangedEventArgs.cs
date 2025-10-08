using Ion.Input;
using System;

namespace Ion.Storage;

public class ItemChangedEventArgs<T>(T item, ItemProperty itemProperty) : EventArgs<T>(item, itemProperty)
{
    public new ItemProperty Parameter => (ItemProperty)base.Parameter;
}

public class ItemCreatedEventArgs<T>(T item) : EventArgs<T>(item)
{
}

public class ItemDeletedEventArgs(string path) : EventArgs<string>(path)
{
    public string Path => A;
}

public class ItemRenamedEventArgs(string oldPath, string newPath) : EventArgs()
{
    public readonly string OldPath = oldPath;

    public readonly string NewPath = newPath;
}