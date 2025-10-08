using Ion.Analysis;
using System.Windows;
using System.Windows.Input;

namespace Ion.Controls;

public class ResultControl() : ContentControl<Result>()
{
    public static readonly ResourceKey IconTemplate = new();

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(ResultControl), new FrameworkPropertyMetadata(null));
    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
}