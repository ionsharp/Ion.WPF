using Ion.Controls;
using Ion.Input;
using Ion.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ion.Core;

/// <summary>Invokes a <see cref="Taskable"/>.</summary>
[Serializable]
public abstract record class MethodPanel() : Panel()
{
    /// <see cref="Region.Property.Protected.Abstract"/>
    #region

    protected abstract TaskStrategy MethodStrategy { get; }

    protected abstract TaskType MethodType { get; }

    #endregion

    /// <see cref="Region.Property.Public"/>
    #region

    public virtual string CancelWarning => "Are you sure you want to cancel?";

    public virtual string CancelWarningTitle => "Cancel";

    public virtual bool ShowCancelWarning => false;

    public virtual string PauseWarning => "Are you sure you want to pause?";

    public virtual string PauseWarningTitle => "Pause";

    public virtual bool ShowPauseWarning => false;

    public virtual string StartWarning => "Are you sure you want to start?";

    public virtual string StartWarningTitle => "Start";

    public virtual bool ShowStartWarning => false;

    ///

    /// <summary>Gets if progress is visible when <see cref="Task"/> is active.</summary>
    public virtual bool IsMethodProgressVisible => true;

    public Taskable<object> Task { get => Get<Taskable<object>>(null, false); private set => Set(value, false); }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private void OnMethodProgressed(Taskable task, TaskProgressedEventArgs e)
        => OnMethodProgressed(e.Progress.NewValue);

    ///

    protected override void OnConstructed()
    {
        base.OnConstructed();
        Task = new(StartSync, StartAsync, MethodStrategy);
    }

    ///

    protected virtual bool CanCancel
        (object parameter) => true;

    protected virtual bool CanPause
        (object parameter) => true;

    protected virtual bool CanStart
        (object parameter) => true;

    ///

    protected virtual void OnMethodProgressed(double progress)
    {
        LastActive = DateTime.Now;
        Progress = progress;
    }

    protected virtual void OnMethodStart()
    {
        IsActive = true; IsLocked = true;

        LastActive = DateTime.Now;
        Progress = 0;

        IsMethodProgressVisible.If(() => IsProgressVisible = true);
        Task.Progressed += OnMethodProgressed;
    }

    protected virtual void OnMethodStop()
    {
        LastActive = DateTime.Now;
        Progress = 0;

        IsProgressVisible = false;
        Task.Progressed -= OnMethodProgressed;

        IsActive = false; IsLocked = false;
    }

    ///

    /// <summary>Already on thread</summary>
    protected abstract void RunSync(object parameter, CancellationToken token);

    /// <summary>Not yet on thread</summary>
    protected abstract Task RunAsync(object parameter, CancellationToken token);

    protected virtual void OnExecuted() { }

    ///

    /// <summary>Already on thread</summary>
    private void StartSync(object parameter, CancellationToken token)
    {
        Dispatch.Do(OnMethodStart);
        RunSync(parameter, token);

        Dispatch.Do(() =>
        {
            OnMethodStop();
            CommandManager.InvalidateRequerySuggested();
        });
    }

    /// <summary>Not yet on thread</summary>
    async protected Task StartAsync(object parameter, CancellationToken token)
    {
        OnMethodStart();
        await RunAsync(parameter, token);

        OnMethodStop();
        CommandManager.InvalidateRequerySuggested();
    }

    ///

    public void Start(object parameter = null) => StartCommand.Execute(parameter);

    #endregion

    /// <see cref="ICommand"/>
    #region

    [Image(Images.Block)]
    [Style(Template.ButtonCancel,
        Index = int.MaxValue,
        IsLockable = true,
        Name = "Cancel",
        Pin = Sides.RightOrBottom)]
    [VisibilityTrigger(nameof(IsActive), true)]
    public virtual ICommand CancelCommand => Commands[nameof(CancelCommand)]
        ??= new RelayCommand(() =>
        {
            if (ShowCancelWarning)
            {
                if (DialogWindow.Show(CancelWarning, CancelWarningTitle, Images.Warning, DialogWindowButton.YesNo) == 1)
                    return;
            }
            Task.Cancel();
        },
        () => Task.IsStarted && CanCancel(null));

    [Image(Images.Pause)]
    [Style(Template.Button,
        Index = int.MaxValue,
        IsLockable = true,
        Name = "Pause",
        Pin = Sides.RightOrBottom)]
    [VisibilityTrigger(nameof(IsActive), true)]
    public virtual ICommand PauseCommand => Commands[nameof(PauseCommand)]
        ??= new RelayCommand(() =>
        {
            if (ShowPauseWarning)
            {
                if (DialogWindow.Show(PauseWarning, PauseWarningTitle, Images.Warning, DialogWindowButton.YesNo) == 1)
                    return;
            }
            Task.Pause();
        },
        () => Task.IsStarted && CanPause(null));

    [Image(Images.Play)]
    [Style(Template.ButtonDefault,
        Index = int.MaxValue,
        IsLockable = true,
        Name = "Start",
        Pin = Sides.RightOrBottom)]
    [VisibilityTrigger(nameof(IsActive), false)]
    public virtual ICommand StartCommand => Commands[nameof(StartCommand)] ??= new RelayCommand(() =>
    {
        if (ShowStartWarning)
        {
            if (DialogWindow.Show(StartWarning, StartWarningTitle, Images.Warning, DialogWindowButton.YesNo) == 1)
                return;
        }
        _ = Task.Start(null, MethodType);
    },
    () => !Task.IsStarted && CanStart(null));

    #endregion
}