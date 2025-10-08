using Ion.Collect;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class FontStyleBox : ComboBox
{
    public FontStyleBox() : base() => SetCurrentValue(ItemsSourceProperty, new ListObservable<FontStyle>
    {
        FontStyles.Italic,
        FontStyles.Normal,
        FontStyles.Oblique
    });
}