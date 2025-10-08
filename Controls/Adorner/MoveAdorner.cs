using Ion.Core;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Ion.Controls;

public class MoveAdorner : TransformAdorner
{
    #region Properties

    private Canvas canvas;

    ///

    private readonly Dictionary<FrameworkElement, Point> elementStart = [];

    ///

    private Point mouseStart;
    private Point mouseCurrent;

    ///

    private readonly Thumb Thumb;

    public double Snap => XElement.GetMoveSnap(Element);

    #endregion

    #region MoveAdorner

    public MoveAdorner(FrameworkElement element) : base(element)
    {
        BuildThumb(ref Thumb, Cursors.Hand, 10, 10);
    }

    #endregion

    #region Methods

    private void OnMoveStarted(object sender, DragStartedEventArgs e)
    {
        if (!CanHandle(sender as Thumb))
            return;

        canvas = Element.GetParent<Canvas>();
        if (canvas is null)
            return;

        foreach (FrameworkElement i in canvas.Children)
        {
            if (i.DataContext is ILock l && l.IsLocked)
                continue;

            if (i.DataContext is ISelect j && j.IsSelected)
                elementStart.Add(i, i.GetTopLeft());
        }

        mouseStart
            = System.Windows.Input.Mouse.GetPosition(canvas);
    }

    private void OnMoving(object sender, DragDeltaEventArgs e)
    {
        if (!CanHandle(sender as Thumb) || canvas is null)
            return;

        mouseCurrent
            = System.Windows.Input.Mouse.GetPosition(canvas);

        if (mouseCurrent - mouseStart is System.Windows.Vector mouseDifference)
        {
            foreach (FrameworkElement i in canvas.Children)
            {
                if (elementStart.ContainsKey(i))
                {
                    var result = mouseDifference.BoundSize(elementStart[i], new(0, 0), new Size(canvas.ActualWidth, canvas.ActualHeight), new Size(i.ActualWidth, i.ActualHeight), XElement.GetMoveSnap(i), !XElement.GetCanMoveOutside(i));
                    i.SetTopLeft(result.X, result.Y);
                }
            }
        }

        Element.InvalidateMeasure();
    }

    private void OnMoveStopped(object sender, DragCompletedEventArgs e)
    {
        canvas = null;
        elementStart.Clear();
    }

    ///

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        //Desired width/height
        var dW
            = AdornedElement.DesiredSize.Width;
        var dH
            = AdornedElement.DesiredSize.Height;

        //Adorner width/height
        var aW
            = DesiredSize.Width;
        var aH
            = DesiredSize.Height;

        Thumb.Arrange(new Rect((dW / 2) - (aW / 2), (dH / 2) - (aH / 2), aW, aH));
        return finalSize;
    }

    /// <inheritdoc/>
    protected override void BuildThumb(ref Thumb thumb, Cursor cursor, double height = DefaultThumbHeight, double width = DefaultThumbWidth, DependencyProperty property = null)
        => base.BuildThumb(ref thumb, cursor, height, width, XElement.MoveThumbStyleProperty);

    ///

    /// <inheritdoc/>
    public override void Subscribe()
    {
        base.Subscribe();
        Thumb.DragCompleted
            += OnMoveStopped;
        Thumb.DragDelta
            += OnMoving;
        Thumb.DragStarted
            += OnMoveStarted;
    }

    /// <inheritdoc/>
    public override void Unsubscribe()
    {
        base.Unsubscribe();
        Thumb.DragCompleted
            -= OnMoveStopped;
        Thumb.DragDelta
            -= OnMoving;
        Thumb.DragStarted
            -= OnMoveStarted;
    }

    #endregion
}