using System;

namespace Ion.Controls;

[CLSCompliant(false)]
public class SByteUpDown : NumericUpDown<sbyte>
{
    public override sbyte AbsoluteMaximum => sbyte.MaxValue;

    public override sbyte AbsoluteMinimum => sbyte.MinValue;

    public override sbyte DefaultIncrement => 1;

    public override sbyte DefaultValue => 0;

    public override bool IsRational => false;

    public override bool IsSigned => true;

    public SByteUpDown() : base() { }

    protected override sbyte GetValue(string input) => Convert.ToSByte(input);

    protected override string ToString(sbyte input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((sbyte)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((sbyte)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((sbyte)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}