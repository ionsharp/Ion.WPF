using Ion;
using Ion.Colors;
using System.IO;
using System.Linq;

namespace Ion.Storage;

/// <inheritdoc/>
public class ItemList : StorageList<Item>
{
    /// <see cref="Region.Property.Indexor"/>

    protected override Item this[string path] => this.FirstOrDefault(i => i.Path == path);

    /// <see cref="Region.Constructor"/>
    #region

    public ItemList() : base() { }

    public ItemList(Filter filter) : base(filter) { }

    public ItemList(string path, Filter filter) : base(path, filter) { }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    protected override Item ToDrive(DriveInfo input)
    {
        var result = new Drive(input);
        result.Refresh();
        return result;
    }

    protected override Item ToFile(string input)
    {
        var result = Shortcut.Is(input) ? new Shortcut(input) : new File(input);
        result.Refresh();
        return result;
    }

    protected override Item ToFolder(string input)
    {
        var result = new Folder(input);
        result.Refresh();
        return result;
    }

    ///

    protected override void OnItemCreated(Item i)
    {
        base.OnItemCreated(i);
        i.Refresh();
    }

    protected override void OnItemRenamed(RenamedEventArgs e)
    {
        base.OnItemRenamed(e);
        this[e.OldFullPath]?.Refresh(e.FullPath);
    }

    ///

    protected override void OnAdded(Collect.ListAddedEventArgs e)
    {
        base.OnAdded(e);
        subscribed.If(() => e.NewItem.If<Container>(i => i.Items.Subscribe()));
    }

    protected override void OnRemoved(Collect.ListRemovedEventArgs e)
    {
        base.OnRemoved(e);
        e.OldItem.If<Container>(i => i.Items.Clear());
    }

    ///

    public override void Subscribe()
    {
        base.Subscribe();
        this.ForEach<Container>
            (i => i.Items.Subscribe());
        this.ForEach<Shortcut>
            (i => i.Items.Subscribe());
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        this.ForEach<Container>
            (i => i.Items.Unsubscribe());
        this.ForEach<Shortcut>
            (i => i.Items.Unsubscribe());
    }

    #endregion
}