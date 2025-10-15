using Ion.Controls;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ion.Core;

/// <summary>
/// A lightweight <see cref="App"/>.
/// </summary>
[Using<AppModelBase>]
[Using<AppResources>]
[Using<IAppView>]
[Using<IAppViewModel>]
public abstract class AppTool : App
{
    public AppModelBase Model { get; private set; }

    public AppResources Theme { get; private set; }

    public IAppView View { get; private set; }

    public IAppViewModel ViewModel { get; private set; }

    public AppTool() : base()
    {
        Model = XAssembly.GetDerivedTypes<AppModelBase>(XAssembly.Get(AssemblySource.Entry)).FirstOrDefault<Type>().Create<AppModelBase>() ??
            throw new NotImplementedException(typeof(AppModelBase).FullName);

        Theme = [];
        Theme.LoadTheme(DefaultThemes.Light);
    }

    protected override void OnLoaded(AppLoadedEventArgs e)
    {
        base.OnLoaded(e);
        View
            = (Application.Current.MainWindow
            = XAssembly.GetDerivedTypes<IAppView>(XAssembly.Get(AssemblySource.Entry)).FirstOrDefault<Type>().Create<IAppView>() as Window
            ?? throw new NotImplementedException(typeof(IAppView).FullName)) as IAppView;

        ViewModel = XAssembly.GetDerivedTypes<IAppViewModel>(XAssembly.Get(AssemblySource.Entry)).FirstOrDefault<Type>().Create<IAppViewModel>() ??
            throw new NotImplementedException(typeof(IAppViewModel).FullName);
        ViewModel.View = View;

        View.DataContext = ViewModel;
        View.Show();
    }
}