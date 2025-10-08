using Ion.Data;
using Ion.Numeral;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<ComboBoxItem>]
public static class XComboBoxItem
{
    public static readonly ResourceKey SeparatorStyleKey = new();

    /// <see cref="Region.Property"/>
    #region

    #region Icon

    public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(object), typeof(XComboBoxItem), new FrameworkPropertyMetadata(null));
    public static object GetIcon(ComboBoxItem i) => i.GetValue(IconProperty);
    public static void SetIcon(ComboBoxItem i, object input) => i.SetValue(IconProperty, input);

    #endregion

    #region IconSize

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.RegisterAttached("IconSize", typeof(MSize<double>), typeof(XComboBoxItem), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(DoubleSizeTypeConverter))]
    public static MSize<double> GetIconSize(ComboBoxItem i) => (MSize<double>)i.GetValue(IconSizeProperty);
    public static void SetIconSize(ComboBoxItem i, MSize<double> input) => i.SetValue(IconSizeProperty, input);

    #endregion

    #region IconTemplate

    public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.RegisterAttached("IconTemplate", typeof(DataTemplate), typeof(XComboBoxItem), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetIconTemplate(FrameworkElement i) => (DataTemplate)i.GetValue(IconTemplateProperty);
    public static void SetIconTemplate(FrameworkElement i, DataTemplate input) => i.SetValue(IconTemplateProperty, input);

    #endregion

    #region IsSelected

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(XComboBoxItem), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));
    public static bool GetIsSelected(ComboBoxItem i) => (bool)i.GetValue(IsSelectedProperty);
    public static void SetIsSelected(ComboBoxItem i, bool input) => i.SetValue(IsSelectedProperty, input);

    private static void OnIsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ComboBoxItem item)
            item.GetParent().OnSelected(item);
    }

    #endregion

    #region (private) Parent

    private static readonly DependencyProperty ParentProperty = DependencyProperty.RegisterAttached("Parent", typeof(ComboBox), typeof(XComboBoxItem), new FrameworkPropertyMetadata(null));

    private static ComboBox GetParent(this ComboBoxItem i) => i.GetValueOrSetDefault(ParentProperty, () => i.GetParent<ComboBox>());

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    internal static bool isSelected(this ComboBoxItem input) => GetIsSelected(input);

    public static void Select(this ComboBoxItem i, bool input) => SetIsSelected(i, input);

    public static void SelectInverse(this ComboBoxItem i) => SetIsSelected(i, !GetIsSelected(i));

    #endregion
}