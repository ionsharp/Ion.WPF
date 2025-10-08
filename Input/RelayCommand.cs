using System;
using System.Windows.Input;

namespace Ion.Input;

/// <summary>
/// An <see cref="IRelayCommand"/> without parameter.
/// </summary>
/// <remarks>
/// <para>If WPF4.5+, use <b>GalaSoft.MvvmLight.CommandWpf</b> (instead of <b>GalaSoft.MvvmLight.Command</b>) to enable (or restore) <see cref="CommandManager"/>.</para>
/// </remarks>
public class RelayCommand : ICommand, IRelayCommand
{
    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (_CanExecute != null)
            {
                // add event handler to local handler backing field in a thread safe manner
                EventHandler handler2;
                EventHandler canExecuteChanged = _RequerySuggestedLocal;

                do
                {
                    handler2 = canExecuteChanged;
                    EventHandler handler3 = (EventHandler)Delegate.Combine(handler2, value);
                    canExecuteChanged = System.Threading.Interlocked.CompareExchange<EventHandler>(
                            ref _RequerySuggestedLocal,
                            handler3,
                            handler2);
                }
                while (canExecuteChanged != handler2);

                CommandManager.RequerySuggested += value;
            }
        }
        remove
        {
            if (_CanExecute != null)
            {
                // removes an event handler from local backing field in a thread safe manner
                EventHandler handler2;
                EventHandler canExecuteChanged = this._RequerySuggestedLocal;

                do
                {
                    handler2 = canExecuteChanged;
                    EventHandler handler3 = (EventHandler)Delegate.Remove(handler2, value);
                    canExecuteChanged = System.Threading.Interlocked.CompareExchange<EventHandler>(
                            ref this._RequerySuggestedLocal,
                            handler3,
                            handler2);
                }
                while (canExecuteChanged != handler2);

                CommandManager.RequerySuggested -= value;
            }
        }
    }

    private EventHandler _RequerySuggestedLocal;

    private readonly WeakAction _Execute;

    private readonly WeakFunc<bool> _CanExecute;

    /// <exception cref="ArgumentNullException"/>
    public RelayCommand(Action execute, bool canExecute = true) : this(execute, () => canExecute) { }

    /// <exception cref="ArgumentNullException"/>
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        Throw.IfNull(execute, nameof(execute));
        Throw.IfNull(canExecute, nameof(canExecute));

        _Execute = new WeakAction(execute);
        if (canExecute != null)
            _CanExecute = new WeakFunc<bool>(canExecute);
    }

    public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

    public bool CanExecute(object parameter)
    {
        return _CanExecute == null || (_CanExecute.IsStatic || _CanExecute.IsAlive) && _CanExecute.Execute();
    }

    public virtual void Execute(object parameter)
    {
        if (CanExecute(parameter) && _Execute != null && (_Execute.IsStatic || _Execute.IsAlive))
            _Execute.Execute();
    }
}