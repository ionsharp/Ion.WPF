using Ion.Collect;
using Ion.Colors;
using Ion.Core;
using System.Windows;

namespace Ion.Controls;

public class Triggers() : ListObservable<Trigger>()
{
    internal FrameworkElement Element { get => this.Get<FrameworkElement>(); set => this.Set(value); }

    protected override void OnAdded(ListAddedEventArgs e)
    {
        base.OnAdded(e);
        e.NewItem.If<Trigger>(i =>
        {
            i.Element = Element;
            i.Bind(FrameworkElement.DataContextProperty, nameof(FrameworkElement.DataContext), Element);
        });
    }

    protected override void OnRemoved(ListRemovedEventArgs e)
    {
        base.OnRemoved(e);
        e.OldItem.If<Trigger>(i =>
        {
            i.Element = null;
            i.Unbind(FrameworkElement.DataContextProperty);
        });
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(Element))
            this.ForEach(i => i.Element = e.NewValue as FrameworkElement);
    }
}