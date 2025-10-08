using Ion.Analysis;
using Ion.Input;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ion.Controls;

[Extend<ItemsControl>]
public static class XItemsControl
{
    /// <see cref="Region.Field"/>
    #region

    public static readonly ResourceKey EmptyHorizontalTemplateKey = new();

    public static readonly ResourceKey EmptyVerticalTemplateKey = new();

    public static readonly ResourceKey GroupHeaderTemplate = new();

    public static readonly ResourceKey GroupStyleKey = new();

    public static readonly ResourceKey MenuGroupStyle = new();

    #endregion

    /// <see cref="Region.Property"/>
    #region

    #region AddCommand

    public static readonly RoutedUICommand AddCommand = new("AddCommand", "AddCommand", typeof(XItemsControl));

    private static void OnAddCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is ItemsControl control)
        {
            if (control.ItemsSource is IList list)
            {
                Type result = default;
                foreach (var i in list.GetType().GetBaseTypes())
                {
                    var j = i.GetGenericArguments();
                    if (j.Length == 1)
                    {
                        result = j[0];
                        break;
                    }
                }

                if (result != null)
                {
                    object instance = null;
                    Try.Do(() => instance = result.Create<object>());
                    if (instance != null)
                        list.Add(instance);
                }
            }
        }
    }

    private static void OnAddCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
            e.CanExecute = dataGrid.CanUserAddRows;

        e.CanExecute = true;
    }

    #endregion

    #region AutoSizeColumns

    /// <summary>
    /// Applies GridUnit.Star GridLength to all columns.
    /// </summary>
    public static readonly DependencyProperty AutoSizeColumnsProperty = DependencyProperty.RegisterAttached("AutoSizeColumns", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false, OnAutoSizeColumnsChanged));
    public static bool GetAutoSizeColumns(ItemsControl i) => (bool)i.GetValue(AutoSizeColumnsProperty);
    public static void SetAutoSizeColumns(ItemsControl i, bool input) => i.SetValue(AutoSizeColumnsProperty, input);

    private static void OnAutoSizeColumnsChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not DataGrid && sender is not ListView && sender is not TreeView)
            throw new NotSupportedException();

        if (sender is DataGrid dataGrid)
        {
            var l = (bool)e.NewValue ? new DataGridLength(1.0, DataGridLengthUnitType.Star) : new DataGridLength(1.0, DataGridLengthUnitType.Auto);
            dataGrid.Columns.ForEach(i => i.Width = l);
        }
        if (sender is ListView listView)
        {
            listView.AddHandlerAttached(true, AutoSizeColumnsProperty, i =>
            {
                UpdateColumnWidth(listView);
                listView.SizeChanged
                    += AutoSizeColumns_SizeChanged;
            }, i =>
            {
                listView.SizeChanged
                    -= AutoSizeColumns_SizeChanged;
            });
        }
    }

    private static void AutoSizeColumns_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is ListView listView)
        {
            if (listView.IsLoaded)
                UpdateColumnWidth(listView);
        }
    }

    ///

    private static void UpdateColumnWidth(ListView listView)
    {
        //Pull the stretch columns fromt the tag property.
        var columns = listView.Tag as List<GridViewColumn>;
        double specifiedWidth = 0;

        if (listView.View is GridView gridView)
        {
            if (columns is null)
            {
                //Instance if its our first run.
                columns = [];
                // Get all columns with no width having been set.
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (!(column.Width >= 0))
                        columns.Add(column);
                    else
                    {
                        specifiedWidth += column.ActualWidth;
                    }
                }
            }
            else
            {
                // Get all columns with no width having been set.
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (!columns.Contains(column))
                        specifiedWidth += column.ActualWidth;
                }
            }

            // Allocate remaining space equally.
            foreach (GridViewColumn column in columns)
            {
                var newWidth = (listView.ActualWidth - specifiedWidth) / columns.Count;
                if (newWidth >= 10)
                    column.Width = newWidth - 10;
            }

            //Store the columns in the TAG property for later use.
            listView.Tag = columns;
        }
    }

    #endregion

    #region CanDragSelect

    /// <summary>Gets or sets if items can be drag selected. Items must implement <see cref="ISelect"/>.</summary>
    public static readonly DependencyProperty CanDragSelectProperty = DependencyProperty.RegisterAttached("CanDragSelect", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false));
    public static bool GetCanDragSelect(DependencyObject i) => (bool)i.GetValue(CanDragSelectProperty);
    public static void SetCanDragSelect(DependencyObject i, bool input) => i.SetValue(CanDragSelectProperty, input);

    #endregion

    #region CanDragSelectGlobally

    public static readonly DependencyProperty CanDragSelectGloballyProperty = DependencyProperty.RegisterAttached("CanDragSelectGlobally", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false));
    public static bool GetCanDragSelectGlobally(ItemsControl i) => (bool)i.GetValue(CanDragSelectGloballyProperty);
    public static void SetCanDragSelectGlobally(ItemsControl i, bool input) => i.SetValue(CanDragSelectGloballyProperty, input);

    #endregion

    #region (readonly) ContainerIndex

    private static readonly DependencyPropertyKey ContainerIndexKey = DependencyProperty.RegisterAttachedReadOnly("ContainerIndex", typeof(int), typeof(XItemsControl), new FrameworkPropertyMetadata(-1));
    public static readonly DependencyProperty ContainerIndexProperty = ContainerIndexKey.DependencyProperty;
    public static int GetContainerIndex(FrameworkElement i) => (int)i.GetValue(ContainerIndexProperty);
    private static void SetContainerIndex(FrameworkElement i, int input) => i.SetValue(ContainerIndexKey, input);

    #endregion

    #region ContainerIndexEnable

    public static readonly DependencyProperty ContainerIndexEnableProperty = DependencyProperty.RegisterAttached("ContainerIndexEnable", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false));
    public static bool GetContainerIndexEnable(ItemsControl i) => (bool)i.GetValue(ContainerIndexEnableProperty);
    public static void SetContainerIndexEnable(ItemsControl i, bool input) => i.SetValue(ContainerIndexEnableProperty, input);

    #endregion

    #region ContainerIndexOrigin

    public static readonly DependencyProperty ContainerIndexOriginProperty = DependencyProperty.RegisterAttached("ContainerIndexOrigin", typeof(int), typeof(XItemsControl), new FrameworkPropertyMetadata(0));
    public static int GetContainerIndexOrigin(ItemsControl i) => (int)i.GetValue(ContainerIndexOriginProperty);
    public static void SetContainerIndexOrigin(ItemsControl i, int input) => i.SetValue(ContainerIndexOriginProperty, input);

    #endregion

    #region (private) Count

    private static readonly DependencyProperty CountProperty = DependencyProperty.RegisterAttached("Count", typeof(int), typeof(XItemsControl), new FrameworkPropertyMetadata(0, OnCountChanged));
    private static void OnCountChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ItemsControl>(i => OnCountChanged(i, e.Convert<int>()));

    #endregion

    #region DragScrollOffset

    public static readonly DependencyProperty DragScrollOffsetProperty = DependencyProperty.RegisterAttached("DragScrollOffset", typeof(double), typeof(XItemsControl), new FrameworkPropertyMetadata(8.0));
    public static double GetDragScrollOffset(DependencyObject i) => (double)i.GetValue(DragScrollOffsetProperty);
    public static void SetDragScrollOffset(DependencyObject i, double input) => i.SetValue(DragScrollOffsetProperty, input);

    #endregion

    #region DragScrollOffsetMaximum

    public static readonly DependencyProperty DragScrollOffsetMaximumProperty = DependencyProperty.RegisterAttached("DragScrollOffsetMaximum", typeof(double), typeof(XItemsControl), new FrameworkPropertyMetadata(32.0));
    public static double GetDragScrollOffsetMaximum(DependencyObject i) => (double)i.GetValue(DragScrollOffsetMaximumProperty);
    public static void SetDragScrollOffsetMaximum(DependencyObject i, double input) => i.SetValue(DragScrollOffsetMaximumProperty, input);

    #endregion

    #region DragScrollTolerance

    public static readonly DependencyProperty DragScrollToleranceProperty = DependencyProperty.RegisterAttached("DragScrollTolerance", typeof(double), typeof(XItemsControl), new FrameworkPropertyMetadata(5.0));
    public static double GetDragScrollTolerance(DependencyObject i) => (double)i.GetValue(DragScrollToleranceProperty);
    public static void SetDragScrollTolerance(DependencyObject i, double input) => i.SetValue(DragScrollToleranceProperty, input);

    #endregion

    #region EmptyTemplate

    public static readonly DependencyProperty EmptyTemplateProperty = DependencyProperty.RegisterAttached("EmptyTemplate", typeof(DataTemplate), typeof(XItemsControl), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetEmptyTemplate(ItemsControl i) => (DataTemplate)i.GetValue(EmptyTemplateProperty);
    public static void SetEmptyTemplate(ItemsControl i, DataTemplate input) => i.SetValue(EmptyTemplateProperty, input);

    #endregion

    #region EmptyTemplateVisibility

    public static readonly DependencyProperty EmptyTemplateVisibilityProperty = DependencyProperty.RegisterAttached("EmptyTemplateVisibility", typeof(Visibility), typeof(XItemsControl), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public static Visibility GetEmptyTemplateVisibility(ItemsControl i) => (Visibility)i.GetValue(EmptyTemplateVisibilityProperty);
    public static void SetEmptyTemplateVisibility(ItemsControl i, Visibility input) => i.SetValue(EmptyTemplateVisibilityProperty, input);

    #endregion

    #region Extend

    public static readonly DependencyProperty ExtendProperty = DependencyProperty.RegisterAttached("Extend", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false, OnExtendChanged));
    public static bool GetExtend(ItemsControl i) => (bool)i.GetValue(ExtendProperty);
    public static void SetExtend(ItemsControl i, bool input) => i.SetValue(ExtendProperty, input);

    private static void OnExtendChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ItemsControl control)
            control.AddHandlerAttached((bool)e.NewValue, ExtendProperty, OnLoaded, OnUnloaded);
    }

    #endregion

    #region GroupStyle

    public static readonly DependencyProperty GroupStyleProperty = DependencyProperty.RegisterAttached("GroupStyle", typeof(GroupStyle), typeof(XItemsControl), new FrameworkPropertyMetadata(null, OnGroupStyleChanged));
    public static GroupStyle GetGroupStyle(ItemsControl i) => (GroupStyle)i.GetValue(GroupStyleProperty);
    public static void SetGroupStyle(ItemsControl i, GroupStyle input) => i.SetValue(GroupStyleProperty, input);

    private static void OnGroupStyleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ItemsControl control)
        {
            if (e.OldValue is GroupStyle oldStyle)
                control.GroupStyle.Remove(oldStyle);

            if (e.NewValue is GroupStyle newStyle)
                control.GroupStyle.Add(newStyle);
        }
    }

    #endregion

    #region (readonly) IsEmpty

    private static readonly DependencyPropertyKey IsEmptyKey = DependencyProperty.RegisterAttachedReadOnly("IsEmpty", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(true, null, new CoerceValueCallback(OnIsEmptyCoerced)));
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyKey.DependencyProperty;
    public static bool GetIsEmpty(ItemsControl i) => (bool)i.GetValue(IsEmptyProperty);
    private static object OnIsEmptyCoerced(DependencyObject sender, object input)
        => sender is ItemsControl i && i.Items.Count == 0;

    #endregion

    #region (readonly) FrameworkElement > IsLastContainer

    private static readonly DependencyPropertyKey IsLastContainerKey = DependencyProperty.RegisterAttachedReadOnly("IsLastContainer", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsLastContainerProperty = IsLastContainerKey.DependencyProperty;
    public static bool GetIsLastContainer(FrameworkElement i) => (bool)i.GetValue(IsLastContainerProperty);
    private static void SetIsLastContainer(FrameworkElement i, bool input) => i.SetValue(IsLastContainerKey, input);

    #endregion

    #region KeySelect

    public static readonly DependencyProperty KeySelectProperty = DependencyProperty.RegisterAttached("KeySelect", typeof(bool), typeof(XItemsControl), new FrameworkPropertyMetadata(false, OnKeySelectChanged));
    public static bool GetKeySelect(ItemsControl i) => (bool)i.GetValue(KeySelectProperty);
    public static void SetKeySelect(ItemsControl i, bool input) => i.SetValue(KeySelectProperty, input);

    private static void OnKeySelectChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not DataGrid && sender is not ListBox && sender is not TreeView)
            throw new NotSupportedException();

        if (sender is ItemsControl control)
        {
            control.AddHandlerAttached((bool)e.NewValue, KeySelectProperty, k =>
            {
                control.KeyDown
                    += KeySelect_KeyDown;
            }, k =>
            {
                control.KeyDown
                    -= KeySelect_KeyDown;

                GetKeySelectTimer(control).Stop();
                GetKeySelectTimer(control).Tick -= KeySelect_Tick;

                KeySelectTimers.Remove(GetKeySelectTimer(control));
                SetKeySelectTimer(control, null);
            });
        }
    }

    private static void KeySelect_KeyDown(object sender, KeyEventArgs e)
    {
        var listBox = sender as ListBox;

        char? character = e.Key.Character();
        if (character is null)
        {
            SetKeySelectQuery(listBox, null);
            return;
        }

        if (GetKeySelectQuery(listBox) is null)
            SetKeySelectQuery(listBox, string.Empty);

        SetKeySelectQuery(listBox, $"{GetKeySelectQuery(listBox)}{character}");

        var timer = GetKeySelectTimer(listBox);
        timer.Stop();
        timer.Start();

        object item = null;
        var items = listBox.ItemsSource as IList;

        var comparer = GetKeySelectComparer(listBox);
        if (items != null)
        {
            foreach (var i in items)
            {
                if (comparer.Compare(Instance.GetPropertyValue(i, GetKeySelectProperty(listBox)), GetKeySelectQuery(listBox)))
                {
                    item = i;
                    break;
                }
            }
        }

        if (item is null)
        {
            SetKeySelectQuery(listBox, null);
            return;
        }

        foreach (var i in items)
        {
            if (i != item)
            {
                if (listBox.ItemContainerGenerator.ContainerFromItem(i) is ListViewItem j)
                    j.IsSelected = false;
            }
        }

        listBox.ScrollIntoView(item);
        if (listBox.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem k)
            k.IsSelected = true;
    }

    private static void KeySelect_Tick(object sender, EventArgs e)
    {
        if (sender is DispatcherTimer i)
        {
            i.Stop();
            SetKeySelectQuery(KeySelectTimers[i], null);
        }
    }

    #endregion

    #region KeySelectComparer

    public static readonly DependencyProperty KeySelectComparerProperty = DependencyProperty.RegisterAttached("KeySelectComparer", typeof(IKeySelector), typeof(XItemsControl), new FrameworkPropertyMetadata(null));
    public static IKeySelector GetKeySelectComparer(ItemsControl i) => i.GetValueOrSetDefault<IKeySelector>(KeySelectComparerProperty, () => new KeySelector());
    public static void SetKeySelectComparer(ItemsControl i, IKeySelector input) => i.SetValue(KeySelectComparerProperty, input);

    #endregion

    #region KeySelectProperty

    public static readonly DependencyProperty KeySelectPropertyProperty = DependencyProperty.RegisterAttached("KeySelectProperty", typeof(string), typeof(XItemsControl), new FrameworkPropertyMetadata(null));
    public static string GetKeySelectProperty(ItemsControl i) => (string)i.GetValue(KeySelectPropertyProperty);
    public static void SetKeySelectProperty(ItemsControl i, string input) => i.SetValue(KeySelectPropertyProperty, input);

    #endregion

    #region (private) KeySelectQuery

    private static readonly DependencyProperty KeySelectQueryProperty = DependencyProperty.RegisterAttached("KeySelectQuery", typeof(string), typeof(XItemsControl), new FrameworkPropertyMetadata(string.Empty));

    private static string GetKeySelectQuery(ItemsControl i) => (string)i.GetValue(KeySelectQueryProperty);
    private static void SetKeySelectQuery(ItemsControl i, string input) => i.SetValue(KeySelectQueryProperty, input);

    #endregion

    #region (private) KeySelectTimer

    private static readonly Dictionary<DispatcherTimer, ItemsControl> KeySelectTimers = [];
    private static readonly DependencyProperty KeySelectTimerProperty = DependencyProperty.RegisterAttached("KeySelectTimer", typeof(DispatcherTimer), typeof(XItemsControl), new FrameworkPropertyMetadata(null));

    private static DispatcherTimer GetKeySelectTimer(ItemsControl i) => i.GetValueOrSetDefault(KeySelectTimerProperty, () =>
    {
        var result = new DispatcherTimer() { Interval = 2.Seconds() };
        result.Tick += KeySelect_Tick;
        KeySelectTimers.Add(result, i);
        return result;
    });
    private static void SetKeySelectTimer(ItemsControl i, DispatcherTimer input) => i.SetValue(KeySelectTimerProperty, input);

    #endregion

    #region SelectionButton

    public static readonly DependencyProperty SelectionButtonProperty = DependencyProperty.RegisterAttached("SelectionButton", typeof(MouseButton), typeof(XItemsControl), new FrameworkPropertyMetadata(MouseButton.Left));
    public static MouseButton GetSelectionButton(ItemsControl i) => (MouseButton)i.GetValue(SelectionButtonProperty);
    public static void SetSelectionButton(ItemsControl i, MouseButton input) => i.SetValue(SelectionButtonProperty, input);

    #endregion

    #region SelectionGlobalPredicate

    public static readonly DependencyProperty SelectionGlobalPredicateProperty = DependencyProperty.RegisterAttached("SelectionGlobalPredicate", typeof(Func<bool>), typeof(XItemsControl), new FrameworkPropertyMetadata(null));
    public static Func<bool> GetSelectionGlobalPredicate(ItemsControl i) => (Func<bool>)i.GetValue(SelectionGlobalPredicateProperty);
    public static void SetSelectionGlobalPredicate(ItemsControl i, Func<bool> input) => i.SetValue(SelectionGlobalPredicateProperty, input);

    #endregion

    #region (private) Source

    private static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(object), typeof(XItemsControl), new FrameworkPropertyMetadata(null, OnSourceChanged));

    private static void OnSourceChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ItemsControl>(i => OnSourceChanged(i, new(e.OldValue, e.NewValue)));

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    static XItemsControl() { }

    /// <see cref="Region.Method.Private"/>
    #region

    private static void OnCountChanged(ItemsControl control, ValueChange<int> input)
    {
        lock (IsEmptyProperty)
            control.InvalidateProperty(IsEmptyProperty);

        GetContainerIndexEnable(control).If(() => SetContainerIndex(control));
    }

    private static void OnSourceChanged(ItemsControl control, ValueChange input)
    {
        if (input.NewValue is ListCollectionView)
            control.Bind(CountProperty, $"{nameof(ListCollectionView.SourceCollection)}.{nameof(IList.Count)}", input.NewValue);

        else if (input.NewValue is IList)
            control.Bind(CountProperty, $"{nameof(IList.Count)}", input.NewValue);

        else control.Unbind(CountProperty);

        lock (IsEmptyProperty)
            control.InvalidateProperty(IsEmptyProperty);

        GetContainerIndexEnable(control).If(() => SetContainerIndex(control));
    }

    ///

    private static void OnLoaded(ItemsControl control)
    {
        lock (IsEmptyProperty)
            control.InvalidateProperty(IsEmptyProperty);

        Unsubscribe(control); Subscribe(control);
    }

    private static void OnUnloaded(ItemsControl control)
    {
        Unsubscribe(control);
    }

    ///

    private static void SetContainerIndex(ItemsControl control)
    {
        var lastIndex = control.Items.Count - 1;
        for (var i = lastIndex; i >= 0; i--)
        {
            var container = control.GetContainer(i);
            if (container is not null)
            {
                SetContainerIndex(container, GetContainerIndexOrigin(control) + i);
                SetIsLastContainer(container, i == lastIndex);
            }
        }
    }

    private static void Subscribe(ItemsControl control)
    {
        control.Bind(SourceProperty, nameof(ItemsControl.ItemsSource), control);
    }

    private static void Unsubscribe(ItemsControl control)
    {
        control.Unbind(CountProperty);
        control.Unbind(SourceProperty);
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public static void SelectNone(this ItemsControl control)
    {
        if (control is System.Windows.Controls.Primitives.Selector selector)
            selector.SelectNone();

        else if (control is TreeView view)
            view.SelectNone();
    }

    public static FrameworkElement GetContainer(this ItemsControl control, int index)
        => control.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;

    public static FrameworkElement GetContainer(this ItemsControl control, object item)
        => control.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;

    public static object GetItem(this ItemsControl control, DependencyObject container)
        => control.ItemContainerGenerator.ItemFromContainer(container);

    public static Result TrySelectNone(this ItemsControl control)
        => Try.Do(() => control.SelectNone());

    #endregion
}