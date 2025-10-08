using Ion.Data;
using System.Windows;
using System.Windows.Data;

namespace Ion.Controls;

/// <inheritdoc/>
public class Equal : BindResult
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<object, object>(true, i =>
    {
        var j = ((bool, Results, object))i.ActualParameter;
        var k = i.Value == j.Item3 || Equals(i.Value, j.Item3) || ReferenceEquals(i.Value, j.Item3);
        return GetResult(k, j.Item1, j.Item2);
    },
        i =>
        {
            var j = ((bool, Results, object))i.ActualParameter;
            if (j.Item2 == Results.Boolean)
            {
                var k = (bool)i.Value;
                k = j.Item1 ? !k : k;

                if (k) return j.Item3;
            }
            if (j.Item2 == Results.Visibility)
            {
                var k = (Visibility)i.Value;
                k = j.Item1 ? k.Invert() : k;

                if (k == Visibility.Visible)
                    return j.Item3;
            }
            return No.Thing;
        });

    private readonly object value;
    public object Value { get => value; set => SetValue(ref value, value); }

    public Equal() : this(Paths.Dot) { }

    public Equal(string path) : this(path, null) { }

    public Equal(string path, object value) : base(path, DefaultConverter)
    {
        Value = value;
    }

    protected override object GetConverterParameter() => (Invert, Result, Value);
}