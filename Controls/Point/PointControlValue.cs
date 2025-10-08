using Ion.Core;
using Ion.Numeral;
using System;

namespace Ion.Controls;

public record class PointControlValue : Model
{
    public readonly PointControl Source;

    public double X { get => Get(.0); set => Set(value); }

    public double Y { get => Get(.0); set => Set(value); }

    public PointControlValue(PointControl source, double x, double y) : base()
    {
        Source = source;
        X = x; Y = y;
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        Source.UpdateSource();
    }

    public override string ToString(string format, IFormatProvider provider) => IVector2.StringFormat.F(X, Y);
}