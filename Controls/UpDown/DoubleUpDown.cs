using System;

namespace Ion.Controls;

public class DoubleUpDown : NumericUpDown<double>
{
    public override double AbsoluteMaximum => double.MaxValue;

    public override double AbsoluteMinimum => double.MinValue;

    public override double DefaultIncrement => 1;

    public override double DefaultValue => 0;

    public override bool IsRational => false;

    public override bool IsSigned => true;

    public DoubleUpDown() : base() { }

    protected override double GetValue(string i) { _ = double.TryParse(i, out double j); return j; }

    protected override string ToString(double input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((double)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((double)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((double)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}