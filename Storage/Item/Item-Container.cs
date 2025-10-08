using Ion.Core;
using System.IO;

namespace Ion.Storage;

/// <summary>Represents a <see cref="Folder"/> or <see cref="Drive"/>.</summary>
public abstract record class Container(ItemType type, FileOrigin origin, string path) : Item(type, origin, path)
{
    [Hide]
    public ItemList Items { get => Get(new ItemList()); private set => Set(value); }

    [Styles.Path(Template.PathFolder,
        CanEdit = false,
        Pin = Sides.LeftOrTop)]
    public override string Path { get => base.Path; set => base.Path = value; }

    public override FileSystemInfo Read() => Try.Get(() => new DirectoryInfo(Path));

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        e.PropertyName.IfNotNull(i => i == nameof(Path), i => Items.Path = Path);
    }
}