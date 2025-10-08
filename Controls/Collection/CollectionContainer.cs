using Ion.Data;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Ion.Controls;

[Extend<CollectionContainer>]
public static class XCollectionContainer
{
    /// <see cref="Region.Property"/>
    #region

    #region GroupConverter

    public static readonly DependencyProperty GroupConverterProperty = DependencyProperty.RegisterAttached("GroupConverter", typeof(IValueConverter), typeof(XCollectionContainer), new FrameworkPropertyMetadata(null, OnGroupConverterChanged));
    public static IValueConverter GetGroupConverter(CollectionContainer i) => (IValueConverter)i.GetValue(GroupConverterProperty);
    public static void SetGroupConverter(CollectionContainer i, IValueConverter input) => i.SetValue(GroupConverterProperty, input);

    #endregion

    #region GroupConverterSelector

    public static readonly DependencyProperty GroupConverterSelectorProperty = DependencyProperty.RegisterAttached("GroupConverterSelector", typeof(ConverterSelector), typeof(XCollectionContainer), new FrameworkPropertyMetadata(null, OnGroupConverterSelectorChanged));
    public static ConverterSelector GetGroupConverterSelector(CollectionContainer i) => (ConverterSelector)i.GetValue(GroupConverterSelectorProperty);
    public static void SetGroupConverterSelector(CollectionContainer i, ConverterSelector input) => i.SetValue(GroupConverterSelectorProperty, input);

    #endregion

    #region GroupDirection

    public static readonly DependencyProperty GroupDirectionProperty = DependencyProperty.RegisterAttached("GroupDirection", typeof(ListSortDirection), typeof(XCollectionContainer), new FrameworkPropertyMetadata(ListSortDirection.Ascending, OnGroupDirectionChanged));
    public static ListSortDirection GetGroupDirection(CollectionContainer i) => (ListSortDirection)i.GetValue(GroupDirectionProperty);
    public static void SetGroupDirection(CollectionContainer i, ListSortDirection input) => i.SetValue(GroupDirectionProperty, input);

    #endregion

    #region GroupName

    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.RegisterAttached("GroupName", typeof(string), typeof(XCollectionContainer), new FrameworkPropertyMetadata(null, OnGroupNameChanged, OnGroupNameCoerced));
    public static string GetGroupName(CollectionContainer i) => (string)i.GetValue(GroupNameProperty);
    public static void SetGroupName(CollectionContainer i, string input) => i.SetValue(GroupNameProperty, input);

    #endregion

    #region NoGroupName

    public static readonly DependencyProperty NoGroupNameProperty = DependencyProperty.RegisterAttached("NoGroupName", typeof(string), typeof(XCollectionContainer), new FrameworkPropertyMetadata("None", OnNoGroupNameChanged));
    public static string GetNoGroupName(CollectionContainer i) => (string)i.GetValue(NoGroupNameProperty);
    public static void SetNoGroupName(CollectionContainer i, string input) => i.SetValue(NoGroupNameProperty, input);

    #endregion

    #region NoSortName

    public static readonly DependencyProperty NoSortNameProperty = DependencyProperty.RegisterAttached("NoSortName", typeof(string), typeof(XCollectionContainer), new FrameworkPropertyMetadata("None", OnNoSortNameChanged));
    public static string GetNoSortName(CollectionContainer i) => (string)i.GetValue(NoSortNameProperty);
    public static void SetNoSortName(CollectionContainer i, string input) => i.SetValue(NoSortNameProperty, input);

    #endregion

    #region SortByGroup

    public static readonly DependencyProperty SortByGroupProperty = DependencyProperty.RegisterAttached("SortByGroup", typeof(bool), typeof(XCollectionContainer), new FrameworkPropertyMetadata(true, OnSortByGroupChanged));
    public static bool GetSortByGroup(CollectionContainer i) => (bool)i.GetValue(SortByGroupProperty);
    public static void SetSortByGroup(CollectionContainer i, bool input) => i.SetValue(SortByGroupProperty, input);

    #endregion

    #region SortComparer

    public static readonly DependencyProperty SortComparerProperty = DependencyProperty.RegisterAttached("SortComparer", typeof(IComparer), typeof(XCollectionContainer), new FrameworkPropertyMetadata(null, OnSortComparerChanged));
    public static IComparer GetSortComparer(CollectionContainer i) => (IComparer)i.GetValue(SortComparerProperty);
    public static void SetSortComparer(CollectionContainer i, IComparer input) => i.SetValue(SortComparerProperty, input);

    #endregion

    #region SortDirection

    public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached("SortDirection", typeof(ListSortDirection), typeof(XCollectionContainer), new FrameworkPropertyMetadata(ListSortDirection.Ascending, OnSortDirectionChanged));
    public static ListSortDirection GetSortDirection(CollectionContainer i) => (ListSortDirection)i.GetValue(SortDirectionProperty);
    public static void SetSortDirection(CollectionContainer i, ListSortDirection input) => i.SetValue(SortDirectionProperty, input);

    #endregion

    #region SortName

    public static readonly DependencyProperty SortNameProperty = DependencyProperty.RegisterAttached("SortName", typeof(object), typeof(XCollectionContainer), new FrameworkPropertyMetadata(null, OnSortNameChanged, OnSortNameCoerced));
    public static object GetSortName(CollectionContainer i) => i.GetValue(SortNameProperty);
    public static void SetSortName(CollectionContainer i, object input) => i.SetValue(SortNameProperty, input);

    #endregion

    #region Source

    public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(IEnumerable), typeof(XCollectionContainer), new FrameworkPropertyMetadata(null, OnSourceChanged));
    public static IEnumerable GetSource(CollectionContainer i) => (IEnumerable)i.GetValue(SourceProperty);
    public static void SetSource(CollectionContainer i, IEnumerable input) => i.SetValue(SourceProperty, input);
    private static void OnSourceChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<CollectionContainer>(i => OnSourceChanged(i, e.Convert<IEnumerable>()));

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private static object OnNameCoerced(object i)
        => i?.ToString().Trim() is string j && !j.IsEmpty() ? j : null;

    private static void SetGroup(CollectionContainer control)
    {
        if (control.Collection is ListCollectionView list)
        {
            list.GroupDescriptions.Clear();

            var groupName
                = GetGroupName(control);
            var groupConverter
                = GetGroupConverterSelector(control)?.SelectConverter(groupName) ?? GetGroupConverter(control);

            if (groupConverter is not null || groupName is not null)
            {
                PropertyGroupDescription description = new() { Converter = groupConverter };
                if (description.Converter is null)
                    description.PropertyName = groupName;

                list.GroupDescriptions.Add(description);
            }
        }
    }

    private static void SetSort(CollectionContainer control)
    {
        if (control.Collection is ListCollectionView list)
        {
            list.SortDescriptions.Clear();
            if (GetSortComparer(control) is IComparer comparer)
            {
                list.CustomSort = comparer;
                list.Refresh();
            }
            else
            {
                list.CustomSort = null;
                if (GetSortByGroup(control) && GetGroupName(control) is object groupName)
                    list.SortDescriptions.Add(new SortDescription($"{groupName}", GetGroupDirection(control)));

                if (GetSortName(control) is object sortName && !Equals(sortName, GetNoSortName(control)))
                    list.SortDescriptions.Add(new SortDescription($"{sortName}", GetSortDirection(control)));
            }
        }
    }

    ///

    ///<remarks>Affects <see cref="ListCollectionView.GroupDescriptions"/>.</remarks>
    private static void OnGroupConverterChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetGroup(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.GroupDescriptions"/>.</remarks>
    private static void OnGroupConverterSelectorChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetGroup(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnGroupDirectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetSort(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.GroupDescriptions"/> and <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnGroupNameChanged(object sender, DependencyPropertyChangedEventArgs e)
    { SetGroup(sender as CollectionContainer); SetSort(sender as CollectionContainer); }

    private static object OnGroupNameCoerced(DependencyObject sender, object i)
        => OnNameCoerced(i);

    ///<remarks>Affects <see cref="ListCollectionView.GroupDescriptions"/> and <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnNoGroupNameChanged(object sender, DependencyPropertyChangedEventArgs e)
    { SetGroup(sender as CollectionContainer); SetSort(sender as CollectionContainer); }

    ///<remarks>Affects <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnNoSortNameChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetSort(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnSortByGroupChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetSort(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnSortComparerChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetSort(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnSortDirectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetSort(sender as CollectionContainer);

    ///<remarks>Affects <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnSortNameChanged(object sender, DependencyPropertyChangedEventArgs e)
        => SetSort(sender as CollectionContainer);

    private static object OnSortNameCoerced(DependencyObject sender, object i)
        => OnNameCoerced(i);

    ///<remarks>Affects <see cref="ListCollectionView.GroupDescriptions"/> and <see cref="ListCollectionView.SortDescriptions"/>.</remarks>
    private static void OnSourceChanged(CollectionContainer control, ValueChange<IEnumerable> input)
    {
        control.Collection = input.NewValue;
        SetGroup(control); SetSort(control);
    }

    #endregion
}