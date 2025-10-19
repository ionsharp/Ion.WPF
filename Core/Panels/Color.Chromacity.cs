using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Reflection;

namespace Ion.Core;

/// <inheritdoc/>
[Styles.Object(Image = Images.Thermometer, Name = "Chromacity",
    Description = "Calculate the chromacity of color.",
    Filter = Filter.None, GroupName = MemberGroupName.None)]
public record class ColorChromacityPanel() : Panel()
{
    /// <see cref="Region.Key"/>

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Field"/>

    private readonly Handle handle = false;

    /// <see cref="Region.Property"/>

    [StyleInherit]
    [StyleOverride(nameof(StyleAttribute.Index), 0)]
    [StyleOverride(nameof(StyleAttribute.Options), Option.Copy | Option.Paste | Option.Replace)]
    [StyleOverride(nameof(StyleAttribute.ReplaceItems), nameof(DefaultChromacity))]
    public Vector2 Chromacity { get => Get(ColorProfile.DefaultWhite); set => Set(value); }

    public static object DefaultChromacity
    {
        get
        {
            var result = new ListObservable<Namable<Vector2>>();
            foreach (var i in typeof(Illuminant2).GetProperties(BindingFlags.Public | BindingFlags.Static))
                result.Add(new(i.Name + " (2°)", (Vector2)i.GetValue(null)));

            foreach (var i in typeof(Illuminant10).GetProperties(BindingFlags.Public | BindingFlags.Static))
                result.Add(new(i.Name + " (10°)", (Vector2)i.GetValue(null)));

            return result;
        }
    }

    [Styles.Number(2000.0, 12000.0, 100.0,
        Gradient = ["ff1c00", "FFF", "bbd0ff"],
        Pin = Sides.LeftOrTop,
        RightText = "K",
        ValueFormat = NumberFormat.Default)]
    public double Illuminant { get => Get(ChromacityTemperature.GetTemperature((XY)ColorProfile.Default.Chromacity)); set => Set(value); }

    [StyleInherit]
    [StyleOverride(nameof(StyleAttribute.Index), 1)]
    [StyleOverride(nameof(StyleAttribute.Options), Option.Copy | Option.Paste)]
    public Vector3 White { get => Get(Vector3.One); set => Set(value); }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Chromacity):
                handle.DoInternal(() =>
                {
                    Illuminant = ChromacityTemperature.GetTemperature((XY)Chromacity);
                    White = (XYZ)(xyY)(XY)Chromacity;
                });
                break;

            case nameof(Illuminant):
                handle.DoInternal(() =>
                {
                    Chromacity = ChromacityTemperature.GetChromacity(Illuminant);
                    White = (XYZ)(xyY)(XY)Chromacity;
                });
                break;

            case nameof(White):
                handle.DoInternal(() =>
                {
                    Chromacity = (XY)(xyY)(XYZ)White;
                    Illuminant = ChromacityTemperature.GetTemperature((XY)Chromacity);
                });
                break;
        }
    }
}