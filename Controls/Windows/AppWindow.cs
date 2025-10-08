using Ion.Core;
using System.Windows;

namespace Ion.Controls;

public abstract class AppWindow() : Window(), IAppWindow
{
    public static readonly ResourceKey MenuBelow = new();

    public static readonly ResourceKey MenuMain = new();
}