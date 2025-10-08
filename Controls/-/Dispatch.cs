using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Ion;

public class Dispatch
{
    public static void Do(Action action, DispatcherPriority priority = DispatcherPriority.Background)
        => Application.Current.Dispatcher.Invoke(action, priority);

    public static void Invoke(params Action[] actions)
        => Invoke(DispatcherPriority.Background, actions);

    public static void Invoke(DispatcherPriority priority, params Action[] actions)
        => actions.ForEach(i => Application.Current.Dispatcher.Invoke(i, priority));

    async public static Task BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Background)
        => await Application.Current.Dispatcher.BeginInvoke(action, priority);

    async public static Task BeginInvoke(DispatcherPriority priority, Delegate callback, object arguments)
        => await Application.Current.Dispatcher.BeginInvoke(priority, callback, arguments);

    public static DispatcherOperation InvokeReturn(Action action, DispatcherPriority priority = DispatcherPriority.Background)
        => Application.Current.Dispatcher.BeginInvoke(action, priority);
}