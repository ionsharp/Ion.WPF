using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<GridViewColumn>]
public static class XGridViewColumn
{
    /// <see cref="Region.Property"/>

    #region CanSort

    public static readonly DependencyProperty CanSortProperty = DependencyProperty.RegisterAttached("CanSort", typeof(bool), typeof(XGridViewColumn), new FrameworkPropertyMetadata(false));
    public static bool GetCanSort(GridViewColumn i) => (bool)i.GetValue(CanSortProperty);
    public static void SetCanSort(GridViewColumn i, bool input) => i.SetValue(CanSortProperty, input);

    #endregion

    #region SortName

    public static readonly DependencyProperty SortNameProperty = DependencyProperty.RegisterAttached("SortName", typeof(object), typeof(XGridViewColumn), new FrameworkPropertyMetadata(null));
    public static object GetSortName(GridViewColumn i) => i.GetValue(SortNameProperty);
    public static void SetSortName(GridViewColumn i, object input) => i.SetValue(SortNameProperty, input);

    #endregion

    /// <see cref="Region.Method"/>

    public static GridViewColumn Clone(this GridViewColumn oldColumn)
    {
        var newColumn = new GridViewColumn()
        {
            CellTemplate
                = oldColumn.CellTemplate,
            CellTemplateSelector
                = oldColumn.CellTemplateSelector,
            DisplayMemberBinding
                = oldColumn.DisplayMemberBinding,
            Header
                = oldColumn.Header,
            HeaderContainerStyle
                = oldColumn.HeaderContainerStyle,
            HeaderStringFormat
                = oldColumn.HeaderStringFormat,
            HeaderTemplate
                = oldColumn.HeaderTemplate,
            HeaderTemplateSelector
                = oldColumn.HeaderTemplateSelector,
            Width
                = oldColumn.Width,
        };
        SetCanSort(newColumn, GetCanSort(oldColumn));
        SetSortName(newColumn, GetSortName(oldColumn));
        return newColumn;
    }
}