using Ion.Controls;
using System.Collections.Generic;

namespace Ion.Core;

/// <inheritdoc/>
public abstract record class
    AppModelDockFile<A, B, C, D, E>() :
    AppModelDock<A, B, C, D, E>(), IAppModelDockFile
        where A : AppData
        where B : DockAppMenu
        where C : AppView
        where D : IFileDockViewModel
        where E : Document
{
    protected override void OnAppLoaded(IList<string> arguments)
    {
        base.OnAppLoaded(arguments);
        ViewModel.Open(arguments);
    }

    protected override void OnAppReloaded(IList<string> arguments)
    {
        base.OnAppReloaded(arguments);
        ViewModel.Open(arguments);
    }

    /// <see cref="IAppModelDockFile"/>

    IFileDockViewModel IAppModelDockFile.ViewModel => ViewModel;
}