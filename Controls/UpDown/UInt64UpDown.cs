using System;
using static System.Math;

namespace Ion.Controls;

[CLSCompliant(false)]
public class UInt64UpDown : NumericUpDown<ulong>
{
    public override ulong AbsoluteMaximum => ulong.MaxValue;

    public override ulong AbsoluteMinimum => ulong.MinValue;

    public override ulong DefaultIncrement => 1;

    public override ulong DefaultValue => 0;

    public override bool IsRational => true;

    public override bool IsSigned => false;

    public UInt64UpDown() : base() { }

    protected override ulong GetValue(string input) => Convert.ToUInt64(input);

    protected override string ToString(ulong input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Clamp((ulong)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Clamp((ulong)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Clamp((ulong)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}