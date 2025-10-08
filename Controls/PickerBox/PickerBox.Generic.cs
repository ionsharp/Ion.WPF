using Ion.Input;
using System;

namespace Ion.Controls;

public abstract class PickerBox<T> : PickerBox
{
    public event EventHandler<EventArgs<T>> ValueChanged;

    protected abstract T DefaultValue { get; }

    protected PickerBox() : base() { }

    protected virtual void OnValueChanged(Value<T> input) => ValueChanged?.Invoke(this, new EventArgs<T>(input.NewValue));

    protected abstract T GetValue();

    protected abstract void SetValue(T i);
}