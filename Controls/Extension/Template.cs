using System;
using System.Windows;
using System.Windows.Markup;

namespace Ion.Controls;

/// <summary>See <see cref="Templates"/>.</summary>
public sealed class TemplateExtension() : MarkupExtension()
{
    public Templates Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => Key switch { Templates.Default => new DataTemplate() };
}