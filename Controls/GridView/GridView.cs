using Ion;
using Ion.Collect;
using Ion.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<GridView>]
public static class XGridView
{
    #region (private) ColumnIndex

    private static readonly DependencyProperty ColumnIndexProperty = DependencyProperty.RegisterAttached("ColumnIndex", typeof(int), typeof(XGridView), new FrameworkPropertyMetadata(-1));
    private static int GetColumnIndex(GridViewColumn i) => (int)i.GetValue(ColumnIndexProperty);
    private static void SetColumnIndex(GridViewColumn i, int input) => i.SetValue(ColumnIndexProperty, input);

    #endregion

    #region Columns

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns", typeof(ListObservable), typeof(XGridView), new FrameworkPropertyMetadata(null, OnColumnsChanged));
    public static ListObservable GetColumns(GridView i) => (ListObservable)i.GetValue(ColumnsProperty);
    public static void SetColumns(GridView i, ListObservable input) => i.SetValue(ColumnsProperty, input);

    private static void OnColumnsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is GridView view)
        {
            if (e.NewValue is ListObservable columns)
            {
                foreach (GridViewColumn oldColumn in columns)
                    view.Columns.Add(oldColumn.Clone());
            }
        }
    }

    #endregion

    #region ColumnVisibility

    public static readonly DependencyProperty ColumnVisibilityProperty = DependencyProperty.RegisterAttached("ColumnVisibility", typeof(Flag), typeof(XGridView), new FrameworkPropertyMetadata(default(Flag), OnColumnVisibilityChanged));
    public static Flag GetColumnVisibility(GridView i) => (Flag)i.GetValue(ColumnVisibilityProperty);
    public static void SetColumnVisibility(GridView i, Flag input) => i.SetValue(ColumnVisibilityProperty, input);

    private static void OnColumnVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is GridView view)
        {
            var oldFlag = (Flag)e.OldValue;
            var newFlag = (Flag)e.NewValue;

            //Update columns
            newFlag.Each((i, j) =>
            {
                var name = i.ToString();
                var find = new Predicate<GridViewColumn>(x => XGridViewColumn.GetSortName(x)?.ToString() == name);

                if (j)
                {
                    //Show (does this work?)
                    var column = GetHiddenColumns(view).FirstOrDefault<GridViewColumn>(x => find(x));
                    if (column is not null)
                    {
                        GetHiddenColumns(view).Remove(column);
                        view.Columns.Insert(GetColumnIndex(column), column.Clone());
                    }
                }
                else
                {
                    //Hide (does this work?)
                    var column = view.Columns.FirstOrDefault<GridViewColumn>(x => find(x));
                    if (column is not null)
                    {
                        SetColumnIndex(column, view.Columns.IndexOf(column));
                        GetHiddenColumns(view).Add(column);
                        view.Columns.Remove(column);
                    }
                }
            });

            //(Clear | Create) menu
            if (GetColumnVisibilityMenu(view) is null || !oldFlag.SameAs(newFlag))
            {
                //Clear (and unsubscribe from) old menu
                if (GetColumnVisibilityMenu(view) is ContextMenu oldMenu)
                {
                    foreach (MenuItem i in oldMenu.Items)
                    {
                        i.Checked
                            -= ColumnVisibilityMenu_Checked;
                        i.Unchecked
                            -= ColumnVisibilityMenu_Unchecked;
                    }
                    oldMenu.Items.Clear();
                }
                SetColumnVisibilityMenu(view, null);

                //Create (and subscribe to) new menu
                var newMenu = new ContextMenu();
                foreach (var i in newFlag.Keys)
                {
                    var item = new MenuItem() { Header = i, IsCheckable = true, StaysOpenOnClick = true };
                    item.Checked
                        += ColumnVisibilityMenu_Checked;
                    item.Unchecked
                        += ColumnVisibilityMenu_Unchecked;

                    item.Tag = view;
                    newMenu.Items.Add(item);
                }
                SetColumnVisibilityMenu(view, newMenu);
            }

            //Update menu
            if (GetColumnVisibilityMenu(view) is ContextMenu menu)
            {
                GetColumnVisibilityHandle(view).DoInternal(() =>
                {
                    foreach (MenuItem i in menu.Items)
                        i.IsChecked = newFlag.Has(i.Header);
                });
            }
        }
    }

    private static void ColumnVisibilityMenu_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            var view = item.Tag as GridView;
            GetColumnVisibilityHandle(view).DoInternal(() => SetColumnVisibility(view, GetColumnVisibility(view).AddFlag(item.Header)));
        }
    }

    private static void ColumnVisibilityMenu_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            var view = item.Tag as GridView;
            GetColumnVisibilityHandle(view).DoInternal(() => SetColumnVisibility(view, GetColumnVisibility(view).RemoveFlag(item.Header)));
        }
    }

    #endregion

    #region (private) ColumnVisibilityHandle

    private static readonly DependencyProperty ColumnVisibilityHandleProperty = DependencyProperty.RegisterAttached("ColumnVisibilityHandle", typeof(Handle), typeof(XGridView), new FrameworkPropertyMetadata(null));
    private static Handle GetColumnVisibilityHandle(GridView i) => i.GetValueOrSetDefault(ColumnVisibilityHandleProperty, () => (Handle)false);

    #endregion

    #region (internal) ColumnVisibilityMenu

    internal static readonly DependencyProperty ColumnVisibilityMenuProperty = DependencyProperty.RegisterAttached("ColumnVisibilityMenu", typeof(ContextMenu), typeof(XGridView), new FrameworkPropertyMetadata(null));
    internal static ContextMenu GetColumnVisibilityMenu(GridView i) => (ContextMenu)i.GetValue(ColumnVisibilityMenuProperty);
    private static void SetColumnVisibilityMenu(GridView i, ContextMenu input) => i.SetValue(ColumnVisibilityMenuProperty, input);

    #endregion

    #region (private) HiddenColumns

    private static readonly DependencyProperty HiddenColumnsProperty = DependencyProperty.RegisterAttached("HiddenColumns", typeof(List<GridViewColumn>), typeof(XGridView), new FrameworkPropertyMetadata(null));
    private static List<GridViewColumn> GetHiddenColumns(GridView i) => i.GetValueOrSetDefault<List<GridViewColumn>>(HiddenColumnsProperty, () => []);

    #endregion
}