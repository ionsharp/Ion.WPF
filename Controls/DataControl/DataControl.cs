using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public abstract class DataControl : Control
{
    /// <see cref="DragTemplate"/>
    #region 

    public static readonly DependencyProperty DragTemplateProperty = DependencyProperty.Register(nameof(DragTemplate), typeof(DataTemplate), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DragTemplate
    {
        get => (DataTemplate)GetValue(DragTemplateProperty);
        set => SetValue(DragTemplateProperty, value);
    }

    #endregion

    /// <see cref="ItemToolTipTemplate"/>
    #region 

    public static readonly DependencyProperty ItemToolTipTemplateProperty = DependencyProperty.Register(nameof(ItemToolTipTemplate), typeof(DataTemplate), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemToolTipTemplate
    {
        get => (DataTemplate)GetValue(ItemToolTipTemplateProperty);
        set => SetValue(ItemToolTipTemplateProperty, value);
    }

    #endregion

    /// <see cref="ItemToolTipHeaderTemplate"/>
    #region 

    public static readonly DependencyProperty ItemToolTipHeaderTemplateProperty = DependencyProperty.Register(nameof(ItemToolTipHeaderTemplate), typeof(DataTemplate), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemToolTipHeaderTemplate
    {
        get => (DataTemplate)GetValue(ItemToolTipHeaderTemplateProperty);
        set => SetValue(ItemToolTipHeaderTemplateProperty, value);
    }

    #endregion

    /// <see cref="ItemToolTipHeaderIconTemplate"/>
    #region 

    public static readonly DependencyProperty ItemToolTipHeaderIconTemplateProperty = DependencyProperty.Register(nameof(ItemToolTipHeaderIconTemplate), typeof(DataTemplate), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ItemToolTipHeaderIconTemplate
    {
        get => (DataTemplate)GetValue(ItemToolTipHeaderIconTemplateProperty);
        set => SetValue(ItemToolTipHeaderIconTemplateProperty, value);
    }

    #endregion

    /// <see cref="Model"/>
    #region 

    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(object), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public object Model
    {
        get => GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    #endregion

    /// <see cref="NoItemTemplate"/>
    #region 

    public static readonly DependencyProperty NoItemTemplateProperty = DependencyProperty.Register(nameof(NoItemTemplate), typeof(DataTemplate), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public DataTemplate NoItemTemplate
    {
        get => (DataTemplate)GetValue(NoItemTemplateProperty);
        set => SetValue(NoItemTemplateProperty, value);
    }

    #endregion

    /// <see cref="NoItemSourceTemplate"/>
    #region

    public static readonly DependencyProperty NoItemSourceTemplateProperty = DependencyProperty.Register(nameof(NoItemSourceTemplate), typeof(DataTemplate), typeof(DataControl), new FrameworkPropertyMetadata(null));
    public DataTemplate NoItemSourceTemplate
    {
        get => (DataTemplate)GetValue(NoItemSourceTemplateProperty);
        set => SetValue(NoItemSourceTemplateProperty, value);
    }

    #endregion

    protected DataControl() : base() { }
}