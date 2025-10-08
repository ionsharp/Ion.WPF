using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public sealed class DockGroupControl : Grid, IDockControl
{
    private static readonly DependencyPropertyKey OrientationKey = DependencyProperty.RegisterReadOnly(nameof(Orientation), typeof(Orient), typeof(DockDocumentControl), new FrameworkPropertyMetadata(Orient.Horizontal));
    public static readonly DependencyProperty OrientationProperty = OrientationKey.DependencyProperty;
    public Orient Orientation
    {
        get => (Orient)GetValue(OrientationProperty);
        private set => SetValue(OrientationKey, value);
    }

    ///

    public DockControl DockControl => Root.DockControl;

    public DockRootControl Root { get; private set; }

    ///

    public DockGroupControl(DockRootControl root, Orient orientation) : base()
    {
        Root = root; Orientation = orientation;
    }
}