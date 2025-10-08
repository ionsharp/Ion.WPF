using Ion.Controls;
using Ion.Text;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace Ion.Local;

[Extend<string>]
public static class XString
{
    public static string Localize(this string i, string prefix = "", string suffix = "", string format = null, Casing casing = Casing.Original)
    {
        var result = (string)LocExtension.GetLocalizedValue(typeof(string), i, LocalizeDictionary.Instance.SpecificCulture, null);
        result = result.IsEmpty() ? TextExtension.MissingKeyFormat.F(prefix, format?.F(i) ?? i, suffix) : $"{prefix}{format?.F(result) ?? result}{suffix}";
        return result.ToString(casing);
    }
}