using System.Windows.Controls;

namespace Ion.Controls;

[Extend<DataGridColumn>]
public static class XDataGridColumn
{
    /// <see cref="Region.Method"/>

    public static DataGridColumn Clone(this DataGridColumn input)
    {
        if (input is DataGridTextColumn x)
        {
            return new DataGridTextColumn()
            {
                Binding
                    = x.Binding,
                CanUserReorder
                    = x.CanUserReorder,
                CanUserResize
                    = x.CanUserResize,
                CanUserSort
                    = x.CanUserSort,
                CellStyle
                    = x.CellStyle,
                ClipboardContentBinding
                    = x.ClipboardContentBinding,
                DragIndicatorStyle
                    = x.DragIndicatorStyle,
                EditingElementStyle
                    = x.EditingElementStyle,
                ElementStyle
                    = x.ElementStyle,
                FontFamily
                    = x.FontFamily,
                FontSize
                    = x.FontSize,
                FontStyle
                    = x.FontStyle,
                FontWeight
                    = x.FontWeight,
                Foreground
                    = x.Foreground,
                Header
                    = x.Header,
                HeaderStringFormat
                    = x.HeaderStringFormat,
                HeaderTemplate
                    = x.HeaderTemplate,
                HeaderTemplateSelector
                    = x.HeaderTemplateSelector,
                IsReadOnly
                    = x.IsReadOnly,
                MaxWidth
                    = x.MaxWidth,
                MinWidth
                    = x.MinWidth,
                SortDirection
                    = x.SortDirection,
                SortMemberPath
                    = x.SortMemberPath,
                Visibility
                    = x.Visibility,
                Width
                    = x.Width,
            };
        }
        if (input is DataGridTemplateColumn y)
        {
            return new DataGridTemplateColumn()
            {
                CanUserReorder
                    = y.CanUserReorder,
                CanUserResize
                    = y.CanUserResize,
                CanUserSort
                    = y.CanUserSort,
                CellEditingTemplate
                    = y.CellEditingTemplate,
                CellEditingTemplateSelector
                    = y.CellEditingTemplateSelector,
                CellStyle
                    = y.CellStyle,
                CellTemplate
                    = y.CellTemplate,
                CellTemplateSelector
                    = y.CellTemplateSelector,
                ClipboardContentBinding
                    = y.ClipboardContentBinding,
                DragIndicatorStyle
                    = y.DragIndicatorStyle,
                Header
                    = y.Header,
                HeaderStringFormat
                    = y.HeaderStringFormat,
                HeaderTemplate
                    = y.HeaderTemplate,
                HeaderTemplateSelector
                    = y.HeaderTemplateSelector,
                IsReadOnly
                    = y.IsReadOnly,
                MaxWidth
                    = y.MaxWidth,
                MinWidth
                    = y.MinWidth,
                SortDirection
                    = y.SortDirection,
                SortMemberPath
                    = y.SortMemberPath,
                Visibility
                    = y.Visibility,
                Width
                    = y.Width,
            };
        }
        return null;
    }
}