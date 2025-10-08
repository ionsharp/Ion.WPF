using System;

namespace Ion.Controls;

public class DecimalUpDown : NumericUpDown<decimal>
{
    public override decimal AbsoluteMaximum => decimal.MaxValue;

    public override decimal AbsoluteMinimum => decimal.MinValue;

    public override decimal DefaultIncrement => 1m;

    public override decimal DefaultValue => 0m;

    public override bool IsRational => false;

    public override bool IsSigned => true;

    public DecimalUpDown() : base() => Increment = 1m;

    protected override decimal GetValue(string input) => Convert.ToDecimal(input);

    protected override string ToString(decimal input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((decimal)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((decimal)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((decimal)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}