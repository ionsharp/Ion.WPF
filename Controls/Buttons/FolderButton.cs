using Ion.Storage;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class FolderButton : ImageButton, IStorageControl
{
    /// <see cref="Region.Field"/>

    private readonly ContextMenu DefaultMenu;

    private readonly FolderButtonDropHandler DropHandler;

    /// <see cref="Region.Property"/>
    #region

    public Storage.ItemList Items { get; private set; } = new(Ion.Storage.Filter.Default);

    public static readonly DependencyProperty ItemStyleProperty = DependencyProperty.Register(nameof(ItemStyle), typeof(Style), typeof(FolderButton), new FrameworkPropertyMetadata(null));
    public Style ItemStyle
    {
        get => (Style)GetValue(ItemStyleProperty);
        set => SetValue(ItemStyleProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(FolderButton), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(FolderButton), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector ItemTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
        set => SetValue(ItemTemplateSelectorProperty, value);
    }

    public string Path
    {
        get => XStorage.GetPath(this);
        set => XStorage.SetPath(this, value);
    }

    #endregion

    /// <see cref="Region.Constructor"/>

    public FolderButton() : base()
    {
        DropHandler = new(this);
        GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(this, DropHandler);

        this.AddHandler(OnLoad, OnUnload);

        DefaultMenu = new();
        DefaultMenu.Bind(ContextMenu.ItemContainerStyleProperty,
            nameof(ItemStyle),
            this);
        DefaultMenu.Bind(ContextMenu.ItemsSourceProperty,
            nameof(Items),
            this);
        DefaultMenu.Bind(ContextMenu.ItemTemplateProperty,
            nameof(ItemTemplate),
            this);
        DefaultMenu.Bind(ContextMenu.ItemTemplateSelectorProperty,
            nameof(ItemTemplateSelector),
            this);

        SetCurrentValue(MenuProperty, DefaultMenu);
    }

    /// <see cref="Region.Method"/>

    private void OnLoad()
    {
        Items.Subscribe();
        _ = Items.RefreshAsync(Path);

        this.AddPathChanged(OnPathChanged);
    }

    private void OnUnload()
    {
        Items.Unsubscribe();
        Items.Clear();

        this.RemovePathChanged(OnPathChanged);
    }

    protected override void OnMenuChanged(ValueChange<ContextMenu> i)
    {
        base.OnMenuChanged(i);
        if (i.OldValue is not null)
            throw new NotSupportedException();
    }

    protected virtual void OnPathChanged(object sender, PathChangedEventArgs e) => _ = Items.RefreshAsync(e.Path);
}