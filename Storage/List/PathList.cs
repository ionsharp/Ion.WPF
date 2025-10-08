using System.IO;
using System.Linq;

namespace Ion.Storage;

public class PathList : StorageList<string>
{
    /// <see cref="Region.Property.Indexor"/>

    protected override string this[string path] => this.FirstOrDefault(i => i == path);

    /// <see cref="Region.Constructor"/>
    #region

    public PathList() : base() { }

    public PathList(Filter filter) : base(filter) { }

    public PathList(string path, Filter filter) : base(path, filter) { }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    protected override void OnItemRenamed(RenamedEventArgs e)
    {
        base.OnItemRenamed(e);
        var index = IndexOf(e.OldFullPath);
        this[index] = e.FullPath;
    }

    ///

    protected override string ToDrive(DriveInfo input) => input.Name;

    protected override string ToFile(string input) => input;

    protected override string ToFolder(string input) => input;

    #endregion
}