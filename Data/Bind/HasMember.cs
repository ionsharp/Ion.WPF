using Ion.Data;
using Ion.Reflect;
using System;
using System.Windows.Data;

namespace Ion.Controls;

/// <inheritdoc/>
public abstract class HasMember(string path) : BindResult(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<object, object>(0, i =>
    {
        var type = i.Value as Type ?? i.Value.GetType();

        var j = (Tuple<bool, Results, string, Func<Type, string, bool>>)i.ActualParameter;
        var k = j.Item4.Invoke(type, j.Item3);
        return GetResult(k, j.Item1, j.Item2);
    });

    protected abstract Func<Type, string, bool> Action { get; }

    private string name;
    public string Name { get => name; set => SetValue(ref name, value); }

    protected HasMember() : this(Paths.Dot) { }

    protected override object GetConverterParameter() => Tuple.Create(Invert, Result, Name, Action);
}

/// <inheritdoc/>
public class HasField(string path) : HasMember(path)
{
    protected override Func<Type, string, bool> Action => (i, j) => Instance.HasField(i, j);

    public HasField() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class HasProperty(string path) : HasMember(path)
{
    protected override Func<Type, string, bool> Action => (i, j) => Instance.HasProperty(i, j);

    public HasProperty() : this(Paths.Dot) { }
}