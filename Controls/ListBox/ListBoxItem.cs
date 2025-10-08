using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<ListBoxItem>]
public static class XListBoxItem
{
    public static readonly ResourceKey TemplateKey = new();

    #region LastSelected

    public static readonly DependencyProperty LastSelectedProperty = DependencyProperty.RegisterAttached("LastSelected", typeof(bool), typeof(XListBoxItem), new FrameworkPropertyMetadata(false));
    public static bool GetLastSelected(ListBoxItem i) => (bool)i.GetValue(LastSelectedProperty);
    public static void SetLastSelected(ListBoxItem i, bool value) => i.SetValue(LastSelectedProperty, value);

    #endregion
}