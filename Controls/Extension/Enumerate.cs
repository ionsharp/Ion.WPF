using Ion.Collect;
using Ion.Reflect;
using System;
using System.Windows;
using System.Windows.Markup;

namespace Ion.Controls;

public sealed class EnumerateExtension(Type type) : MarkupExtension()
{
    public Browse Appear { get; set; } = Browse.Visible;

    public bool Sort { get; set; }

    public bool String { get; set; }

    public Type Type { get; set; } = type;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Type is not null)
        {
            if (String)
                return new ListObservableOfString(Type.GetEnumValues(Appear, i => i.ToString(), Sort));

            return new ListObservable<Enum>(Type.GetEnumValues(Appear, Sort));
        }
        return DependencyProperty.UnsetValue;
    }
}