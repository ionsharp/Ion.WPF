using System.Windows;
using System.Windows.Input;

namespace Ion.Controls;

public class SystemMenuItem : Freezable
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(SystemMenuItem), new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(SystemMenuItem));
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(SystemMenuItem));
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(int), typeof(SystemMenuItem));
    public int Id
    {
        get => (int)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    protected override Freezable CreateInstanceCore()
        => new SystemMenuItem();

    private static void OnCommandChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        var result = i as SystemMenuItem;
        if (result is not null && e.NewValue is not null)
            result.Command = e.NewValue as ICommand;
    }
}