using Ion.Data;
using System.Windows.Data;

namespace Ion.Controls;

[Convert<double, object[]>]
public class BarGraphConverter() : MultiValueConverter<object>(3, i =>
{
    if (i.Values[0] is double x)
    {
        if (i.Values[1] is double y)
        {
            if (i.Values[2] is double z)
                return y > 0 ? x / y * z : Binding.DoNothing;
        }
    }
    return No.Thing;
})
{ public static BarGraphConverter Default { get; private set; } = new(); }