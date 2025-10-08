using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

[TemplateVisualState(Name = "Large",
    GroupName = "SizeStates")]
[TemplateVisualState(Name = "Small",
    GroupName = "SizeStates")]
[TemplateVisualState(Name = "Inactive",
    GroupName = "ActiveStates")]
[TemplateVisualState(Name = "Active",
    GroupName = "ActiveStates")]
public class ProgressCircle : Control
{
    private List<Action> deferred = [];

    ///

    public static readonly DependencyProperty BindableWidthProperty = DependencyProperty.Register("BindableWidth", typeof(double), typeof(ProgressCircle), new FrameworkPropertyMetadata(default(double), OnBindableWidthChanged));
    public double BindableWidth
    {
        get => (double)GetValue(BindableWidthProperty);
        private set => SetValue(BindableWidthProperty, value);
    }

    private static void OnBindableWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ProgressCircle ring)
        {
            var action = new Action(() =>
            {
                ring.SetEllipseDiameter
                    ((double)e.NewValue);
                ring.SetEllipseOffset
                    ((double)e.NewValue);
                ring.SetMaxSideLength
                    ((double)e.NewValue);
            });

            if (ring.deferred != null)
                ring.deferred.Add(action);

            else action();
        }
    }

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(ProgressCircle), new FrameworkPropertyMetadata(true, OnIsActiveChanged));
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnIsActiveChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ProgressCircle ring)
            ring.UpdateActiveState();
    }

    public static readonly DependencyProperty IsLargeProperty = DependencyProperty.Register("IsLarge", typeof(bool), typeof(ProgressCircle), new FrameworkPropertyMetadata(true, OnIsLargeChanged));
    public bool IsLarge
    {
        get => (bool)GetValue(IsLargeProperty);
        set => SetValue(IsLargeProperty, value);
    }

    private static void OnIsLargeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ProgressCircle ring)
            ring.UpdateLargeState();
    }

    public static readonly DependencyProperty EllipseDiameterProperty = DependencyProperty.Register("EllipseDiameter", typeof(double), typeof(ProgressCircle), new FrameworkPropertyMetadata(default(double)));
    public double EllipseDiameter
    {
        get => (double)GetValue(EllipseDiameterProperty);
        private set => SetValue(EllipseDiameterProperty, value);
    }

    public static readonly DependencyProperty EllipseOffsetProperty = DependencyProperty.Register("EllipseOffset", typeof(Thickness), typeof(ProgressCircle), new FrameworkPropertyMetadata(default(Thickness)));
    public Thickness EllipseOffset
    {
        get => (Thickness)GetValue(EllipseOffsetProperty);
        private set => SetValue(EllipseOffsetProperty, value);
    }

    public static readonly DependencyProperty EllipseDiameterScaleProperty = DependencyProperty.Register("EllipseDiameterScale", typeof(double), typeof(ProgressCircle), new FrameworkPropertyMetadata(1D));
    public double EllipseDiameterScale
    {
        get => (double)GetValue(EllipseDiameterScaleProperty);
        set => SetValue(EllipseDiameterScaleProperty, value);
    }

    public static readonly DependencyProperty MaxSideLengthProperty = DependencyProperty.Register("MaxSideLength", typeof(double), typeof(ProgressCircle), new FrameworkPropertyMetadata(default(double)));
    public double MaxSideLength
    {
        get => (double)GetValue(MaxSideLengthProperty);
        private set => SetValue(MaxSideLengthProperty, value);
    }

    ///

    static ProgressCircle()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressCircle), new FrameworkPropertyMetadata(typeof(ProgressCircle)));

        var OnVisibilityChanged = new PropertyChangedCallback((sender, e) =>
        {
            if (e.NewValue != e.OldValue)
            {
                var ring = (ProgressCircle)sender;
                //auto set IsActive to false if we're hiding it.
                if ((Visibility)e.NewValue != Visibility.Visible)
                {
                    //sets the value without overriding it's binding (if any).
                    ring.SetCurrentValue(ProgressCircle.IsActiveProperty, false);
                }
                else
                {
                    // #1105 don't forget to re-activate
                    ring.IsActive = true;
                }
            }
        });
        VisibilityProperty.OverrideMetadata(typeof(ProgressCircle), new FrameworkPropertyMetadata(OnVisibilityChanged));
    }

    public ProgressCircle()
    {
        SizeChanged += OnSizeChanged;
    }

    ///

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => BindableWidth = ActualWidth;

    ///

    private void SetMaxSideLength(double width)
        => SetCurrentValue(MaxSideLengthProperty, width <= 20 ? 20 : width);

    private void SetEllipseDiameter(double width)
        => SetCurrentValue(EllipseDiameterProperty, width / 8d * EllipseDiameterScale);

    private void SetEllipseOffset(double width)
        => SetCurrentValue(EllipseOffsetProperty, new Thickness(0, width / 2, 0, 0));

    ///

    private void UpdateLargeState()
    {
        Action action;

        if (IsLarge)
            action = () => VisualStateManager.GoToState(this, "Large", true);
        else
            action = () => VisualStateManager.GoToState(this, "Small", true);

        if (deferred != null)
            deferred.Add(action);

        else action();
    }

    private void UpdateActiveState()
    {
        Action action;

        if (IsActive)
            action = () => VisualStateManager.GoToState(this, "Active", true);
        else
            action = () => VisualStateManager.GoToState(this, "Inactive", true);

        if (deferred != null)
            deferred.Add(action);

        else
            action();
    }

    ///

    public override void OnApplyTemplate()
    {
        UpdateLargeState();
        UpdateActiveState();

        base.OnApplyTemplate();
        if (deferred != null)
        {
            foreach (var action in deferred)
                action();
        }
        deferred = null;
    }
}