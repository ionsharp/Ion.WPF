using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class MemberControl() : ItemsControl(), IMemberControl
{
    /// <see cref="Orientation"/>
    #region

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(MemberControl), new FrameworkPropertyMetadata(Orientation.Vertical));
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    protected override DependencyObject GetContainerForItemOverride() => new MemberControlItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is MemberControlItem;

    #endregion
}