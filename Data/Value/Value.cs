using Ion.Core;
using System.Windows;

namespace Ion;

/// <summary>An old and new value.</summary>
public class Value<T>(T oldValue, T newValue) : object()
{
    public readonly T OldValue = oldValue;

    public readonly T NewValue = newValue;

    public Value(T newValue) : this(default, newValue) { }

    public static implicit operator Value<T>(DependencyPropertyChangedEventArgs e)
        => new(e.OldValue.As<T>(), e.NewValue.As<T>());

    public static implicit operator Value<T>(PropertySetEventArgs e)
        => new(e.OldValue.As<T>(), e.NewValue.As<T>());
}