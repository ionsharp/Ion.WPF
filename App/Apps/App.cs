using Ion.Analysis;
using Ion.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Ion;

public abstract class App : Application, IApp
{
    public event Ion.Core.UnhandledExceptionEventHandler ExceptionUnhandled;

    public event AppLoadedEventHandler Loaded;

    new public static App Current => Application.Current as App;

    public App() : base()
    {
        AppDomain.CurrentDomain.UnhandledException
            += OnExceptionUnhandled;
        DispatcherUnhandledException
            += OnExceptionUnhandled;
        TaskScheduler.UnobservedTaskException
            += OnExceptionUnhandled;
    }

    private void OnExceptionUnhandled(object sender, System.UnhandledExceptionEventArgs e)
    {
        OnExceptionUnhandled(UnhandledExceptions.AppDomain, e.ExceptionObject as Exception);
    }

    private void OnExceptionUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
#if DEBUG
        e.Handled = false;
#else
    e.Handled = true;
#endif
        OnExceptionUnhandled(UnhandledExceptions.Dispatcher, e.Exception);
    }

    private void OnExceptionUnhandled(object sender, UnobservedTaskExceptionEventArgs e)
    {
#if DEBUG

#else
    e.SetObserved();
#endif
        OnExceptionUnhandled(UnhandledExceptions.TaskScheduler, e.Exception);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        OnLoaded(new AppLoadedEventArgs(e));
    }

    protected virtual void OnExceptionUnhandled(UnhandledExceptions type, Exception e)
    {
        ExceptionUnhandled?.Invoke(this, new Ion.Core.UnhandledExceptionEventArgs(type, new Error(e)));

        while (e != null)
        {
            Debug.WriteLine($"[UNHANDLED] {e.Message}");
            e = e.InnerException;
        }
    }

    protected virtual void OnLoaded(AppLoadedEventArgs e) => Loaded?.Invoke(this, e);
}