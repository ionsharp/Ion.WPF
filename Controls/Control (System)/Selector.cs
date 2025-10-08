using Ion.Collect;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[Extend<System.Windows.Controls.Primitives.Selector>]
public static class XSelector
{
    /// <see cref="Region.Property"/>

    #region SelectedItems

    private static readonly DependencyPropertyKey SelectedItemsKey = DependencyProperty.RegisterAttachedReadOnly("SelectedItems", typeof(IListObservable), typeof(XSelector), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsKey.DependencyProperty;
    public static IListObservable GetSelectedItems(System.Windows.Controls.Primitives.Selector i) => i.GetValueOrSetDefault<IListObservable>(SelectedItemsKey, () => new ListObservable<object>());

    #endregion

    /// <see cref="Region.Constructor"/>

    static XSelector()
    {
        EventManager.RegisterClassHandler(typeof(System.Windows.Controls.Primitives.Selector), System.Windows.Controls.Primitives.Selector.SelectionChangedEvent,
            new SelectionChangedEventHandler(OnSelectionChanged), true);
    }

    /// <see cref="Region.Method"/>
    #region

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.Primitives.Selector selector)
        {
            var selection = GetSelectedItems(selector);
            e.AddedItems?.
                ForEach(i => selection.Add(i));
            e.RemovedItems?.
                ForEach(i => selection.Remove(i));
        }
    }

    private static void SelectAll(ItemsControl input)
    {
        foreach (var i in input.Items)
        {
            var j = input.GetContainer(i);
            j.Select(true);
        }
    }

    public static void SelectAll(this System.Windows.Controls.Primitives.Selector input)
    {
        if (input is DataGrid dataGrid)
        {
            if (dataGrid.SelectionMode == DataGridSelectionMode.Extended)
                SelectAll(dataGrid as ItemsControl);
        }
        if (input is ListBox listBox)
        {
            if (listBox.SelectionMode != SelectionMode.Single)
                SelectAll(listBox as ItemsControl);
        }
    }

    public static void SelectNone(this System.Windows.Controls.Primitives.Selector input) => input.SelectedIndex = -1;

    #endregion
}