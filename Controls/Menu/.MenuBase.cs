using System.Windows;
using System.Windows.Controls.Primitives;

namespace Ion.Controls;

[Extend<MenuBase>]
public static class XMenuBase
{
    /// <see cref="Region.Property"/>

    #region ItemClearStyle

    public static readonly DependencyProperty ItemClearStyleProperty = DependencyProperty.RegisterAttached("ItemClearStyle", typeof(Style), typeof(XMenuBase), new FrameworkPropertyMetadata(null));
    public static Style GetItemClearStyle(MenuBase i) => (Style)i.GetValue(ItemClearStyleProperty);
    public static void SetItemClearStyle(MenuBase i, Style input) => i.SetValue(ItemClearStyleProperty, input);

    #endregion

    #region ItemPlaceholderStyle

    public static readonly DependencyProperty ItemPlaceholderStyleProperty = DependencyProperty.RegisterAttached("ItemPlaceholderStyle", typeof(Style), typeof(XMenuBase), new FrameworkPropertyMetadata(null));
    public static Style GetItemPlaceholderStyle(MenuBase i) => (Style)i.GetValue(ItemPlaceholderStyleProperty);
    public static void SetItemPlaceholderStyle(MenuBase i, Style input) => i.SetValue(ItemPlaceholderStyleProperty, input);

    #endregion

    #region Source

    public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(object), typeof(XMenuBase), new FrameworkPropertyMetadata(null, OnSourceChanged));
    public static object GetSource(MenuBase i) => i.GetValue(SourceProperty);
    public static void SetSource(MenuBase i, object input) => i.SetValue(SourceProperty, input);

    #endregion

    #region SourceModel (private|readonly)

    private static readonly DependencyPropertyKey SourceModelKey = DependencyProperty.RegisterAttachedReadOnly("SourceModel", typeof(MenuModel), typeof(XMenuBase), new FrameworkPropertyMetadata(null));
    private static MenuModel GetSourceModel(MenuBase i) => i.GetValueOrSetDefault<MenuModel>(SourceModelKey, () => new(i));

    #endregion

    /// <see cref="Region.Method"/>

    private static void OnSourceChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
        => i.If<MenuBase>(j => GetSourceModel(j).Load(GetSource(j)));
}