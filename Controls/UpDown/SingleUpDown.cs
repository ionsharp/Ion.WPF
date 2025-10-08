using System;

namespace Ion.Controls;

public class SingleUpDown : NumericUpDown<float>
{
    public override float AbsoluteMaximum => float.MaxValue;

    public override float AbsoluteMinimum => float.MinValue;

    public override float DefaultIncrement => 1f;

    public override float DefaultValue => 0f;

    public override bool IsRational => false;

    public override bool IsSigned => true;

    public SingleUpDown() : base() { }

    protected override float GetValue(string input) { _ = float.TryParse(input, out var value); return value; }

    protected override string ToString(float input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((float)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((float)input, AbsoluteMinimum, Value);

    protected override object OnValueCoerced(object input) => Math.Clamp((float)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Value + Increment);

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Value - Increment);
}