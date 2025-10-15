using Ion.Core;
using System.Windows;

namespace Ion.Controls;

public abstract class AppView() : Window(), IAppView
{
    public static readonly ResourceKey MenuBelow = new();

    public static readonly ResourceKey MenuMain = new();
}