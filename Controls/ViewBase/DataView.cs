using System.Windows;

namespace Ion.Controls;

public class DataView : DefaultView
{
    public static readonly DependencyProperty ItemDescriptionTemplateProperty = DependencyProperty.Register(nameof(ItemDescriptionTemplate), typeof(DataTemplate), typeof(DataView), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemDescriptionTemplate
    {
        get => (DataTemplate)GetValue(ItemDescriptionTemplateProperty);
        set => SetValue(ItemDescriptionTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemDetail1TemplateProperty = DependencyProperty.Register(nameof(ItemDetail1Template), typeof(DataTemplate), typeof(DataView), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemDetail1Template
    {
        get => (DataTemplate)GetValue(ItemDetail1TemplateProperty);
        set => SetValue(ItemDetail1TemplateProperty, value);
    }

    public static readonly DependencyProperty ItemDetail2TemplateProperty = DependencyProperty.Register(nameof(ItemDetail2Template), typeof(DataTemplate), typeof(DataView), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemDetail2Template
    {
        get => (DataTemplate)GetValue(ItemDetail2TemplateProperty);
        set => SetValue(ItemDetail2TemplateProperty, value);
    }

    public static readonly DependencyProperty ItemImageTemplateProperty = DependencyProperty.Register(nameof(ItemImageTemplate), typeof(DataTemplate), typeof(DataView), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemImageTemplate
    {
        get => (DataTemplate)GetValue(ItemImageTemplateProperty);
        set => SetValue(ItemImageTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemNameTemplateProperty = DependencyProperty.Register(nameof(ItemNameTemplate), typeof(DataTemplate), typeof(DataView), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemNameTemplate
    {
        get => (DataTemplate)GetValue(ItemNameTemplateProperty);
        set => SetValue(ItemNameTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(nameof(ItemSize), typeof(double), typeof(DataView), new FrameworkPropertyMetadata(32.0));
    public double ItemSize
    {
        get => (double)GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
    }

    public static readonly DependencyProperty ItemSizeIncrementProperty = DependencyProperty.Register(nameof(ItemSizeIncrement), typeof(double), typeof(DataView), new FrameworkPropertyMetadata(1.0));
    public double ItemSizeIncrement
    {
        get => (double)GetValue(ItemSizeIncrementProperty);
        set => SetValue(ItemSizeIncrementProperty, value);
    }

    public static readonly DependencyProperty ItemSizeMaximumProperty = DependencyProperty.Register(nameof(ItemSizeMaximum), typeof(double), typeof(DataView), new FrameworkPropertyMetadata(512.0));
    public double ItemSizeMaximum
    {
        get => (double)GetValue(ItemSizeMaximumProperty);
        set => SetValue(ItemSizeMaximumProperty, value);
    }

    public static readonly DependencyProperty ItemSizeMinimumProperty = DependencyProperty.Register(nameof(ItemSizeMinimum), typeof(double), typeof(DataView), new FrameworkPropertyMetadata(16.0));
    public double ItemSizeMinimum
    {
        get => (double)GetValue(ItemSizeMinimumProperty);
        set => SetValue(ItemSizeMinimumProperty, value);
    }
}