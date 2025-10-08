using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<ListBox>]
public static class XListBox
{
    #region LastSelected

    internal static readonly DependencyProperty LastSelectedProperty = DependencyProperty.RegisterAttached("LastSelected", typeof(ListBoxItem), typeof(XListBox), new FrameworkPropertyMetadata(null));
    internal static ListBoxItem GetLastSelected(ListBox i) => (ListBoxItem)i.GetValue(LastSelectedProperty);
    internal static void SetLastSelected(ListBox i, ListBoxItem value) => i.SetValue(LastSelectedProperty, value);

    #endregion

    #region ShowLastSelected

    public static readonly DependencyProperty ShowLastSelectedProperty = DependencyProperty.RegisterAttached("ShowLastSelected", typeof(bool), typeof(XListBox), new FrameworkPropertyMetadata(false, OnShowLastSelectedChanged));
    public static bool GetShowLastSelected(ListBox i) => (bool)i.GetValue(ShowLastSelectedProperty);
    public static void SetShowLastSelected(ListBox i, bool value) => i.SetValue(ShowLastSelectedProperty, value);

    private static void OnShowLastSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ListBox listBox)
            listBox.AddHandlerAttached((bool)e.NewValue, ShowLastSelectedProperty, i => i.SelectionChanged += ShowLastSelected_SelectionChanged, i => i.SelectionChanged -= ShowLastSelected_SelectionChanged);
    }

    private static void ShowLastSelected_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox.SelectedItems.Count > 0)
        {
            foreach (var i in listBox.Items)
            {
                if (listBox.ItemContainerGenerator.ContainerFromItem(i) is ListBoxItem j)
                    XListBoxItem.SetLastSelected(j, false);
            }

            var last = e.AddedItems?.Last();
            if (listBox.ItemContainerGenerator.ContainerFromItem(last) is ListBoxItem k)
                SetLastSelected(listBox, k);
        }
        else
        {
            var lastSelected = GetLastSelected(listBox);
            foreach (var i in listBox.Items)
            {
                if (listBox.ItemContainerGenerator.ContainerFromItem(i) is ListBoxItem j)
                    XListBoxItem.SetLastSelected(j, j.Equals(lastSelected));
            }
        }
    }

    #endregion
}