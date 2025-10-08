using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

public abstract class UpDown<T> : UpDown, IUpDown<T>
{
    #region Properties

    protected readonly Handle handle = false;

    /// <summary>
    /// The absolute maximum value possible.
    /// </summary>
    public abstract T AbsoluteMaximum { get; }

    /// <summary>
    /// The absolute minimum value possible.
    /// </summary>
    public abstract T AbsoluteMinimum { get; }

    /// <summary>
    /// The default value.
    /// </summary>
    public abstract T DefaultValue { get; }

    public static readonly DependencyProperty<T, UpDown<T>> MaximumProperty = new(nameof(Maximum), new FrameworkPropertyMetadata(default(T), OnMaximumChanged, OnMaximumCoerced));
    public T Maximum
    {
        get => MaximumProperty.Get(this);
        set => MaximumProperty.Set(this, value);
    }

    private static void OnMaximumChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<UpDown<T>>().OnMaximumChanged(e.Convert<T>());
    private static object OnMaximumCoerced(DependencyObject i, object Value) => i.As<UpDown<T>>().OnMaximumCoerced(Value);

    public static readonly DependencyProperty<T, UpDown<T>> MinimumProperty = new(nameof(Minimum), new FrameworkPropertyMetadata(default(T), OnMinimumChanged, OnMinimumCoerced));
    public T Minimum
    {
        get => MinimumProperty.Get(this);
        set => MinimumProperty.Set(this, value);
    }

    private static void OnMinimumChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<UpDown<T>>().OnMinimumChanged(e.Convert<T>());
    private static object OnMinimumCoerced(DependencyObject i, object Value) => i.As<UpDown<T>>().OnMinimumCoerced(Value);

    public static readonly DependencyProperty<T, UpDown<T>> ValueProperty = new(nameof(Value), new FrameworkPropertyMetadata(default(T), OnValueChanged, OnValueCoerced));
    public T Value
    {
        get => ValueProperty.Get(this);
        set => ValueProperty.Set(this, value);
    }

    private static void OnValueChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<UpDown<T>>().OnValueChanged(e.Convert<T>());
    private static object OnValueCoerced(DependencyObject i, object Value) => i.As<UpDown<T>>().OnValueCoerced(Value);

    #endregion

    #region UpDown

    protected UpDown() : base()
    {
        SetCurrentValue(MaximumProperty.Property, AbsoluteMaximum);
        SetCurrentValue(MinimumProperty.Property, AbsoluteMinimum);
        SetCurrentValue(ValueProperty.Property, DefaultValue);
        OnValueChanged(new ValueChange<T>(Value));
    }

    #endregion

    #region Methods

    #region Abstract

    protected abstract T GetValue(string Value);

    protected abstract object OnMaximumCoerced(object Value);

    protected abstract object OnMinimumCoerced(object Value);

    protected abstract object OnValueCoerced(object Value);

    protected abstract string ToString(T Value);

    #endregion

    #region Overrides

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        handle.DoInternal(() =>
        {
            var i = ToString(Value);
            if (i != Text)
                Text = i;
        });
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        handle.DoInternal(() => Try.Do(() => Value = GetValue(Text), e => Value = default));
    }

    public sealed override void ValueToMaximum() => SetCurrentValue(ValueProperty.Property, Maximum);

    public sealed override void ValueToMinimum() => SetCurrentValue(ValueProperty.Property, Minimum);

    #endregion

    #region Virtual

    protected virtual void OnMaximumChanged(ValueChange<T> input) { }

    protected virtual void OnMinimumChanged(ValueChange<T> input) { }

    protected virtual void OnValueChanged(ValueChange<T> input) => handle.DoInternal(() => SetText(ToString(input.NewValue)));

    #endregion

    #endregion
}