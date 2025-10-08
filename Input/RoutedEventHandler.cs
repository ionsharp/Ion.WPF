using System.Windows;

namespace Ion.Input;

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1>(DependencyObject sender, RoutedEventArgs<T1> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2>(DependencyObject sender, RoutedEventArgs<T1, T2> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2, T3>(DependencyObject sender, RoutedEventArgs<T1, T2, T3> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2, T3, T4>(DependencyObject sender, RoutedEventArgs<T1, T2, T3, T4> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2, T3, T4, T5>(DependencyObject sender, RoutedEventArgs<T1, T2, T3, T4, T5> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2, T3, T4, T5, T6>(DependencyObject sender, RoutedEventArgs<T1, T2, T3, T4, T5, T6> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2, T3, T4, T5, T6, T7>(DependencyObject sender, RoutedEventArgs<T1, T2, T3, T4, T5, T6, T7> e);

/// <inheritdoc cref="RoutedEventHandler"/>
public delegate void RoutedEventHandler<T1, T2, T3, T4, T5, T6, T7, TRest>(DependencyObject sender, RoutedEventArgs<T1, T2, T3, T4, T5, T6, T7, TRest> e);