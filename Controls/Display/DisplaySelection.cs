using Ion.Input;
using Ion.Input.Global;
using Ion.Numeral;
using Ion.Numeral.Models;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public sealed class DisplaySelection : Display<ItemsControl>
{
    private ListenerMouse mouseListener;
    private ScrollViewer scrollViewer;

    ///

    private readonly MArea<double> Selection = new(0);
    private readonly List<Rect> selections = [];

    ///

    private bool isDragging;
    private Rect previousSelection;
    private Point startPoint;

    ///

    private double ScrollOffset
        => XItemsControl.GetDragScrollOffset(Control);

    private double ScrollOffsetMaximum
        => XItemsControl.GetDragScrollOffsetMaximum(Control);

    private double ScrollTolerance
        => XItemsControl.GetDragScrollTolerance(Control);

    private Select SelectionMode
    {
        get
        {
            bool result()
            {
                if (Control is ListBox a)
                    return a.SelectionMode == System.Windows.Controls.SelectionMode.Single;

                if (Control is DataGrid b)
                    return b.SelectionMode == DataGridSelectionMode.Single;

                if (Control is TreeView c)
                    return XTreeView.GetSelectionMode(c) == Controls.Select.One;

                return false;
            }
            return result() ? Controls.Select.One : Controls.Select.OneOrMore;
        }
    }

    ///

    public DisplaySelection() : base() => Content = Selection;

    ///

    protected override object OnBackgroundCoerced(object input) => input is Brush i ? i : Brushes.Transparent;

    ///

    protected override void OnLoaded()
    {
        base.OnLoaded();
        //If not present, assume abscence is intended!
        scrollViewer = this.GetParent<ScrollViewer>(); //throw new ParentNotFoundException<DisplaySelection, ScrollViewer>();

        if (XItemsControl.GetCanDragSelectGlobally(Control))
        {
            mouseListener = new(new Input.Global.WinApi.GlobalHooker());

            mouseListener.MouseDown
                += OnGlobalMouseDown;
            mouseListener.MouseMove
                += OnGlobalMouseMove;
            mouseListener.MouseUp
                += OnGlobalMouseUp;

            mouseListener.Start();
        }
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();
        if (mouseListener != null)
        {
            mouseListener.Stop();

            mouseListener.MouseDown
                -= OnGlobalMouseDown;
            mouseListener.MouseMove
                -= OnGlobalMouseMove;
            mouseListener.MouseUp
                -= OnGlobalMouseUp;

            mouseListener.Dispose();
            mouseListener = null;
        }
    }

    ///

    private bool IsItemHit(System.Windows.Forms.MouseEventArgs e)
    {
        foreach (var i in Control.Items)
        {
            var container = Control.GetContainer(i);
            if (container != null)
            {
                var j = new Rect(Canvas.GetLeft(container), Canvas.GetTop(container), container.ActualWidth, container.ActualHeight);
                if (j.Contains(new Point(e.Location.X, e.Location.Y)))
                    return true;
            }
        }
        return false;
    }

    ///

    private void OnGlobalMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (Control is null)
            return;

        if (!XItemsControl.GetCanDragSelectGlobally(Control))
            return;

        if (XItemsControl.GetSelectionGlobalPredicate(Control)?.Invoke() == false)
            return;

        if (!XItemsControl.GetCanDragSelect(Control))
            return;

        if (IsItemHit(e))
            return;

        if (e.Button == System.Windows.Forms.MouseButtons.Left && SelectionMode == Controls.Select.OneOrMore)
        {
            isDragging = true;

            Panel.SetZIndex(this, int.MaxValue);

            startPoint = new(e.Location.X, e.Location.Y);
            if (!ModifierKeys.Control.Pressed() && !ModifierKeys.Shift.Pressed())
            {
                selections.Clear();
                Control.TrySelectNone();
            }

            previousSelection = new Rect();
        }
    }

    private void OnGlobalMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (isDragging)
        {
            var selection = GetSelection(startPoint, new(e.Location.X, e.Location.Y));
            if (isDragging)
            {
                Selection.X = selection.X; Selection.Y = selection.Y;
                Selection.Height = selection.Height; Selection.Width = selection.Width;

                var tLeft
                    = new Point(Selection.TopLeft().X, Selection.TopLeft().Y);
                var bRight
                    = new Point(Selection.BottomRight().X, Selection.BottomRight().Y);

                Select(Control, new Rect(tLeft, bRight));
            }
        }
    }

    private void OnGlobalMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (Control is null || !XItemsControl.GetCanDragSelectGlobally(Control))
            return;

        if (isDragging)
        {
            var endPoint = new Point(e.Location.X, e.Location.Y);
            isDragging = false;

            Panel.SetZIndex(this, 0);

            if (!Try.Do(() => selections.Add(previousSelection)))
                Try.Do(() => selections.Clear());

            Selection.X = 0; Selection.Y = 0; Selection.Height = 0; Selection.Width = 0;
            startPoint = default;
        }
    }

    ///

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (!XItemsControl.GetCanDragSelect(Control))
            return;

        if (e.ChangedButton == MouseButton.Left && SelectionMode == Controls.Select.OneOrMore)
        {
            isDragging = true;

            Panel.SetZIndex(this, int.MaxValue);

            CaptureMouse();
            startPoint = e.GetPosition(this);
            if (!ModifierKeys.Control.Pressed() && !ModifierKeys.Shift.Pressed())
                selections.Clear();

            previousSelection = new Rect();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (isDragging)
        {
            var selection = GetSelection(startPoint, e.GetPosition(this));
            //If still dragging after determining selection
            if (isDragging)
            {
                //Update the visual selection
                Selection.X = selection.X; Selection.Y = selection.Y;
                Selection.Height = selection.Height; Selection.Width = selection.Width;

                var tLeft
                    = new Point(Selection.TopLeft().X, Selection.TopLeft().Y);
                var bRight
                    = new Point(Selection.BottomRight().X, Selection.BottomRight().Y);

                //Select the items that lie below it
                Select(Control, new Rect(tLeft, bRight));
                //Scroll as mouse moves
                Scroll(e.GetPosition(Control));
            }
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.LeftButton == MouseButtonState.Released && isDragging)
        {
            var endPoint = e.GetPosition(this);
            isDragging = false;

            Panel.SetZIndex(this, 0);

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            if (!Try.Do(() => selections.Add(previousSelection)))
                Try.Do(() => selections.Clear());

            Selection.X = 0; Selection.Y = 0; Selection.Height = 0; Selection.Width = 0;
            startPoint = default;
        }
    }

    ///

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ContentProperty)
        {
            if (Content?.Equals(Selection) == false)
                throw new PropertyNotMutable<DisplaySelection>(nameof(Content));
        }
    }

    ///

    private Rect GetItemBounds(FrameworkElement i)
    {
        var topLeft = i.TranslatePoint(new Point(0, 0), this);
        return new Rect(topLeft.X, topLeft.Y, i.ActualWidth, i.ActualHeight);
    }

    private Rect GetSelection(Point a, Point b)
    {
        b = new Point(Math.Clamp(b.X, 0, ActualWidth), Math.Clamp(b.Y, 0, ActualHeight));

        double
            x = a.X < b.X ? a.X : b.X,
            y = a.Y < b.Y ? a.Y : b.Y;

        //Now, the point is precisely what it should be
        var width
            = (a.X > b.X ? a.X - b.X : b.X - a.X).Abs();
        var height
            = (a.Y > b.Y ? a.Y - b.Y : b.Y - a.Y).Abs();

        return new Rect(new Point(x, y), new Size(width, height));
    }

    ///

    /// <summary>
    /// Scroll based on current position.
    /// </summary>
    /// <param name="point"></param>
    private void Scroll(Point point)
    {
        if (scrollViewer is null)
            return;

        double x = point.X, y = point.Y;

        //Up
        var y1 = ScrollTolerance;
        var y1i = y1 - y;
        y1i = y1i < 0 ? y1i : 0;
        y1i = ScrollOffset + y1i > ScrollOffsetMaximum ? ScrollOffsetMaximum : ScrollOffset + y1i;

        //Bottom
        var y0 = Control.ActualHeight - ScrollTolerance;
        var y0i = y - y0;
        y0i = y0i < 0 ? 0 : y0i;
        y0i = ScrollOffset + y0i > ScrollOffsetMaximum ? ScrollOffsetMaximum : ScrollOffset + y0i;

        //Right
        var x1 = Control.ActualWidth - ScrollTolerance;
        var x1i = x - x1;
        x1i = x1i < 0 ? 0 : x1i;
        x1i = ScrollOffset + x1i > ScrollOffsetMaximum ? ScrollOffsetMaximum : ScrollOffset + x1i;

        //Left
        var x0 = ScrollTolerance;
        var x0i = x0 - x;
        x0i = x0i < 0 ? 0 : x0i;
        x0i = ScrollOffset + x0i > ScrollOffsetMaximum ? ScrollOffsetMaximum : ScrollOffset + x0i;

        //Up
        if (y < y1)
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - y1i);

        //Bottom 
        else if (y > y0)
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + y0i);

        //Left  
        if (x < x0)
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - x0i);

        //Right
        else if (x > x1)
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + x1i);
    }

    ///

    /// <summary>
    /// Gets whether or not the given <see cref="Rect"/> intersects with any previous selection.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private bool? IntersectsWith(Rect input)
    {
        var j = 0;

        var result = false;
        Try.Do(() =>
        {
            foreach (var i in selections)
            {
                if (input.IntersectsWith(i))
                {
                    result = j % 2 == 0;
                    j++;
                }
            }
        }, e => j = 0);
        return j == 0 ? null : (bool?)result;
    }

    ///

    /// <summary>
    /// Select items in control based on given area.
    /// </summary>
    /// <param name="control"></param>
    /// <param name="area"></param>
    private void Select(ItemsControl control, Rect area)
    {
        foreach (var i in control.Items)
        {
            var item = i is FrameworkElement j ? j : control.ItemContainerGenerator.ContainerFromItem(i) as FrameworkElement;
            if (item is null || item.Visibility != Visibility.Visible)
                continue;

            var itemBounds = GetItemBounds(item);

            //Check if current (or previous) selection intersects with item bounds
            bool? intersectsWith = null;
            if (itemBounds.IntersectsWith(area))
                intersectsWith = true;

            else if (itemBounds.IntersectsWith(previousSelection))
                intersectsWith = false;

            bool? result;
            if ((ModifierKeys.Control.Pressed() || ModifierKeys.Shift.Pressed()))
            {
                //Check whether or not the current item intersects with any previous selection
                var intersectedWith = IntersectsWith(itemBounds);

                //If current item has never insected with a previous selection...
                if (intersectedWith is null)
                {
                    result = intersectsWith;
                }
                else
                {
                    result = intersectedWith.Value;
                    //If current item also intersects with current (or previous) selection, flip it once more
                    if (intersectsWith != null && intersectsWith.Value)
                        result = !result;
                }
            }
            else result = intersectsWith;

            //If we are allowed to make a selection, make it
            if (result != null)
                item.TrySelect(result.Value);

            //If TreeViewItem, repeat the above for each child
            if (item is TreeViewItem)
                Select(item as ItemsControl, area);
        }
        previousSelection = area;
    }
}