using GongSolutions.Wpf.DragDrop;
using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Local;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Storage;
using Ion.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab,
    MemberView = View.Option)]
[Serializable]
public record class DataPanel : Panel, IFrameworkElementReference
{
    /// <see cref="Tab"/>
    #region

    [TabView(View = Ion.View.Main)]
    private enum Tab
    {
        [TabStyle(Image = Images.General)]
        General,
        [TabStyle(Image = Images.Log)]
        Log,
        [TabStyle(Image = Images.View)]
        View
    }

    #endregion

    /// <see cref="Region.Field"/>
    #region

    public static readonly ReferenceKey<DataControl> ControlKey = new();

    public static readonly ReferenceKey<System.Windows.Controls.Primitives.Selector> SelectorKey = new();

    public const string DefaultGroupName = "None";

    public const string DefaultItemName = "Item";

    protected static CacheByTypeList _Clipboard => Appp.Cache;

    private static IMultiValueConverter visibilityConverter;
    public static IMultiValueConverter VisibilityConverter => visibilityConverter ??= new MultiValueConverter<Visibility>(i =>
    {
        if (i.Values?.Length >= 2)
        {
            if (i.Values[0] is DataPanel panel)
                return panel.OnItemFilter(i.Values[1]).ToVisibility();
        }
        return Visibility.Visible;
    });

    #endregion

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="Region.Property.Protected"/>

    protected bool AnySelected => SelectedItems?.Count > 0;

    /// <see cref="Region.Property.Public"/>
    #region

    public DataControl Control { get => Get<DataControl>(); private set => Set(value); }

    public IDropTarget DropHandler { get => Get<IDropTarget>(null); protected set => Set(value); }

    public int GroupNameIndex { get => Get(0); set => Set(value); }

    public object GroupName { get => Get<object>(); set => Set(value); }

    public bool IsEditing { get => Get(false); set => Set(value); }

    [Styles.List(Template.List,
        ItemAction = ItemAction.All,
        ItemAddMethod = nameof(AddPreset),
        View = View.Option)]
    [Styles.Text(CanEdit = false, Options = Option.Copy | Option.Edit | Option.Paste,
        ValuePath = nameof(ItemPreset.Name),
        TargetItem = typeof(ItemPreset))]
    public ListObservable<ItemPreset> ItemPresets { get => Get(new ListObservable<ItemPreset>()); private set => Set(value); }

    public int ItemPresetCount => ItemPresets.Count;

    public IList Items { get => Get<IList>(); set => Set(value); }

    public int ItemTypeCount => ItemTypes.Count();

    public ListCollectionView ItemView { get => Get<ListCollectionView>(); protected set => Set(value); }

    public bool ItemVisibility { get => Get(false); set => Set(value); }

    public ListObservableOfString SearchHistory { get => Get((ListObservableOfString)([])); private set => Set(value); }

    public int SearchNameIndex { get => Get(0); set => Set(value); }

    public object SearchName { get => Get<object>(); set => Set(value); }

    public int SearchNameCount => SearchNames?.Count ?? 0;

    [PropertyPanel.Target]
    public IReadOnlyCollection<object> SelectedItems { get => Get<IReadOnlyCollection<object>>(); protected set => Set(value); }

    public System.Windows.Controls.Primitives.Selector Selector { get => Get<System.Windows.Controls.Primitives.Selector>(); private set => Set(value); }

    public int SortNameIndex { get => Get(0); set => Set(value); }

    public object SortName { get => Get<object>(); set => Set(value); }

    #endregion

    /// <see cref="Region.Property.Public.Override"/>
    #region

    public sealed override string Title
        => TitlePrefix
        + (TitleLocalizedHere ? Instance.GetName(this).Localize() : Instance.GetName(this))
        + TitleSuffix
        + $" ({TitleCount})";

    public sealed override bool TitleLocalized => false;

    public virtual bool TitleLocalizedHere => true;

    #endregion

    /// <see cref="Region.Property.Public.Virtual"/>
    #region

    public virtual Type ItemValueType => ItemType;

    public virtual bool CanAdd => true;

    public virtual bool CanAddFromPreset => true;

    public virtual bool CanClear => true;

    public virtual bool CanClone => true;

    public virtual bool CanCopy => true;

    public virtual bool CanCopyTo => true;

    public virtual bool CanCut => true;

    public virtual bool CanDrag => false;

    public virtual bool CanDragSelect => true;

    public virtual bool CanDrop => false;

    public virtual bool CanEdit => true;

    public virtual bool CanGroup => true;

    public virtual bool CanKeySelect => true;

    public virtual bool CanMoveTo => true;

    public virtual bool CanPaste => true;

    public virtual bool CanRefresh => true;

    public virtual bool CanRemove => true;

    public virtual bool CanSearch => true;

    public virtual bool CanSort => true;

    public virtual IValueConverter GroupConverter { get; }

    public virtual ConverterSelector GroupConverterSelector { get; }

    public virtual string ItemName => Instance.GetName(ItemType) ?? DefaultItemName;

    public virtual Type ItemType { get; }

    protected virtual Dictionary<Type, Func<object>> ItemTypeHandlers { get; }

    public virtual IEnumerable<Type> ItemTypes { get { yield return ItemType; } }

    public virtual string KeySelectProperty { get; }

    public virtual int TitleCount => ItemCount;

    /// <summary>Text to prepend to title (after localization).</summary>
    public virtual string TitlePrefix { get; } = "";

    /// <summary>Text to append to title (after localization).</summary>
    public virtual string TitleSuffix { get; } = "";

    #endregion

    /// <see cref="View.Footer"/>
    #region

    [Group(0)]
    [Styles.Text(CanEdit = false,
        CanSelect = false,
        Index = 0,
        NameIcon = Images.Dot,
        View = View.Footer,
        ValueFormat = "{0} items")]
    public virtual int ItemCount => Items?.Count ?? 0;

    [Group(1)]
    [Styles.Text(CanEdit = false,
        CanSelect = false,
        Index = 1,
        NameIcon = Images.Dot,
        View = View.Footer,
        ValueFormat = "{0} selected")]
    [VisibilityTrigger(nameof(ItemSelectedCount), Comparison.Greater, 0)]
    public virtual int ItemSelectedCount => SelectedItems?.Count ?? 0;

    #endregion

    /// <see cref="View.Header"/>
    #region

    [Styles.List(Template.ListCombo, NameHide = true, Name = "Name", Pin = Sides.RightOrBottom,
        Index = int.MaxValue - 1,
        Placeholder = "By",
        View = View.Header,
        SelectedIndexProperty = nameof(SearchNameIndex),
        SelectedItemProperty = nameof(SearchName))]
    [Group(GroupDefault.Search)]
    [VisibilityTrigger(nameof(CanSearch), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(SearchNameCount), Comparison.Greater, 0)]
    public IList SearchNames { get => Get<IList>(); protected set => Set(value); }

    [Group(GroupDefault.Search)]
    [Style(NameHide = true, Index = int.MaxValue, Name = "Options", Pin = Sides.RightOrBottom,
        View = View.Header)]
    [VisibilityTrigger(nameof(CanSearch), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(SearchNameCount), Comparison.Greater, 0)]
    public SearchOptions SearchOptions { get => Get<SearchOptions>(new()); set => Set(value); }

    [Group(GroupDefault.Search)]
    [Image(Images.Search)]
    [Styles.Text(NameHide = true, Index = int.MaxValue - 2, Pin = Sides.RightOrBottom,
        EnterCommand = nameof(SearchCommand),
        EnterImage = Images.Search,
        Name = "Search",
        Placeholder = "Search...",
        View = View.Header,
        SuggestionProperty = nameof(SearchHistory),
        SuggestionCommandProperty = nameof(SearchSuggestionCommand),
        ValueTrigger = BindTrigger.PropertyChanged,
        Width = 150)]
    [VisibilityTrigger(nameof(CanSearch), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(SearchNameCount), Comparison.Greater, 0)]
    public string SearchText { get => Get(string.Empty); set => Set(value); }

    #endregion

    /// <see cref="View.Option"/>
    #region

    /// <see cref="Tab.General"/>
    #region

    /// <see cref="GroupDefault.Format"/>
    #region

    [Group(GroupDefault.Format)]
    [Description("The file size format.")]
    [Name("Size format")]
    [Style(Tab = Tab.General, View = View.Option)]
    public FileSizeFormat SizeFormat { get => Get(FileSizeFormat.BinaryUsingSI); set => Set(value); }

    [Group(GroupDefault.Format)]
    [Name("Time format")]
    [Style(Tab = Tab.General,
        View = View.Option)]
    public string TimeFormat { get => Get(Time.TimeFormat.Relative); set => Set(value); }

    #endregion

    /// <see cref="GroupDefault.Group"/>
    #region

    [Group(GroupDefault.Group)]
    [Style(Tab = Tab.General,
        Name = "Direction",
        View = View.Option)]
    [VisibilityTrigger(nameof(CanGroup), Comparison.Equal, true)]
    public ListSortDirection GroupDirection { get => Get(ListSortDirection.Ascending); set => Set(value); }

    [Group(GroupDefault.Group)]
    [Name("Name")]
    [Styles.List(Template.ListCombo,
        Tab = Tab.General,
        View = View.Option,
        SelectedIndexProperty = nameof(GroupNameIndex),
        SelectedItemProperty = nameof(GroupName))]
    [VisibilityTrigger(nameof(CanGroup), Comparison.Equal, true)]
    public IList GroupNames { get => Get<IList>(); protected set => Set(value); }

    #endregion

    /// <see cref="GroupDefault.Preset"/>
    #region

    [Group(GroupDefault.Preset)]
    [Description("The default name of new presets.")]
    [Name("NewName")]
    [Styles.Text(Tab = Tab.General,
        View = View.Option)]
    public string NewPresetName { get => Get(ItemPreset.DefaultName); set => Set(value); }

    #endregion

    /// <see cref="GroupDefault.Sort"/>
    #region

    [Group(GroupDefault.Sort)]
    [Style(Name = "Direction",
        Tab = Tab.General,
        View = View.Option)]
    [VisibilityTrigger(nameof(CanSort), Comparison.Equal, true)]
    public ListSortDirection SortDirection { get => Get(ListSortDirection.Descending); set => Set(value); }

    [Group(GroupDefault.Sort)]
    [Styles.List(Template.ListCombo, Name = "Name",
        Tab = Tab.General,
        View = View.Option,
        SelectedIndexProperty = nameof(SortNameIndex),
        SelectedItemProperty = nameof(SortName))]
    [VisibilityTrigger(nameof(CanSort), Comparison.Equal, true)]
    public IList SortNames { get => Get<IList>(); protected set => Set(value); }

    #endregion

    /// <see cref="GroupDefault.Warn"/>
    #region

    [Group(GroupDefault.Warn), Name("Before clearing")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforeClearing { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Warn), Name("Before cloning")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforeCloning { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Warn)]
    [Name("Before copying")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforeCopying { get => Get(false); set => Set(value); }

    [Group(GroupDefault.Warn), Name("Before cutting")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforeCutting { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Warn)]
    [Name("Before moving")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforeMoving { get => Get(false); set => Set(value); }

    [Group(GroupDefault.Warn), Name("Before pasting")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforePasting { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Warn), Name("Before removing")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool WarnBeforeRemoving { get => Get(true); set => Set(value); }

    #endregion

    #endregion

    /// <see cref="Tab.Log"/>
    #region

    [Group(GroupDefault.Item), Name("On added")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool LogOnItemAdded { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Item), Name("On clear")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool LogOnItemClear { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Item), Name("On cloned")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool LogOnItemCloned { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Item), Name("On copied")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool LogOnItemCopied { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Item), Name("On moved")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool LogOnItemMoved { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Item), Name("On pasted")]
    [Style(Tab = Tab.Log,
        View = View.Option)]
    public bool LogOnItemPasted { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Item), Name("On removed")]
    [Style(View = View.Option)]
    public bool LogOnItemRemoved { get => Get(true); set => Set(value); }

    #endregion

    /// <see cref="Tab.View"/>
    #region

    /// <see cref="GroupDefault.Show"/>
    #region

    [Group(GroupDefault.Show)]
    [Description("A bullet to show next to each item.")]
    [Name("Bullet")]
    [Style(Tab = Tab.View,
        View = View.Option)]
    public Bullet ShowBullet { get => Get(Bullet.Circle); set => Set(value); }

    [Group(GroupDefault.Show)]
    [Description("A check to show next to each item.")]
    [Name("Check box")]
    [Style(Tab = Tab.View,
        View = View.Option)]
    public bool ShowCheckBox { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Show)]
    [Name("Columns")]
    [Style(Template.EnumFlagButton,
        Tab = Tab.View,
        NameHide = true,
        View = View.Option)]
    public Flag ShowColumns { get => Get<Flag>(); set => Set(value); }

    #endregion

    #endregion

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    protected DataPanel() : base()
    {
        //GroupNames
        if (CanGroup)
        {
            GroupNames = new ListObservableOfString() { DefaultGroupName };
            ItemType.GetProperties(Instance.Flag.Public).Where(i => i.GetAttribute<FilterAttribute>()?.Filter.HasFlag(Ion.Filter.Group) == true)
                .ForEach(i => GroupNames.Add(i.Name));
        }

        //SearchNames
        if (CanSearch)
        {
            SearchNames = new ListObservableOfString();
            ItemType.GetProperties(Instance.Flag.Public).Where(i => i.GetAttribute<FilterAttribute>()?.Filter.HasFlag(Ion.Filter.Search) == true)
                .ForEach(i => SearchNames.Add(i.Name));
        }

        //SortNames
        if (CanSort)
        {
            SortNames = new ListObservableOfString();
            ItemType.GetProperties(Instance.Flag.Public).Where<PropertyInfo>(i => i.GetAttribute<FilterAttribute>()?.Filter.HasFlag(Ion.Filter.Sort) == true)
                .ForEach(i => SortNames.Add(i.Name));
        }

        //ShowColumns
        ShowColumns = new Flag();
        ItemType.GetProperties(Instance.Flag.Public).Where<PropertyInfo>(i => i.GetAttribute<FilterAttribute>()?.Filter.HasFlag(Filter.Show) == true)
            .ForEach(i => ShowColumns = ShowColumns.AddOrSet(i.Name, true));
    }

    /// <see cref="IFrameworkElementReference"/>

    void IFrameworkElementReference.SetReference(IFrameworkElementKey key, FrameworkElement e)
    {
        if (key == ControlKey)
        {
            if (e is DataControl control)
                Control = control;
        }
        if (key == SelectorKey)
        {
            if (e is System.Windows.Controls.Primitives.Selector selector)
            {
                Selector = selector;

                selector.SelectionChanged -= OnSelectionChanged;
                selector.SelectionChanged += OnSelectionChanged;

                selector.Unloaded -= OnSelectorUnloaded;
                selector.Unloaded += OnSelectorUnloaded;
            }
        }
    }

    /// <see cref="Region.Method.Private"/>
    #region 

    private void OnItemPresetsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Reset(() => ItemPresetCount);
    }

    private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Reset(() => ItemCount);
        Reset(() => Title);

        OnItemsChanged();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                OnItemAdded(e.NewItems[0]);
                break;

            case NotifyCollectionChangedAction.Remove:
                OnItemRemoved(e.OldItems[0]);
                break;
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e) => Reset(() => Title);

    [Refactor("Only known way to get current selection quickly.")]
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var result = new List<object>();
        if (Selector is not null)
        {
            foreach (var i in Selector.Items)
            {
                var j = i is FrameworkElement k ? k : Selector.GetContainer(i);
                if (j is DataGridRow x && x.IsSelected)
                    result.Add(i);

                else if (j is ListBoxItem y && y.IsSelected)
                    result.Add(i);

                else if (j is ListViewItem z && z.IsSelected)
                    result.Add(i);
            }
        }
        SelectedItems = result.Count == 0 ? null : [.. result];
    }

    private void OnSelectorPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) => OnSelectorPreviewMouseRightButtonUp(e);

    private void OnSelectorPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) => OnSelectorPreviewMouseRightButtonDown(e);

    private void OnSelectorPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) => OnSelectorPreviewMouseLeftButtonUp(e);

    private void OnSelectorPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => OnSelectorPreviewMouseLeftButtonDown(e);

    private void OnSelectorKeyDown(object sender, KeyEventArgs e) => OnSelectorKeyDown(e);

    private void OnSelectorKeyUp(object sender, KeyEventArgs e) => OnSelectorKeyUp(e);

    private void OnSelectorUnloaded(object sender, RoutedEventArgs e)
    {
        sender.If((System.Windows.Controls.Primitives.Selector i) => { i.SelectionChanged -= OnSelectionChanged; i.Unloaded -= OnSelectorUnloaded; });

        SelectedItems = null;
        Selector = null;
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region 

    protected void ApplyGroup()
    {
        if (ItemView is not null)
        {
            if (ItemType?.Implements<IComparable>() == true)
            {
                ItemView.GroupDescriptions.Clear();
                if (GroupName is not null)
                {
                    var groupName = $"{GroupName}";
                    if (groupName != DefaultGroupName)
                    {
                        PropertyGroupDescription description = new() { Converter = GroupConverterSelector?.SelectConverter(groupName) ?? GroupConverter };
                        if (description.Converter is null)
                            description.PropertyName = groupName;

                        ItemView.GroupDescriptions.Add(description);
                    }
                }
                ItemView.Refresh();
            }
        }
    }

    protected void ApplySearch(string input)
    {
        if (!input.IsEmpty())
        {
            if (SearchHistory.Contains(input))
                SearchHistory.Remove(input);

            SearchHistory.Insert(0, input);
            OnSearched(input);
        }
    }

    protected void ApplySort()
    {
        if (ItemView is not null)
        {
            if (ItemType?.Implements<IComparable>() == true)
            {
                ItemView.SortDescriptions.Clear();
                if (GroupName is not null)
                {
                    var groupName = $"{GroupName}";
                    if (groupName != DefaultGroupName)
                        ItemView.SortDescriptions.Add(new System.ComponentModel.SortDescription($"{GroupName}", GroupDirection));
                }

                if (SortName is not null)
                    ItemView.SortDescriptions.Add(new System.ComponentModel.SortDescription($"{SortName}", SortDirection));

                ItemView.Refresh();
            }
        }
    }

    protected void Warn(string actionName, Void action, Func<bool> gWarn, Action<bool> sWarn)
    {
        var noWarning = new Accessor<bool>(gWarn, sWarn);
        if (!noWarning.Get())
        {
            Dialog.ShowResult(actionName,
                new Warning($"Are you sure you want to {actionName.ToLower()} the selected {ItemName?.ToLower() ?? ItemName}s?"),
                i => i.IfEqual(0, action), Buttons.YesNo);
        }
        else action();
    }

    #endregion

    /// <see cref="Region.Method.Protected.Virtual"/>
    #region 

    protected virtual object GetDefaultItem() => ItemValueType.Create<object>();

    protected virtual IDropTarget GetDropHandler(System.Windows.Controls.Primitives.Selector selector)
        => new DropHandler<System.Windows.Controls.Primitives.Selector, object>(selector);

    protected virtual Images GetItemIcon() => Images.Dot;

    protected virtual bool OnItemFilter(object input)
    {
        if (CanSearch)
        {
            var searchName = SearchName?.ToString();
            if (input is not null && Instance.HasProperty(input, searchName))
            {
                var a = Instance.GetPropertyValue(input, searchName)?.ToString();
                var b = SearchText;

                if (!a.IsEmpty() && !b.IsEmpty())
                    return (SearchOptions ?? new SearchOptions()).Assert(a, b);
            }
        }
        return true;
    }

    protected virtual void OnItemAdded(object input)
    {
        LogOnItemAdded.If(() => Log.Write<Success.ItemAdd>());
    }

    protected virtual void OnItemClick(object input) { }

    protected virtual void OnItemCloned(object item)
    {
        LogOnItemCloned.If(() => Log.Write<Success.ItemClone>());
    }

    protected virtual void OnItemDoubleClick(object input) { }

    protected virtual void OnItemsCleared()
    {
        LogOnItemClear.If(() => Log.Write<Success.ItemClear>());
    }

    protected virtual void OnItemsChanged() { }

    protected virtual void OnItemsCopied(IReadOnlyCollection<object> items)
    {
        LogOnItemCopied.If(() => Log.Write(new Success.ItemCopy(items)));
    }

    protected virtual void OnItemsMoved(IReadOnlyCollection<object> items)
    {
        LogOnItemMoved.If(() => Log.Write(new Success.ItemMove(items)));
    }

    protected virtual void OnItemsPasted(IReadOnlyCollection<object> items)
    {
        LogOnItemPasted.If(() => Log.Write(new Success.ItemPaste(items)));
    }

    protected virtual void OnItemsSet(object oldValue, object newValue)
    {
        oldValue.If<IListObservable>(i => i.CollectionChanged -= OnItemsChanged);
        newValue.If<IListObservable>(i =>
        {
            i.CollectionChanged
                -= OnItemsChanged;
            i.CollectionChanged
                += OnItemsChanged;

            ItemView
                = new ListCollectionView(i);

            Set(() => GroupNameIndex, 0, true);
            Set(() => SortNameIndex, 0, true);

            ApplyGroup(); ApplySort();
        },
        () => ItemView = null);
    }

    protected virtual void OnItemRemoved(object input)
    {
        LogOnItemRemoved.If(() => Log.Write<Success.ItemRemove>());
    }

    protected virtual void OnSearched(string input) { }

    protected virtual void OnSelectorPreviewMouseRightButtonUp(MouseButtonEventArgs e) { }

    protected virtual void OnSelectorPreviewMouseRightButtonDown(MouseButtonEventArgs e) { }

    protected virtual void OnSelectorPreviewMouseLeftButtonUp(MouseButtonEventArgs e) { }

    protected virtual void OnSelectorPreviewMouseLeftButtonDown(MouseButtonEventArgs e) { }

    protected virtual void OnSelectorKeyDown(KeyEventArgs e) { }

    protected virtual void OnSelectorKeyUp(KeyEventArgs e) { }

    ///

    protected virtual void Add(Type input)
    {
        var type = input ?? ItemType;

        object result = type is not null && ItemTypeHandlers?.ContainsKey(type) == true ? ItemTypeHandlers[type]() : GetDefaultItem();
        Dialog.ShowObject($"Add {ItemName}", result, Resource.GetImageUri(Images.Plus), i =>
        {
            if (i == 0)
                Items.Add(result);
        },
        Buttons.SaveCancel);
    }

    protected virtual void AddFromPreset(ItemPreset preset)
    {
        var value = Instance.CloneDeep(preset.Value, new CreateFromObject());
        value.IfNotNull(_ =>
        {
            Dialog.ShowObject($"Add {ItemName}", value, Resource.GetImageUri(Images.Plus), i =>
            {
                if (i == 0)
                    Items.Add(value);
            },
            Buttons.SaveCancel);
        });
    }

    protected virtual void AddPreset() { }

    protected virtual void Duplicate(IReadOnlyCollection<object> items)
    {
        Warn("Clone", () =>
        {
            foreach (var i in items)
            {
                var j = Instance.CloneDeep(i, new CreateFromObject());
                OnItemCloned(j);

                j.IfNotNull(k => Items.Add(k));
            }
        },
        () => !WarnBeforeCloning, i => WarnBeforeCloning = !i);
    }

    protected virtual void Copy(IReadOnlyCollection<object> items)
    {
        _Clipboard.Add(items);
        OnItemsCopied(items);
    }

    protected virtual void CopyTo(IReadOnlyCollection<object> items)
    {
        Warn("CopyTo", () =>
        {
            var form = new DataPanelForm(this, Appp.Model.As<IAppModelDock>().ViewModel.Panels);
            Dialog.ShowObject("CopyTo", form, Resource.GetImageUri(Images.CopyTo), i =>
            {
                if (i == 0)
                {
                    form.CopyTo(items);
                    OnItemsCopied(items);
                }
            },
            Buttons.ContinueCancel);
        },
        () => !WarnBeforeCopying, i => WarnBeforeCopying = !i);
    }

    protected virtual void Cut(IReadOnlyCollection<object> items) => Warn("Cut", () =>
    {
        Copy(items);
        Remove(items);
    },
    () => !WarnBeforeCutting, i => WarnBeforeCutting = !i);

    protected virtual void Edit(IReadOnlyCollection<object> items)
    {
        object i = items.Count == 1 ? items.First() : items;
        Dialog.ShowObject("Edit", i, Resource.GetImageUri(Images.Pencil), Buttons.Done);
    }

    protected virtual void MoveTo(IReadOnlyCollection<object> items)
    {
        Warn("MoveTo", () =>
        {
            var form = new DataPanelForm(this, Appp.Model.As<IAppModelDock>().ViewModel.Panels);
            Dialog.ShowObject("MoveTo", form, Resource.GetImageUri(Images.CopyTo), i =>
            {
                if (i == 0)
                {
                    form.MoveTo(items);
                    OnItemsMoved(items);
                }
            },
            Buttons.ContinueCancel);
        },
        () => !WarnBeforeMoving, i => WarnBeforeMoving = !i);
    }

    protected virtual void Paste()
    {
        Warn("Paste", () =>
        {
            List<object> items = [];
            foreach (var i in _Clipboard[ItemType].Value)
            {
                var clone = i.CloneDeep(new CreateFromObject());
                Items.Add(clone);
                items.Add(clone);
            }
            OnItemsPasted([.. items]);
        },
        () => !WarnBeforePasting, i => WarnBeforePasting = !i);
    }

    protected virtual void Remove(IReadOnlyCollection<object> items) => Warn("Remove", () => SelectedItems.ForEach(i => Items.Remove(i)),
        () => !WarnBeforeRemoving, i => WarnBeforeRemoving = !i);

    protected virtual void RemoveAll() => Warn("Clear", () =>
    {
        Items.Clear();
        OnItemsCleared();
    },
    () => !WarnBeforeClearing, i => WarnBeforeClearing = !i);

    protected virtual void SaveAsPreset(IReadOnlyCollection<object> items)
    {
        foreach (var i in items)
        {
            var value = Instance.CloneDeep(i, new CreateFromObject());
            value.IfNotNull(_ => ItemPresets.Add(new ItemPreset(NewPresetName, value)));
        }
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(GroupDirection):
            case nameof(GroupName):
                ApplyGroup();
                break;

            case nameof(ItemCount):
                Reset(() => TitleCount);
                break;

            case nameof(Items):
                Reset(() => ItemCount);
                OnItemsSet(e.OldValue, e.NewValue);
                break;

            case nameof(SearchName):
            case nameof(SearchOptions):
            case nameof(SearchText):
                Reset(() => ItemVisibility);
                break;

            case nameof(SelectedItems):
                Reset(() => ItemSelectedCount);
                break;

            case nameof(Selector):
                e.OldValue.If((System.Windows.Controls.Primitives.Selector i) =>
                {
                    i.KeyDown
                        -= OnSelectorKeyDown;
                    i.KeyUp
                        -= OnSelectorKeyUp;

                    i.PreviewMouseLeftButtonDown
                        -= OnSelectorPreviewMouseLeftButtonDown;
                    i.PreviewMouseLeftButtonUp
                        -= OnSelectorPreviewMouseLeftButtonUp;

                    i.PreviewMouseRightButtonDown
                        -= OnSelectorPreviewMouseRightButtonDown;
                    i.PreviewMouseRightButtonUp
                        -= OnSelectorPreviewMouseRightButtonUp;
                });
                e.NewValue.If((System.Windows.Controls.Primitives.Selector i) =>
                {
                    DropHandler = GetDropHandler(i);

                    i.KeyDown
                        += OnSelectorKeyDown;
                    i.KeyUp
                        += OnSelectorKeyUp;

                    i.PreviewMouseLeftButtonDown
                        += OnSelectorPreviewMouseLeftButtonDown;
                    i.PreviewMouseLeftButtonUp
                        += OnSelectorPreviewMouseLeftButtonUp;

                    i.PreviewMouseRightButtonDown
                        += OnSelectorPreviewMouseRightButtonDown;
                    i.PreviewMouseRightButtonUp
                        += OnSelectorPreviewMouseRightButtonUp;
                });
                break;

            case nameof(SortDirection):
            case nameof(SortName):
                ApplySort();
                break;

            case nameof(TitleCount):
            case nameof(TitleLocalized):
            case nameof(TitlePrefix):
            case nameof(TitleSuffix):
                Reset(() => Title);
                break;
        }
    }

    public override void Subscribe()
    {
        base.Subscribe();
        Appp.Get<AppModel>()
            .IfNotNull(i => i.SetLanguage += OnLanguageChanged);
        ItemPresets.CollectionChanged
            += OnItemPresetsChanged;
    }

    public override void Unsubscribe()
    {
        base.Subscribe();
        Appp.Get<AppModel>()
            .IfNotNull(i => i.SetLanguage -= OnLanguageChanged);
        ItemPresets.CollectionChanged
            -= OnItemPresetsChanged;
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    /// <see cref="GroupDefault.Add"/>
    #region

    private ICommand addCommand;
    [Description("Add an item.")]
    [Group(GroupDefault.Add)]
    [Image(Images.Plus)]
    [Name("Add")]
    [Style(NameHide = true, Index = 0,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanAdd), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(ItemTypeCount), 1)]
    public ICommand AddCommand => addCommand ??= new RelayCommand<Type>(Add,
    i => Items is not null && CanAdd);

    [Description("Add an item.")]
    [Group(GroupDefault.Add)]
    [Image(Images.Plus)]
    [Name("Add")]
    [Styles.List(Template.ListButton,
        NameHide = true,
        Index = 0,
        ItemAction = ItemAction.Add | ItemAction.Move | ItemAction.Remove,
        ItemCommand = nameof(AddCommand),
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanAdd), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(ItemTypeCount), Comparison.Greater, 1)]
    public ListCollectionView AddCommandList => new(new ListObservable<Type>(ItemTypes));

    public ICommand AddFromPresetCommand => Commands[nameof(AddFromPresetCommand)] ??= new RelayCommand<ItemPreset>(AddFromPreset,
    i => Items is not null && CanAdd);

    [Description("Add an item from a preset.")]
    [Group(GroupDefault.Add)]
    [Image(Images.PlusPreset)]
    [Name("AddFromPreset")]
    [Styles.List(Template.ListButton,
        NameHide = true,
        Index = 0,
        ItemCommand = nameof(AddFromPresetCommand),
        ItemGroupName = nameof(ItemPreset.Group),
        ItemSortName = nameof(ItemPreset.Name),
        View = View.Header)]
    [VisibilityTrigger(nameof(CanAdd), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(CanAddFromPreset), Comparison.Equal, true)]
    [VisibilityTrigger(nameof(ItemPresetCount), Comparison.Greater, 1)]
    public ListCollectionView AddFromPresetCommandList => new(ItemPresets);

    #endregion

    /// <see cref="GroupDefault.Copy"/>
    #region

    [Description("Copy (and remove) items.")]
    [Group(GroupDefault.Copy)]
    [Image(Images.Cut)]
    [Name("Cut")]
    [Style(NameHide = true, Index = 0,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanCut), Comparison.Equal, true)]
    public ICommand CutCommand => Commands[nameof(CutCommand)] ??= new RelayCommand(() => Cut(SelectedItems),
        () => Items is not null && AnySelected && CanCut);

    [Description("Copy items.")]
    [Group(GroupDefault.Copy)]
    [Image(Images.Copy)]
    [Name("Copy")]
    [Style(NameHide = true, Index = 1,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanCopy), Comparison.Equal, true)]
    public ICommand CopyCommand => Commands[nameof(CopyCommand)] ??= new RelayCommand(() => Copy(SelectedItems),
        () => Items is not null && AnySelected && CanCopy);

    [Description("Copy items to another group.")]
    [Group(GroupDefault.Copy)]
    [Image(Images.CopyTo)]
    [Name("Copy to")]
    [Style(NameHide = true, Index = 2,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanCopyTo), Comparison.Equal, true)]
    public ICommand CopyToCommand => Commands[nameof(CopyToCommand)] ??= new RelayCommand(() => CopyTo(SelectedItems),
        () => AnySelected && CanCopyTo);

    [Description("Clone items.")]
    [Group(GroupDefault.Copy)]
    [Image(Images.Clone)]
    [Name("Clone")]
    [Style(NameHide = true, Index = 3,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanClone), Comparison.Equal, true)]
    public ICommand CloneCommand => Commands[nameof(CloneCommand)] ??= new RelayCommand(() => Duplicate(SelectedItems),
        () => Items is not null && AnySelected && CanClone);

    [Description("Paste items.")]
    [Group(GroupDefault.Copy)]
    [Image(Images.Paste)]
    [Name("Paste")]
    [Style(NameHide = true, Index = 4,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanPaste), Comparison.Equal, true)]
    public ICommand PasteCommand => Commands[nameof(PasteCommand)] ??= new RelayCommand(Paste,
        () => Items is not null && CanPaste && _Clipboard.Contains(ItemType));

    [Description("Move items to a group in another panel.")]
    [Group(GroupDefault.Copy)]
    [Image(Images.MoveTo)]
    [Name("MoveTo")]
    [Style(NameHide = true, Index = 6,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanMoveTo), Comparison.Equal, true)]
    public ICommand MoveToCommand => Commands[nameof(MoveToCommand)] ??= new RelayCommand(() => MoveTo(SelectedItems),
        () => AnySelected && CanMoveTo);

    #endregion

    /// <see cref="GroupDefault.Data"/>
    #region

    [Description("Refresh data.")]
    [Group(GroupDefault.Data)]
    [Image(Images.Refresh)]
    [Name("Refresh")]
    [Style(View = View.HeaderOption)]
    [VisibilityTrigger(nameof(CanRefresh), Comparison.Equal, true)]
    public ICommand RefreshCommand => Commands[nameof(RefreshCommand)] ??= new RelayCommand(() => ItemView.Refresh(), () => ItemView is not null);

    #endregion

    /// <see cref="GroupDefault.Edit"/>
    #region

    private ICommand editCommand;
    [Description("Edit items.")]
    [Group(GroupDefault.Edit)]
    [Image(Images.Pencil)]
    [Name("Edit")]
    [Style(NameHide = true,
        View = View.ItemOption)]
    [VisibilityTrigger(nameof(CanEdit), Comparison.Equal, true)]
    public ICommand EditCommand => editCommand ??= new RelayCommand(() => Edit(SelectedItems),
        () => Items is not null && AnySelected && CanEdit);

    private ICommand undoCommand;
    [Description("Undo last action.")]
    [Group(GroupDefault.Edit)]
    [Image(Images.Undo)]
    [Name("Undo")]
    [Style(NameHide = true,
        View = View.HeaderOption)]
    public ICommand UndoCommand => undoCommand ??= new RelayCommand(() =>
    {

    }, () => false);

    private ICommand redoCommand;
    [Description("Redo last action.")]
    [Group(GroupDefault.Edit)]
    [Image(Images.Redo)]
    [Name("Redo")]
    [Style(NameHide = true,
        View = View.HeaderOption)]
    public ICommand RedoCommand => redoCommand ??= new RelayCommand(() =>
    {

    }, () => false);

    #endregion

    /// <see cref="GroupDefault.Remove"/>
    #region

    [Description("Remove items.")]
    [Group(GroupDefault.Remove)]
    [Image(Images.Remove)]
    [Name("Remove")]
    [Style(NameHide = true, Index = 1,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanRemove), Comparison.Equal, true)]
    public ICommand RemoveCommand => Commands[nameof(RemoveCommand)] ??= new RelayCommand(() => Remove(SelectedItems),
    () => Items is not null && AnySelected && CanRemove);

    [Description("Remove all items.")]
    [Group(GroupDefault.Remove)]
    [Image(Images.RemoveAll)]
    [Name("RemoveAll")]
    [Style(NameHide = true,
        View = View.Header | View.ItemOption)]
    [VisibilityTrigger(nameof(CanClear), Comparison.Equal, true)]
    public ICommand RemoveAllCommand => Commands[nameof(RemoveAllCommand)] ??= new RelayCommand(RemoveAll, () => Items?.Count > 0);

    #endregion

    /// <see cref="GroupDefault.Save"/>
    #region

    [Description("Save items as presets.")]
    [Group(GroupDefault.Save)]
    [Image(Images.SaveAsPreset)]
    [Name("SaveAsPreset")]
    [Style(NameHide = true,
        View = View.Header | View.HeaderOption | View.ItemOption)]
    [VisibilityTrigger(nameof(CanAddFromPreset), Comparison.Equal, true)]
    public ICommand SaveAsPresetCommand => Commands[nameof(SaveAsPresetCommand)] ??= new RelayCommand(() => SaveAsPreset(SelectedItems),
        () => AnySelected && CanAddFromPreset);

    #endregion

    /// <see cref="HideAttribute"/>
    #region

    public ICommand ClickCommand => Commands[nameof(ClickCommand)]
        ??= new RelayCommand<object>(OnItemClick);

    public ICommand DoubleClickCommand => Commands[nameof(DoubleClickCommand)]
        ??= new RelayCommand<object>(OnItemDoubleClick);

    public ICommand SearchCommand => Commands[nameof(SearchCommand)]
        ??= new RelayCommand<string>(ApplySearch, i => !i.IsEmpty());

    public ICommand SearchSuggestionCommand => Commands[nameof(SearchSuggestionCommand)]
        ??= new RelayCommand<string>(ApplySearch);

    public ICommand SelectAllCommand => Commands[nameof(SelectAllCommand)]
        ??= new RelayCommand(() => Selector.SelectAll(), () => Selector is not null && Items.Count > 0);

    public ICommand UnselectAllCommand => Commands[nameof(UnselectAllCommand)]
        ??= new RelayCommand(() => Selector.SelectNone(), () => Selector is not null && Items.Count > 0);

    #endregion

    #endregion
}