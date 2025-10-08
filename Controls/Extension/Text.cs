using Ion.Text;
using XAMLMarkupExtensions.Base;

namespace Ion.Controls;

public class TextExtension(object key) : WPFLocalizeExtension.Extensions.LocExtension(key)
{
    /// <see cref="Region.Field"/>
    #region

    public static readonly string MissingKeyFormat = "{0}[Key: {1}]{2}";

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public Casing Case { get; set; } = Casing.Original;

    public string Format { get; set; }

    public string Prefix { get; set; }

    public string Suffix { get; set; }

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    /// <inheritdoc/>
    public TextExtension() : this(default) { }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
    {
        var result = base.FormatOutput(endPoint, info) as string;
        if (result != null)
        {
            result = $"{Prefix}{Format?.F(result) ?? result}{Suffix}";
            result = result.ToString(Case);
        }
        return result;
    }

    #endregion
}