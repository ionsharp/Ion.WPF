using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class RadioButtonGroup : ItemsControl
{
    public static readonly ResourceKey ItemContainerStyleKey = new();
    private readonly Handle handle = false;

    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(RadioButtonGroup), new FrameworkPropertyMetadata(string.Empty));
    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(RadioButtonGroup), new FrameworkPropertyMetadata(-1));
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public RadioButtonGroup() : base()
    {
        this.AddHandler(i =>
        {
            Items.As<INotifyCollectionChanged>().CollectionChanged += RadioGroup_CollectionChanged;
            foreach (var j in Items)
            {
                if (this.GetContainer(j) is RadioButton k)
                {
                    if (k.IsChecked == true)
                    {
                        handle.DoInternal(() => SetCurrentValue(SelectedIndexProperty, Items.IndexOf(j)));
                        break;
                    }
                }
            }
        }, i =>
        {
            Items.As<INotifyCollectionChanged>().CollectionChanged -= RadioGroup_CollectionChanged;
            foreach (var j in Items)
            {
                if (this.GetContainer(j) is RadioButton k)
                    k.Checked -= OnChecked;
            }
        });
    }

    private void RadioGroup_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (this.GetContainer(e.NewItems[0]) is RadioButton i)
                    i.Checked += OnChecked;

                break;

            case NotifyCollectionChangedAction.Move:
                break;

            case NotifyCollectionChangedAction.Remove:
                if (this.GetContainer(e.OldItems[0]) is RadioButton j)
                    j.Checked -= OnChecked;

                break;

            case NotifyCollectionChangedAction.Replace:
                if (this.GetContainer(e.OldItems[0]) is RadioButton a)
                    a.Checked -= OnChecked;

                if (this.GetContainer(e.NewItems[0]) is RadioButton b)
                    b.Checked += OnChecked;

                break;

            case NotifyCollectionChangedAction.Reset:
                break;
                foreach (var m in e.OldItems)
                {
                    if (this.GetContainer(m) is RadioButton n)
                        n.Checked -= OnChecked;
                }
        }
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button)
        {
            var item = this.GetItem(button);
            if (Items.Contains(item))
                handle.DoInternal(() => SetCurrentValue(SelectedIndexProperty, Items.IndexOf(item)));
        }
    }

    protected override DependencyObject GetContainerForItemOverride() => new RadioButton();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is RadioButton;
}