using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class ColorSelectorView() : Control()
{
    public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(nameof(Document), typeof(ColorDocument), typeof(ColorSelectorView), new FrameworkPropertyMetadata(null));
    public ColorDocument Document
    {
        get => (ColorDocument)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(ColorFileDockViewModelData), typeof(ColorSelectorView), new FrameworkPropertyMetadata(null));
    public ColorFileDockViewModelData Options
    {
        get => (ColorFileDockViewModelData)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }
}