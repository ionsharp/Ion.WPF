using System.Windows;

namespace Ion.Controls;

public sealed class DockWindow : Window
{
    public DockRootControl Root => Content as DockRootControl;

    public DockWindow() : base() { }
}