using Ion.Data;
using System.Windows;
using System.Windows.Data;

namespace Ion.Behavior;

public class MemberGroupVisibilityBehavior() : MemberGroupBehavior()
{
    /// <see cref="Region.Property"/>

    protected override IMultiValueConverter TargetConverter => new MultiValueConverter<Visibility>(i =>
    {
        if (i.Values?.Length > 0)
        {
            foreach (var j in i.Values)
            {
                if (j is bool k && k)
                    return Visibility.Visible;
            }
        }
        return Visibility.Collapsed;
    });

    protected override DependencyProperty TargetProperty => VisibilityProperty;

    #region (private) Visibility

    private static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(nameof(Visibility), typeof(Visibility), typeof(MemberGroupVisibilityBehavior), new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
    private Visibility Visibility
    {
        get => (Visibility)GetValue(VisibilityProperty);
        set => SetValue(VisibilityProperty, value);
    }
    private static void OnVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.If<MemberGroupVisibilityBehavior>(i => i.AssociatedObject.IfNotNull(j => j.Visibility = (Visibility)e.NewValue));

    #endregion
}