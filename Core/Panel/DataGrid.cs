using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Collections;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Core;

/// <see cref="DataGridPanel"/>
#region

/// <inheritdoc/>
[Styles.ObjectAttribute(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab,
    MemberView = Ion.View.Option)]
[Serializable]
public abstract record class DataGridPanel() : DataPanel()
{
    [TabView(View = Ion.View.Main)]
    private enum Tab
    {
        [TabStyle(Image = Images.General)]
        General
    }

    /// <see cref="Region.Property.Public"/>

    public DataGridHeadersVisibility HeaderVisibility { get => Get(DataGridHeadersVisibility.All); set => Set(value); }

    [Group(GroupDefault.View)]
    [Style(Pin = Sides.RightOrBottom,
        Tab = Tab.General,
        View = Ion.View.Footer)]
    public DataGridViews View { get => Get(DataGridViews.Detail); set => Set(value); }

    /// <see cref="Region.Property.Public.Virtual"/>

    public virtual bool CanAddRows => true;

    public virtual bool CanDeleteRows => true;

    public virtual bool CanReorderColumns => true;

    public virtual bool CanResizeColumns => true;

    public virtual bool CanResizeRows => true;

    public virtual bool CanSortColumns => true;

    /// <see cref="Region.Constructor"/>

    protected DataGridPanel(IList items) : this() => Items = items;
}

#endregion

/// <see cref="DataGridPanel{T}"/>
#region

/// <inheritdoc/>
[Serializable]
public abstract record class DataGridPanel<T>(IList input) : DataGridPanel(input)
{
    [Hide, NonSerializable]
    public override Type ItemType => typeof(T);

    protected DataGridPanel() : this(default(IList)) { }
}

#endregion

/// <see cref="XmlDataGridPanel{T}"/>
#region

/// <inheritdoc/>
[Serializable]
public abstract record class XmlDataGridPanel<T>(IList input) : DataGridPanel<T>(input)
{
    protected XmlDataGridPanel() : this(default(IList)) { }

    /// <see cref="ICommand"/>

    [Group(GroupDefault.Data)]
    [Image(Images.Open)]
    [Name("Open")]
    [Style(View = Ion.View.HeaderOption)]
    public virtual ICommand OpenDataCommand => Commands[nameof(OpenDataCommand)] ??= new RelayCommand(() => (Items as IListWritable).Load(), () => Items is IListWritable);

    [Group(GroupDefault.Data)]
    [Image(Images.Save)]
    [Name("Save")]
    [Style(View = Ion.View.HeaderOption)]
    public virtual ICommand SaveDataCommand => Commands[nameof(SaveDataCommand)] ??= new RelayCommand(() => (Items as IListWritable).Save(), () => Items is IListWritable);
}

#endregion