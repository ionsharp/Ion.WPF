using Ion.Core;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<GridViewColumnHeader>]
public static class XGridViewColumnHeader
{
    /// <see cref="Region.Property"/>

    #region (private) LastDirection

    private static readonly DependencyProperty LastDirectionProperty = DependencyProperty.RegisterAttached("LastDirection", typeof(ListSortDirection), typeof(XGridViewColumnHeader), new FrameworkPropertyMetadata(default(ListSortDirection)));

    private static ListSortDirection GetLastDirection(GridViewColumnHeader i) => (ListSortDirection)i.GetValue(LastDirectionProperty);
    private static void SetLastDirection(GridViewColumnHeader i, ListSortDirection input) => i.SetValue(LastDirectionProperty, input);

    #endregion

    #region (private) Parent

    private static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(ListView), typeof(XGridViewColumnHeader), new FrameworkPropertyMetadata(null));
    private static ListView GetParent(GridViewColumnHeader i) => (ListView)i.GetValue(ParentProperty);
    private static void SetParent(GridViewColumnHeader i, ListView input) => i.SetValue(ParentProperty, input);

    #endregion

    #region SortDirection

    public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.RegisterAttached("SortDirection", typeof(ListSortDirection?), typeof(XGridViewColumnHeader), new FrameworkPropertyMetadata(null));
    public static ListSortDirection? GetSortDirection(GridViewColumnHeader i) => (ListSortDirection?)i.GetValue(SortDirectionProperty);
    private static void SetSortDirection(GridViewColumnHeader i, ListSortDirection? input) => i.SetValue(SortDirectionProperty, input);

    #endregion

    /// <see cref="Region.Constructor"/>

    static XGridViewColumnHeader()
    {
        EventManager.RegisterClassHandler(typeof(GridViewColumnHeader), System.Windows.Controls.Primitives.ButtonBase.ClickEvent,
            new RoutedEventHandler(OnClick), true);
    }

    /// <see cref="Region.Method"/>

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is GridViewColumnHeader header)
        {
            var listView = GetParent(header);
            if (listView is null)
            {
                listView = header.GetParent<ListView>();
                SetParent(header, listView);
            }

            if (listView is not null && header.Column is not null && XGridViewColumn.GetCanSort(header.Column))
            {
                var direction = ListSortDirection.Ascending;
                if (header.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (header != XListView.GetLastClicked(listView))
                        direction = ListSortDirection.Ascending;
                    else direction
                        = GetLastDirection(header) == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                if (listView.DataContext is DataPanel dataPanel)
                {
                    if (dataPanel.CanSort)
                    {
                        dataPanel.SortDirection = direction;
                        dataPanel.SortName = XGridViewColumn.GetSortName(header.Column);
                    }
                }

                SetSortDirection(header, direction);

                // Remove arrow from previously sorted header
                if (XListView.GetLastClicked(listView) != null && XListView.GetLastClicked(listView) != header)
                    SetSortDirection(XListView.GetLastClicked(listView), null);

                XListView.SetLastClicked(listView, header);
                SetLastDirection(header, direction);
            }
        }
    }
}