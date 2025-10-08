using GongSolutions.Wpf.DragDrop;
using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Local;
using Ion.Reflect;
using Ion.Storage;
using Ion.Threading;
using Ion.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Description("Explore the file system.")]
[Image(Images.WindowsExplorer)]
[Name("Explore")]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
[Serializable]
public record class ItemViewPanel : DataViewPanel<Item>
{
    private enum Group { File, Folder }

    [TabView(View = Ion.View.Main)]
    private new enum Tab
    {
        [TabStyle(Image = Images.Filter)]
        Filter
    }

    /// <see cref="Region.Event"/>

    [field: NonSerialized]
    public event EventHandler<EventArgs<string>> FileOpened;

    [field: NonSerialized]
    public event EventHandler<EventArgs<string>> FolderOpened;

    /// <see cref="Region.Field"/>
    #region

    public static readonly new Controls.ResourceKey Template = new();

    public static readonly ListLimit DefaultHistoryLimit = new(50);

    private static readonly Dictionary<string, FolderOptions> FolderOptions = [];

    private readonly Handle handleFolderOptions = false;

    private readonly Taskable itemLengthTask;

    private readonly Taskable<IReadOnlyCollection<object>> selectionLengthTask;

    private readonly Storage.ItemList items = [];

    #endregion

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="Region.Property.Public.Override"/>
    #region

    public override bool CanAdd => false;

    public override bool CanAddFromPreset => false;

    public override bool CanClone => false;

    public override bool CanCopy => false;

    public override bool CanCopyTo => false;

    public override bool CanCut => false;

    public override bool CanDrag => true;

    public override bool CanDrop => true;

    public override bool CanMoveTo => false;

    public override bool CanPaste => false;

    public override bool CanRemove => false;

    public override bool CanSearch => true;

    public override ConverterSelector GroupConverterSelector => new ItemGroupConverterSelector();

    public override int ItemCount => base.ItemCount;

    public override int ItemSelectedCount => base.ItemSelectedCount;

    public override string ItemNameLabel => "Name";

    public override string ItemDescriptionLabel => "Type";

    public override string ItemDetail1Label => "Last modified";

    public override string ItemDetail2Label => "Size";

    public override string KeySelectProperty => nameof(Item.Name);

    #endregion

    /// <see cref="Region.Property.Public.Virtual"/>
    #region

    public virtual string DefaultPath => FilePath.Root;

    public virtual bool InvalidPathAlert => true;

    public virtual string InvalidPathAlertMessage => "FolderDoesNotExist";

    public virtual string InvalidPathAlertTitle => "FolderNotFound";

    #endregion

    /// <see cref="Region.Property.Public"/>
    #region

    [PropertyPanel.Target]
    public Item Folder => Path.IfNotNullGet(i => new Folder(i));

    public HistoryOfString History { get => Get<HistoryOfString>(); set => Set(value); }

    public bool IsAddressVisible { get => Get(false); set => Set(value); }

    public bool IsReadOnly { get => Get(false); set => Set(value); }

    public long ItemLength { get => Get<long>(); private set => Set(value); }

    public MouseClick OpenFileOn { get => Get(MouseClick.Double); set => Set(value); }

    public MouseClick OpenFolderOn { get => Get(MouseClick.Double); set => Set(value); }

    public long SelectionLength { get => Get<long>(); private set => Set(value); }

    #endregion

    /// <see cref="View.Footer"/>
    #region

    [Styles.TextAttribute(Pin = Sides.RightOrBottom,
        CanEdit = false,
        CanSelect = false,
        View = Ion.View.Footer)]
    public string ItemCountText
    {
        get
        {
            var length = ItemCount > 0
                ? $" ({new FileSize(ItemLength).ToString(SizeFormat)})"
                : "";

            return $"{ItemCount} items{length}";
        }
    }

    [Styles.TextAttribute(Pin = Sides.RightOrBottom,
        CanEdit = false,
        CanSelect = false,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(ItemSelectedCount), Comparison.Greater, 0)]
    public string SelectionCountText
    {
        get
        {
            var length = ItemSelectedCount > 0
                ? $" ({new FileSize(SelectionLength).ToString(SizeFormat)})"
                : "";

            return $"{ItemSelectedCount} selected{length}";
        }
    }

    #endregion

    /// <see cref="View.Header"/>
    #region

    [Style(Ion.Template.Address, NameHide = true, Index = int.MinValue, View = Ion.View.Header)]
    public string Path { get => Get(FilePath.Root); set => Set(value); }

    #endregion

    /// <see cref="View.Option"/>
    #region

    /// <see cref="Tab.Filter"/>
    #region

    /// <see cref="Group.File"/>

    [Group(Group.File)]
    [Description("Only show files with these attributes.")]
    [Name("Attributes")]
    [Style(Ion.Template.EnumFlag,
        Tab = Tab.Filter,
        View = Ion.View.Option)]
    public ItemAttributes FileAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Group(Group.File)]
    [Description("Only show files with these extensions (separated by semicolon). Show all files if empty.")]
    [Name("Extensions")]
    [Styles.Text(Ion.Template.Token,
        Tab = Tab.Filter,
        View = Ion.View.Option,
        ValueTrigger = BindTrigger.LostFocus)]
    public string FileExtensions { get => Get(string.Empty); set => Set(value); }

    [Group(Group.File)]
    [Description("File extensions.")]
    [Name("FileExtensions")]
    [Style(Tab = Tab.Filter,
        View = Ion.View.Option)]
    public bool ShowFileExtensions { get => Get(false); set => Set(value); }

    [Group(Group.File)]
    [Description("Show files.")]
    [Name("Files")]
    [Style(Tab = Tab.Filter,
        View = Ion.View.Option)]
    public bool ShowFiles { get => Get(true); set => Set(value); }

    /// <see cref="Group.Folder"/>

    [Group(Group.Folder)]
    [Description("Only show folders with these attributes."), Name("FolderAttributes")]
    [Style(Ion.Template.EnumFlag,
        Tab = Tab.Filter,
        View = Ion.View.Option)]
    public ItemAttributes FolderAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    #endregion

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    public ItemViewPanel() : base()
    {
        itemLengthTask
            = new(UpdateLength, TaskStrategy.FinishAndRestart);
        selectionLengthTask
            = new(null, UpdateSelectionLength, TaskStrategy.CancelAndRestart);

        Items
            = items;
        ShowBullet
            = Text.Bullet.None;
    }

    public ItemViewPanel(IEnumerable<string> fileExtensions) : this() => FileExtensions = fileExtensions.ToString(";");

    /// <see cref="Region.Method"/>
    #region

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnRefreshed(StorageList<Item> sender) => OnRefreshed();

    private void OnRefreshing(StorageList<Item> sender) => OnRefreshing();

    private void Open(Item item)
    {
        switch (item.Type)
        {
            case Storage.ItemType.Drive:
            case Storage.ItemType.Folder:
                if (!IsReadOnly)
                {
                    Path = item.Path;
                    OnFolderOpened(item.Path);
                }
                break;

            case Storage.ItemType.File:
                OnFileOpened(item.Path);
                break;

            case Storage.ItemType.Shortcut:
                var targetPath = Shortcut.TargetPath(item.Path);
                if (Storage.Folder.Exists(targetPath))
                {
                    if (!IsReadOnly)
                    {
                        Path = targetPath;
                        OnFolderOpened(targetPath);
                    }
                }
                else if (Storage.File.Exists(targetPath))
                    OnFileOpened(targetPath);

                else OnFileOpened(item.Path);
                break;
        }
    }

    private async Task UpdateLength(CancellationToken token)
        => ItemLength = await Storage.Folder.GetSize(Path, token);

    private async Task UpdateSelectionLength(IReadOnlyCollection<object> selection, CancellationToken token)
    {
        SelectionLength = 0;
        if (selection?.Count > 0)
        {
            foreach (Item i in selection.Cast<Item>())
            {
                if (token.IsCancellationRequested)
                    break;

                var type = XItemPath.GetType(i.Path);
                switch (type)
                {
                    case Storage.ItemType.File:
                    case Storage.ItemType.Shortcut:
                        Try.Do(() =>
                        {
                            var fileInfo = new FileInfo(i.Path);
                            SelectionLength += fileInfo.Length;
                        });
                        break;

                    case Storage.ItemType.Folder:
                        SelectionLength += await Storage.Folder.GetSize(i.Path, token);
                        break;
                }
            }
        }
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    /// <see cref="Region.Method.Protected.Virtual"/>

    protected override IDropTarget GetDropHandler(System.Windows.Controls.Primitives.Selector selector)
        => new ItemViewDropHandler(this, selector);

    protected override void OnItemClick(object i)
    {
        base.OnItemClick(i);
        if (i is Item j && ((i is Storage.File && OpenFileOn == MouseClick.Single) || (i is Folder && OpenFolderOn == MouseClick.Single)))
            Open(j);
    }

    protected override void OnItemDoubleClick(object i)
    {
        base.OnItemDoubleClick(i);
        if (i is Item j && ((i is Storage.File && OpenFileOn == MouseClick.Double) || (i is Folder && OpenFolderOn == MouseClick.Double)))
            Open(j);
    }

    protected override bool OnItemFilter(object input)
    {
        if (input is Item item)
        {
            if (item is Storage.File file)
            {
                if (ShowFiles)
                {
                    if (!FileAttributes.HasFlag(ItemAttributes.Hidden))
                    {
                        if (file.IsHidden)
                            return false;
                    }

                    if (!FileAttributes.HasFlag(ItemAttributes.ReadOnly))
                    {
                        if (file.IsReadOnly)
                            return false;
                    }

                    var extensions = FileExtensions?.Split(';', StringSplitOptions.RemoveEmptyEntries).ToArray<string, string>(FilePath.GetExtension);
                    if (extensions is null || !extensions.Any() || extensions.Contains(FilePath.GetExtension(file.Path)))
                        return base.OnItemFilter(item);

                    return false;
                }
            }
            if (item is Storage.Folder folder)
            {
                if (folder.IsHidden && !FolderAttributes.HasFlag(ItemAttributes.Hidden))
                    return false;

                if (folder.IsReadOnly && !FolderAttributes.HasFlag(ItemAttributes.ReadOnly))
                    return false;
            }
        }
        return base.OnItemFilter(input);
    }

    protected override void OnSelectorKeyUp(KeyEventArgs e)
    {
        base.OnSelectorKeyUp(e);
        if (FocusManager.GetFocusedElement(Selector) is not TextBox)
        {
            switch (e.Key)
            {
                case Key.Back:
                    History?.Undo(i => Path = i);
                    break;

                case Key.Enter:
                    Item goTo = null;
                    foreach (var i in SelectedItems)
                    {
                        if (i is Item item)
                        {
                            if (item is Container)
                                goTo = item;

                            else if (item is Storage.Shortcut && Storage.Shortcut.TargetsFolder(item.Path))
                                goTo = item;

                            else if (item is Storage.File)
                                Open(item);
                        }
                    }

                    if (goTo is not null)
                        Open(goTo);

                    break;

                case Key.Delete:
                    foreach (var i in SelectedItems)
                    {
                        if (i is Item j && j is not Drive)
                            XItemPath.Recycle(j.Path);
                    }
                    break;
            }
        }
    }

    protected override void OnSelectorPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnSelectorPreviewMouseLeftButtonDown(e);
        if (e.ClickCount == 2)
        {
            if (e.OriginalSource.As<DependencyObject>().GetParent<GridViewColumnHeader>() is null && e.OriginalSource.As<DependencyObject>().GetParent<ListViewItem>() is null)
                XItemPath.OpenInWindowsExplorer(Path);
        }
    }

    protected override void OnSelectorPreviewMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnSelectorPreviewMouseRightButtonUp(e);
        if (e.OriginalSource.As<DependencyObject>().GetParent<GridViewColumnHeader>() is null)
        {
            Try.Do(() =>
            {
                var point = Selector.PointToScreen(e.GetPosition(Selector)).Int32();
                if (SelectedItems is null || SelectedItems.Count == 0)
                {
                    if (Storage.Folder.Exists(Path))
                    {
                        ShellContextMenu.Show(point, new DirectoryInfo(Path));
                        return;
                    }
                }
                else ShellContextMenu.Show(point, SelectedItems.Select((Item i) => i.Read()).ToArray());
            },
            e => Log.Write(e));
        }
    }

    /// <see cref="Region.Method.Protected.Virtual"/>

    protected virtual void OnFileOpened(string filePath)
    {
        Storage.File.TryOpen(filePath).If<Error>(i => Log.Write(i));
        FileOpened?.Invoke(this, new(filePath));
    }

    protected virtual void OnFolderOpened(string folderPath)
    {
        FolderOpened?.Invoke(this, new(folderPath));
    }

    protected virtual void OnPathChanged(string oldValue, string newValue)
    {
        items.Refresh(newValue);

        var folderOptions = GetFolderOptions(newValue);
        handleFolderOptions.Do(() =>
        {
            GroupDirection
                = folderOptions.SortDirection;
            GroupName
                = folderOptions.GroupName;
            SortDirection
                = folderOptions.SortDirection;
            SortName
                = folderOptions.SortName;
            View
                = folderOptions.View;
        });
    }

    protected virtual string OnPathChanging(string oldValue, string newValue)
    {
        var oldPath = oldValue.IsEmpty() ? DefaultPath ?? FilePath.Root : oldValue;
        if (!Storage.Folder.Exists(newValue))
        {
            if (newValue != FilePath.Root)
            {
                if (InvalidPathAlert)
                    Dialog.ShowResult(InvalidPathAlertTitle?.Localize(), new Error(InvalidPathAlertMessage?.Localize()?.F(newValue)), Buttons.Ok);

                return oldPath;
            }
        }
        return newValue;
    }

    protected virtual void OnRefreshed()
        => _ = itemLengthTask.Start();

    protected virtual void OnRefreshing() { }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public static FolderOptions GetFolderOptions(string path)
        => (FolderOptions)FolderOptions.GetOrAdd(path.IsEmpty() || path == FilePath.Root ? FilePath.Root : path, () => new FolderOptions());

    /// <see cref="Region.Method.Public.Override"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(FileAttributes):
            case nameof(FileExtensions):
            case nameof(FolderAttributes):
            case nameof(ShowFiles):
                Reset(() => ItemVisibility);
                break;

            case nameof(GroupDirection):
                handleFolderOptions.DoInternal(() => GetFolderOptions(Path).GroupDirection = GroupDirection);
                break;
            case nameof(GroupName):
                handleFolderOptions.DoInternal(() => GetFolderOptions(Path).GroupName = GroupName);
                break;

            case nameof(ItemCount):
            case nameof(ItemLength):
                Reset(() => ItemCountText);
                break;

            case nameof(ItemSelectedCount):
                _ = selectionLengthTask.Start(SelectedItems);
                Reset(() => SelectionCountText);
                break;

            case nameof(Path):
                Reset(() => Folder);
                OnPathChanged(e.OldValue?.ToString(), e.NewValue?.ToString());
                break;

            case nameof(SelectionLength):
                Reset(() => SelectionCountText);
                break;

            case nameof(SortDirection):
                handleFolderOptions.DoInternal(() => GetFolderOptions(Path).SortDirection = SortDirection);
                break;

            case nameof(SortName):
                handleFolderOptions.DoInternal(() => GetFolderOptions(Path).SortName = SortName);
                break;

            case nameof(View):
                handleFolderOptions.DoInternal(() => GetFolderOptions(Path).View = View);
                break;
        }
    }

    public override void OnSettingProperty(PropertySettingEventArgs e)
    {
        base.OnSettingProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Path):
                e.NewValue = OnPathChanging(e.OldValue?.ToString(), e.NewValue?.ToString());
                break;
        }
    }

    public override void Subscribe()
    {
        base.Subscribe();
        items.Subscribe();
        items.Refreshed += OnRefreshed;
        items.Refreshing += OnRefreshing;
        //_ = items.RefreshAsync(Path);
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        items.Unsubscribe();
        items.Refreshed -= OnRefreshed;
        items.Refreshing -= OnRefreshing;
        items.Clear();
    }

    #endregion

    #endregion

    /// <see cref="ICommand"/>

    public ICommand RenameCommand => Commands[nameof(RenameCommand)] ??= new RelayCommand<Reference2>(i =>
    {
        if (i.First is TextBox box)
        {
            if (i.Second is Item item)
            {
                return;
                var a = item.Path;
                var b = $@"{box.Text}";

                //If file
                if (item is Storage.File)
                {
                    //If file extensions aren't visible (only applies to files!)
                    if (!ShowFileExtensions)
                    {
                        //Append file extension to it!
                        b = $"{b}{System.IO.Path.GetExtension(a)}";
                    }
                }

                var result = XItemPath.Move(item.Path, System.IO.Path.GetDirectoryName(item.Path), b);
                if (result is Error)
                {
                    box.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
                    Dialog.ShowResult("Rename", new Error($"Renaming failed!"), Buttons.Ok);
                }
            }
        }
    });
}