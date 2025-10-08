using System;
using System.Windows.Markup;

namespace Ion.Controls;

/// <summary>See <see cref="Strings"/>.</summary>
public sealed class StringExtension() : MarkupExtension()
{
    public Strings Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => Key switch { Strings.Empty => "" };
}