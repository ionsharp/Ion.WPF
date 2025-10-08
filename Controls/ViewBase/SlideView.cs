using System.Windows;

namespace Ion.Controls;

public class SlideView : DataView
{
    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(nameof(Index), typeof(int), typeof(SlideView), new FrameworkPropertyMetadata(0));
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public SlideView() : base() { }
}