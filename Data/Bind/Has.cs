using Ion;
using Ion.Controls;
using Ion.Reflect;
using System;
using System.Windows.Data;

namespace Ion.Data;

/// <inheritdoc/>
public class HasAttribute(string path) : BindResult(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<object, object>(false, i =>
    {
        var parameter
                = (Tuple<bool, Results, Type>)i.ActualParameter;

        var result = false;

        if (i.Value is not null)
        {
            if (i.Value is Enum a)
                result = a.HasAttribute(parameter.Item3);

            if (i.Value is Member b)
                result = b.Data?.Any(i => i.GetType().Inherits(parameter.Item3)) == true;

            if (i.Value is Type c)
                result = c.HasAttribute(parameter.Item3);

            result = i.Value.GetType().HasAttribute(parameter.Item3);
        }

        parameter.Item1.If(() => result = !result);
        return parameter.Item2 == Results.Boolean ? result : result.ToVisibility();
    });

    private Type attribute;
    public Type Attribute { get => attribute; set => SetValue(ref attribute, value); }

    public HasAttribute() : this(Paths.Dot) { }

    protected override object GetConverterParameter() => Tuple.Create(Invert, Result, Attribute);
}

/// <inheritdoc/>
public class HasMembers(string path) : BindResult(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<object, object>(false, i =>
    {
        var parameter
                = (Tuple<bool, Results, Type, View?>)i.ActualParameter;

        var result = false;

        var type = i.Value as Type ?? i.Value.GetType();
        if (parameter.Item3 is not null)
            result = Instance.HasMemberWithAttribute(type, parameter.Item3);

        if (parameter.Item4 is not null)
            result = Instance.HasMemberWithAttribute(type, typeof(StyleAttribute), i => Equals(i.As < StyleAttribute >().View, parameter.Item4.Value));

        parameter.Item1.If(() => result = !result);
        return parameter.Item2 == Results.Boolean ? result : result.ToVisibility();
    });

    private Type withAttribute;
    public Type WithAttribute { get => withAttribute; set => SetValue(ref withAttribute, value); }

    private View? withView;
    public View? WithSection { get => withView; set => SetValue(ref withView, value); }

    public HasMembers() : this(Paths.Dot) { }

    protected override object GetConverterParameter() => Tuple.Create(Invert, Result, WithAttribute, WithSection);
}