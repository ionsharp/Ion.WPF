using Ion.Data;
using System;
using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

[Name(nameof(ConsoleOptions))]
public record class ConsoleOptions : ControlOptions<ConsoleBox>
{
    public SolidColorBrush Background { get => Get(Brushes.Black, ValueConverter.Cache.Get<ConvertSolidColorBrushToString>()); set => Set(value, ValueConverter.Cache.Get<ConvertSolidColorBrushToString>()); }

    [Ion.Styles.Path(Template.PathFile)]
    public string BackgroundImage { get => Get(""); set => Set(value); }

    public System.Windows.Media.Stretch BackgroundStretch { get => Get(System.Windows.Media.Stretch.Fill); set => Set(value); }

    public FontFamily FontFamily { get => Get(new FontFamily("Consolas"), ValueConverter.Cache.Get<ConvertFontFamilyToString>()); set => Set(value, ValueConverter.Cache.Get<ConvertFontFamilyToString>()); }

    [Ion.Styles.Number(12.0, 48.0, 1.0)]
    public double FontSize { get => Get(16.0); set => Set(value); }

    public SolidColorBrush Foreground { get => Get(Brushes.White, ValueConverter.Cache.Get<ConvertSolidColorBrushToString>()); set => Set(value, ValueConverter.Cache.Get<ConvertSolidColorBrushToString>()); }

    [Hide]
    public string Output { get => Get(""); set => Set(value); }

    public TextWrapping TextWrap { get => Get(TextWrapping.NoWrap); set => Set(value); }

    public ConsoleOptions() : base() { }
}