using System;

namespace Ion.Controls;

public class Int64UpDown : NumericUpDown<long>
{
    public override long AbsoluteMaximum => long.MaxValue;

    public override long AbsoluteMinimum => long.MinValue;

    public override long DefaultIncrement => 1L;

    public override long DefaultValue => 0L;

    public override bool IsRational => true;

    public override bool IsSigned => true;

    public Int64UpDown() : base() { }

    protected override long GetValue(string input) { _ = long.TryParse(input, out var value); return value; }

    protected override string ToString(long input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((long)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((long)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((long)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}