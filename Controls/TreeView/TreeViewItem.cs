﻿using Ion.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<TreeViewItem>]
public static class XTreeViewItem
{
    public static readonly ResourceKey ToggleButtonTemplateKey = new();

    #region Properties

    #region ExpandedCommand

    public static readonly DependencyProperty ExpandedCommandProperty = DependencyProperty.RegisterAttached("ExpandedCommand", typeof(ICommand), typeof(XTreeViewItem), new FrameworkPropertyMetadata(OnExpandedCommandChanged));
    public static ICommand GetExpandedCommand(TreeViewItem i) => (ICommand)i.GetValue(ExpandedCommandProperty);
    public static void SetExpandedCommand(TreeViewItem i, ICommand input) => i.SetValue(ExpandedCommandProperty, input);

    private static void OnExpandedCommandChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        if (i is TreeViewItem treeViewItem)
            treeViewItem.AddHandlerAttached(e.NewValue is ICommand, ExpandedCommandProperty, i => i.Expanded += ExpandedCommand_Expanded, i => i.Expanded -= ExpandedCommand_Expanded);
    }

    private static void ExpandedCommand_Expanded(object sender, RoutedEventArgs e)
    {
        GetExpandedCommand(sender as TreeViewItem)?.Execute();
        e.Handled = true;
    }

    #endregion

    #region SelectedCommand

    public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.RegisterAttached("SelectedCommand", typeof(ICommand), typeof(XTreeViewItem), new FrameworkPropertyMetadata(null));
    public static ICommand GetSelectedCommand(TreeViewItem i) => (ICommand)i.GetValue(SelectedCommandProperty);
    public static void SetSelectedCommand(TreeViewItem i, ICommand value) => i.SetValue(SelectedCommandProperty, value);

    #endregion

    #region SelectedCommandParameter

    public static readonly DependencyProperty SelectedCommandParameterProperty = DependencyProperty.RegisterAttached("SelectedCommandParameter", typeof(object), typeof(XTreeViewItem), new FrameworkPropertyMetadata(null));
    public static object GetSelectedCommandParameter(TreeViewItem i) => i.GetValue(SelectedCommandParameterProperty);
    public static void SetSelectedCommandParameter(TreeViewItem i, object value) => i.SetValue(SelectedCommandParameterProperty, value);

    #endregion

    #region IsSelected

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(XTreeViewItem), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));
    public static bool GetIsSelected(TreeViewItem i) => (bool)i.GetValue(IsSelectedProperty);
    public static void SetIsSelected(TreeViewItem i, bool input) => i.SetValue(IsSelectedProperty, input);

    private static void OnIsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is TreeViewItem item)
        {
            if (item.GetParent<TreeView>() is TreeView treeView)
            {
                var items = XTreeView.GetSelectedItems(treeView);
                items.Remove(item.DataContext);

                if (GetIsSelected(item))
                {
                    items.Add(item.DataContext);
                    item.IsSelected = true;

                    GetSelectedCommand(item)?.Execute(GetSelectedCommandParameter(item));
                }
            }
        }
    }

    #endregion

    #endregion

    #region Methods

    public static int GetDepth(this TreeViewItem input)
    {
        var i = -1;
        var j = input as DependencyObject;
        while (j is TreeViewItem)
        {
            i++;
            j = j.GetParent<TreeViewItem>();
        }
        return i;
    }

    #endregion
}