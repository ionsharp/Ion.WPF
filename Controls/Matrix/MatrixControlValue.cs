using Ion.Core;

namespace Ion.Controls;

public record class MatrixControlValue : Model
{
    public readonly MatrixControl Control;

    public double Value { get => Get<double>(); set => Set(value); }

    public double Weight { get => Get<double>(); set => Set(value); }

    public MatrixControlValue(MatrixControl control, double value) : base()
    {
        Control = control; Value = value;
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(Value))
            Control.Update();
    }
}