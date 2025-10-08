using Ion.Analysis;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Ion.Core;

public abstract class App : Application, IApp
{
    /// <see cref="Region.Event"/>
    #region

    public event UnhandledExceptionEventHandler ExceptionUnhandled;

    public event AppLoadedEventHandler Loaded;

    #endregion

    /// <see cref="Region.Property"/>

    new public static App Current => Application.Current as App;

    protected abstract Type ModelType { get; }

    /// <see cref="Region.Constructor"/>

    protected App() : base()
    {
        AppDomain.CurrentDomain.UnhandledException
            += OnExceptionUnhandled;
        DispatcherUnhandledException
            += OnExceptionUnhandled;
        TaskScheduler.UnobservedTaskException
            += OnExceptionUnhandled;

        Appp.Model = ModelType.Create<IAppModel>();
    }

    /// <see cref="Region.Method.Private"/>
    #region

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

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        OnLoaded(e.Args);

        Appp.Model.Start(e);
    }

    protected virtual void OnExceptionUnhandled(UnhandledExceptions type, Exception e)
        => ExceptionUnhandled?.Invoke(this, new UnhandledExceptionEventArgs(type, new Error(e)));

    protected virtual void OnLoaded(IList<string> arguments) => Loaded?.Invoke(this, arguments);

    #endregion

    /// <see cref="IApp"/>

    IAppModel IApp.Model => Appp.Model;
}