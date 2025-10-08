using Ion;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Core;
using Ion.Imaging;
using Ion.Media;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Storage;
using Ion.Text;
using Ion.Time;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ion.Data;

/// <see cref="MultiValueConverter"/>
#region

/// <inheritdoc/>
[Convert<object, object[]>]
public class MultiValueConverter() : object(), IMultiValueConverter
{
    public static readonly CacheByType<IMultiValueConverter> Cache = new();

    private readonly Func<MultiValueConverterData, object> convert;

    private readonly uint Length;

    public MultiValueConverter(Func<MultiValueConverterData, object> to) : this() => convert = to;

    public MultiValueConverter(uint length, Func<MultiValueConverterData, object> to) : this()
    {
        Length = length; convert = to;
    }

    public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (Length == 0 || values?.Length >= Length)
            return convert.Invoke(new MultiValueConverterData(values, targetType, parameter, culture));

        return Binding.DoNothing;
    }

    public object Convert(object value, object parameter = null) => Convert([value], parameter);

    public object Convert(object[] values, object parameter = null) => Convert(values, null, parameter, null);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

#endregion

/// <see cref="MultiValueConverter{Result}"/>
#region

/// <inheritdoc/>
public class MultiValueConverter<T> : MultiValueConverter
{
    public MultiValueConverter() : base() { }

    public MultiValueConverter(Func<MultiValueConverterData, object> to) : base(to) { }

    public MultiValueConverter(uint length, Func<MultiValueConverterData, object> to) : base(length, to) { }
}

#endregion

/// <see cref="MultiValueConverters"/>
#region

public static class MultiValueConverters
{
    /// <see cref="Name"/>
    #region

    public enum Name
    {
        AandBorC,
        Array,
        Background,
        Bullet,
        Color,
        DoubleToPoint,
        Gradient,
        Field,
        FieldValue,
        FieldValueStatic,
        FileSize,
        HorizontalCenter,
        Image,
        Math,
        MethodFromObject,
        MethodFromType,
        MSizeDouble,
        Opacity,
        Path,
        Property,
        PropertyValue,
        PropertyValueStatic,
        ShapeClip,
        ShapeToGeometry,
        Substring,
        String,
        StringReplace,
        TextFormat,
        TimeLeft,
        ToString,
        Tuple,
        Unit,
        Zoom
    }

    #endregion

    public static Dictionary<Name, MultiValueConverter> Names { get; private set; }
    = new()
    {
        /// <see cref="Name.AandBorC"/>
        { Name.AandBorC,
            new(3, i =>
            {
                if (i.Values[0] is bool a)
                {
                    if (i.Values[1] is bool b)
                    {
                        if (i.Values[2] is bool c)
                            return (a && (b || c)).ToVisibility();
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.Array"/>
        { Name.Array,
            new(i => i.Values) },
        /// <see cref="Name.Background"/>
        { Name.Background,
            new(2, i =>
            {
                if (i.Values[0] is BackgroundType type)
                {
                    if (i.Values[1] is Color color)
                    {
                        if (type == BackgroundType.Color)
                            return new SolidColorBrush(color);

                        if (i.Values?.Length >= 4)
                        {
                            if (i.Values[2] is Gradient gradient)
                            {
                                if (i.Values[3] is GradientType gradientType)
                                {
                                    if (gradientType == GradientType.Linear)
                                    {
                                        gradient.Convert(out LinearGradientBrush brush1);
                                        return brush1;
                                    }
                                    if (gradientType == GradientType.Radial)
                                    {
                                        gradient.Convert(out RadialGradientBrush brush2);
                                        return brush2;
                                    }
                                }
                            }
                        }
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.Bullet"/>
        { Name.Bullet,
            new(1, i =>
            {
                if (i.Values[0] is Bullet bullet)
                {
                    switch (i.Values.Length)
                    {
                        case 1:
                            return bullet.ToString(1);

                        case 2:
                            double.TryParse($"{i.Values[1]}", out double index);
                            return bullet.ToString(index);
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.Color"/>
        { Name.Color,
            new(4, i =>
            {
                if (i.Values[2] is bool normalize)
                {
                    if (i.Values[3] is int precision)
                    {
                        RGB rgb = default;
                        if (i.Values[0] is Color color)
                        {
                            color.Convert(out ByteVector4 v1);
                            rgb = XColorVector.Convert(v1);
                        }

                        if (i.Values[0] is ByteVector4 @byte)
                        {
                            rgb = XColorVector.Convert(@byte);
                        }

                        if (rgb is null)
                            return Binding.DoNothing;

                        Type type = null;
                        if (i.Values[1] is string sModel)
                            type = IColor.GetTypes().FirstOrDefault(j => j.Name == sModel);

                        if (type is null)
                            return Binding.DoNothing;

                        var result = rgb.To(type, ColorProfile.Default);

                        Numeral.Vector finalResult = normalize
                            ? new Numeral.Vector(new Ion.Numeral.Vector(result.ToArray()).Normalize(IColor.Minimum(type), IColor.Maximum(type)).New(i => i.Round(precision)).NewType(xxx => (double)xxx))
                            : result.ToArray().ToArray<double>(j => j.Round(precision));

                        return $"{finalResult}";
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.DoubleToPoint"/>
        { Name.DoubleToPoint,
            new(2, i =>
            {
                if (i.Values[0] is double x)
                {
                    if (i.Values[1] is double y)
                        return new Point(x, y);
                }
                return default(Point);
            }) },
        /// <see cref="Name.Field"/>
        { Name.Field,
            new(1, i =>
            {
                if (i.Values[0] is string a)
                {
                    if (i.Values[1] is Type b)
                        return Try.Get(() => b.GetField(a));

                    return a;
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.FieldValue"/>
        { Name.FieldValue,
            new(1, i =>
            {
                if (i.Values[0] is string a)
                {
                    if (i.Values[1] is Type b)
                    {
                        if (i.Values[2] is object c)
                            return Try.Get(() => b.GetField(a).GetValue(c));

                        return Try.Get(() => b.GetField(a).GetValue(null));
                    }
                    return a;
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.FieldValueStatic"/>
        { Name.FieldValueStatic,
            new(1, i =>
            {
                if (i.Values[0] is object a)
                {
                    if (i.Values.Length > 1 && i.Values[1] is Type b)
                        return Try.Get(() => b.GetField($"{a}").GetValue(null));

                    return a;
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.FileSize"/>
        { Name.FileSize,
            new(2, i =>
            {
                if (i.Values[1] is FileSizeFormat format)
                {
                    if (i.Values[0] is long a)
                        return new FileSize(a).ToString(format);

                    if (i.Values[0] is ulong b)
                        return new FileSize(b.ToInt64()).ToString(format);
                }
                return string.Empty;
            }) },
        /// <see cref="Name.Gradient"/>
        { Name.Gradient,
            new(4, i =>
            {
                if (i.Values[0] is string[] colors)
                {
                    if (i.Values[1] is double value)
                    {
                        if (i.Values[2] is double minimum)
                        {
                            if (i.Values[3] is double maximum)
                            {
                                var totalProgress = new Range<double>(minimum, maximum).ToRange(0, 1, value);

                                double offset = 0;

                                Color? result = null;

                                List<Color> gradient = [];

                                var k = 1.0 / (Convert.ToDouble(colors.Length) - 1);
                                for (int j = 0; j < colors.Length; j++, offset += k)
                                {
                                    var a = XColor.Convert(new ByteVector4(colors[j]));

                                    gradient.Add(a);
                                    if (j < colors.Length - 1 && totalProgress > offset && totalProgress < offset + k)
                                    {
                                        var b = XColor.Convert(new ByteVector4(colors[j + 1]));

                                        var localProgress = (totalProgress - offset) / k;

                                        var va = new ByteVector4(a.R, a.G, a.B, a.A);
                                        var vb = new ByteVector4(b.R, b.G, b.B, b.A);
                                        var vc = va.Blend(vb, BlendModes.Normal, localProgress);

                                        result = Color.FromArgb(vc.A, vc.R, vc.G, vc.B);
                                        gradient.Add(result.Value);
                                        break;
                                    }
                                }

                                if (result != null)
                                {
                                    if ($"{i.Parameter}" == "0")
                                    {
                                        var g = new LinearGradientBrush();
                                        for (var j = 0; j < gradient.Count; j++)
                                            g.GradientStops.Add(new GradientStop(gradient[j], Convert.ToDouble(j) / (Convert.ToDouble(gradient.Count) - 1)));

                                        return g;
                                    }
                                    else return new SolidColorBrush(result.Value);
                                }
                            }
                        }
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.HorizontalCenter"/>
        { Name.HorizontalCenter,
            new(2, i =>
            {
                if (i.Values.FirstOrDefault(j => j == DependencyProperty.UnsetValue) != null)
                    return double.NaN;

                double aWidth = (double)i.Values[0];
                double bWidth = (double)i.Values[1];
                return (aWidth / 2.0) - (bWidth / 2.0);
            }) },
        /// <see cref="Name.Image"/>
        { Name.Image,
            new(1, i =>
            {
                if (i.Values[0] is object image)
                {
                    Uri result
                        = i.Values[1] is AssemblyProject imageAssembly
                        ? Resource.GetImageUri(image, imageAssembly)
                        : Resource.GetImageUri(image);

                    if (result is not null)
                        return XImageSource.Convert(result);
                }
                return No.Thing;
            }) },
        /// <see cref="Name.Math"/>
        { Name.Math,
            new(i =>
            {
                if (i.Values?.Length == 3)
                {
                    if (i.Values[0] is double a && i.Values[2] is double b)
                    {
                        var act = (Operator)i.Values[1];
                        switch (act)
                        {
                            case Operator.Add:
                                return a + b;
                            case Operator.Divide:
                                return a / b;
                            case Operator.Modulo:
                                return a % b;
                            case Operator.Multiply:
                                return a * b;
                            case Operator.Subtract:
                                return a - b;
                        }
                    }
                }
                else if (i.Values?.Length > 0)
                {
                    if (i.Values[0] is double firstValue)
                    {
                        var result = firstValue;
                        Operator? m = null;
                        for (var j = 1; j < i.Values.Length; j++)
                        {
                            if (i.Values[j] is Operator action)
                            {
                                m = action;
                            }
                            else if (m != null && i.Values[j] is double nextValue)
                            {
                                switch (m)
                                {
                                    case Operator.Add:
                                        result += nextValue;
                                        break;
                                    case Operator.Divide:
                                        result /= nextValue;
                                        break;
                                    case Operator.Modulo:
                                        result %= nextValue;
                                        break;
                                    case Operator.Multiply:
                                        result *= nextValue;
                                        break;
                                    case Operator.Subtract:
                                        result -= nextValue;
                                        break;
                                }
                                m = null;
                            }
                        }
                        return result;
                    }
                }
                return default(double);
            }) },
        /// <see cref="Name.MethodFromObject"/>
        { Name.MethodFromObject,
            new(2, i =>
            {
                if (i.Values[0] is object @object)
                {
                    if (i.Values[1] is string methodName)
                        return Try.Get(() => @object.GetType().GetMethod(methodName));
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.MethodFromType"/>
        { Name.MethodFromType,
            new(2, i =>
            {
                if (i.Values[0] is Type type)
                {
                    if (i.Values[1] is string methodName)
                        return Try.Get(() => type.GetMethod(methodName));
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.MSizeDouble"/>
        { Name.MSizeDouble,
            new(i =>
            {
                if (i.Values?.Length == 2)
                {
                    if (i.Values[0] is double a)
                    {
                        if (i.Values[1] is double b)
                            return new MSize<double>(a, b);
                    }
                }
                return No.Thing;
            }) },
        /// <see cref="Name.Opacity"/>
        { Name.Opacity,
            new(i =>
            {
                var result = 1.0;
                if (i.Values?.Length > 0)
                {
                    foreach (var j in i.Values)
                    {
                        if (j is double k)
                            result *= k;
                    }
                }
                return result;
            }) },
        /// <see cref="Name.Path"/>
        { Name.Path,
            new(i =>
            {
                string result = null;
                if (i.Values?.Length > 0)
                {
                    foreach (var j in i.Values)
                    {
                        if (j is null || j.ToString().IsWhite())
                            continue;

                        result = result is null
                            ? $"{j}" : $"{result}.{j}";
                    }
                }
                return result is not null ? new PropertyPath(result) : Binding.DoNothing;
            }) },
        /// <see cref="Name.Property"/>
        { Name.Property,
            new(2, i =>
            {
                var type = i.Values[0] as Type ?? i.Values[0].GetType();
                if (i.Values[1] is string propertyName)
                    return Try.Get(() => type.GetProperty(propertyName));

                return Binding.DoNothing;
            }) },
        /// <see cref="Name.PropertyValue"/>
        { Name.PropertyValue,
            new(2, i =>
            {
                var type = i.Values[0] as Type ?? i.Values[0]?.GetType();
                if (i.Values[1] is string propertyName)
                    return Try.Get(() => type.GetProperty(propertyName).GetValue(i.Values[0]));

                return Binding.DoNothing;
            }) },
        /// <see cref="Name.PropertyValueStatic"/>
        { Name.PropertyValueStatic,
            new(2, i =>
            {
                var type = i.Values[0] as Type ?? i.Values[0].GetType();
                if (i.Values[1] is string propertyName)
                    return Try.Get(() => type.GetProperty(propertyName).GetValue(null));

                return Binding.DoNothing;
            }) },
        /// <see cref="Name.ShapeClip"/>
        { Name.ShapeClip,
            new(i =>
            {
                if (i.Values?.Length == 1)
                {
                    if (i.Values[0] is Shape j)
                        return Try.Get(() => j.Create(1, 1)) ?? Binding.DoNothing;
                }
                else if (i.Values?.Length == 2)
                {
                    if (i.Values[0] is IList j)
                    {
                        if (i.Values[1] is int k)
                        {
                            if (k >= 0 && k < j.Count)
                            {
                                if (j[k] is NamableGroup<Shape> l)
                                    return Try.Get(() => l.Value.Create(1, 1)) ?? Binding.DoNothing;
                            }
                        }
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.ShapeToGeometry"/>
        { Name.ShapeToGeometry,
            new(3, i =>
            {
                if (i.Values[0] is Shape shape)
                {
                    if (i.Values[1] is double height)
                    {
                        if (i.Values[2] is double width)
                            return shape.Create(height, width);
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.String"/>
        { Name.String,
            new(i =>
            {
                if (i.Values?.Length > 0)
                {
                    var result = string.Empty;
                    foreach (var j in i.Values)
                        result = $"{result}{j}";

                    return result;
                }
                return null;
            }) },
        /// <see cref="Name.StringReplace"/>
        { Name.StringReplace,
            new(3, i =>
            {
                if (i.Values[0] is object j)
                {
                    if (i.Values[1] is string a)
                    {
                        if (i.Values[2] is string b)
                            return $"{j}"?.Replace(a, b);
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.Substring"/>
        { Name.Substring,
            new(2, i =>
            {
                if (i.Values[0] is object a)
                {
                    if (i.Values[1] is int b)
                    {
                        var result = $"{a}";
                        return Try.Do(() =>
                        {
                            if (i.Values?.Length == 3)
                            {
                                if (i.Values[2] is int c)
                                {
                                    result = result.Substring(b, c);
                                    return;
                                }
                            }
                            result = result[b..];
                        })
                        ? result : "";
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.TextFormat"/>
        { Name.TextFormat,
            new(2, i =>
            {
                if (i.Values[0] is object a)
                {
                    if (i.Values[1] is Format b)
                        return new FormatText(b, a);
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.TimeLeft"/>
        { Name.TimeLeft,
            new(3, i =>
            {
                if (i.Values[0] is TimeSpan && i.Values[1] is long v && i.Values[2] is long v1)
                {
                    var bytesRead = v;
                    var bytesTotal = v1;

                    var result = (TimeSpan)i.Values[0];
                    return TimeSpan.FromSeconds(result.ToLeft(bytesRead, bytesTotal).TotalSeconds.Round()).ToString();
                }
                return string.Empty;
            }) },
        /// <see cref="Name.ToString"/>
        { Name.ToString,
            new(1, i => i.Values[0].ToString()) },
        /// <see cref="Name.Tuple"/>
        { Name.Tuple,
            new(1, i =>
            {
                if (i.Values?.Length >= 1)
                {
                    if (i.Values[0] is object a)
                    {
                        if (i.Values.Length >= 2)
                        {
                            if (i.Values[1] is object b)
                            {
                                if (i.Values.Length >= 3)
                                {
                                    if (i.Values[2] is object c)
                                    {
                                        if (i.Values.Length >= 4)
                                        {
                                            if (i.Values[3] is object d)
                                            {
                                                if (i.Values.Length >= 5)
                                                {
                                                    if (i.Values[4] is object e)
                                                        return Tuple.Create(a, b, c, d, e);
                                                }
                                                return Tuple.Create(a, b, c, d);
                                            }
                                        }
                                        return Tuple.Create(a, b, c);
                                    }
                                }
                                return Tuple.Create(a, b);
                            }
                        }
                        return Tuple.Create(a);
                    }
                }
                return Binding.DoNothing;
            }) },
        /// <see cref="Name.Unit"/>
        { Name.Unit,
            new(2, i =>
            {
                var j = i.Values[0];
                if ((j is double || j is double? || j is int || j is int?) && i.Values[1] is UnitType)
                {
                    var value = double.Parse($"{j}");

                    var funit = i.Parameter is UnitType ? (UnitType)i.Parameter : UnitType.Pixel;
                    var tunit = i.Values[1].As<UnitType>();

                    var resolution = i.Values.Length > 2 ? (float)i.Values[2] : 72f;
                    var places = i.Values.Length > 3 ? (int)i.Values[3] : 3;

                    var aUnit = new Unit(value, funit, resolution);
                    var bUnit = aUnit.Convert(tunit).Round(places);
                    return $"{bUnit}";
                }
                return string.Empty;
            }) },
        /// <see cref="Name.Zoom"/>
        { Name.Zoom,
            new(2, i => i.Values[0] is double value && i.Values[1] is double zoom && zoom != 0 ? value / zoom : default) }
    };
}

#endregion