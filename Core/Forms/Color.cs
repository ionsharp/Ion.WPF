using Ion.Data;
using Ion.Reflect;
using System.Windows.Media;

namespace Ion.Core;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class ColorForm : Form
{
    [Styles.Color(Template.ColorText, NameHide = true, ValueConvert = typeof(ConvertColorToString))]
    public Color Color { get => Get(System.Windows.Media.Colors.White); set => Set(value); }

    public ColorForm(Color color)
    {
        Color = color;
    }
}