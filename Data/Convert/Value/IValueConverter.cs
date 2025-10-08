using System.Windows.Data;

namespace Ion.Data;

[Extend<IValueConverter>]
public static class XValueConverter
{
    public static object Convert(this IValueConverter i, object j) => i.Convert(j, null, null, null);
}