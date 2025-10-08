using Ion;
using Ion.Collect;
using Ion.Text;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<DataGrid>]
public static class XDataGrid
{
    /// <see cref="Region.Field"/>

    public static readonly ResourceKey GroupStyle = new();

    /// <see cref="Region.Property"/>

    #region AddCommand

    public static readonly DependencyProperty AddCommandProperty = DependencyProperty.RegisterAttached("AddCommand", typeof(ICommand), typeof(XDataGrid), new FrameworkPropertyMetadata(null));
    public static ICommand GetAddCommand(DataGrid i) => (ICommand)i.GetValue(AddCommandProperty);
    public static void SetAddCommand(DataGrid i, ICommand input) => i.SetValue(AddCommandProperty, input);

    #endregion

    #region AutoGenerateColumnHandler (https://stackoverflow.com/questions/4000132/is-there-a-way-to-hide-a-specific-column-in-a-datagrid-when-autogeneratecolumns)

    public static readonly DependencyProperty AutoGenerateColumnHandlerProperty = DependencyProperty.RegisterAttached("AutoGenerateColumnHandler", typeof(bool), typeof(XDataGrid), new UIPropertyMetadata(false, OnAutoGenerateColumnHandlerChanged));
    public static bool GetAutoGenerateColumnHandler(DependencyObject i) => (bool)i.GetValue(AutoGenerateColumnHandlerProperty);
    public static void SetAutoGenerateColumnHandler(DependencyObject i, bool j) => i.SetValue(AutoGenerateColumnHandlerProperty, j);

    private static void OnAutoGenerateColumnHandlerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid grid)
            e.NewValue.If<bool>(i => grid.AutoGeneratingColumn += AutoGenerateColumnHandler_AutoGeneratingColumn, () => grid.AutoGeneratingColumn -= AutoGenerateColumnHandler_AutoGeneratingColumn);
    }

    private static void AutoGenerateColumnHandler_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.PropertyDescriptor is PropertyDescriptor descriptor)
        {
            foreach (Attribute i in descriptor.Attributes)
            {
                if (i is BrowsableAttribute j && !j.Browsable || i is HideAttribute)
                {
                    e.Cancel = true;
                    break;
                }

                var name = (i as DisplayNameAttribute)?.DisplayName ?? (i as NameAttribute)?.Name;
                if (name != null)
                    e.Column.Header = name;
            }
        }
    }

    #endregion

    #region CanSelectColumns

    public static readonly DependencyProperty CanSelectColumnsProperty = DependencyProperty.RegisterAttached("CanSelectColumns", typeof(bool), typeof(XDataGrid), new FrameworkPropertyMetadata(false, OnCanSelectColumnsChanged));
    public static bool GetCanSelectColumns(DataGrid i) => (bool)i.GetValue(CanSelectColumnsProperty);
    public static void SetCanSelectColumns(DataGrid i, bool input) => i.SetValue(CanSelectColumnsProperty, input);

    private static void OnCanSelectColumnsChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
            dataGrid.AddHandlerAttached((bool)e.NewValue, CanSelectColumnsProperty, i => i.PreviewMouseDown += CanSelectColumns_PreviewMouseDown, i => i.PreviewMouseDown -= CanSelectColumns_PreviewMouseDown);
    }

    private static void CanSelectColumns_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            if (e.OriginalSource?.As<DependencyObject>().GetParent<DataGridColumnHeader>() is DataGridColumnHeader header)
            {
                grid.SelectedCells.Clear();
                foreach (var item in grid.Items)
                    grid.SelectedCells.Add(new DataGridCellInfo(item, header.Column));
            }
        }
    }

    #endregion

    #region Columns

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns", typeof(ListObservable), typeof(XDataGrid), new FrameworkPropertyMetadata(null, OnColumnsChanged));
    public static ListObservable GetColumns(DataGrid i) => (ListObservable)i.GetValue(ColumnsProperty);
    public static void SetColumns(DataGrid i, ListObservable input) => i.SetValue(ColumnsProperty, input);

    private static void OnColumnsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            if (GetColumns(grid) is ListObservable columns)
            {
                foreach (DataGridColumn i in columns.Cast<DataGridColumn>())
                {
                    var j = i.Clone();
                    j.IfNotNull(grid.Columns.Add);
                }
            }
        }
    }

    #endregion

    #region ColumnVisibility

    public static readonly DependencyProperty ColumnVisibilityProperty = DependencyProperty.RegisterAttached("ColumnVisibility", typeof(Flag), typeof(XDataGrid), new FrameworkPropertyMetadata(default(Flag), OnColumnVisibilityChanged));
    public static Flag GetColumnVisibility(DataGrid i) => (Flag)i.GetValue(ColumnVisibilityProperty);
    public static void SetColumnVisibility(DataGrid i, Flag input) => i.SetValue(ColumnVisibilityProperty, input);

    private static void OnColumnVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid grid)
        {
            var oldFlag = (Flag)e.OldValue;
            var newFlag = (Flag)e.NewValue;

            //Update columns
            newFlag.Each((i, j) =>
            {
                var name = i.ToString();
                var find = new Predicate<DataGridColumn>(x => x.SortMemberPath == name);

                if (j)
                {
                    //Show
                    var column = grid.Columns.FirstOrDefault<DataGridColumn>(x => find(x));
                    column.IfNotNull(k => k.Visibility = Visibility.Visible);
                }
                else
                {
                    //Hide
                    var column = grid.Columns.FirstOrDefault<DataGridColumn>(x => find(x));
                    column.IfNotNull(k => k.Visibility = Visibility.Collapsed);
                }
            });

            //(Clear | Create) menu
            if (GetColumnVisibilityMenu(grid) is null || !oldFlag.SameAs(newFlag))
            {
                //Clear (and unsubscribe from) old menu
                if (GetColumnVisibilityMenu(grid) is ContextMenu oldMenu)
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
                SetColumnVisibilityMenu(grid, null);

                //Create (and subscribe to) new menu
                var newMenu = new ContextMenu();
                foreach (var i in newFlag.Keys)
                {
                    var item = new MenuItem() { Header = i, IsCheckable = true, StaysOpenOnClick = true };
                    item.Checked
                        += ColumnVisibilityMenu_Checked;
                    item.Unchecked
                        += ColumnVisibilityMenu_Unchecked;

                    item.Tag = grid;
                    newMenu.Items.Add(item);
                }
                SetColumnVisibilityMenu(grid, newMenu);
            }

            //Update menu
            if (GetColumnVisibilityMenu(grid) is ContextMenu menu)
            {
                GetColumnVisibilityHandle(grid).DoInternal(() =>
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
            var grid = item.Tag as DataGrid;
            GetColumnVisibilityHandle(grid).DoInternal(() => SetColumnVisibility(grid, GetColumnVisibility(grid).AddFlag(item.Header)));
        }
    }

    private static void ColumnVisibilityMenu_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item)
        {
            var grid = item.Tag as DataGrid;
            GetColumnVisibilityHandle(grid).DoInternal(() => SetColumnVisibility(grid, GetColumnVisibility(grid).RemoveFlag(item.Header)));
        }
    }

    #endregion

    #region (private) ColumnVisibilityHandle

    private static readonly DependencyProperty ColumnVisibilityHandleProperty = DependencyProperty.RegisterAttached("ColumnVisibilityHandle", typeof(Handle), typeof(XDataGrid), new FrameworkPropertyMetadata(null));
    private static Handle GetColumnVisibilityHandle(DataGrid i) => i.GetValueOrSetDefault(ColumnVisibilityHandleProperty, () => (Handle)false);

    #endregion

    #region (internal) ColumnVisibilityMenu

    internal static readonly DependencyProperty ColumnVisibilityMenuProperty = DependencyProperty.RegisterAttached("ColumnVisibilityMenu", typeof(ContextMenu), typeof(XDataGrid), new FrameworkPropertyMetadata(null));
    internal static ContextMenu GetColumnVisibilityMenu(DataGrid i) => (ContextMenu)i.GetValue(ColumnVisibilityMenuProperty);
    private static void SetColumnVisibilityMenu(DataGrid i, ContextMenu input) => i.SetValue(ColumnVisibilityMenuProperty, input);

    #endregion

    #region DisplayRowNumber

    public static readonly DependencyProperty DisplayRowNumberProperty = DependencyProperty.RegisterAttached("DisplayRowNumber", typeof(bool), typeof(XDataGrid), new FrameworkPropertyMetadata(false, OnDisplayRowNumberChanged));
    public static bool GetDisplayRowNumber(DataGrid i) => (bool)i.GetValue(DisplayRowNumberProperty);
    public static void SetDisplayRowNumber(DataGrid i, bool input) => i.SetValue(DisplayRowNumberProperty, input);

    private static void OnDisplayRowNumberChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            dataGrid.AddHandlerAttached((bool)e.NewValue, DisplayRowNumberProperty, i =>
            {
                i.LayoutUpdated
                    += DisplayRowNumber_OnLayoutUpdated;
                i.LoadingRow
                    += DisplayRowNumber_OnLoadingRow;
                i.SelectionChanged
                    += DisplayRowNumber_OnSelectionChanged;
                i.SizeChanged
                    += DisplayRowNumber_OnSizeChanged;
                i.UnloadingRow
                    += DisplayRowNumber_OnLoadingRow;

                UpdateAllRowNumbers(i);
            }, i =>
            {
                i.LayoutUpdated
                    -= DisplayRowNumber_OnLayoutUpdated;
                i.LoadingRow
                    -= DisplayRowNumber_OnLoadingRow;
                i.SelectionChanged
                    -= DisplayRowNumber_OnSelectionChanged;
                i.SizeChanged
                    -= DisplayRowNumber_OnSizeChanged;
                i.UnloadingRow
                    -= DisplayRowNumber_OnLoadingRow;

                i.GetVisualChildren<DataGridRow>().ForEach(i => i.Header = null);
            });
        }
    }

    ///

    private static void UpdateAllRowNumbers(DataGrid dataGrid) => dataGrid.GetVisualChildren<DataGridRow>().ForEach(i => UpdateRowNumber(dataGrid, i));

    private static void UpdateRowNumber(DataGrid grid, DataGridRow dataGridRow)
    {
        var label = dataGridRow.Header as TextBullet ?? new();
        label.Bullet
            = GetDisplayRowNumberBullet(grid);
        label.Value
            = dataGridRow.GetIndex() + GetDisplayRowNumberOffset(grid);

        dataGridRow.Header = label;
    }

    ///

    private static void DisplayRowNumber_OnSelectionChanged(object sender, EventArgs e)
        => UpdateAllRowNumbers((DataGrid)sender);

    private static void DisplayRowNumber_OnLayoutUpdated(object sender, EventArgs e)
        => UpdateAllRowNumbers((DataGrid)sender);

    private static void DisplayRowNumber_OnSizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateAllRowNumbers((DataGrid)sender);

    private static void DisplayRowNumber_OnLoadingRow(object sender, DataGridRowEventArgs e)
        => UpdateAllRowNumbers((DataGrid)sender);

    #endregion

    #region DisplayRowNumberBullet

    public static readonly DependencyProperty DisplayRowNumberBulletProperty = DependencyProperty.RegisterAttached("DisplayRowNumberBullet", typeof(Bullet), typeof(XDataGrid), new FrameworkPropertyMetadata(Bullet.NumberPeriod, OnDisplayRowNumberBulletChanged));
    public static Bullet GetDisplayRowNumberBullet(DependencyObject i) => (Bullet)i.GetValue(DisplayRowNumberBulletProperty);
    public static void SetDisplayRowNumberBullet(DependencyObject i, Bullet input) => i.SetValue(DisplayRowNumberBulletProperty, input);

    private static void OnDisplayRowNumberBulletChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            if (GetDisplayRowNumber(dataGrid))
                UpdateAllRowNumbers(dataGrid);
        }
    }

    #endregion

    #region DisplayRowNumberOffset

    public static readonly DependencyProperty DisplayRowNumberOffsetProperty = DependencyProperty.RegisterAttached("DisplayRowNumberOffset", typeof(int), typeof(XDataGrid), new FrameworkPropertyMetadata(1, OnDisplayRowNumberOffsetChanged));
    public static int GetDisplayRowNumberOffset(DependencyObject i) => (int)i.GetValue(DisplayRowNumberOffsetProperty);
    public static void SetDisplayRowNumberOffset(DependencyObject i, int input) => i.SetValue(DisplayRowNumberOffsetProperty, input);

    private static void OnDisplayRowNumberOffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            if (GetDisplayRowNumber(dataGrid))
                UpdateAllRowNumbers(dataGrid);
        }
    }

    #endregion

    #region ScrollAddedIntoView

    public static readonly DependencyProperty ScrollAddedIntoViewProperty = DependencyProperty.RegisterAttached("ScrollAddedIntoView", typeof(bool), typeof(XDataGrid), new FrameworkPropertyMetadata(false, OnScrollAddedIntoViewChanged));
    public static bool GetScrollAddedIntoView(DataGrid i) => (bool)i.GetValue(ScrollAddedIntoViewProperty);
    public static void SetScrollAddedIntoView(DataGrid i, bool input) => i.SetValue(ScrollAddedIntoViewProperty, input);

    private static void OnScrollAddedIntoViewChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
            dataGrid.AddHandlerAttached((bool)e.NewValue, ScrollAddedIntoViewProperty, i => i.LoadingRow += ScrollAddedIntoView_LoadingRow, i => i.LoadingRow -= ScrollAddedIntoView_LoadingRow);
    }

    private static void ScrollAddedIntoView_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (sender is DataGrid dataGrid)
            dataGrid.ScrollIntoView(e.Row.Item);
    }

    #endregion
}