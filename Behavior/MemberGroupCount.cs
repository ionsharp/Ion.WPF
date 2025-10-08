using Ion.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ion.Behavior;

public class MemberGroupCountBehavior() : MemberGroupBehavior()
{
    /// <see cref="Region.Property"/>

    protected override IMultiValueConverter TargetConverter => new MultiValueConverter<int>(i =>
    {
        var result = 0;
        if (i.Values?.Length > 0)
        {
            foreach (var j in i.Values)
            {
                if (j is bool k && k)
                    result++;
            }
        }
        return result;
    });

    protected override DependencyProperty TargetProperty => CountProperty;

    #region (private) Count

    private static readonly DependencyProperty CountProperty = DependencyProperty.Register(nameof(Count), typeof(int), typeof(MemberGroupCountBehavior), new FrameworkPropertyMetadata(0, OnCountChanged));
    private int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
    private static void OnCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.If<MemberGroupCountBehavior>(i => i.AssociatedObject.If<TextBlock>(j => j.Text = i.Format.F(e.NewValue)));

    #endregion

    #region Format

    public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(MemberGroupCountBehavior), new FrameworkPropertyMetadata("{0}"));
    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    #endregion
}