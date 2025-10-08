using Ion.Reflect;
using System;
using System.Reflection;
using System.Windows.Data;

namespace Ion.Data;

/// <inheritdoc/>
public class Is(string path) : BindResult(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<object, object>(false, i =>
    {
        var j = (Tuple<bool, Results, Type>)i.ActualParameter;
        var k = false;

        var type = i.Value.GetType();
        if (j.Item3 is Type b)
            k = type.Equals(b) || type.IsSubclassOf(b) || (b.GetTypeInfo().IsInterface && type.Implements(b));

        return GetResult(k, j.Item1, j.Item2);
    });

    private Type type;
    public Type Type { get => type; set => SetValue(ref type, value); }

    public Is() : this(Paths.Dot) { }

    public Is(string path, Type type) : this(path) => Type = type;

    protected override object GetConverterParameter() => Tuple.Create(Invert, Result, Type);
}