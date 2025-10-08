using System;

namespace Ion.Controls;

public class Int16UpDown : NumericUpDown<short>
{
    public override short AbsoluteMaximum => short.MaxValue;

    public override short AbsoluteMinimum => short.MinValue;

    public override short DefaultIncrement => 1;

    public override short DefaultValue => 0;

    public override bool IsRational => true;

    public override bool IsSigned => true;

    public Int16UpDown() : base() { }

    protected override short GetValue(string input) => Convert.ToInt16(input);

    protected override string ToString(short input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((short)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((short)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((short)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Convert.ToInt16(Value + Increment));

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Convert.ToInt16(Value - Increment));
}