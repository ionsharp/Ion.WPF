using System.Windows;

namespace Ion.Input;

/// <inheritdoc/>
public class RoutedEventArgs<T1>(RoutedEvent Event, T1 A, object Source = null) : RoutedEventArgs(Event, Source)
{
    public readonly T1 A = A;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2>(RoutedEvent Event, T1 A, T2 B, object Source = null) : RoutedEventArgs<T1>(Event, A, Source)
{
    public T2 B { get; } = B;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2, T3>(RoutedEvent Event, T1 A, T2 B, T3 C, object Source = null) : RoutedEventArgs<T1, T2>(Event, A, B, Source)
{
    public T3 C { get; } = C;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2, T3, T4>(RoutedEvent Event, T1 A, T2 B, T3 C, T4 D, object Source = null) : RoutedEventArgs<T1, T2, T3>(Event, A, B, C, Source)
{
    public T4 D { get; } = D;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2, T3, T4, T5>(RoutedEvent Event, T1 A, T2 B, T3 C, T4 D, T5 E, object Source = null) : RoutedEventArgs<T1, T2, T3, T4>(Event, A, B, C, D, Source)
{
    public T5 E { get; } = E;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2, T3, T4, T5, T6>(RoutedEvent Event, T1 A, T2 B, T3 C, T4 D, T5 E, T6 F, object Source = null) : RoutedEventArgs<T1, T2, T3, T4, T5>(Event, A, B, C, D, E, Source)
{
    public T6 F { get; } = F;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2, T3, T4, T5, T6, T7>(RoutedEvent Event, T1 A, T2 B, T3 C, T4 D, T5 E, T6 F, T7 G, object Source = null) : RoutedEventArgs<T1, T2, T3, T4, T5, T6>(Event, A, B, C, D, E, F, Source)
{
    public T7 G { get; } = G;
}

/// <inheritdoc/>
public class RoutedEventArgs<T1, T2, T3, T4, T5, T6, T7, TRest>(RoutedEvent Event, T1 A, T2 B, T3 C, T4 D, T5 E, T6 F, T7 G, TRest Rest, object Source = null) : RoutedEventArgs<T1, T2, T3, T4, T5, T6, T7>(Event, A, B, C, D, E, F, G, Source)
{
    public TRest Rest { get; } = Rest;
}