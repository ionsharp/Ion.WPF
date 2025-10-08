using Ion.Collect;
using System.Windows;
using System.Windows.Markup;

namespace Ion.Controls;

[ContentProperty(nameof(Columns))]
public class DataGridControl : DataControl
{
    public static readonly ResourceKey DetailViewTemplate = new();

    public static readonly ResourceKey TextViewTemplate = new();

    public static readonly ResourceKey TemplateKey = new();

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(ListObservable), typeof(DataGridControl), new FrameworkPropertyMetadata(null));
    public ListObservable Columns
    {
        get => this.GetValueOrSetDefault(ColumnsProperty, () => new ListObservable());
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty LineStyleProperty = DependencyProperty.Register(nameof(LineStyle), typeof(Style), typeof(DataGridControl), new FrameworkPropertyMetadata(null));
    public Style LineStyle
    {
        get => (Style)GetValue(LineStyleProperty);
        set => SetValue(LineStyleProperty, value);
    }

    public static readonly DependencyProperty LineTemplateProperty = DependencyProperty.Register(nameof(LineTemplate), typeof(DataTemplate), typeof(DataGridControl), new FrameworkPropertyMetadata(null));
    public DataTemplate LineTemplate
    {
        get => (DataTemplate)GetValue(LineTemplateProperty);
        set => SetValue(LineTemplateProperty, value);
    }

    public static readonly DependencyProperty PanelProperty = DependencyProperty.Register(nameof(Panel), typeof(object), typeof(DataGridControl), new FrameworkPropertyMetadata(null));
    public object Panel
    {
        get => GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public DataGridControl() : base() { }
}