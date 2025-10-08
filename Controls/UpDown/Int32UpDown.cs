using System;

namespace Ion.Controls;

public class Int32UpDown : NumericUpDown<int>
{
    public override int AbsoluteMaximum => int.MaxValue;

    public override int AbsoluteMinimum => int.MinValue;

    public override int DefaultIncrement => 1;

    public override int DefaultValue => 0;

    public override bool IsRational => true;

    public override bool IsSigned => true;

    public Int32UpDown() : base() { }

    protected override int GetValue(string i) { _ = int.TryParse(i, out int j); return j; }

    protected override string ToString(int input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((int)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((int)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((int)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}