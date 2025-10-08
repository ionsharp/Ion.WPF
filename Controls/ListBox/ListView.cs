using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<ListView>]
public static class XListView
{
    #region (internal) LastClicked

    internal static readonly DependencyProperty LastClickedProperty = DependencyProperty.RegisterAttached("LastClicked", typeof(GridViewColumnHeader), typeof(XListView), new FrameworkPropertyMetadata(null));
    internal static GridViewColumnHeader GetLastClicked(ListView i) => (GridViewColumnHeader)i.GetValue(LastClickedProperty);
    internal static void SetLastClicked(ListView i, GridViewColumnHeader input) => i.SetValue(LastClickedProperty, input);

    #endregion
}