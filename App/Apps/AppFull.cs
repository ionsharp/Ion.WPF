using Ion.Analysis;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Ion.Core;

public abstract class AppFull : App, IAppFull
{
    /// <see cref="Region.Property"/>

    new public static AppFull Current => App.Current as AppFull;

    protected abstract Type ModelType { get; }

    /// <see cref="Region.Constructor"/>

    protected AppFull() : base()
    {
        Appp.Model = ModelType.Create<IAppModel>();
    }

    /// <see cref="Region.Method.Protected"/>

    protected override void OnLoaded(AppLoadedEventArgs e)
    {
        base.OnLoaded(e);
        Appp.Model.Start(e.Source);
    }

    /// <see cref="IAppFull"/>

    IAppModel IAppFull.Model => Appp.Model;
}