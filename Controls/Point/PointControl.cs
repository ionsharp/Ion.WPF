using Ion;
using Ion.Collect;
using Ion.Input;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public class PointControl : Control
{
    #region Properties

    internal readonly Handle Handle = false;

    private System.Windows.Shapes.Ellipse target;

    private PointControlValue targetPoint;

    public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(nameof(Points), typeof(PointCollection), typeof(PointControl), new FrameworkPropertyMetadata(null, OnPointsChanged));
    public PointCollection Points
    {
        get => (PointCollection)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    private static void OnPointsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<PointControl>().OnPointsChanged(e);

    private static readonly DependencyPropertyKey MovablePointsKey = DependencyProperty.RegisterReadOnly(nameof(MovablePoints), typeof(ListObservable<PointControlValue>), typeof(PointControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty MovablePointsProperty = MovablePointsKey.DependencyProperty;
    public ListObservable<PointControlValue> MovablePoints
    {
        get => (ListObservable<PointControlValue>)GetValue(MovablePointsProperty);
        private set => SetValue(MovablePointsKey, value);
    }

    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(PointControl), new FrameworkPropertyMetadata(1d));
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    #endregion

    #region PointControl

    public PointControl() : base()
    {
        MovablePoints = [];
    }

    #endregion

    #region Methods

    private Point? addPoint;

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.RightButton == MouseButtonState.Pressed)
        {
            var point = e.GetPosition(this);
            addPoint = new(Math.Clamp(point.X / Zoom, 0, 1), Math.Clamp(point.Y / Zoom, 0, 1));
            return;
        }

        if (e.OriginalSource is System.Windows.Shapes.Ellipse ellipse)
        {
            target
                = ellipse;
            targetPoint
                = ellipse.DataContext as PointControlValue;

            target.CaptureMouse();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (targetPoint != null)
        {
            var point = e.GetPosition(this);
            targetPoint.X = Math.Clamp(point.X / Zoom, 0, 1); targetPoint.Y = Math.Clamp(point.Y / Zoom, 0, 1);
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        target?.ReleaseMouseCapture();
        target = null; targetPoint = null;
    }

    protected virtual void OnPointsChanged(Value<PointCollection> input)
    {
        Handle.DoInternal(() =>
        {
            MovablePoints.Clear();
            input.NewValue?.ForEach(i => MovablePoints.Add(new(this, i.X, i.Y)));
        });
    }

    internal void UpdateSource()
        => Handle.Do(() => SetCurrentValue(PointsProperty, new PointCollection(MovablePoints.Select(i => new Point(i.X, i.Y)))));

    #endregion

    #region Commands

    private ICommand addCommand;
    public ICommand AddCommand => addCommand ??= new RelayCommand(() =>
    {
        var result = new PointControlValue(this, addPoint.Value.X, addPoint.Value.Y);
        MovablePoints.Add(result);

        UpdateSource();
        addPoint = null;
    },
    () => addPoint != null);

    private ICommand insertAfterCommand;
    public ICommand InsertAfterCommand => insertAfterCommand ??= new RelayCommand<PointControlValue>(i =>
    {
        var result = new PointControlValue(this, 0, 0);

        var index = MovablePoints.IndexOf(i) + 1;
        if (index >= MovablePoints.Count)
            MovablePoints.Add(result);

        else MovablePoints.Insert(index, result);
        UpdateSource();
    }, i => i != null);

    private ICommand insertBeforeCommand;
    public ICommand InsertBeforeCommand => insertBeforeCommand ??= new RelayCommand<PointControlValue>(i =>
    {
        MovablePoints.Insert(MovablePoints.IndexOf(i), new(this, 0, 0));
        UpdateSource();

    }, i => i != null);

    private ICommand removeCommand;
    public ICommand RemoveCommand => removeCommand ??= new RelayCommand<PointControlValue>(i =>
    {
        MovablePoints.Remove(i);
        UpdateSource();
    }, i => i != null);

    #endregion
}