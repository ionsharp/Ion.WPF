using System;
using static System.Math;

namespace Ion.Controls;

[CLSCompliant(false)]
public class UInt16UpDown : NumericUpDown<ushort>
{
    public override ushort AbsoluteMaximum => ushort.MaxValue;

    public override ushort AbsoluteMinimum => ushort.MinValue;

    public override ushort DefaultIncrement => 1;

    public override ushort DefaultValue => System.Convert.ToUInt16(0);

    public override bool IsRational => true;

    public override bool IsSigned => false;

    public UInt16UpDown() : base() { }

    protected override ushort GetValue(string input) => Convert.ToUInt16(input);

    protected override string ToString(ushort input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Clamp((ushort)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Clamp((ushort)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Clamp((ushort)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, System.Convert.ToUInt16(Value + Increment));

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, System.Convert.ToUInt16(Value - Increment));
}