using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ion.Controls;

[ContentProperty(nameof(Templates))]
public class TypeTemplateSelector() : DataTemplateSelector()
{
    public enum Matches { Equal, Implement, ImplementOrInherit, Inherit, TopMost }

    /// <see cref="Region.Property"/>

    public virtual DataTemplate Default { get; set; } = new();

    public virtual Matches Match { get; set; } = Matches.Equal;

    public List<DataTemplate> Templates { get; set; } = [];

    /// <see cref="Region.Method"/>

    private DataTemplate GetTemplateFrom(Type[] types)
    {
        if (types?.Length > 0)
        {
            foreach (var i in types)
            {
                if (Templates.FirstOrDefault(j => i == GetTypeFrom(j)) is DataTemplate k)
                    return k;
            }
        }
        return default;
    }

    private static Type GetTypeFrom(DataTemplate i)
        => i is KeyTemplate j ? j.DataKey as Type : i.DataType as Type;

    private DataTemplate Select(object i)
    {
        DataTemplate result = default;
        switch (Match)
        {
            case Matches.TopMost:
                result = GetTemplateFrom(i as Type[] ?? []);
                break;

            default:

                var type = i as Type ?? i?.GetType();
                if (type is null) break;

                result = Templates.FirstOrDefault(x =>
                {
                    var a = type;
                    var b = x is KeyTemplate y ? y.DataKey as Type : x.DataType as Type;

                    return Match switch
                    {
                        Matches.Equal
                            => b is not null
&& a == b,

                        Matches.Implement
                            => b is not null
&& a.Implements(b),

                        Matches.ImplementOrInherit
                            => b is not null
&& (a.Implements(b) || a.Inherits(b)),

                        Matches.Inherit
                            => b is not null
&& a.Inherits(b),

                        _ => false
                    };
                });
                break;
        }
        return result ?? Default;
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container) => Select(item);
}