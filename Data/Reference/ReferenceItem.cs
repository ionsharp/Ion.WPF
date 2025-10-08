using Ion.Reflect;
using System.Windows;

namespace Ion.Data;

public class ReferenceItem() : Reference<MemberBase>()
{
    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(object), typeof(ReferenceItem), new FrameworkPropertyMetadata(null, OnModelChanged));
    public object Item
    {
        get => (object)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly DependencyProperty MemberProperty = DependencyProperty.Register(nameof(Member), typeof(Member), typeof(ReferenceItem), new FrameworkPropertyMetadata(null, OnModelChanged));
    public Member Member
    {
        get => (Member)GetValue(MemberProperty);
        set => SetValue(MemberProperty, value);
    }

    private static void OnModelChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
        => i.If<ReferenceItem>(j =>
        {
            //MemberItem result = null;
            //if (j.Member is not null)
            //result = new(j.Member, j.Item);

            //j.SetCurrentValue(DataProperty, result);
        });
}