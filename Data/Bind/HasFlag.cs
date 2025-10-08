using Ion.Data;
using System;
using System.Windows.Data;

namespace Ion.Controls;

/// <inheritdoc/>
public class HasFlag(string path) : BindResult(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<object, object>(0, i =>
    {
        var field = i.Value as Enum;

        var j  = (Tuple<bool, Results, Enum>)i.ActualParameter;
        var k = field.HasFlag(j.Item3);
        return GetResult(k, j.Item1, j.Item2);
    });

    private Enum value;
    public Enum Value { get => value; set => SetValue(ref this.value, value); }

    public HasFlag() : this(Paths.Dot) { }

    protected override object GetConverterParameter() => Tuple.Create(Invert, Result, Value);
}