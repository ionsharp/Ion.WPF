using Ion.Controls;
using Ion.Data;
using Ion.Reflect;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Ion.Behavior;

public class MemberGroupBehavior() : Behavior<FrameworkElement>()
{
    /// <see cref="Region.Property"/>

    protected virtual IMultiValueConverter TargetConverter { get; }

    protected virtual DependencyProperty TargetProperty { get; }

    #region Items

    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items), typeof(object), typeof(MemberGroupBehavior), new FrameworkPropertyMetadata(null, Update));
    public object Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    #endregion

    #region Tab

    public static readonly DependencyProperty TabProperty = DependencyProperty.Register(nameof(Tab), typeof(object), typeof(MemberGroupBehavior), new FrameworkPropertyMetadata(null, Update));
    public object Tab
    {
        get => GetValue(TabProperty);
        set => SetValue(TabProperty, value);
    }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private static void Update(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<MemberGroupBehavior>().Update();

    protected override void OnAttached()
    {
        base.OnAttached();
        Update();
    }

    protected virtual void Update()
    {
        if (AssociatedObject is not null)
        {
            if (Items is ReadOnlyObservableCollection<object> items)
            {
                this.Unbind(TargetProperty);

                var binding = new MultiBind
                {
                    Converter = TargetConverter
                };

                foreach (var i in items)
                {
                    if (i is Member j)
                    {
                        if (Tab is not MemberTab tab || $"{tab.Source}" == $"{j.Tab}")
                            binding.Bindings.Add(new Binding(nameof(Member.IsTrulyVisible)) { Source = j });
                    }
                }

                this.Bind(TargetProperty, binding);
            }
        }
    }

    #endregion
}