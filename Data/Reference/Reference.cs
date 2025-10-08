using System.Windows;

namespace Ion.Data;

/// <see cref="Reference"/>
#region

public abstract class Reference<T>() : Freezable()
{
    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(T), typeof(Reference<T>), new FrameworkPropertyMetadata(default(T)));
    public T Data
    {
        get => (T)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Freezable CreateInstanceCore() => new Reference();

    protected Reference(T data) : this() => SetCurrentValue(DataProperty, data);
}

public class Reference(object data) : Reference<object>(data)
{
    public Reference() : this(null) { }
}

#endregion

/// <see cref="Reference2"/>
#region

public abstract class Reference2<T1, T2> : Freezable
{
    public static readonly DependencyProperty FirstProperty = DependencyProperty.Register(nameof(First), typeof(T1), typeof(Reference2<T1, T2>), new FrameworkPropertyMetadata(default(T1)));
    public T1 First
    {
        get => (T1)GetValue(FirstProperty);
        set => SetValue(FirstProperty, value);
    }

    public static readonly DependencyProperty SecondProperty = DependencyProperty.Register(nameof(Second), typeof(T2), typeof(Reference2<T1, T2>), new FrameworkPropertyMetadata(default(T2)));
    public T2 Second
    {
        get => (T2)GetValue(SecondProperty);
        set => SetValue(SecondProperty, value);
    }

    protected Reference2() : base() { }

    protected Reference2(T1 first, T2 second) : this()
    {
        SetCurrentValue(FirstProperty, first); SetCurrentValue(SecondProperty, second);
    }
}

public class Reference2(object first, object second) : Reference2<object, object>(first, second)
{
    public Reference2() : this(null, null) { }

    protected override Freezable CreateInstanceCore() => new Reference2();
}

#endregion