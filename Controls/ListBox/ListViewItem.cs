using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<ListViewItem>]
public static class XListViewItem
{
    #region ParentHasColumns

    public static readonly DependencyProperty ParentHasColumnsProperty = DependencyProperty.RegisterAttached("ParentHasColumns", typeof(bool), typeof(XListViewItem), new FrameworkPropertyMetadata(false));
    public static bool GetParentHasColumns(ListViewItem i) => (bool)i.GetValue(ParentHasColumnsProperty);
    public static void SetParentHasColumns(ListViewItem i, bool value) => i.SetValue(ParentHasColumnsProperty, value);

    #endregion
}