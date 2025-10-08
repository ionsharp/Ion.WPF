using Ion.Collect;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class DataViewControl : DataControl
{
    /// <see cref="Region.Field"/>
    #region

    public static readonly ResourceKey BulletTemplate = new();

    public static readonly ResourceKey CheckTemplate = new();

    public static readonly ResourceKey ControlModelTemplate = new();

    public static readonly ResourceKey ControlSelectorTemplate = new();

    public static readonly ResourceKey ItemDescriptionTemplateKey = new();

    public static readonly ResourceKey ItemDetail1TemplateKey = new();

    public static readonly ResourceKey ItemDetail2TemplateKey = new();

    public static readonly ResourceKey ItemImageTemplateKey = new();

    public static readonly ResourceKey ItemNameTemplateKey = new();

    public static readonly ResourceKey MaximumToolTipWidth = new();

    public static readonly ResourceKey MinimumToolTipWidth = new();

    public static readonly ResourceKey TemplateKey = new();

    public static readonly ResourceKey ViewBlockItemStyle = new();

    public static readonly ResourceKey ViewBlockItemTemplate = new();

    public static readonly ResourceKey ViewDefaultItemStyle = new();

    public static readonly ResourceKey ViewDetailItemStyle = new();

    public static readonly ResourceKey ViewPageItemStyle = new();

    public static readonly ResourceKey ViewPageItemTemplate = new();

    public static readonly ResourceKey ViewSlideItemStyle = new();

    public static readonly ResourceKey ViewSlideItemTemplate = new();

    public static readonly ResourceKey ViewThumbItemStyle = new();

    public static readonly ResourceKey ViewThumbItemTemplate = new();

    public static readonly ResourceKey ViewTileItemStyle = new();

    public static readonly ResourceKey ViewTileItemTemplate = new();

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(ListObservable), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public ListObservable Columns
    {
        get => (ListObservable)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ItemContextMenuProperty = DependencyProperty.Register(nameof(ItemContextMenu), typeof(ContextMenu), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public ContextMenu ItemContextMenu
    {
        get => (ContextMenu)GetValue(ItemContextMenuProperty);
        set => SetValue(ItemContextMenuProperty, value);
    }

    public static readonly DependencyProperty ItemDescriptionSortNameProperty = DependencyProperty.Register(nameof(ItemDescriptionSortName), typeof(object), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public object ItemDescriptionSortName
    {
        get => (object)GetValue(ItemDescriptionSortNameProperty);
        set => SetValue(ItemDescriptionSortNameProperty, value);
    }

    public static readonly DependencyProperty ItemDescriptionTemplateProperty = DependencyProperty.Register(nameof(ItemDescriptionTemplate), typeof(DataTemplate), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemDescriptionTemplate
    {
        get => (DataTemplate)GetValue(ItemDescriptionTemplateProperty);
        set => SetValue(ItemDescriptionTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemDescriptionTemplateSelectorProperty = DependencyProperty.Register(nameof(ItemDescriptionTemplateSelector), typeof(KeyTemplateSelector), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public KeyTemplateSelector ItemDescriptionTemplateSelector
    {
        get => (KeyTemplateSelector)GetValue(ItemDescriptionTemplateSelectorProperty);
        set => SetValue(ItemDescriptionTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty ItemDetail1TemplateProperty = DependencyProperty.Register(nameof(ItemDetail1Template), typeof(DataTemplate), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemDetail1Template
    {
        get => (DataTemplate)GetValue(ItemDetail1TemplateProperty);
        set => SetValue(ItemDetail1TemplateProperty, value);
    }

    public static readonly DependencyProperty ItemDetail1TemplateSelectorProperty = DependencyProperty.Register(nameof(ItemDetail1TemplateSelector), typeof(KeyTemplateSelector), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public KeyTemplateSelector ItemDetail1TemplateSelector
    {
        get => (KeyTemplateSelector)GetValue(ItemDetail1TemplateSelectorProperty);
        set => SetValue(ItemDetail1TemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty ItemDetail2TemplateProperty = DependencyProperty.Register(nameof(ItemDetail2Template), typeof(DataTemplate), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemDetail2Template
    {
        get => (DataTemplate)GetValue(ItemDetail2TemplateProperty);
        set => SetValue(ItemDetail2TemplateProperty, value);
    }

    public static readonly DependencyProperty ItemDetail2TemplateSelectorProperty = DependencyProperty.Register(nameof(ItemDetail2TemplateSelector), typeof(KeyTemplateSelector), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public KeyTemplateSelector ItemDetail2TemplateSelector
    {
        get => (KeyTemplateSelector)GetValue(ItemDetail2TemplateSelectorProperty);
        set => SetValue(ItemDetail2TemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty ItemImageTemplateProperty = DependencyProperty.Register(nameof(ItemImageTemplate), typeof(DataTemplate), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemImageTemplate
    {
        get => (DataTemplate)GetValue(ItemImageTemplateProperty);
        set => SetValue(ItemImageTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemImageTemplateSelectorProperty = DependencyProperty.Register(nameof(ItemImageTemplateSelector), typeof(KeyTemplateSelector), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public KeyTemplateSelector ItemImageTemplateSelector
    {
        get => (KeyTemplateSelector)GetValue(ItemImageTemplateSelectorProperty);
        set => SetValue(ItemImageTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty ItemNameTemplateProperty = DependencyProperty.Register(nameof(ItemNameTemplate), typeof(DataTemplate), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemNameTemplate
    {
        get => (DataTemplate)GetValue(ItemNameTemplateProperty);
        set => SetValue(ItemNameTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemNameTemplateSelectorProperty = DependencyProperty.Register(nameof(ItemNameTemplateSelector), typeof(KeyTemplateSelector), typeof(DataViewControl), new FrameworkPropertyMetadata(null));
    public KeyTemplateSelector ItemNameTemplateSelector
    {
        get => (KeyTemplateSelector)GetValue(ItemNameTemplateSelectorProperty);
        set => SetValue(ItemNameTemplateSelectorProperty, value);
    }

    #endregion

    /// <see cref="Region.Constructor"/>

    public DataViewControl() : base() { }
}