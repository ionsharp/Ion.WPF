using Ion.Collect;
using Ion.Numeral;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

[TemplatePart(Name = nameof(PART_Grid), Type = typeof(Grid))]
public class PatternControl : Control
{
    private Grid PART_Grid;

    #region Properties

    private PatternControlLine currentLine;
    private Pattern currentPattern;
    private bool isDrawing;
    private readonly Handle handle = false;

    public event EventHandler<EventArgs> Drawn;

    public static readonly DependencyProperty CanvasLengthProperty = DependencyProperty.Register(nameof(CanvasLength), typeof(double), typeof(PatternControl), new FrameworkPropertyMetadata(255d, OnCanvasLengthChanged));
    public double CanvasLength
    {
        get => (double)GetValue(CanvasLengthProperty);
        set => SetValue(CanvasLengthProperty, value);
    }

    private static void OnCanvasLengthChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<PatternControl>().Refresh();

    public static readonly DependencyProperty ClosedLineStrokeProperty = DependencyProperty.Register(nameof(ClosedLineStroke), typeof(Brush), typeof(PatternControl), new FrameworkPropertyMetadata(Brushes.Green));
    public Brush ClosedLineStroke
    {
        get => (Brush)GetValue(ClosedLineStrokeProperty);
        set => SetValue(ClosedLineStrokeProperty, value);
    }

    public static readonly DependencyProperty DotBackgroundProperty = DependencyProperty.Register(nameof(DotBackground), typeof(Brush), typeof(PatternControl), new FrameworkPropertyMetadata(default(Brush)));
    public Brush DotBackground
    {
        get => (Brush)GetValue(DotBackgroundProperty);
        set => SetValue(DotBackgroundProperty, value);
    }

    public static readonly DependencyProperty DotBorderBrushProperty = DependencyProperty.Register(nameof(DotBorderBrush), typeof(Brush), typeof(PatternControl), new FrameworkPropertyMetadata(default(Brush)));
    public Brush DotBorderBrush
    {
        get => (Brush)GetValue(DotBorderBrushProperty);
        set => SetValue(DotBorderBrushProperty, value);
    }

    public static readonly DependencyProperty DotBorderThicknessProperty = DependencyProperty.Register(nameof(DotBorderThickness), typeof(Thickness), typeof(PatternControl), new FrameworkPropertyMetadata(default(Thickness)));
    public Thickness DotBorderThickness
    {
        get => (Thickness)GetValue(DotBorderThicknessProperty);
        set => SetValue(DotBorderThicknessProperty, value);
    }

    public static readonly DependencyProperty DotLengthProperty = DependencyProperty.Register(nameof(DotLength), typeof(double), typeof(PatternControl), new FrameworkPropertyMetadata(48d, OnDotLengthChanged));
    public double DotLength
    {
        get => (double)GetValue(DotLengthProperty);
        set => SetValue(DotLengthProperty, value);
    }

    private static void OnDotLengthChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<PatternControl>().Refresh();

    public static readonly DependencyProperty DotsProperty = DependencyProperty.Register(nameof(Dots), typeof(ListObservable<PatternControlDot>), typeof(PatternControl), new FrameworkPropertyMetadata(null));
    public ListObservable<PatternControlDot> Dots
    {
        get => (ListObservable<PatternControlDot>)GetValue(DotsProperty);
        private set => SetValue(DotsProperty, value);
    }

    public static readonly DependencyProperty InnerDotBackgroundProperty = DependencyProperty.Register(nameof(InnerDotBackground), typeof(Brush), typeof(PatternControl), new FrameworkPropertyMetadata(default(Brush)));
    public Brush InnerDotBackground
    {
        get => (Brush)GetValue(InnerDotBackgroundProperty);
        set => SetValue(InnerDotBackgroundProperty, value);
    }

    public static readonly DependencyProperty InnerDotConnectedBackgroundProperty = DependencyProperty.Register(nameof(InnerDotConnectedBackground), typeof(Brush), typeof(PatternControl), new FrameworkPropertyMetadata(default(Brush)));
    public Brush InnerDotConnectedBackground
    {
        get => (Brush)GetValue(InnerDotConnectedBackgroundProperty);
        set => SetValue(InnerDotConnectedBackgroundProperty, value);
    }

    public static readonly DependencyProperty InnerDotLengthProperty = DependencyProperty.Register(nameof(InnerDotLength), typeof(double), typeof(PatternControl), new FrameworkPropertyMetadata(28d, OnInnerDotLengthChanged));
    public double InnerDotLength
    {
        get => (double)GetValue(InnerDotLengthProperty);
        set => SetValue(InnerDotLengthProperty, value);
    }

    private static void OnInnerDotLengthChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<PatternControl>().Refresh();

    public static readonly DependencyProperty IsDrawingEnabledProperty = DependencyProperty.Register(nameof(IsDrawingEnabled), typeof(bool), typeof(PatternControl), new FrameworkPropertyMetadata(true));
    public bool IsDrawingEnabled
    {
        get => (bool)GetValue(IsDrawingEnabledProperty);
        set => SetValue(IsDrawingEnabledProperty, value);
    }

    public static readonly DependencyProperty LineStrokeProperty = DependencyProperty.Register(nameof(LineStroke), typeof(double), typeof(PatternControl), new FrameworkPropertyMetadata(10d));
    public double LineStroke
    {
        get => (double)GetValue(LineStrokeProperty);
        set => SetValue(LineStrokeProperty, value);
    }

    public static readonly DependencyProperty OpenLineStrokeProperty = DependencyProperty.Register(nameof(OpenLineStroke), typeof(Brush), typeof(PatternControl), new FrameworkPropertyMetadata(Brushes.LightGray));
    public Brush OpenLineStroke
    {
        get => (Brush)GetValue(OpenLineStrokeProperty);
        set => SetValue(OpenLineStrokeProperty, value);
    }

    public static readonly DependencyProperty LinesProperty = DependencyProperty.Register(nameof(Lines), typeof(ListObservable<PatternControlLine>), typeof(PatternControl), new FrameworkPropertyMetadata(null));
    public ListObservable<PatternControlLine> Lines
    {
        get => (ListObservable<PatternControlLine>)GetValue(LinesProperty);
        private set => SetValue(LinesProperty, value);
    }

    public static readonly DependencyProperty PatternProperty = DependencyProperty.Register(nameof(Pattern), typeof(Pattern), typeof(PatternControl), new FrameworkPropertyMetadata(null, OnPatternChanged));
    public Pattern Pattern
    {
        get => (Pattern)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    private static void OnPatternChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<PatternControl>().OnPatternChanged((Pattern)e.NewValue);

    #endregion

    #region PatternControl

    public PatternControl() : base()
    {
        //SetCurrentValue(PatternProperty, new());

        Dots
            = [];
        Lines
            = [];

        Refresh();
    }

    #endregion

    #region Methods

    private bool AlreadyConnected(Point point)
    {
        var result = false;
        foreach (var i in Lines)
        {
            if (i != currentLine)
            {
                if
                (
                    (i.X1 == point.X && i.Y1 == point.Y)
                    ||
                    (i.X2 == point.X && i.Y2 == point.Y)
                )
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    private void ConnectDot(Numeral.Line<int> point)
    {
        foreach (var i in Dots)
        {
            if
            (
                !i.IsConnected
                &&
                (
                    (i.Position.X == point.X1 && i.Position.Y == point.Y1)
                    ||
                    (i.Position.X == point.X2 && i.Position.Y == point.Y2)
                )
            )
            {
                i.IsConnected = true;
            }
        }
    }

    private static Point GetPoint(int length, Point point)
    {
        var lengthHalf = length / 2;

        var action = new Func<double, int>(value =>
        {
            if (value >= 0 && value < length)
            {
                return lengthHalf;
            }
            else if (value >= length && value < length * 2)
            {
                return length + lengthHalf;
            }
            else if (value >= length * 2 && value < length * 3)
                return (length * 2) + lengthHalf;

            return 0;
        });

        var x = action(point.X);
        var y = action(point.Y);
        return new Point(x, y);
    }

    private static Point? GetPoint(int length, int hotLength, Point point)
    {
        var lengthHalf = length / 2;

        var hotLengthStart = lengthHalf - (hotLength / 2);
        var hotLengthEnd = hotLengthStart + hotLength;

        var action = new Func<double, int?>(value =>
        {
            if (value >= hotLengthStart && value < hotLengthEnd)
            {
                return lengthHalf;
            }
            else if (value >= length + hotLengthStart && value < length + hotLengthEnd)
            {
                return length + lengthHalf;
            }
            else if (value >= length * 2 + hotLengthStart && value < length * 2 + hotLengthEnd)
                return (length * 2) + lengthHalf;

            return null;
        });

        var x = action(point.X);
        var y = action(point.Y);
        if (x is null || y is null)
            return null;

        return new Point(x.Value, y.Value);
    }

    private void Refresh()
    {
        Dots.Clear();
        var length = Convert.ToInt32(CanvasLength / 3);

        var x = length / 2;
        var y = length / 2;

        for (var i = 0; i < 9; i++)
        {
            Dots.Add(new PatternControlDot(new Point(x, y)));
            if (x == length * 2 + (length / 2))
            {
                x = length / 2;
                y += length;
            }
            else x += length;
        }
    }

    ///

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (IsDrawingEnabled)
        {
            var result = e.GetPosition(PART_Grid);
            OnDrawingStarted(result);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (e.LeftButton == MouseButtonState.Pressed && isDrawing)
        {
            var result = e.GetPosition(PART_Grid);
            OnDrawing(result);
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (isDrawing)
            OnDrawingEnded();
    }

    ///

    protected virtual void OnDrawing(Point MousePosition)
    {
        currentLine.X2 = MousePosition.X;
        currentLine.Y2 = MousePosition.Y;

        var targetPoint = GetPoint(Convert.ToInt32(CanvasLength / 3), Convert.ToInt32(InnerDotLength), MousePosition);
        if (targetPoint != null)
        {
            if (!AlreadyConnected(targetPoint.Value))
            {
                var linearPoint = new Line<int>(Convert.ToInt32(currentLine.X1), Convert.ToInt32(currentLine.Y1), Convert.ToInt32(targetPoint.Value.X), Convert.ToInt32(targetPoint.Value.Y));

                currentLine.X2 = linearPoint.X2;
                currentLine.Y2 = linearPoint.Y2;
                currentLine.IsOpen = false;

                ConnectDot(linearPoint);
                currentPattern = currentPattern.Insert(linearPoint);

                currentLine = new PatternControlLine(true, linearPoint.X2, linearPoint.Y2, linearPoint.X2, linearPoint.Y2);
                Lines.Add(currentLine);
            }
        }
    }

    protected virtual void OnDrawingEnded()
    {
        if (currentLine != null)
        {
            Lines.Remove(currentLine);
            currentLine = null;
        }
        if (currentPattern != default)
        {
            handle.Do(() =>
            {
                SetCurrentValue(PatternProperty, currentPattern);
                currentPattern = default;
            });
        }

        isDrawing = false;
        Drawn?.Invoke(this, new EventArgs());
    }

    protected virtual void OnDrawingStarted(Point MousePosition)
    {
        isDrawing = true;

        currentPattern = default;
        Reset();

        var point = GetPoint(Convert.ToInt32(CanvasLength / 3), MousePosition);

        currentLine = new PatternControlLine(true, point.X, point.Y);
        Lines.Add(currentLine);
    }

    ///

    protected virtual void OnPatternChanged(Pattern input)
    {
        handle.DoInternal((Action)(() =>
        {
            Reset();
            foreach (var j in input)
            {
                Lines.Add(new PatternControlLine(false, j));
                ConnectDot(j);
            }
        }));
    }

    ///

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_Grid = Template.FindName(nameof(PART_Grid), this) as Grid;
    }

    public void Reset()
    {
        Lines.Clear();
        Dots.ForEach(i => i.IsConnected = false);
    }

    #endregion
}