using Ion.Collect;
using Ion.Core;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

public abstract class DockContentControl : TabControlCache, IDockControl, IDockContentControl, IDockContentSource, IDockPanelSource
{
    public DockControl DockControl => Root.DockControl;

    public DockRootControl Root { get; private set; }

    private readonly ListObservable<Content> source = [];
    public IListObservable Source => source;

    ///

    public static readonly DependencyProperty ActiveProperty = DependencyProperty.Register(nameof(Active), typeof(bool), typeof(DockContentControl), new FrameworkPropertyMetadata(false));
    public bool Active
    {
        get => (bool)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    ///

    public ColumnDefinition ColumnDefinition
    {
        get
        {
            var parent = Parent as Grid;
            var index = parent.Children.IndexOf(this);
            if (parent.ColumnDefinitions.Count > 0)
            {
                if (index < parent.ColumnDefinitions.Count)
                    return parent.ColumnDefinitions[index];
            }
            return null;
        }
    }

    public RowDefinition RowDefinition
    {
        get
        {
            var parent = Parent as Grid;
            var index = parent.Children.IndexOf(this);
            if (parent.RowDefinitions.Count > 0)
            {
                if (index < parent.RowDefinitions.Count)
                    return parent.RowDefinitions[index];
            }
            return null;
        }
    }

    ///

    protected DockContentControl(DockRootControl root) : base()
    {
        ItemsSource = Source;
        Root = root;
    }

    ///

    private void Activate()
    {
        DockControl.EachControl(i =>
        {
            if (ReferenceEquals(i.Root, Root))
                i.SetCurrentValue(ActiveProperty, i.Equals(this));

            return true;
        });
        Root.Activate(this);
    }

    private void Deactivate()
    {
        Root.DockControl.EachControl(i =>
        {
            if (i.Root.Equals(Root))
                SetCurrentValue(ActiveProperty, false);

            return true;
        });
        Root.Deactivate(this);
    }


    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        Activate();
    }

    ///

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        if (newValue != null && !ReferenceEquals(newValue, source))
            throw new NotSupportedException();
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        if (Root.DockControl.ActivateButtonState == MouseButtonState.Pressed)
        {
            if (Root.DockControl.ActivateButton == e.ChangedButton)
                Activate();
        }
        base.OnPreviewMouseDown(e);
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
        if (Root.DockControl.ActivateButtonState == MouseButtonState.Released)
        {
            if (Root.DockControl.ActivateButton == e.ChangedButton)
                Activate();
        }
        base.OnPreviewMouseUp(e);
    }
}