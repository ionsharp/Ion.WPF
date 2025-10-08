using System;
using System.Windows.Input;

namespace Ion.Input;

/// <summary>
/// An <see cref="IRelayCommand"/> with parameter.
/// </summary>
/// <remarks>
/// <para>If WPF4.5+, use <b>GalaSoft.MvvmLight.CommandWpf</b> (instead of <b>GalaSoft.MvvmLight.Command</b>) to enable (or restore) <see cref="CommandManager"/>.</para>
/// </remarks>
public class RelayCommand<Parameter> : ICommand, IRelayCommand
{
    public event EventHandler CanExecuteChanged
    {
        add
        {
            if (_CanExecute != null)
                CommandManager.RequerySuggested += value;
        }
        remove
        {
            if (_CanExecute != null)
                CommandManager.RequerySuggested -= value;
        }
    }

    private readonly WeakAction<Parameter> _Execute;

    private readonly WeakFunc<Parameter, bool> _CanExecute;

    /// <exception cref="ArgumentNullException"/>
    public RelayCommand(Action<Parameter> execute, bool canExecute = true) : this(execute, i => canExecute) { }

    /// <exception cref="ArgumentNullException"/>
    public RelayCommand(Action<Parameter> execute, Func<Parameter, bool> canExecute)
    {
        Throw.IfNull(execute, nameof(execute));
        Throw.IfNull(canExecute, nameof(canExecute));

        _Execute = new WeakAction<Parameter>(execute);
        if (canExecute != null)
            _CanExecute = new WeakFunc<Parameter, bool>(canExecute);
    }

    public void RaiseCanExecuteChanged()
    {
#if SILVERLIGHT
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#elif NETFX_CORE
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#elif XAMARIN
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
#else
        CommandManager.InvalidateRequerySuggested();
#endif
    }

    public bool CanExecute(object parameter)
    {
        if (_CanExecute == null)
            return true;

        if (_CanExecute.IsStatic || _CanExecute.IsAlive)
        {
            if (parameter == null
#if NETFX_CORE
                && typeof(T).GetTypeInfo().IsValueType)
#else
                && typeof(Parameter).IsValueType)
#endif
            {
                return _CanExecute.Execute(default);
            }

            if (parameter == null || parameter is Parameter)
                return _CanExecute.Execute((Parameter)parameter);
        }
        return false;
    }

    public virtual void Execute(object parameter)
    {
        var val = parameter;
        if (CanExecute(val) && _Execute != null && (_Execute.IsStatic || _Execute.IsAlive))
        {
            if (val == null)
            {
                if (typeof(Parameter).IsValueType)
                    _Execute.Execute(default);

                else _Execute.Execute((Parameter)val);
            }
            else _Execute.Execute((Parameter)val);
        }
    }
}