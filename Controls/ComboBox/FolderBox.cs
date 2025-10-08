using Ion.Storage;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class FolderBox : ComboBox, IStorageControl
{
    /// <see cref="Region.Field"/>

    private readonly FolderBoxDropHandler DropHandler;

    private readonly Storage.ItemList items = new(Ion.Storage.Filter.Default);

    /// <see cref="Region.Property"/>

    public string Path
    {
        get => XStorage.GetPath(this);
        set => XStorage.SetPath(this, value);
    }

    /// <see cref="Region.Constructor"/>

    static FolderBox()
    {
        ItemsSourceProperty.OverrideMetadata(typeof(FolderBox), new FrameworkPropertyMetadata(null, null, OnItemsSourceCoerced));
    }

    public FolderBox() : base()
    {
        DropHandler = new(this);
        GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(this, DropHandler);

        this.AddHandler(OnLoad, OnUnload);
        SetCurrentValue(ItemsSourceProperty, items);
    }

    /// <see cref="Region.Method"/>

    private void OnLoad()
    {
        items.Subscribe();
        _ = items.RefreshAsync(Path);

        this.AddPathChanged(OnPathChanged);
    }

    private void OnUnload()
    {
        items.Unsubscribe();
        items.Clear();

        this.RemovePathChanged(OnPathChanged);
    }

    private static object OnItemsSourceCoerced(DependencyObject sender, object input)
    {
        if (sender is FolderBox box)
        {
            if (input != box.items)
                throw new NotSupportedException();
        }
        return input;
    }

    protected virtual void OnPathChanged(object sender, PathChangedEventArgs e) => _ = items.RefreshAsync(e.Path);
}