using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Ion.Core;

/// <see cref="IDataGroupPanel"/>
#region

public interface IDataGroupPanel
{
    Type ItemType { get; }

    Type ItemValueType { get; }

    IListWritable Groups { get; }

    IItem Create(object item);

    IItem Create(string name, string description, object item);
}

#endregion

/// <see cref="DataGroupPanelTab"/>
#region

[TabView(View = View.Main)]
public enum DataGroupPanelTab
{
    [TabStyle(Image = Images.Group)]
    Group
}

#endregion

/// <see cref="DataGroupPanel{T}"/>
#region

/// <inheritdoc/>
[Styles.Object(MemberViewType = MemberViewType.Tab)]
public abstract record class DataGroupPanel<T> : DataViewPanel<T>, IDataGroupPanel
{
    /// <see cref="Region.Property.Public.Override"/>

    public override Type ItemValueType => typeof(T);

    public override Type ItemType => typeof(Item<T>);

    public override IEnumerable<Type> ItemTypes { get { yield return ItemValueType; } }

    /// <see cref="Region.Property.Public"/>
    #region

    public T SelectedItem => SelectedItems?.FirstOrDefault<object>() is Item<T> result ? result.Value : default;

    public object SelectedGroup { get => Get<object>(); set => Set(value); }

    public int SelectedGroupIndex { get => Get(-1); set => Set(value); }

    #endregion

    /// <see cref="View.Header"/>
    #region

    [Group(GroupDefault.Group, Index = -1)]
    [Style(Index = 0,
        View = Ion.View.Header)]
    public Images GroupIcon => Images.Group;

    [Description("The current group.")]
    [Group(GroupDefault.Group, Index = -1)]
    [Name("Group")]
    [Styles.List(Template.ListCombo,
        NameHide = true,
        Index = 1,
        Placeholder = "Select a group",
        View = Ion.View.Header,
        SelectedIndexProperty = nameof(SelectedGroupIndex),
        SelectedItemProperty = nameof(SelectedGroup))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItemGroup.Name),
        TargetItem = typeof(IItemGroup))]
    public GroupListWritable<T> Groups { get => Get<GroupListWritable<T>>(); set => Set(value); }
    IListWritable IDataGroupPanel.Groups => Groups;

    #endregion

    /// <see cref="View.Option"/>
    #region

    /// <see cref="DataViewPanel.Tab.Group"/>
    #region

    /// <see cref="GroupDefault.New"/>
    #region

    [Group(GroupDefault.New)]
    [Description("The suffix to append to the name of cloned items.")]
    [Name("CloneSuffix")]
    [Styles.TextAttribute(Tab = Tab.Group, View = Ion.View.Option)]
    public string NewCloneSuffix { get => Get(" (Clone)"); set => Set(value); }

    [Group(GroupDefault.New)]
    [Description("The default name of new groups.")]
    [Name("GroupName")]
    [Styles.TextAttribute(Tab = Tab.Group, View = Ion.View.Option)]
    public string NewGroupName { get => Get("Untitled group"); set => Set(value); }

    [Group(GroupDefault.New)]
    [Description("The default description of new items.")]
    [Name("ItemDescription")]
    [Styles.TextAttribute(Tab = Tab.Group, View = Ion.View.Option)]
    public string NewItemDescription { get => Get("No description"); set => Set(value); }

    [Group(GroupDefault.New)]
    [Description("The default name of new items.")]
    [Name("ItemName")]
    [Styles.TextAttribute(Tab = Tab.Group, View = Ion.View.Option)]
    public string NewItemName { get => Get("Untitled"); set => Set(value); }

    #endregion

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    protected DataGroupPanel() : base() { }

    protected DataGroupPanel(IListWritable groups) : this() => Groups = (GroupListWritable<T>)groups;

    /// <see cref="Region.Method.Protected.Abstract"/>

    protected abstract IEnumerable<ItemGroup<T>> GetDefaultGroups();

    /// <see cref="Region.Method.Protected.Override"/>
    #region

    protected override void Add(Type input)
    {
        T value = input is Type type && ItemTypeHandlers?.ContainsKey(type) == true ? (T)ItemTypeHandlers[type]() : (T)GetDefaultItem();

        var oldItem = new Item<T>(NewItemName, NewItemDescription, value);
        var newItem = new GroupValueForm<T>(Groups, oldItem, SelectedGroupIndex);

        Dialog.ShowObject($"Add {ItemName}", newItem, Resource.GetImageUri(Images.Plus), i =>
        {
            if (i == 0)
            {
                newItem.GroupIndex = newItem.GroupIndex == -1 ? SelectedGroupIndex : newItem.GroupIndex;
                Groups[newItem.GroupIndex].Add((Item<T>)newItem.Value);
            }
        },
        Buttons.SaveCancel);
    }

    protected override void AddFromPreset(ItemPreset preset)
    {
        var value = Instance.CloneDeep(preset.Value, new CreateFromObject());
        value.IfNotNull(_ =>
        {
            Dialog.ShowObject($"Add {ItemName}", value, Resource.GetImageUri(Images.Plus), i =>
            {
                if (i == 0)
                    Items.Add(this.As<IDataGroupPanel>().Create(preset.Name, NewItemDescription, value));
            },
            Buttons.SaveCancel);
        });
    }

    protected override void SaveAsPreset(IReadOnlyCollection<object> items)
    {
        foreach (IItem i in items.Cast<IItem>())
        {
            var value = Instance.CloneDeep(i.Value, new CreateFromObject());
            value.IfNotNull(_ => ItemPresets.Add(new ItemPreset(i.Name ?? NewPresetName, value)));
        }
    }

    protected override void OnItemCloned(object item)
    {
        if (item is Item<T> i)
            i.Name += NewCloneSuffix;
    }

    #endregion

    /// <see cref="Region.Method.Public"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Groups):
                Groups.IfNotNull(i => i.Count == 0, i => GetDefaultGroups().ForEach(j => i.Add(j)));
                //SelectedGroupIndex = SelectedGroupIndex == -1 ? 0 : SelectedGroupIndex;
                break;

            case nameof(SelectedGroup):
                Items = SelectedGroup as IListObservable;
                break;

            case nameof(SelectedItems):
                Reset(() => SelectedItem);
                break;
        }
    }

    /// <see cref="IDataGroupPanel"/>

    IItem IDataGroupPanel.Create(object item) => new Item<T>(NewItemName, NewItemDescription, (T)item);

    IItem IDataGroupPanel.Create(string name, string description, object item) => new Item<T>(name, description, (T)item);

    /// <see cref="ICommand"/>
    #region

    [Group(GroupDefault.Group)]
    [Image(Images.FolderAdd)]
    [Name("Add group")]
    [Style(Index = 2,
        View = Ion.View.HeaderOption)]
    public ICommand AddGroupCommand => Commands[nameof(AddGroupCommand)] ??= new RelayCommand(() =>
    {
        var result = new NameForm(NewGroupName);
        Dialog.ShowObject("Add group", result, Resource.GetImageUri(Images.FolderAdd), i =>
        {
            if (i == 0)
                Groups.Add(new ItemGroup<T>(result.Name));
        },
        Buttons.SaveCancel);
    },
    () => Groups != null);

    [Group(GroupDefault.Group)]
    [Image(Images.FolderEdit)]
    [Name("Edit group")]
    [Style(Index = 1,
        View = Ion.View.HeaderOption)]
    public ICommand EditGroupCommand => Commands[nameof(EditGroupCommand)] ??= new RelayCommand(() =>
    {
        var result = new NameForm((SelectedGroup as ItemGroup<T>).Name);
        Dialog.ShowObject("Edit group", result, Resource.GetImageUri(Images.FolderEdit), i =>
        {
            if (i == 0)
                (SelectedGroup as ItemGroup<T>).Name = result.Name;
        },
        Buttons.Done);
    },
    () => SelectedGroup != null);

    [Group(GroupDefault.Group)]
    [Image(Images.FolderDelete)]
    [Name("Delete group")]
    [Style(Index = 3,
        View = Ion.View.HeaderOption)]
    public ICommand RemoveGroupCommand => Commands[nameof(RemoveGroupCommand)] ??= new RelayCommand(() =>
    {
        Dialog.ShowResult("Remove group", new Warning("Are you sure you want to remove the selected group?"), i =>
        {
            if (i == 0)
                Groups.Remove(SelectedGroup as ItemGroup<T>);
        },
        Buttons.YesNo);
    },
    () => Groups?.Contains(SelectedGroup as ItemGroup<T>) == true);

    [Group(GroupDefault.Group)]
    [Image(Images.FolderReset)]
    [Name("Reset groups")]
    [Style(Index = 4,
        View = Ion.View.HeaderOption)]
    public ICommand ResetGroupsCommand => Commands[nameof(ResetGroupsCommand)] ??= new RelayCommand(() =>
    {
        Dialog.ShowResult("Reset groups", new Warning("Are you sure you want to reset all groups?"), i =>
        {
            if (i == 0)
            {
                Groups.Clear();
                GetDefaultGroups().ForEach(i => Groups.Add(i));
            }
        },
        Buttons.YesNo);
    });

    ///

    [Group(GroupDefault.Export)]
    [Image(Images.Export)]
    [Name("Export")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand ExportCommand
        => Commands[nameof(ExportCommand)] ??= new RelayCommand(() => Groups.Export([SelectedGroup as ItemGroup<T>]), () => SelectedGroup != null);

    [Group(GroupDefault.Export)]
    [Image(Images.ExportAll)]
    [Name("ExportAll")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand ExportAllCommand
        => Commands[nameof(ExportAllCommand)] ??= new RelayCommand(() => Groups.Export(Groups));

    [Group(GroupDefault.Import)]
    [Image(Images.Import)]
    [Name("Import")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand ImportCommand
        => Commands[nameof(ImportCommand)] ??= new RelayCommand(() => Groups.Import());

    ///

    [Group(GroupDefault.Data)]
    [Image(Images.Save)]
    [Name("Save")]
    [Style(View = Ion.View.HeaderOption)]
    public virtual ICommand SaveCommand => Commands[nameof(SaveCommand)] ??= new RelayCommand(() => Groups.Save().If<Error>(i => Dialog.ShowResult("Save", i, Buttons.Ok)), () => Groups is not null);

    #endregion
}

#endregion