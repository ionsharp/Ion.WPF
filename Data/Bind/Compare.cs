using Ion.Data;
using System;
using System.Windows.Data;
using O = Ion.Comparison;
using X = System.Convert;

namespace Ion.Controls;

/// <inheritdoc/>
public abstract class Compare(string path, IValueConverter converter) : BindResult(path, converter) { }

/// <inheritdoc/>
public abstract class Compare<T>(string path, IValueConverter converter) : Compare(path, converter) where T : IComparable
{
    private O type = O.Equal;
    public virtual O Type { get => type; set => SetValue(ref type, value); }

    private T value;
    public T Value { get => value; set => SetValue(ref this.value, value); }

    protected static bool Check(T a, O x, T b)
    {
        return x switch
        {
            O.Equal => a.CompareTo(b) == 0,
            O.NotEqual => a.CompareTo(b) != 0,
            O.Greater => a.CompareTo(b) > 0,
            O.GreaterOrEqual => a.CompareTo(b) >= 0,
            O.Lesser => a.CompareTo(b) < 0,
            O.LesserOrEqual => a.CompareTo(b) <= 0,
        };
    }

    protected sealed override object GetConverterParameter() => (Invert, Result, Value, Type);
}

///

/// <inheritdoc/>
public class CompareByte(string path) : Compare<decimal>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<byte, object>(true, i =>
    {
        var j = ((bool, Results, byte, O))i.ActualParameter;
        bool result = Check(X.ToByte(i.Value), j.Item4, X.ToByte(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public CompareByte() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class CompareDecimal(string path) : Compare<decimal>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<decimal, object>(true, i =>
    {
        var j = ((bool, Results, decimal, O))i.ActualParameter;
        bool result = Check(X.ToDecimal(i.Value), j.Item4, X.ToDecimal(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public CompareDecimal() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class CompareDouble(string path) : Compare<double>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<double, object>(true, i =>
    {
        var j = ((bool, Results, double, O))i.ActualParameter;
        bool result = Check(X.ToDouble(i.Value), j.Item4, X.ToDouble(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public CompareDouble() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class Compare16(string path) : Compare<short>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<short, object>(true, i =>
    {
        var j = ((bool, Results, short, O))i.ActualParameter;
        bool result = Check(X.ToInt16(i.Value), j.Item4, X.ToInt16(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public Compare16() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class Compare32(string path) : Compare<int>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<int, object>(true, i =>
    {
        var j = ((bool, Results, int, O))i.ActualParameter;
        bool result = Check(X.ToInt32(i.Value), j.Item4, X.ToInt32(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public Compare32() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class Compare64(string path) : Compare<long>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<long, object>(true, i =>
    {
        var j = ((bool, Results, long, O))i.ActualParameter;
        bool result = Check(X.ToInt64(i.Value), j.Item4, X.ToInt64(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public Compare64() : this(Paths.Dot) { }
}

/// <inheritdoc/>
public class CompareSingle(string path) : Compare<float>(path, DefaultConverter)
{
    public static readonly IValueConverter DefaultConverter = new ValueConverter<float, object>(true, i =>
    {
        var j = ((bool, Results, float, O))i.ActualParameter;
        bool result = Check(X.ToSingle(i.Value), j.Item4, X.ToSingle(j.Item3));
        return GetResult(result, j.Item1, j.Item2);
    });

    public CompareSingle() : this(Paths.Dot) { }
}