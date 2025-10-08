using System;
using System.Windows.Input;

namespace Ion.Controls;

public class ByteUpDown : NumericUpDown<byte>
{
    public override byte AbsoluteMaximum => byte.MaxValue;

    public override byte AbsoluteMinimum => byte.MinValue;

    public override byte DefaultIncrement => 1;

    public override byte DefaultValue => 0;

    public override bool IsRational => true;

    public override bool IsSigned => false;

    public ByteUpDown() : base() { }

    protected override byte GetValue(string input) => Convert.ToByte(input);

    protected override string ToString(byte input) => input.ToString(StringFormat);

    protected override bool CanIncrease() => Value < Maximum;

    protected override bool CanDecrease() => Value > Minimum;

    protected override object OnMaximumCoerced(object input) => Math.Clamp((byte)input, Value, AbsoluteMaximum);

    protected override object OnMinimumCoerced(object input) => Math.Clamp((byte)input, AbsoluteMinimum, Value);

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        base.OnPreviewTextInput(e);
        e.Handled = CaretIndex > 0 && e.Text == "-" || e.Handled;
    }

    protected override object OnValueCoerced(object input) => Math.Clamp((byte)input, Minimum, Maximum);

    public override void Increase() => SetCurrentValue(ValueProperty.Property, Convert.ToByte((Value + Increment)));

    public override void Decrease() => SetCurrentValue(ValueProperty.Property, Convert.ToByte((Value - Increment)));
}