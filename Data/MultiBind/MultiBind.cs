using Ion.Controls;
using System;
using System.Windows;
using System.Windows.Data;

namespace Ion.Data;

/// <see cref="MultiBind"/>
#region

public class MultiBind : MultiBinding
{
    public MultiValueConverters.Name ConverterName { set => Converter = MultiValueConverters.Names[value]; }

    public Type ConverterType { set => Converter = MultiValueConverter.Cache[value]; }

    public int Way
    {
        set => Mode =
            value == 0 ?
            BindingMode.Default :
            value == 1 ?
            BindingMode.OneWay :
            value == 2 ?
            BindingMode.TwoWay :
            BindingMode.OneWayToSource;
    }

    public MultiBind() : base() => Mode = BindingMode.OneWay;

    public MultiBind(string path) : this() => Bindings.Add(new Binding(path));
}

#endregion

/// <see cref="MultiBindEqual"/>
#region

/// <inheritdoc/>
public class MultiBindEqual : MultiBindResult
{
    public static readonly IMultiValueConverter DefaultConverter = new MultiValueConverter<object>(i =>
    {
        if (i.Values?.Length == 2)
        {
            var p
            = (Tuple<bool, Results>)i.Parameter;
            var q
                = i.Values[0] == i.Values[1]
                || Equals(i.Values[0], i.Values[1])
                || ReferenceEquals(i.Values[0], i.Values[1]);

            p.Item1.If(() => q = !q);
            return p is null || p.Item2 == Results.Boolean
                ? q : q.ToVisibility();
        }
        return No.Thing;
    });

    public MultiBindEqual() : base() => Converter = DefaultConverter;
}

#endregion

/// <see cref="MultiBindTrue"/>
#region

/// <inheritdoc/>
public class MultiBindTrue : MultiBindResult
{
    public enum Types { All, Any, None }

    public static readonly IMultiValueConverter DefaultConverter = new MultiValueConverter<object>(i =>
    {
        if (i.Values?.Length > 0)
        {
            var parameter = (Tuple<bool, Results, Types>)i.Parameter;

            bool result = default;

            if (false) { }
            else if (parameter.Item3 == Types.All)
            {
                result = true;
                foreach (object value in i.Values)
                {
                    if ((value is bool a && !a) || (value is Visibility b && b == Visibility.Collapsed))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else if (parameter.Item3 == Types.Any)
            {
                result = false;
                foreach (object value in i.Values)
                {
                    if ((value is bool a && a) || (value is Visibility b && b == Visibility.Visible))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else if (parameter.Item3 == Types.None)
            {
                foreach (object value in i.Values)
                {
                    if ((value is bool a && !a) || (value is Visibility b && b == Visibility.Collapsed))
                    {
                        result = false;
                        break;
                    }
                }
            }

            parameter.Item1.If(() => result = !result);
            return parameter.Item2 == Results.Boolean
                ? result : result.ToVisibility();
        }
        return No.Thing;
    });

    public override bool Invert
    {
        get => base.Invert;
        set
        {
            base.Invert = value;
            ConverterParameter = Tuple.Create(value, Result, For);
        }
    }

    public override Results Result
    {
        get => base.Result;
        set
        {
            base.Result = value;
            ConverterParameter = Tuple.Create(Invert, value, For);
        }
    }

    private Types @for = Types.All;
    public Types For
    {
        get => @for;
        set
        {
            @for = value;
            ConverterParameter = Tuple.Create(Invert, Result, value);
        }
    }

    public MultiBindTrue() : base()
    {
        Converter = DefaultConverter;
        ConverterParameter = Tuple.Create(Invert, Result, For);
    }
}

#endregion