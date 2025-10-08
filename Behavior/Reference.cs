using Ion.Controls;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Ion.Behavior;

public class ReferenceBehavior : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(nameof(Key), typeof(IFrameworkElementKey), typeof(ReferenceBehavior), new FrameworkPropertyMetadata(null));
    public IFrameworkElementKey Key
    {
        get => (IFrameworkElementKey)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(ReferenceBehavior), new FrameworkPropertyMetadata(null));
    public object Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private void Update()
    {
        if (AssociatedObject is not null)
        {
            if (Key is not null)
            {
                if (Source is IFrameworkElementReference source)
                    source.SetReference(Key, AssociatedObject);
            }
        }
    }

    protected override void OnAttached() => Update();

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        Update();
    }
}