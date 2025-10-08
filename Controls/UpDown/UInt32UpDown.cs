using System;
using static System.Math;

namespace Ion.Controls;

[CLSCompliant(false)]
public class UInt32UpDown : NumericUpDown<uint>
{
    public override uint AbsoluteMaximum => uint.MaxValue;

    public override uint AbsoluteMinimum => uint.MinValue;

    public override uint DefaultIncrement => 1;

    public override uint DefaultValue => 0;

    public override bool IsRational => true;

    public override bool IsSigned => false;

    public UInt32UpDown() : base() { }

    protected override uint GetValue(string input) => Convert.ToUInt32(input);

    protected override string ToString(uint input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Clamp((uint)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Clamp((uint)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Clamp((uint)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}