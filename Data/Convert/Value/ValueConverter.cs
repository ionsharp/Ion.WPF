using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Core;
using Ion.Imaging;
using Ion.Local;
using Ion.Media;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Storage;
using Ion.Text;
using Ion.Time;
using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using X = System.Convert;

namespace Ion.Data;

/// <see cref="ValueConverter"/>
#region

/// <summary>Converts something into something else.</summary>
[Convert<object, object>]
public abstract class ValueConverter : IConvert, IValueConverter
{
    /// <see cref="Region.Field"/>
    #region

    public static readonly CacheByType<IValueConverter> Cache = new();

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public abstract Type SourceType { get; }

    public abstract Type TargetType { get; }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

    public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

    #endregion
}

#endregion

/// <see cref="ValueConverter{A, B}"/>
#region

/// <inheritdoc/>
public class ValueConverter<A, B> : ValueConverter, IConvert<A, B>
{
    /// <see cref="Region.Field"/>
    #region

    private readonly Func<ValueConverterInput<A>, ValueConverterOutput<B>> convert;

    private readonly Func<ValueConverterInput<B>, ValueConverterOutput<A>> convertBack;

    #endregion

    /// <see cref="Region.Property"/>
    #region

    protected virtual bool AllowNull { get; set; }

    public override Type SourceType => typeof(A);

    public override Type TargetType => typeof(B);

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    protected ValueConverter() : base() { }

    public ValueConverter(Bit allowNull, Func<ValueConverterInput<A>, ValueConverterOutput<B>> to, Func<ValueConverterInput<B>, ValueConverterOutput<A>> back = null) : base()
    {
        AllowNull = allowNull;
        convert = to; convertBack = back;
    }

    public ValueConverter(Bit allowNull, Func<ValueConverterInput<A>, ValueConverterOutput<B>> to, bool same) : this(allowNull, to)
    {
        if (same) convertBack = to as Func<ValueConverterInput<B>, ValueConverterOutput<A>>;
    }

    public ValueConverter(Func<ValueConverterInput<A>, ValueConverterOutput<B>> to, Func<ValueConverterInput<B>, ValueConverterOutput<A>> back = null) : this(0, to, back) { }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    protected virtual ValueConverterOutput<B> Convert(ValueConverterInput<A> i) => convert?.Invoke(i);

    protected virtual ValueConverterOutput<A> ConvertBack(ValueConverterInput<B> i) => convertBack?.Invoke(i);

    protected virtual bool IsSource(object input) => input is A;

    protected virtual bool IsTarget(object input) => input is B;

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (AllowNull || IsSource(value))
        {
            var result = Convert(new ValueConverterInput<A>(value, parameter) { TargetType = targetType, Culture = culture });
            if (result.ActualValue is not Nothing)
                return result.ActualValue;
        }
        return Binding.DoNothing;
    }

    public B Convert(object value, object parameter = null) => (B)Convert(value, null, parameter, null);

    B IConvert<B>.ConvertTo() => throw new NotSupportedException();

    B IConvert<A, B>.Convert(A a) => Convert(a);

    ///

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (AllowNull || IsTarget(value))
        {
            var result = ConvertBack(new ValueConverterInput<B>(value, parameter));
            if (result.ActualValue is not Nothing)
                return result.ActualValue;
        }
        return Binding.DoNothing;
    }

    public A ConvertBack(object value, object parameter = null) => (A)ConvertBack(value, null, parameter, null);

    object IConvert<B>.ConvertBack(B b) => throw new NotSupportedException();

    A IConvert<A, B>.ConvertBack(B b) => ConvertBack(b);

    #endregion
}

#endregion

/// <see cref="Assembly"/>
#region

[Convert<Assembly, string>]
public class ConvertAssemblyName()
    : ValueConverter<Assembly, string>
    (i => new AssemblyInfo(i.Value).Name)
{ }

#endregion

/// <see cref="Attribute"/>
#region

[Convert<object, object>]
public abstract class AttributeConverter<T>(Type[] types, Func<Attribute, T> getValue = null) : ValueConverter<object, T>()
{
    /// <see cref="Region.Field"/>
    #region

    private readonly Func<Attribute, T> getValue = getValue;

    private readonly Func<Attribute, object, T> getValueWithParameter;

    #endregion

    /// <see cref="Region.Property"/>
    #region

    private readonly Type[] AttributeTypes = types;

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    protected AttributeConverter(Type[] types, Func<Attribute, object, T> getValueWithParameter) : this(types, default(Func<Attribute, T>))
        => this.getValueWithParameter = getValueWithParameter;

    #endregion

    /// <see cref="Region.Method"/>
    #region

    protected override ValueConverterOutput<T> Convert(ValueConverterInput<object> input) => GetValue(input.Value, input.ActualParameter);

    protected virtual T GetFallback() => default;

    protected virtual T GetValue<U>(object i, object parameter, Func<Type, Attribute> action)
    {
        foreach (var j in AttributeTypes)
        {
            if (action(j) is Attribute k)
            {
                if (getValue is not null) return getValue(k);
                if (getValueWithParameter is not null) return getValueWithParameter(k, parameter);
            }
        }
        return default;
    }

    protected virtual T GetValue(Enum i, object parameter)
        => GetValue<Enum>(i, parameter, i.GetAttribute);

    protected virtual T GetValue(MemberInfo i, object parameter)
        => GetValue<MemberInfo>(i, parameter, i.GetAttribute);

    protected virtual T GetValue(object i, object parameter)
    {
        Type targetType = default;

        if (i is Enum a)
            return GetValue(a, parameter);

        if (i is MemberInfo b)
        {
            if (b is Type) goto Type;
            return GetValue(b, parameter);
        }

        if (i is MemberBase c)
            return GetValue(c, parameter);

        Type:
        {
            if (i is Type d)
                targetType = d;

            else
            {
                var target = i;
                if (i is object[] j && j.Length > 0)
                    target = j[0];

                if (target is IGeneric k)
                    targetType = k.GetGenericType();

                targetType ??= target?.GetType();
            }

            if (targetType is not null)
                return GetValue(targetType, parameter);
        }

        return GetFallback();
    }

    protected virtual T GetValue(MemberBase i, object parameter)
        => GetValue<MemberBase>(i, parameter, j => i.Data.FirstOrDefault(k => k.GetType().Inherits(j)));

    protected virtual T GetValue(Type i, object parameter)
        => GetValue<Type>(i, parameter, i.GetAttribute);

    #endregion
}

///

[Convert<object, string>]
public class ConvertAttributeDescription() : AttributeConverter<string>([typeof(DescriptionAttribute), typeof(System.ComponentModel.DescriptionAttribute)], i =>
{
    if (i is DescriptionAttribute j)
    {
        if (j.Localize)
            return j.Description.Localize();

        return j.Description;
    }
    return i.As<System.ComponentModel.DescriptionAttribute>()?.Description;
})
{
    protected override string GetFallback() => "NoDescription".Localize();

    protected override string GetValue(MemberBase i, object parameter) => i.Style.GetValue(j => j.Description)?.ToString();
}

[Convert<object, Format>]
public class ConvertAttributeDescriptionFormat() : AttributeConverter<Format>([typeof(DescriptionAttribute), typeof(System.ComponentModel.DescriptionAttribute)], i => (i as DescriptionAttribute)?.Format ?? Format.MarkUp)
{
    protected override Format GetValue(MemberBase i, object parameter) => i.Style.GetValue(j => j.DescriptionFormat);
}

[Convert<object, string>]
public class ConvertAttributeGroup()
    : AttributeConverter<string>
    ([typeof(CategoryAttribute), typeof(GroupAttribute)], i => i.As<CategoryAttribute>()?.Category ?? i.As<GroupAttribute>()?.Name?.ToString())
{ }

[Convert<object, object>]
public class ConvertAttributeImage()
    : AttributeConverter<object>
    ([typeof(ImageAttribute)], (i, parameter) => i.As<ImageAttribute>().IfNotNullGet(j => Resource.GetImage(j.Name, j.NameAssembly, parameter is ImageSize k ? k : ImageSize.Smallest)))
{ }

[Convert<object, object>]
public class ConvertAttributeImageSource()
    : AttributeConverter<object>
    ([typeof(ImageAttribute)], (i, parameter) => i.As<ImageAttribute>().IfNotNullGet(j => XImageSource.Convert(Resource.GetImageUri(j.Name, j.NameAssembly, parameter is ImageSize k ? k : ImageSize.Smallest))))
{ }

[Convert<object, string>]
public class ConvertAttributeName() : AttributeConverter<string>([typeof(DisplayNameAttribute), typeof(NameAttribute)], (i, parameter) =>
{
    if (i is NameAttribute j)
    {
        if (j.Localize)
        {
            if (parameter is null || (parameter is bool forceLocalize && forceLocalize))
                return j.Name.Localize();
        }

        return j.Name;
    }
    return i.As<DisplayNameAttribute>()?.DisplayName;
})
{
    protected override string GetValue(Enum i, object parameter)
        => base.GetValue(i, parameter) ?? $"{i}".GetCamel();

    protected override string GetValue(MemberInfo i, object parameter)
        => base.GetValue(i, parameter) ?? i.Name.GetCamel();

    ///<remarks>Ignore <see cref="MemberBase"/> when considering <see cref="IName"/>.</remarks>
    protected override string GetValue(object i, object parameter)
        => i is not MemberBase && i is IName j ? j.Name : i is string k ? k : base.GetValue(i, parameter);

    protected override string GetValue(MemberBase i, object parameter)
        => i.Style.GetValue(j => j.Name);

    protected override string GetValue(Type i, object parameter)
        => base.GetValue(i, parameter) ?? i.Name.GetCamel();
}

[Convert<object, string>]
public class ConvertAttributeSymbol()
    : AttributeConverter<string>
    ([typeof(SymbolAttribute)], i => $"{i.To<SymbolAttribute>().Symbol}")
{ }

#endregion

/// <see cref="bool"/>
#region

[Convert<Bit, bool>]
public class ConvertBitToBoolean()
    : ValueConverter<Bit, bool>
    (i => (bool)i.Value, i => (Bit)i.Value)
{ }

[Convert<Enum, bool>]
public class ConvertHasFlag()
    : ValueConverter<Enum, bool>
    (i => i.Value.HasFlag((Enum)i.ActualParameter))
{ }

[Convert<Type, bool>]
public class ConvertIsHidden()
    : ValueConverter<Type, bool>
    (i => i.Value is not null && Instance.IsHidden(i.Value))
{ }

[Convert<object, bool>]
public class ConvertIsMaskedImage()
    : ValueConverter<object, bool>
    (i =>
    {
        var attribute = default(ImageAttribute);
        if (i.Value is Enum a)
            attribute = a.GetAttribute<ImageAttribute>();

        if (i.Value is Member b)
            attribute = b.GetAttribute<ImageAttribute>();

        if (i.Value is Type c)
            attribute = c.GetAttribute<ImageAttribute>();

        return attribute?.Mask == true;
    })
{ }

[Convert<bool, BindingMode>]
public class ConvertBooleanToBindingMode()
    : ValueConverter<bool, BindingMode>
    (i => i.Value ? BindingMode.TwoWay : BindingMode.OneWay)
{ }

[Convert<bool, Bit>]
public class ConvertBooleanToBit()
    : ValueConverter<bool, Bit>
    (i => (Bit)i.Value, i => (bool)i.Value)
{ }

[Convert<bool, TextWrapping>]
public class ConvertBooleanToTextWrapping()
    : ValueConverter<bool, TextWrapping>
    (i => i.Value ? TextWrapping.Wrap : TextWrapping.NoWrap, i => i.Value == TextWrapping.Wrap)
{ }

#endregion

/// <see cref="Color"/>
#region

[Convert<ByteVector3, Color>]
public class ConvertByteVector3ToColor()
    : ValueConverter<ByteVector3, Color>
    (i => XColor.Convert(new ByteVector4(i.Value.R, i.Value.G, i.Value.B, byte.MaxValue)),
    i =>
    {
        i.Value.Convert(out ByteVector4 j);
        return new ByteVector3(j.R, j.G, j.B);
    })
{ }

[Convert<ByteVector4, Color>]
public class ConvertByteVector4ToColor()
    : ValueConverter<ByteVector4, Color>
    (i => XColor.Convert(i.Value),
    i =>
    {
        i.Value.Convert(out ByteVector4 j);
        return j;
    })
{ }

[Convert<Color, Color>]
public class ConvertColorWithoutAlpha()
    : ValueConverter<Color, Color>
    (i => Color.FromArgb((byte)255, i.Value.R, i.Value.G, i.Value.B))
{ }

[Convert<System.Drawing.Color, Color>]
public class ConvertColorToColor()
    : ValueConverter<System.Drawing.Color, Color>
    (i => { i.Value.Convert(out Color j); return j; }, i => { i.Value.Convert(out System.Drawing.Color j); return j; })
{ }

[Convert<SolidColorBrush, Color>]
public class ConvertSolidColorBrushToColor()
    : ValueConverter<SolidColorBrush, Color>
    (i => i.Value.Color, i => new SolidColorBrush(i.Parameter == 0 ? i.Value : Color.FromArgb(255, i.Value.R, i.Value.G, i.Value.B)))
{ }

///

[Convert<Color, double>]
public class ConvertBrightness()
    : ValueConverter<Color, double>
    (i =>
    {
        i.Value.Convert(out System.Drawing.Color j);
        return j.GetBrightness();
    })
{ }

[Convert<Color, double>]
public class ConvertHue()
    : ValueConverter<Color, double>
    (i =>
    {
        i.Value.Convert(out System.Drawing.Color j);
        return j.GetHue();
    })
{ }

[Convert<Color, double>]
public class ConvertSaturation()
    : ValueConverter<Color, double>
    (i =>
    {
        i.Value.Convert(out System.Drawing.Color j);
        return j.GetSaturation();
    })
{ }

#endregion

/// <see cref="double"/>
#region

[Convert<long, double>]
public class ConvertBytesToMegaBytes()
    : ValueConverter<long, double>
    (0, input =>
    {
        if (input.ActualValue is null)
            return 0;

        double.TryParse(input.Value.ToString(), out double result);
        return (result / 1024d / 1024d).Round(3);
    })
{ }

[Convert<Numeral.Vector2, double>]
public class ConvertChromacityToKelvin() : ValueConverter<Numeral.Vector2, double>()
{
    protected override ValueConverterOutput<double> Convert(ValueConverterInput<Numeral.Vector2> input) => ChromacityTemperature.GetTemperature((XY)input.Value);

    protected override ValueConverterOutput<Numeral.Vector2> ConvertBack(ValueConverterInput<double> input) => (Numeral.Vector2)ChromacityTemperature.GetChromacity(input.Value);
}

///

[Convert<double, double>]
public class ConvertDoubleHalf() : ValueConverter<double, double>()
{
    protected override ValueConverterOutput<double> Convert(ValueConverterInput<double> input) => input.Value / 2.0;

    protected override ValueConverterOutput<double> ConvertBack(ValueConverterInput<double> input) => input.Value * 2.0;
}

[Convert<double, double>]
public class ConvertDoubleInverse() : ValueConverter<double, double>()
{
    protected override ValueConverterOutput<double> Convert(ValueConverterInput<double> input) => 1 - Math.Clamp(input.Value, 0, 1);

    protected override ValueConverterOutput<double> ConvertBack(ValueConverterInput<double> input) => 1 - Math.Clamp(input.Value, 0, 1);
}

[Convert<double, double>]
public class ConvertDoublePercent() : ValueConverter<double, double>()
{
    protected override ValueConverterOutput<double> Convert(ValueConverterInput<double> input) => input.Value * 100.0;

    protected override ValueConverterOutput<double> ConvertBack(ValueConverterInput<double> input) => input.Value / 100.0;
}

[Convert<double, string>]
public class ConvertDoublePercentString() : ValueConverter<double, string>()
{
    protected override ValueConverterOutput<string> Convert(ValueConverterInput<double> input) => $"{(input.Value * 100.0).Round(input.ActualParameter != null ? input.Parameter : int.MaxValue)}%";
}

[Convert<double, double>]
public class ConvertRadiusToDiameter() : ValueConverter<double, double>()
{
    protected override ValueConverterOutput<double> Convert(ValueConverterInput<double> input) => input.Value * 2.0;

    protected override ValueConverterOutput<double> ConvertBack(ValueConverterInput<double> input) => input.Value / 2.0;
}

[Convert<double, Double1>]
public class ConvertDoubleToOne()
    : ValueConverter<double, Double1>
    (i => (Double1)i.Value, i => (double)i.Value)
{ }

[Convert<double, Point>]
public class ConvertDoubleToPoint()
    : ValueConverter<double, Point>
    (i => new Point(i.Value, i.Value))
{ }

[Convert<double, TimeSpan>]
public class ConvertDoubleToTimeSpan()
    : ValueConverter<double, TimeSpan>
    (i => TimeSpan.FromSeconds(i.Value), i => i.Value.TotalSeconds)
{ }

[Convert<double, Thickness>]
public class ConvertDoubleToThickness()
    : ValueConverter<double, Thickness>
    (i =>
    {
        return i.Parameter switch
        {
            0 => new Thickness(i.Value, 0, 0, 0),
            1 => new Thickness(0, i.Value, 0, 0),
            2 => new Thickness(0, 0, i.Value, 0),
            3 => new Thickness(0, 0, 0, i.Value),
            _ => new Thickness(i.Value)
        };
    })
{ }

///

[Convert<byte, double>]
public class ConvertByteToDouble()
    : ValueConverter<byte, double>
    (i => X.ToDouble(i.Value) / X.ToDouble(byte.MaxValue), i => X.ToByte(i.Value * X.ToDouble(byte.MaxValue)))
{ }

[Convert<Component4, double>]
public class ConvertComponent4ToDouble() : ValueConverter<Component4, double>()
{
    protected override ValueConverterOutput<double> Convert(ValueConverterInput<Component4> input) => (int)input.Value;

    protected override ValueConverterOutput<Component4> ConvertBack(ValueConverterInput<double> input) => (Component4)(int)input.Value;
}

[Convert<DateTime, double>]
public class ConvertDateTimeToDouble()
    : ValueConverter<DateTime, double>
    (i => X.ToDouble(i.Value.Ticks), i => new DateTime(X.ToInt64(i.Value)))
{ }

[Convert<decimal, double>]
public class ConvertDecimalToDouble()
    : ValueConverter<decimal, double>
    (i => X.ToDouble(i.Value), i => X.ToDecimal(i.Value))
{ }

[Convert<Angle, double>]
public class ConvertDegreeToDouble()
    : ValueConverter<Angle, double>
    (i => (double)i.Value, i => (Angle)i.Value)
{ }

[Convert<double, double>]
public class ConvertDegreeToRadian()
    : ValueConverter<double, double>
    (i => (double)new Angle(i.Value).Convert(AngleType.Radian), i => (double)new Angle(i.Value, AngleType.Radian).Convert(AngleType.Degree))
{ }

[Convert<short, double>]
public class ConvertInt16ToDouble()
    : ValueConverter<short, double>
    (i => X.ToDouble(i.Value), i => X.ToInt16(i.Value))
{ }

[Convert<int, double>]
public class ConvertInt32ToDouble()
    : ValueConverter<int, double>
    (i => X.ToDouble(i.Value), i => X.ToInt32(i.Value))
{ }

[Convert<long, double>]
public class ConvertInt64ToDouble()
    : ValueConverter<long, double>
    (i => X.ToDouble(i.Value), i => X.ToInt64(i.Value))
{ }

[Convert<object, double>]
public class ConvertObjectToDouble()
    : ValueConverter<object, double>
    (i => X.ToDouble(i.Value) is double result && double.IsNaN(result) ? No.Thing : result, i => i.Value)
{ }

[Convert<Double1, double>]
public class ConvertDouble1ToDouble()
    : ValueConverter<Double1, double>
    (i => (double)i.Value, i => (Double1)i.Value)
{ }

[Convert<Double1, double>]
public class ConvertSingle1ToDouble()
    : ValueConverter<Single1, double>
    (i => X.ToDouble(i.Value), i => (Single1)X.ToSingle(i.Value))
{ }

[Convert<double, double>]
public class ConvertRadianToDegree()
    : ValueConverter<double, double>
    (i => (double)new Angle(i.Value, AngleType.Radian).Convert(AngleType.Degree), i => (double)new Angle(i.Value).Convert(AngleType.Radian))
{ }

[Convert<sbyte, double>]
public class ConvertSByteToDouble()
    : ValueConverter<sbyte, double>
    (i => X.ToDouble(i.Value), i => X.ToSByte(i.Value))
{ }

[Convert<float, double>]
public class ConvertSingleToDouble()
    : ValueConverter<float, double>
    (i => X.ToDouble(i.Value), i => X.ToSingle(i.Value))
{ }

[Convert<TimeSpan, double>]
public class ConvertTimeSpanToDouble()
    : ValueConverter<TimeSpan, double>
    (i => i.Value.TotalMilliseconds, i => TimeSpan.FromMilliseconds(i.Value))
{ }

[Convert<UDouble, double>]
public class ConvertUDoubleToDouble()
    : ValueConverter<UDouble, double>
    (i => (double)i.Value, i => (UDouble)i.Value)
{ }

[Convert<ushort, double>]
public class ConvertUInt16ToDouble()
    : ValueConverter<ushort, double>
    (i => X.ToDouble(i.Value), i => X.ToUInt16(i.Value))
{ }

[Convert<uint, double>]
public class ConvertUInt32ToDouble()
    : ValueConverter<uint, double>
    (i => X.ToDouble(i.Value), i => X.ToUInt32(i.Value))
{ }

[Convert<ulong, double>]
public class ConvertUInt64ToDouble()
    : ValueConverter<ulong, double>
    (i => X.ToDouble(i.Value), i => X.ToUInt64(i.Value))
{ }

[Convert<USingle, double>]
public class ConvertUSingleToDouble()
    : ValueConverter<USingle, double>
    (i => (double)i.Value, i => (USingle)i.Value)
{ }

#endregion

/// <see cref="Drive"/>
#region

[Convert<string, double>]
public class ConvertDriveSize()
    : ValueConverter<string, double>
    (i =>
    {
        return Try.Get(() =>
        {
            foreach (var j in Drive.Get())
            {
                if (j.Name.ToLower() == i.Value.ToLower())
                    return X.ToDouble(j.TotalSize);
            }
            return 0D;
        });
    })
{ }

[Convert<string, double>]
public class ConvertDriveSizeAvailable()
    : ValueConverter<string, double>
    (i =>
    {
        return Try.Get(() =>
        {
            foreach (var j in Drive.Get())
            {
                if (j.Name.ToLower() == i.Value.ToLower())
                    return X.ToDouble(j.AvailableFreeSpace);
            }
            return 0D;
        });
    })
{ }

[Convert<string, bool>]
public class ConvertDriveSizeLow()
    : ValueConverter<string, bool>
    (i =>
    {
        return Try.Get(() =>
        {
            foreach (var j in Drive.Get())
            {
                if (j.Name.ToLower() == i.Value.ToLower())
                    return j.AvailableFreeSpace < 10000000000L;
            }
            return false;
        });
    })
{ }

[Convert<string, double>]
public class ConvertDriveSizeUsed()
    : ValueConverter<string, double>
    (i =>
    {
        return Try.Get(() =>
        {
            foreach (var j in Drive.Get())
            {
                if (j.Name.ToLower() == i.Value.ToLower())
                    return X.ToDouble(j.TotalSize) - X.ToDouble(j.AvailableFreeSpace);
            }
            return 0D;
        });
    })
{ }

[Convert<string, double>]
public class ConvertDriveSizeUsedPercent()
    : ValueConverter<string, double>
    (i =>
    {
        return Try.Get(() =>
        {
            foreach (var j in Drive.Get())
            {
                if (j.Name.ToLower() == i.Value.ToLower())
                    return X.ToDouble(j.TotalSize) == 0 ? 0 : (X.ToDouble(j.TotalSize) - X.ToDouble(j.AvailableFreeSpace)) / X.ToDouble(j.TotalSize);
            }
            return 0D;
        });
    })
{ }

#endregion

/// <see cref="File"/>
#region

[Convert<string, DateTime>]
public class ConvertFileAccessed()
    : ValueConverter<string, DateTime>
    (i => new FileInfo(i.Value).LastAccessTime)
{ }

[Convert<string, DateTime>]
public class ConvertFileCreated()
    : ValueConverter<string, DateTime>
    (i => new FileInfo(i.Value).CreationTime)
{ }

[Convert<string, string>]
public class ConvertFileExtension()
    : ValueConverter<string, string>
    (i =>
    {
        string result = null;
        return !Try.Do(() => result = Path.GetExtension(i.Value))
            ? (ValueConverterOutput<string>)No.Thing
            : i.Parameter == 0 ? result.Replace(".", string.Empty) : i.Parameter == 1 ? result : throw i.InvalidParameter;
    })
{ }

[Convert<string, bool>]
public class ConvertFileExists()
    : ValueConverter<string, bool>
    (i => Storage.File.Exists(i.Value))
{ }

[Convert<string, bool>]
public class ConvertFileHidden()
    : ValueConverter<string, bool>
    (i => XItemPath.IsHidden(i.Value))
{ }

[Convert<string, DateTime>]
public class ConvertFileModified()
    : ValueConverter<string, DateTime>
    (i => new FileInfo(i.Value).LastWriteTime)
{ }

[Convert<object, string>]
public class ConvertFileName()
    : ValueConverter<object, string>
    (i =>
{
    var path = i.Value.ToString();

    if (path == FilePath.Root)
        return FilePath.RootName;

    if (path.EndsWith(@":\"))
    {
        foreach (var j in Drive.Get())
        {
            if (path.Equals(j.Name))
                return $"{j.VolumeLabel} ({j.Name.Replace(@"\", string.Empty)})";
        }
        return path;
    }

    return Folder.Exists(path) || i.Parameter == 1
        ? System.IO.Path.GetFileName(path)
        : i.Parameter == 0
            ? System.IO.Path.GetFileNameWithoutExtension(path)
            : throw i.InvalidParameter;
})
{ }

[Convert<Uri, string>]
public class ConvertFileNameFromUri()
    : ValueConverter<Uri, string>
    (i =>
    {
        string x = "", y = "";
        foreach (var j in i.Value.OriginalString)
        {
            if (j == '/')
            {
                x = "";
            }
            else x += j;
        }
        foreach (var j in x)
        {
            if (j == '.')
                return y;

            y += j;
        }
        return x;
    })
{ }

[Convert<object, string>]
public class ConvertFileSize()
    : ValueConverter<object, string>
    (i =>
    {
        long result = i.Value is double a ? X.ToInt64(a) : i.Value is long b ? b : i.Value is string c ? X.ToInt64(c) : 0;
        return new FileSize(result).ToString(i.ActualParameter is FileSizeFormat j ? j : FileSizeFormat.BinaryUsingSI);
    })
{ protected override bool IsSource(object i) => i is double || i is long || i is string; }

#endregion

/// <see cref="Folder"/>
#region

[Convert<string, bool>]
public class ConvertFolderExists()
    : ValueConverter<string, bool>
    (i => Folder.Exists(i.Value))
{ }


[Convert<string, string>]
public class ConvertFolderName()
    : ValueConverter<string, string>
    (i => Path.GetDirectoryName(i.Value))
{ }

#endregion

/// <see cref="Gradient"/>
#region

[Convert<Gradient, LinearGradientBrush>]
public class ConvertGradientToLinearGradientBrush()
    : ValueConverter<Gradient, LinearGradientBrush>
    (1, i => { i.Value.Convert(out LinearGradientBrush j); return j; }, i => { i.Value.Convert(out Gradient j); return j; })
{ }

[Convert<Gradient, RadialGradientBrush>]
public class ConvertGradientToRadialGradientBrush()
    : ValueConverter<Gradient, RadialGradientBrush>
    (1, i => { i.Value.Convert(out RadialGradientBrush j); return j; }, i => { i.Value.Convert(out Gradient j); return j; })
{ }

[Convert<LinearGradientBrush, Gradient>]
public class ConvertLinearGradientBrushToGradient()
    : ValueConverter<LinearGradientBrush, Gradient>
    (1, i => { i.Value.Convert(out Gradient j); return j; }, i => { i.Value.Convert(out LinearGradientBrush j); return j; })
{ }

[Convert<RadialGradientBrush, Gradient>]
public class ConvertRadialGradientBrushToGradient()
    : ValueConverter<RadialGradientBrush, Gradient>
    (1, i => { i.Value.Convert(out Gradient j); return j; }, i => { i.Value.Convert(out RadialGradientBrush j); return j; })
{ }

#endregion

/// <see cref="GridLength"/>
#region

[Convert<ControlLength, GridLength>]
public class ConvertControlLengthToGridLength()
    : ValueConverter<ControlLength, GridLength>
    (i => (GridLength)i.Value, i => (ControlLength)i.Value)
{ }

[Convert<DataGridLength, GridLength>]
public class ConvertDataGridLengthToGridLength()
    : ValueConverter<DataGridLength, GridLength>
    (
        i => new GridLength(i.Value.Value, i.Value.UnitType == DataGridLengthUnitType.Star ? GridUnitType.Star : i.Value.UnitType == DataGridLengthUnitType.Pixel ? GridUnitType.Pixel : GridUnitType.Auto),
        i => new DataGridLength(i.Value.Value, i.Value.GridUnitType == GridUnitType.Star ? DataGridLengthUnitType.Star : i.Value.GridUnitType == GridUnitType.Pixel ? DataGridLengthUnitType.Pixel : DataGridLengthUnitType.Auto)
    )
{ }

[Convert<GridLength, ControlLength>]
public class ConvertGridLengthToControlLength()
    : ValueConverter<GridLength, ControlLength>
    (i => (ControlLength)i.Value, i => (GridLength)i.Value)
{ }

[Convert<GridLength, DataGridLength>]
public class ConvertGridLengthToDataGridLength()
    : ValueConverter<GridLength, DataGridLength>
    (
        i => new DataGridLength(i.Value.Value, i.Value.GridUnitType == GridUnitType.Star ? DataGridLengthUnitType.Star : i.Value.GridUnitType == GridUnitType.Pixel ? DataGridLengthUnitType.Pixel : DataGridLengthUnitType.Auto),
        i => new GridLength(i.Value.Value, i.Value.UnitType == DataGridLengthUnitType.Star ? GridUnitType.Star : i.Value.UnitType == DataGridLengthUnitType.Pixel ? GridUnitType.Pixel : GridUnitType.Auto)
    )
{ }

#endregion

/// <see cref="Invert"/>
#region

/// <summary>Supports <see cref="bool"/>, <see cref="Orient"/>, <see cref="Orientation"/>, <see cref="Thickness"/>, and <see cref="Visibility"/>.</summary>
[Convert<object, object>]
public class Invert()
    : ValueConverter<object, object>
    (1, i =>
    {
        if (i.Value is null)
            return No.Thing;

        if (i.Value is bool boolean)
            return !boolean;

        if (i.Value is Orient orient)
            return orient == Orient.Horizontal ? Orient.Vertical : Orient.Horizontal;

        if (i.Value is System.Windows.Controls.Orientation orientation)
            return orientation == System.Windows.Controls.Orientation.Horizontal ? System.Windows.Controls.Orientation.Vertical : System.Windows.Controls.Orientation.Horizontal;

        if (i.Value is Thickness thickness)
            return thickness.Invert();

        if (i.Value is Visibility visibility)
            return visibility.Invert();

        return i.Value;
    }, true)
{ }

#endregion

/// <see cref="Item"/>
#region

[Convert<string, string>]
public class ConvertItemAccessed()
    : ValueConverter<string, string>
    (i => Try.Get(() => new FileInfo(i.Value).LastAccessTime.ToString()))
{ }

[Convert<string, string>]
public class ConvertItemCreated()
    : ValueConverter<string, string>
    (i => Try.Get(() => new FileInfo(i.Value).CreationTime.ToString()))
{ }

[Convert<string, string>]
public class ConvertItemDescription()
    : ValueConverter<string, string>
    (i => Try.Get(() => XItemPath.GetFriendlyDescription(i.Value)))
{ }

[Convert<string, string>]
public class ConvertItemModified()
    : ValueConverter<string, string>
    (i => Try.Get(() => new FileInfo(i.Value).LastWriteTime.Relative()))
{ }

[Convert<string, string>]
public class ConvertItemSize()
    : ValueConverter<string, string>
    (i => Try.Get(() => new FileSize(new FileInfo(i.Value).Length).ToString(i.ActualParameter is FileSizeFormat j ? j : FileSizeFormat.BinaryUsingSI)))
{ }

[Convert<string, ItemType>]
public class ConvertItemType()
    : ValueConverter<string, ItemType>
    (i => Try.Get(() => XItemPath.GetType(i.Value)))
{ }

#endregion

/// <see cref="MethodInfo"/>
#region

[Convert<MethodInfo, string>]
public class ConvertMethodInfoFirstParameterType()
    : ValueConverter<MethodInfo, string>
    (i => (i.Value is MethodInfo method && method.GetParameters()?.Length > 0 && method.GetParameters()[0].ParameterType is Type type ? type : null)?.GetRealName(i.Parameter == 0))
{ }

[Convert<MethodInfo, string>]
public class ConvertMethodInfoReturnType()
    : ValueConverter<MethodInfo, string>
    (i => i.Value.ReturnType.GetRealName(i.Parameter == 0))
{ }

#endregion

/// <see cref="object"/>
#region

[Convert<object, HorizontalAlignment>]
public class ConvertToHorizontalAlignment()
    : ValueConverter<object, HorizontalAlignment>
    (1, i =>
    {
        if (i.Value is AlignX j)
            return j.ToString().Parse<HorizontalAlignment>();

        return No.Thing;
    })
{ }

[Convert<object, VerticalAlignment>]
public class ConvertToVerticalAlignment()
    : ValueConverter<object, VerticalAlignment>
    (1, i =>
    {
        if (i.Value is AlignY j)
            return j.ToString().Parse<VerticalAlignment>();

        return No.Thing;
    })
{ }

[Convert<object, FontFamily>]
public class ConvertToFontFamily()
    : ValueConverter<object, FontFamily>
    (1, i =>
    {
        if (i.Value is FontFamily j)
            return j;

        if (i.Value is string k)
            return new FontFamily(k);

        return No.Thing;
    })
{ }

[Convert<object, NameAttribute>]
public class ConvertToNameAttribute()
    : ValueConverter<object, NameAttribute>
    (1, i =>
    {
        if (i.Value is MemberBase a)
            return new NameAttribute(a.Style.GetValue(i => i.Name));

        else if (i.Value is object b)
        {
            if (b.GetAttribute<StyleAttribute>() is StyleAttribute c)
                return new NameAttribute(c.Name);

            else if (b.GetAttribute<NameAttribute>() is NameAttribute d)
                return d;
        }
        return No.Thing;
    })
{ }

[Convert<object, ImageAttribute>]
public class ConvertToImageAttribute()
    : ValueConverter<object, ImageAttribute>
    (1, i =>
    {
        if (false) { }
        else if (i.Value is MemberBase a)
            return new ImageAttribute() { Name = a.Style.GetValue(i => i.Image) };

        else if (i.Value is object b)
        {
            if (false) { }
            else if (b.GetAttribute<StyleAttribute>() is StyleAttribute c)
                return new ImageAttribute() { Name = c.Image };

            else if (b.GetAttribute<ImageAttribute>() is ImageAttribute d)
                return d;
        }
        return No.Thing;
    })
{ }

[Convert<object, ImageSource>]
public class ConvertToImageSource()
    : ValueConverter<object, ImageSource>
    (1, i =>
    {
        BitmapImage result = null;
        if (i.Value is Images a)
        {
            Try.Do(() =>
            {
                var x = new BitmapImage();
                x.BeginInit();
                x.UriSource = Resource.GetImageUri(a);
                x.EndInit();
                result = x;
            });
        }
        if (i.Value is string b)
        {
            Try.Do(() =>
            {
                var y = new BitmapImage();
                y.BeginInit();
                y.UriSource = new Uri(b, UriKind.Absolute);
                y.EndInit();
                result = y;
            });
        }
        if (i.Value is Uri c)
        {
            Try.Do(() =>
            {
                var z = new BitmapImage();
                z.BeginInit();
                z.UriSource = c;
                z.EndInit();
                result = z;
            });
        }
        return result;
    })
{ }

[Convert<object, ListCollectionView>]
public class ConvertToListCollectionView()
    : ValueConverter<object, ListCollectionView>
    (1, i =>
    {
        if (i.Value is IList x)
            return new ListCollectionView(x);

        if (i.Value is ListCollectionView y)
            return y;

        if (i.Value is IEnumerable z)
            return new ListCollectionView(new ListObservable<object>(z));

        return default;
    })
{ }

[Convert<object, ColorInfo>]
public class ConvertToModelInfo() : ValueConverter<object, ColorInfo>
    (0, i =>
    {
        var model = i.Value as Type ?? (i.Value as ColorViewModel)?.ColorType ?? i.Value?.GetType();
        if (model?.Implements<IColor>() == true)
            return new ColorInfo(model);

        return No.Thing;
    })
{ }

[Convert<object, ListObservable>]
public class ConvertToObjectCollection() : ValueConverter<object, ListObservable>(i =>
{
    var type = i.Value.GetType();
    if (type is not null)
    {
        if (type.IsArray)
            return new ListObservable((object[])i.Value);
    }

    type = i.Value as Type ?? type;
    if (type is not null)
    {
        if (type.IsEnum)
            return type.GetEnumValues(Browse.Visible).IfNotNullGet(j => new ListObservable(j));

        if (i.Value is IEnumerable a)
        {
            var result = new ListObservable();
            a.ForEach(result.Add);
            return result;
        }
    }

    return No.Thing;
})
{ }

[Convert<object, SolidColorBrush>]
public class ConvertToSolidColorBrush()
    : ValueConverter<object, SolidColorBrush>
    (1, i =>
    {
        if (false) { }

        else if (i.Value is ByteVector3 a)
            return new SolidColorBrush(XColor.Convert(new ByteVector4(a)));

        else if (i.Value is ByteVector4 b)
            return new SolidColorBrush(XColor.Convert(b));

        else if (i.Value is Color c)
            return new SolidColorBrush(c);

        else if (i.Value is System.Drawing.Color d)
        {
            XColor.Convert(d, out Color dd);
            return new SolidColorBrush(dd);
        }
        else if (i.Value is SolidColorBrush e)
            return e;

        else if (i.Value is string f)
        {
            ByteVector4? ff = default;
            ff = Try.Get(() => new ByteVector4(f));
            if (ff is not null)
                return new SolidColorBrush(XColor.Convert(ff.Value));
        }

        return No.Thing;
    })
{ }

[Convert<object, ListObservableOfString>]
public class ConvertToStringCollection()
    : ValueConverter<object, ListObservableOfString>
    (1, i =>
    {
        var result = new ListObservableOfString();
        if (i.Value is string c)
            c.Split(';', System.StringSplitOptions.RemoveEmptyEntries).ForEach(j => result.Add(j.ToString()));

        else if (i.Value is IEnumerable a)
            a.ForEach(j => result.Add(j.ToString()));

        else if (i.Value is IList b)
            b.ForEach(j => result.Add(j.ToString()));

        return result;
    })
{ }

[Convert<object, Uri>]
public class ConvertToUri()
    : ValueConverter<object, Uri>
    (1, i =>
    {
        if (i.Value is Images a)
            return Resource.GetImageUri(a, AssemblyProject.WPF);

        if (i.Value is string b)
            return new Uri(b, UriKind.Absolute);

        if (i.Value is Uri c)
            return c;

        return No.Thing;
    })
{ }

#endregion

/// <see cref="Orient"/>
#region

[Convert<Orient, Orientation>]
public class ConvertOrientToOrientation()
    : ValueConverter<Orient, Orientation>
    (i => i.ToString().Parse<Orientation>(), i => i.ToString().Parse<Orient>())
{ }

[Convert<Orientation, Orient>]
public class ConvertOrientationToOrient()
    : ValueConverter<Orientation, Orient>
    (i => i.ToString().Parse<Orient>(), i => i.ToString().Parse<Orientation>())
{ }

#endregion

/// <see cref="Shortcut"/>
#region

[Convert<string, string>]
public class ConvertShortcutTarget()
    : ValueConverter<string, string>
    (i => Shortcut.TargetPath(i.Value))
{ }

#endregion

/// <see cref="Side"/>
#region

[Convert<Side, Dock>]
public class ConvertSideToDock()
    : ValueConverter<Side, Dock>
    (i => i.Value.ToString().Parse<Dock>(), i => i.Value.ToString().Parse<Side>())
{ }

[Convert<Side, Orient>]
public class ConvertSideToOrient()
    : ValueConverter<Side, Orient>
    (i => i.Value == Side.Left || i.Value == Side.Right ? Orient.Horizontal : Orient.Vertical)
{ }

[Convert<Side, Sides>]
public class ConvertSideToSides()
    : ValueConverter<Side, Sides>
    (i => i.Value == Side.Left || i.Value == Side.Top ? Sides.LeftOrTop : Sides.RightOrBottom)
{ }

#endregion

/// <see cref="Result"/>
#region

[Convert<Error, Result>]
public class ConvertErrorToResult()
    : ValueConverter<Error, Result>
    (i => i.Value, i => (Error)i.Value)
{ }

[Convert<Result, Error>]
public class ConvertResultToError()
    : ValueConverter<Result, Error>
    (i => (Error)i.Value, i => i.Value)
{ }

#endregion

/// <see cref="Select"/>
#region

[Convert<Select, SelectionMode>]
public class ConvertSelectToSelectionMode()
    : ValueConverter<Select, SelectionMode>
    (i =>
    {
        return i.Value switch
        {
            Controls.Select.OneOrMore => System.Windows.Controls.SelectionMode.Extended,
            Controls.Select.One or Controls.Select.OneOrNone => System.Windows.Controls.SelectionMode.Single,
            _ => throw new NotSupportedException()
        };
    })
{ }

[Convert<SelectionMode, Select>]
public class ConvertSelectionModeToSelect()
    : ValueConverter<SelectionMode, Select>
    (i =>
    {
        return i.Value switch
        {
            System.Windows.Controls.SelectionMode.Extended or System.Windows.Controls.SelectionMode.Multiple => Controls.Select.OneOrMore,
            System.Windows.Controls.SelectionMode.Single => Controls.Select.One,
            _ => throw new NotSupportedException()
        };
    })
{ }

#endregion

/// <see cref="SolidColorBrush"/>
#region

[Convert<SolidColorBrush, SolidColorBrush>]
public class ConvertSolidColorBrushBrightness() : ValueConverter<SolidColorBrush, SolidColorBrush>(i =>
{
    var color = i.Value.Color;

    color.Convert(out ByteVector4 v);
    var rgb = XColorVector.Convert(v);

    var hsl = new RGB(rgb).To<HSL>(ColorProfile.Default);

    double.TryParse(i.ActualParameter?.ToString(), out double z);
    hsl.Z = z;

    rgb = hsl.To<RGB>(ColorProfile.Default);

    var result = XColor.Convert(XColorVector.Convert(rgb));
    return new SolidColorBrush(result);
})
{ }

[Convert<SolidColorBrush, ByteVector4>]
public class ConvertSolidColorBrushToByteVector4()
    : ValueConverter<SolidColorBrush, ByteVector4>
    (i => { i.Value.Color.Convert(out ByteVector4 result); return result; })
{ }

[Convert<ByteVector4, SolidColorBrush>]
public class ConvertByteVector4ToSolidColorBrush()
    : ValueConverter<object, SolidColorBrush>
    (i =>
    {
        ByteVector4 color = i.Value is ByteVector4 j ? j : (i.Value?.GetType() == typeof(ByteVector4?) ? ((ByteVector4?)i.Value).Value : default);
        return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
    },
    i => new ByteVector4(i.Value.Color.R, i.Value.Color.G, i.Value.Color.B, i.Value.Color.A))
{ }

[Convert<Color, SolidColorBrush>]
public class ConvertColorToSolidColorBrush()
    : ValueConverter<Color, SolidColorBrush>
    (i => new SolidColorBrush(i.Parameter == 0 ? i.Value : Color.FromArgb((byte)255, i.Value.R, i.Value.G, i.Value.B)), i => i.Value.Color)
{ }

[Convert<Color, SolidColorBrush>]
public class ConvertColorToSolidColorBrushBlackOrWhite()
    : ValueConverter<Color, SolidColorBrush>
    (i =>
    {
        var color = i.Value;
        color.Convert(out System.Drawing.Color result);

        var parameter = 0.5;
        if (i.ActualParameter != null)
            double.TryParse($"{i.ActualParameter}", out parameter);

        return new SolidColorBrush(result.GetBrightness() > parameter ? System.Windows.Media.Colors.Black : System.Windows.Media.Colors.White);
    })
{ }

[Convert<string, SolidColorBrush>]
public class ConvertStringToSolidColorBrush()
    : ValueConverter<string, SolidColorBrush>
    (
        i => new ByteVector4(i.Value).IfGet<ByteVector4, SolidColorBrush>(j => new SolidColorBrush(Color.FromArgb(j.A, j.R, j.G, j.B))),
        i => new ByteVector4(i.Value.Color.R, i.Value.Color.G, i.Value.Color.B, i.Value.Color.A).XYZ.ToString()
    )
{ }

#endregion

/// <see cref="SortDirection"/>
#region

[Convert<ListSortDirection, SortDirection>]
public class ConvertListSortDirectionToSortDirection()
    : ValueConverter<ListSortDirection, SortDirection>
    (i => i.ToString().Parse<SortDirection>())
{ }

[Convert<SortDirection, ListSortDirection>]
public class ConvertSortDirectionToListSortDirection()
    : ValueConverter<SortDirection, ListSortDirection>
    (i => i.ToString().Parse<ListSortDirection>())
{ }

#endregion

/// <see cref="string"/>
#region

[Convert<object, string>]
public class ConvertToString()
    : ValueConverter<object, string>
    (i => i.Value.ToString())
{ }

[Convert<object, string>]
public class ConvertToStringCamel()
    : ValueConverter<object, string>
    (i =>
    {
        var result = i.Value.ToString().GetCamel() ?? string.Empty;
        return i.Parameter == 0 ? result : i.Parameter == 1 ? result.Capitalize() : i.Parameter == 2 ? result.ToLower() : throw new NotSupportedException();
    })
{ }

[Convert<object, string>]
public class ConvertToStringLower()
    : ValueConverter<object, string>
    (i => i.Value.ToString().ToLower())
{ }

[Convert<object, string>]
public class ConvertToStringPluralNumber() : ValueConverter<object, string>()
{
    protected override bool IsSource(object input) => input is ushort || input is short || input is uint || input is int || input is long || input is long;

    protected override ValueConverterOutput<string> Convert(ValueConverterInput<object> input)
    {
        var result = X.ToInt32(input.Value);
        return result == 1 ? string.Empty : input.Parameter == 0 ? "s" : "S";
    }
}

[Convert<object, string>]
public class ConvertToStringTimeRelative() : ValueConverter<object, string>()
{
    protected override bool AllowNull => true;

    protected override bool IsSource(object input) => input is DateTime || input is DateTime?;

    protected override ValueConverterOutput<string> Convert(ValueConverterInput<object> input)
    {
        if (input.Value is null)
            return "never";

        if (input.Value is DateTime a)
            return a.Relative();

        return input.Value.As<DateTime?>()?.Relative() ?? (ValueConverterOutput<string>)No.Thing;
    }
}

[Convert<object, string>]
public class ConvertToStringTimeShort() : ValueConverter<object, string>()
{
    protected override bool IsSource(object input) => input is DateTime || input is DateTime? || input is int || input is string;

    protected override ValueConverterOutput<string> Convert(ValueConverterInput<object> input)
    {
        TimeSpan result = TimeSpan.Zero;
        if (input.Value is int a)
            result = TimeSpan.FromSeconds(a);

        else if (input.Value is string b)
            result = TimeSpan.FromSeconds(X.ToInt32(b));

        else
        {
            var now = DateTime.Now;
            if (input.Value is DateTime c)
                result = c > now ? c - now : now - c;

            if (input.Value is DateTime?)
            {
                var d = input.Value as DateTime?;

                if (d is null)
                    return string.Empty;

                result = d.Value > now ? d.Value - now : now - d.Value;
            }
        }

        return result.ToShort(input.Parameter == 1);
    }
}

[Convert<object, string>]
public class ConvertToStringUpper()
    : ValueConverter<object, string>
    (i => i.Value.ToString().ToUpper())
{ }

[Convert<object, string>]
public class ConvertToStringWithFirstLetter()
    : ValueConverter<object, string>
    (i => i.Value.ToString().IfGet<string>(j => !j.IsEmpty(), j => j[..1]))
{ }

[Convert<object, string>]
public class ConvertToStringWithLeadingZeros()
    : ValueConverter<int, string>
    (i => i.Value.ToString("D2"))
{ }

[Convert<object, string>]
public class ConvertToStringWithoutNewLines()
    : ValueConverter<object, string>
    (i => i.Value.ToString().Replace("\n", "").Replace("\r", ""))
{ }

[Convert<object, string>]
public class ConvertToSubstring()
    : ValueConverter<object, string>
    (i =>
    {
        if (i.Value is string || i.Value is Enum)
        {
            var j = i.Value.ToString();
            Try.Do(() => j = j[..(i.Parameter == 0 ? j.Length : i.Parameter)]);
            return j;
        }
        return default;
    })
{ }

///

[Convert<string, int>]
public class ConvertStringLength()
    : ValueConverter<string, int>
    (1, i => i.Value?.Length ?? 0)
{ }

[Convert<string, bool>]
public class ConvertStringToBoolean()
    : ValueConverter<string, bool>
    (1, i => i.Value.IsEmpty() == false, i => default)
{ }

[Convert<string, FontFamily>]
public class ConvertStringToFontFamily()
    : ValueConverter<string, FontFamily>
    (1, i =>
    {
        FontFamily result = null;
        Try.Do(() => result = new FontFamily(i.Value));
        return result;
    },
    i => i.Value.Source)
{ }

///

[Convert<object, string>]
public abstract class ConvertArrayToString()
    : ValueConverter<object, string>
    (i =>
    {
        if (i.Value.GetType().IsArray)
        {
            if (i.Value is IList list)
            {
                var result = string.Empty;

                var index = 0;
                var count = list.Count;

                foreach (var j in list)
                {
                    result += (index == count - 1 ? $"{j}" : $"{j}{X.ToChar(i.ActualParameter?.ToString())}");
                    index++;
                }

                return result;
            }
            return i.Value.ToString();
        }
        return default;
    })
{ }

[Convert<byte, string>]
public class ConvertByteToString()
    : ValueConverter<byte, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToByte(i.Value))
{ }

[Convert<ByteVector2, string>]
public class ConvertByteVector2ToString()
    : ValueConverter<ByteVector2, string>
    (i => i.Value.ToString(), i => new ByteVector2(i.Value))
{ }

[Convert<ByteVector3, string>]
public class ConvertByteVector3ToString()
    : ValueConverter<ByteVector3, string>
    (i => i.Value.ToString(), i => new ByteVector3(i.Value))
{ }

[Convert<ByteVector4, string>]
public class ConvertByteVector4ToString()
    : ValueConverter<ByteVector4, string>
    (i => i.Parameter == 1 ? i.Value.ToString() : i.Value.XYZ.ToString(), i => new ByteVector4(i.Value))
{ }

[Convert<ByteVector4, string>]
public class ConvertByteVector4ToColorName()
    : ValueConverter<ByteVector4, string>
    (i =>
    {
        var color = XColor.Convert(i.Value);
        color.Convert(out ByteVector4 j);

        return Instance.GetName(j);
    })
{ }

[Convert<char, string>]
public class ConvertCharacterToString()
    : ValueConverter<char, string>
    (i => i.Value.ToString(), i => X.ToChar(i.Value))
{ }

[Convert<Color, string>]
public class ConvertColorToString()
    : ValueConverter<Color, string>
    (i =>
    {
        i.Value.Convert(out ByteVector4 j);
        return i.Parameter == 1 ? j.ToString() : j.XYZ.ToString();
    },
    i => XColor.Convert(new ByteVector4(i.Value).A(j => i.Parameter == 1 ? j : i.Parameter == 0 ? (byte)255 : throw new NotSupportedException())))
{ }

[Convert<Color, string>]
public class ConvertColorToStringName()
    : ValueConverter<Color, string>
    (i =>
    {
        i.Value.Convert(out ByteVector4 j);
        return Instance.GetName(j);
    })
{ }

[Convert<Color, string>]
public class ConvertColorToStringShort()
    : ValueConverter<Color, string>
    (i =>
    {
        i.Value.Convert(out ByteVector4 j);

        var k = j.ToString(ByteVector4.StringFormatHexShortSymbol);
        return i.ActualParameter?.ToString().F(k) ?? k;
    })
{ }

[Convert<System.Drawing.Color, string>]
public class ConvertColorInt32ToString()
    : ValueConverter<System.Drawing.Color, string>
    (i =>
    {
        i.Value.Convert(out Color j);
        j.Convert(out ByteVector4 k);
        return i.Parameter == 1 ? k.ToString() : k.XYZ.ToString();
    },
    i =>
    {
        XColor.Convert(new ByteVector4(i.Value).A(x => i.Parameter == 1 ? x : i.Parameter == 0 ? (byte)255 : throw new NotSupportedException()))
            .Convert(out System.Drawing.Color k);

        return k;
    })
{ }

[Convert<DateTime, string>]
public class ConvertDateTimeToString()
    : ValueConverter<DateTime, string>
    (i =>
    {
        if (i.ActualParameter is object format)
        {
            var j = format.ToString();
            if (j == TimeFormat.Relative)
                return i.Value.Relative();

            return i.Value.ToString(j);
        }
        return i.Value.ToString();
    },
    i => { _ = DateTime.TryParse(i.Value, out DateTime j); return j; })
{ }

[Convert<decimal, string>]
public class ConvertDecimalToString()
    : ValueConverter<decimal, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToDecimal(i.Value))
{ }

[Convert<Angle, string>]
public class ConvertDegreeToString()
    : ValueConverter<Angle, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => (Angle)X.ToDouble(i.Value))
{ }

[Convert<double, string>]
public class ConvertDoubleToString()
    : ValueConverter<double, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToDouble(i.Value))
{ }

[Convert<Flag, string>]
public class ConvertFlagToString()
    : ValueConverter<Flag, string>
    (i => i.Value.Keys.ToString(i.ActualParameter?.ToString()))
{ }

[Convert<FontFamily, string>]
public class ConvertFontFamilyToString()
    : ValueConverter<FontFamily, string>
    (1, i => i.Value?.Source ?? "Calibri", input => Try.Get(() => new FontFamily(input.Value)))
{ }

[Convert<Guid, string>]
public class ConvertGuidToString()
    : ValueConverter<Guid, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()),
    i =>
    {
        Guid.TryParse(i.Value, out Guid j);
        return j;
    })
{ }

[Convert<Thickness, string>]
public class ConvertThicknessToString()
    : ValueConverter<Thickness, string>
    (i => i.Value.ToString(),
    i =>
    {
        var values = Enumerable.Select(i.Value.TrimWhite().Split(','), j => { _ = double.TryParse(j, out double k); return k; }).ToArray();
        if (values.Length == 1)
            return new Thickness(values[0]);

        if (values.Length == 2)
            return new Thickness(values[0], values[1], values[0], values[1]);

        if (values.Length == 4)
            return new Thickness(values[0], values[1], values[2], values[3]);

        return new Thickness(0);
    })
{ }

[Convert<short, string>]
public class ConvertInt16ToString()
    : ValueConverter<short, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToInt16(i.Value))
{ }

[Convert<int, string>]
public class ConvertInt32ToString()
    : ValueConverter<int, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToInt32(i.Value))
{ }

[Convert<long, string>]
public class ConvertInt64ToString()
    : ValueConverter<long, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToInt64(i.Value))
{ }

[Convert<IList, string>]
public class ConvertListToString()
    : ValueConverter<IList, string>
    (i => i.Value.Count == 0 ? "(empty collection)" : i.Value.ToString(", "))
{ }

[Convert<Double1, string>]
public class ConvertOneToString()
    : ValueConverter<Double1, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => (Double1)X.ToDouble(i.Value))
{ }

[Convert<sbyte, string>]
public class ConvertSByteToString()
    : ValueConverter<sbyte, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToSByte(i.Value))
{ }

[Convert<float, string>]
public class ConvertSingleToString()
    : ValueConverter<float, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToSingle(i.Value))
{ }

[Convert<SolidColorBrush, string>]
public class ConvertSolidColorBrushToString()
    : ValueConverter<SolidColorBrush, string>
    (i =>
    {
        i.Value.Color.Convert(out ByteVector4 j);
        return j.ToString();
    },
    i => new SolidColorBrush(XColor.Convert(new ByteVector4(i.Value))))
{ }

[Convert<TimeSpan, string>]
public class ConvertTimeSpanToString()
    : ValueConverter<TimeSpan, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => { _ = TimeSpan.TryParse(i.Value, out TimeSpan j); return j; })
{ }

[Convert<UDouble, string>]
public class ConvertUDoubleToString()
    : ValueConverter<UDouble, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => { UDouble.TryParse(i.Value, out UDouble j); return j; })
{ }

[Convert<ushort, string>]
public class ConvertUInt16ToString()
    : ValueConverter<ushort, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToUInt16(i.Value))
{ }

[Convert<uint, string>]
public class ConvertUInt32ToString()
    : ValueConverter<uint, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToUInt32(i.Value))
{ }

[Convert<ulong, string>]
public class ConvertUInt64ToString()
    : ValueConverter<ulong, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => X.ToUInt64(i.Value))
{ }

[Convert<USingle, string>]
public class ConvertUSingleToString()
    : ValueConverter<USingle, string>
    (i => i.Value.ToString(i.ActualParameter?.ToString()), i => { USingle.TryParse(i.Value, out USingle j); return j; })
{ }

[Convert<Uri, string>]
public class ConvertUriToString()
    : ValueConverter<Uri, string>
    (i => i.Value.ToString(), i => { _ = Uri.TryCreate(i.Value, UriKind.RelativeOrAbsolute, out Uri j); return j; })
{ }

[Convert<Version, string>]
public class ConvertVersionToString()
    : ValueConverter<Version, string>
    (i =>
    {
        if (i.Parameter == 1)
        {
            var result = "";

            //1.0.0.0 > 1.0
            if (i.Value.Build == 0 && i.Value.Revision == 0)
                result = $"{i.Value.Major}.{i.Value.Minor}";

            //1.0.1.0 > 1.0.1
            if (i.Value.Build > 0 && i.Value.Revision == 0)
                result = $"{i.Value.Major}.{i.Value.Minor}.{i.Value.Build}";

            //1.0.0.1 > 1.0.0.1
            if (i.Value.Build == 0 && i.Value.Revision > 0)
                result = $"{i.Value.Major}.{i.Value.Minor}.{i.Value.Build}.{i.Value.Revision}";

            return result;
        }
        return i.Value.ToString();
    },
    i =>
    {
        int major = 0, minor = 0, build = 0;
        string[] j = i.Value.Split('.');
        if (j.Length > 0)
        {
            _ = int.TryParse(j[0], out major);
            if (j.Length > 1)
            {
                _ = int.TryParse(j[1], out minor);

                if (j.Length > 2)
                    _ = int.TryParse(j[2], out build);
            }
        }
        return new Version(major, minor, build);
    })
{ }

#endregion

/// <see cref="Type"/>
#region

[Convert<object, Type>]
public class ConvertObjectToType()
    : ValueConverter<object, Type>
    (0, i =>
    {
        Type result = i.Value as Type
            ?? (i.Value as IGeneric)?.GetGenericType()
            ?? (i.Value as object[]).IfNotNullGet(j => Enumerable.Select(j, k => k.GetType()).GetSharedType())
            ?? i.Value.GetType();
        return result;
    })
{ }

[Convert<object, string>]
public class ConvertObjectToTypeName()
    : ValueConverter<object, string>
    (0, i => Instance.AsType(i.Value).Name)
{ }

[Convert<object, string>]
public class ConvertObjectToTypeNameFull()
    : ValueConverter<object, string>
    (0, i => Instance.AsType(i.Value).FullName)
{ }

[Convert<Type, object>]
public class ConvertTypeToObject()
    : ValueConverter<Type, object>
    (1, i => Try.Get(() => i.Value.Create<object>(), e => Log.Write(e)))
{ }

[Convert<Type, string>]
public class ConvertTypeToString()
    : ValueConverter<Type, string>
    (0, i => i.Value.GetRealName(i.Parameter == 0))
{ }

#endregion

/// <see cref="Visibility"/>
#region

[Convert<bool, Visibility>]
public class ConvertBooleanToVisibility() : ValueConverter<object, object>()
{
    protected override bool IsSource(object input) => input is bool || input is bool? || input is Handle || input is Visibility;

    protected override ValueConverterOutput<object> Convert(ValueConverterInput<object> input)
    {
        if (input.ActualValue is bool || input.ActualValue is bool?)
        {
            var i = input.ActualValue is bool j ? j : input.ActualValue is bool? ? ((bool?)input.ActualValue).Value : throw new NotSupportedException();
            var result = i.ToVisibility(input.ActualParameter is Visibility k ? k : Visibility.Collapsed);

            return input.Parameter == 0
                ? result
                : input.Parameter == 1
                    ? result.Invert()
                    : throw input.InvalidParameter;
        }
        return ConvertBack(input);
    }

    protected override ValueConverterOutput<object> ConvertBack(ValueConverterInput<object> input)
    {
        if (input.ActualValue is Visibility visibility)
        {
            var result = visibility == Visibility.Visible;
            return input.Parameter == 0
                ? result
                : input.Parameter == 1
                    ? !result
                    : throw input.InvalidParameter;
        }

        return Convert(input);
    }
}

[Convert<object, Visibility>]
public class ConvertObjectToVisibility()
    : ValueConverter<object, Visibility>
    (1, i => (i.Value is not null).ToVisibility())
{ }

[Convert<string, Visibility>]
public class ConvertStringToVisibility()
    : ValueConverter<string, Visibility>
    (1, i =>
    {
        var j = (!i.Value.IsEmpty()).ToVisibility();
        return i.Parameter == 0 ? j : i.Parameter == 1 ? j.Invert() : throw i.InvalidParameter;
    })
{ }

#endregion