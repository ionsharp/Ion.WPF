using Ion.Controls;
using System.Collections.Generic;

namespace Ion.Core;

/// <inheritdoc/>
public abstract record class
    FileDockAppModel<A, B, C, D, E>() :
    DockAppModel<A, B, C, D, E>(), IFileDockAppModel
        where A : AppData
        where B : DockAppMenu
        where C : AppWindow
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

    /// <see cref="IFileDockAppModel"/>

    IFileDockViewModel IFileDockAppModel.ViewModel => ViewModel;
}