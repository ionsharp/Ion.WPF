using Ion.Collect;
using Ion.Numeral;
using System.Collections.Generic;
using System.Windows.Media;

namespace Ion.Colors;

[Styles.Object]
public record class GradientForm : Form
{
    private enum Group { Gradient }

    [Group(Group.Gradient)]
    [Styles.List(ItemValues = nameof(DefaultColors),
        Caption = "The colors to include in a gradient.")]
    public ListObservable<ByteVector4> Colors { get => Get<ListObservable<ByteVector4>>([]); set => Set(value); }

    [Hide]
    public static object DefaultColors => new ListObservable<Color>()
    {
        System.Windows.Media.Colors.Black,
        System.Windows.Media.Colors.Blue,
        System.Windows.Media.Colors.Cyan,
        System.Windows.Media.Colors.Green,
        System.Windows.Media.Colors.Magenta,
        System.Windows.Media.Colors.Red,
        System.Windows.Media.Colors.Transparent,
        System.Windows.Media.Colors.White,
        System.Windows.Media.Colors.Yellow
    };

    [Group(Group.Gradient)]
    [Styles.Number(2, 10, 1,
        Caption = "The maximum number of colors in a gradient.")]
    public int Combinations { get => Get(3); set => Set(value); }

    [Hide]
    public ByteVector4? Require { get => Get<ByteVector4?>(); set => Set(value); }

    [Group(Group.Gradient)]
    [Style(Caption = "Add a copy of each gradient with colors in reverse order.")]
    public bool Reverse { get => Get(false); set => Set(value); }

    public GradientForm(IEnumerable<ByteVector4> colors) : base()
    {
        Colors = colors?.Count() > 0 ? new(colors) : new();
    }
}