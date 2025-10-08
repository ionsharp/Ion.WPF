using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Ion.Controls;

public abstract class NumericUpDown<T> : UpDown<T>
{
    protected const string Comma = ",";

    protected const string Dash = "-";

    protected const string Period = ".";

    public abstract T DefaultIncrement { get; }

    public virtual Regex Expression => new("^[0-9]?$");

    public abstract bool IsRational { get; }

    public abstract bool IsSigned { get; }

    public static readonly DependencyProperty<T, NumericUpDown<T>> IncrementProperty = new(nameof(Increment), new FrameworkPropertyMetadata(default(T), OnIncrementChanged));
    public T Increment
    {
        get => IncrementProperty.Get(this);
        set => IncrementProperty.Set(this, value);
    }

    private static void OnIncrementChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<NumericUpDown<T>>().OnIncrementChanged(e.Convert<T>());

    public static readonly DependencyProperty<string, NumericUpDown<T>> StringFormatProperty = new(nameof(StringFormat), new FrameworkPropertyMetadata(string.Empty, OnStringFormatChanged));
    public string StringFormat
    {
        get => StringFormatProperty.Get(this);
        set => StringFormatProperty.Set(this, value);
    }

    private static void OnStringFormatChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<NumericUpDown<T>>().OnStringFormatChanged(e.Convert<string>());

    protected NumericUpDown() : base() => SetCurrentValue(IncrementProperty.Property, DefaultIncrement);

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        base.OnPreviewTextInput(e);
        e.Handled = e.Text switch
        {
            Dash => !(IsSigned && CaretIndex == 0 && !Text.Contains(Dash)),
            Period => !(!IsRational && !Text.Contains(Period)),
            _ => !Expression.IsMatch(e.Text),
        };
    }

    protected virtual void OnIncrementChanged(ValueChange<T> input) { }

    protected virtual void OnStringFormatChanged(ValueChange<string> input)
    {
        OnValueChanged(new ValueChange<T>(default, Value));
    }
}