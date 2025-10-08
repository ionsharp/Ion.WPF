using System.Windows;

namespace Ion.Controls;

public sealed class DockDocumentControl(DockRootControl root) : DockContentControl(root)
{
    public static readonly DependencyProperty PersistProperty = DependencyProperty.Register(nameof(Persist), typeof(bool), typeof(DockDocumentControl), new FrameworkPropertyMetadata(false));
    public bool Persist
    {
        get => (bool)GetValue(PersistProperty);
        set => SetValue(PersistProperty, value);
    }
}