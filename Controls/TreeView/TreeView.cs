using Ion;
using Ion.Collect;
using Ion.Controls;
using Ion.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<TreeView>]
public static class XTreeView
{
    #region Properties

    #region CanResizeColumns

    public static readonly DependencyProperty CanResizeColumnsProperty = DependencyProperty.RegisterAttached("CanResizeColumns", typeof(bool), typeof(XTreeView), new FrameworkPropertyMetadata(true));
    public static bool GetCanResizeColumns(TreeView i) => (bool)i.GetValue(CanResizeColumnsProperty);
    public static void SetCanResizeColumns(TreeView i, bool input) => i.SetValue(CanResizeColumnsProperty, input);

    #endregion

    #region CollapseAllCommand

    public static readonly RoutedUICommand CollapseAllCommand = new(nameof(CollapseAllCommand), nameof(CollapseAllCommand), typeof(XItemsControl));

    private static void OnCollapseAll(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TreeView treeView)
            treeView.CollapseAll();
    }

    private static void OnCanCollapseAll(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    #endregion

    #region CollapseSiblingsOnClick

    public static readonly DependencyProperty CollapseSiblingsOnClickProperty = DependencyProperty.RegisterAttached("CollapseSiblingsOnClick", typeof(bool), typeof(XTreeView), new FrameworkPropertyMetadata(false, OnCollapseSiblingsOnClickChanged));
    public static bool GetCollapseSiblingsOnClick(TreeView i) => (bool)i.GetValue(CollapseSiblingsOnClickProperty);
    public static void SetCollapseSiblingsOnClick(TreeView i, bool input) => i.SetValue(CollapseSiblingsOnClickProperty, input);

    private static void OnCollapseSiblingsOnClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TreeView treeView)
            treeView.AddHandlerAttached((bool)e.NewValue, CollapseSiblingsOnClickProperty, i => i.MouseLeftButtonUp += CollapseSiblingsOnClick_MouseLeftButtonUp, i => i.MouseLeftButtonUp -= CollapseSiblingsOnClick_MouseLeftButtonUp);
    }

    private static void CollapseSiblingsOnClick_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        => e.OriginalSource.As<DependencyObject>().GetParent<TreeViewItem>()?.CollapseSiblings();

    #endregion

    #region (private) HandleSelection

    private static readonly DependencyProperty HandleSelectionProperty = DependencyProperty.RegisterAttached("HandleSelection", typeof(Handle), typeof(XTreeView), new FrameworkPropertyMetadata(null));

    private static Handle GetHandleSelection(TreeView i) => i.GetValueOrSetDefault<Handle>(HandleSelectionProperty, () => false);

    #endregion

    #region SelectedIndex

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.RegisterAttached("SelectedIndex", typeof(int[]), typeof(XTreeView), new FrameworkPropertyMetadata(new int[1] { -1 }, OnSelectedIndexChanged));
    public static int[] GetSelectedIndex(TreeView i) => (int[])i.GetValue(SelectedIndexProperty);
    public static void SetSelectedIndex(TreeView i, int[] input) => i.SetValue(SelectedIndexProperty, input);

    private static void OnSelectedIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TreeView view)
        {
            GetHandleSelection(view).DoInternal(() =>
            {
                if (view.Items.Count > 0)
                {
                    object result = null;
                    view.Enumerate((i, j) => result = i, (int[])e.NewValue);
                    SetSelectedItem(view, result);
                }
                view.SelectSingle(GetSelectedItem(view));
            });
        }
    }

    #endregion

    #region SelectedItem

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(XTreeView), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
    public static object GetSelectedItem(TreeView i) => i.GetValue(SelectedItemProperty);
    public static void SetSelectedItem(TreeView i, object input) => i.SetValue(SelectedItemProperty, input);

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TreeView view)
        {
            GetHandleSelection(view).DoInternal(() =>
            {
                if (view.GetContainer(e.NewValue) is TreeViewItem item)
                    SetSelectedIndex(view, item.GetIndex());

                view.SelectSingle(e.NewValue);
            });
        }
    }

    #endregion

    #region SelectedItems

    private static readonly Dictionary<IList, TreeView> SelectionCache = [];
    private static readonly DependencyPropertyKey SelectedItemsKey = DependencyProperty.RegisterAttachedReadOnly("SelectedItems", typeof(IListObservable), typeof(XTreeView), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsKey.DependencyProperty;
    public static IListObservable GetSelectedItems(TreeView i) => i.GetValueOrSetDefault<IListObservable>(SelectedItemsKey, () => new ListObservable<object>());

    #endregion

    #region SelectionMode

    public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.RegisterAttached("SelectionMode", typeof(Select), typeof(XTreeView), new FrameworkPropertyMetadata(Controls.Select.One, OnSelectionModeChanged));
    public static Select GetSelectionMode(TreeView i) => (Select)i.GetValue(SelectionModeProperty);
    public static void SetSelectionMode(TreeView i, Select input) => i.SetValue(SelectionModeProperty, input);

    private static void OnSelectionModeChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TreeView treeView)
        {
            if ((Select)e.NewValue == Controls.Select.One)
            {
                if (GetSelectedItems(treeView).Count > 0)
                    treeView.SelectSingle(treeView.ItemContainerGenerator.ContainerFromItem(GetSelectedItems(treeView)[0]) as TreeViewItem);
            }
        }
    }

    #endregion

    #region (private) SelectionStart

    private static readonly DependencyProperty SelectionStartProperty = DependencyProperty.RegisterAttached("SelectionStart", typeof(TreeViewItem), typeof(XTreeView));

    private static TreeViewItem GetSelectionStart(TreeView i) => (TreeViewItem)i.GetValue(SelectionStartProperty);
    private static void SetSelectionStart(TreeView i, TreeViewItem input) => i.SetValue(SelectionStartProperty, input);

    #endregion

    #region SelectOnRightClick

    public static readonly DependencyProperty SelectOnRightClickProperty = DependencyProperty.RegisterAttached("SelectOnRightClick", typeof(bool), typeof(XTreeView), new FrameworkPropertyMetadata(false, OnSelectOnRightClickChanged));
    public static bool GetSelectOnRightClick(TreeView i) => (bool)i.GetValue(SelectOnRightClickProperty);
    public static void SetSelectOnRightClick(TreeView i, bool input) => i?.SetValue(SelectOnRightClickProperty, input);

    private static void OnSelectOnRightClickChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TreeView treeView)
            treeView.AddHandlerAttached((bool)e.NewValue, SelectOnRightClickProperty, i => i.PreviewMouseRightButtonDown += SelectOnRightClick_PreviewMouseRightButtonDown, i => i.PreviewMouseRightButtonDown -= SelectOnRightClick_PreviewMouseRightButtonDown);
    }

    private static void SelectOnRightClick_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeView treeView)
        {
            var i = e.OriginalSource.As<DependencyObject>().GetParent<TreeViewItem>();
            if (i != null)
            {
                treeView.SelectSingle(i);
                e.Handled = true;
            }
        }
    }

    #endregion

    #endregion

    #region XTreeView

    static XTreeView()
    {
        EventManager.RegisterClassHandler(typeof(TreeView), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(TreeView), UIElement.PreviewMouseLeftButtonDownEvent,
            new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown), true);
        EventManager.RegisterClassHandler(typeof(TreeView), FrameworkElement.UnloadedEvent,
            new RoutedEventHandler(OnUnloaded), true);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TreeView view)
        {
            var selection = GetSelectedItems(view);
            SelectionCache.Add(selection, view);
            selection.CollectionChanged += OnSelectedItemsChanged;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeView view)
        {
            if (e.OriginalSource is DependencyObject i && i is not TreeView)
            {
                if (i.GetVisualParent<TreeViewItem>() is TreeViewItem item)
                    view.Select(item);
            }
        }
    }

    private static void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IList items)
        {
            if (items.Count > 0)
            {
                if (SelectionCache[items] is TreeView view)
                {
                    GetHandleSelection(view).DoInternal(() =>
                    {
                        var i = GetSelectedItems(view)[0];
                        SetSelectedItem(view, i);
                        if (view.GetContainer(i) is TreeViewItem item)
                            SetSelectedIndex(view, item.GetIndex());
                    });
                }
            }
        }
    }

    private static void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is TreeView view)
        {
            var selection = GetSelectedItems(view);
            selection.CollectionChanged -= OnSelectedItemsChanged;
            SelectionCache.Remove(selection);
        }
    }

    #endregion

    #region Methods

    public static void Enumerate(this ItemsControl input, Action<object, ItemsControl> action, params int[] indices) => input.Enumerate<object>(action, indices);

    public static void Enumerate<T>(this ItemsControl input, Action<T, ItemsControl> action, params int[] indices)
    {
        if (input.Items != null && indices.Length > 0)
        {
            var m = indices[0];
            var n = 0;
            foreach (var i in input.Items)
            {
                if (m == n)
                {
                    if (i is T j)
                    {
                        var k = (ItemsControl)input.ItemContainerGenerator.ContainerFromItem(i);
                        action(j, k);

                        k?.Enumerate(action, indices.Skip(1).ToArray());
                    }
                }
                n++;
            }
        }
    }

    ///

    private static IEnumerable<TreeViewItem> GetAll(this ItemsControl input)
    {
        if (input != null)
        {
            for (var i = 0; i < input.Items.Count; i++)
            {
                if (input.ItemContainerGenerator.ContainerFromIndex(i) is TreeViewItem item)
                {
                    yield return item;
                    foreach (var j in (item as ItemsControl).GetAll())
                        yield return j;
                }
            }
        }
        yield break;
    }

    public static IEnumerable<TreeViewItem> GetAll(this TreeView input)
    {
        foreach (var i in (input as ItemsControl).GetAll())
            yield return i;

        yield break;
    }

    public static IEnumerable<TreeViewItem> GetAll(this TreeViewItem input)
    {
        foreach (var i in (input as ItemsControl).GetAll())
            yield return i;

        yield break;
    }

    ///

    public static int[] GetIndex(this TreeViewItem input)
    {
        var result = new List<int>();

        ItemsControl parent = input;
        while (parent != null && parent is not TreeView)
        {
            var nextParent = parent.GetParent<ItemsControl>();
            if (nextParent != null)
                result.Add(nextParent.Items.IndexOf(parent.DataContext));

            parent = nextParent;
        }
        result.Reverse();
        return [.. result];
    }

    public static TreeViewItem GetContainer(this TreeView input, object item) => XItemsControl.GetContainer(input, item) as TreeViewItem;

    public static ItemsControl GetParent(this TreeViewItem input) => input.GetParent<TreeViewItem>() as ItemsControl ?? input.GetParent<TreeView>();

    ///

    public static void Select(this TreeView input, TreeViewItem item)
    {
        if (item is null)
            return;

        var a = GetSelectionMode(input) != Controls.Select.OneOrMore;
        var b = !ModifierKeys.Control.Pressed() && !ModifierKeys.Shift.Pressed();

        if (a || b)
            input.SelectSingle(item);

        else if (ModifierKeys.Control.Pressed())
            input.SelectMultiple(item);

        else if (ModifierKeys.Shift.Pressed())
            input.SelectMultipleBetween(item);
    }

    ///

    private static void SelectMultiple(this TreeView input, TreeViewItem item)
    {
        XTreeViewItem.SetIsSelected(item, !XTreeViewItem.GetIsSelected(item));
        if (GetSelectionStart(input) is null)
        {
            if (XTreeViewItem.GetIsSelected(item))
                SetSelectionStart(input, item);
        }
        else if (GetSelectedItems(input).Count == 0)
            SetSelectionStart(input, null);
    }

    private static void SelectMultipleBetween(this TreeView input, TreeViewItem item, bool modifier = false)
    {
        if (GetSelectionStart(input) != null)
        {
            if (GetSelectionStart(input) == item)
            {
                input.SelectSingle(item);
                return;
            }

            var allItems = new List<TreeViewItem>(input.GetAll());

            var between = false;
            foreach (var i in allItems)
            {
                if (i == item || i == GetSelectionStart(input))
                {
                    between = !between;
                    XTreeViewItem.SetIsSelected(i, true);
                    continue;
                }
                if (between)
                {
                    XTreeViewItem.SetIsSelected(i, true);
                    continue;
                }
                if (!modifier)
                    XTreeViewItem.SetIsSelected(i, false);
            }
        }
    }

    ///

    public static void SelectNone(this TreeView input)
    {
        if (GetSelectionMode(input) != Controls.Select.One)
            input.GetAll().ForEach(i => XTreeViewItem.SetIsSelected(i, false));
    }

    ///

    public static void SelectSingle(this TreeView input, TreeViewItem item)
    {
        foreach (var i in input.GetAll())
        {
            var select = i == item
&& (GetSelectionMode(input) == Controls.Select.One
|| !XTreeViewItem.GetIsSelected(i));

            XTreeViewItem.SetIsSelected(i, select);
            if (select)
                SetSelectionStart(input, i);
        }
    }

    public static void SelectSingle(this TreeView input, object item)
    {
        foreach (var i in input.GetAll())
        {
            var select = i.DataContext == item
&& (GetSelectionMode(input) == Controls.Select.One
|| !XTreeViewItem.GetIsSelected(i));

            XTreeViewItem.SetIsSelected(i, select);
            if (select)
                SetSelectionStart(input, i);
        }
    }

    ///

    public static void CollapseAll(this TreeView input) => input.GetAll().ForEach(i => i.IsExpanded = false);

    public static void CollapseSiblings(this TreeViewItem input)
    {
        if (input.GetParent() is ItemsControl parent)
        {
            for (var i = parent.Items.Count - 1; i >= 0; i--)
            {
                if (i >= parent.Items.Count)
                    break;

                if (parent.ItemContainerGenerator.ContainerFromItem(parent.Items[i]) is TreeViewItem j)
                    j.IsExpanded = j.Equals(input);
            }
        }
    }

    public static void ExpandAll(this TreeView input) => input.GetAll().ForEach(i => i.IsExpanded = true);

    #endregion
}