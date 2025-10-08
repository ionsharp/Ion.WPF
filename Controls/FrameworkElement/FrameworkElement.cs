using Ion;
using Ion.Collect;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using Ion.Imaging;
using Ion.Input;
using Ion.Numeral;
using Ion.Numeral.Models;
using Ion.Storage;
using Ion.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Ion.Controls;

[Extend<FrameworkElement>]
public static class XElement
{
    public static readonly ResourceKey FocusVisualStyleKey = new();

    /// <see cref="Region.Field"/>

    public const double DisabledOpacity = 0.6;

    /// <see cref="Region.Property"/>
    #region

    #region CanMove

    /// <summary>Gets or sets if children can be dragged. Supports <see cref="ILock"/>.</summary>
    public static readonly DependencyProperty CanMoveProperty = DependencyProperty.RegisterAttached("CanMove", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnCanMoveChanged));
    public static bool GetCanMove(FrameworkElement i) => (bool)i.GetValue(CanMoveProperty);
    public static void SetCanMove(FrameworkElement i, bool input) => i.SetValue(CanMoveProperty, input);

    private static void OnCanMoveChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, CanMoveProperty, CanMove_Loaded, CanMove_Unloaded);
    }

    ///

    private static void CanMove_Loaded(FrameworkElement i)
    {
        var result = i.AddAdorner<MoveAdorner>(() => new(i));
        SetMoveAdorner(i, result);
    }

    private static void CanMove_Unloaded(FrameworkElement i)
    {
        i.RemoveAdorners<MoveAdorner>();
        SetMoveAdorner(i, null);
    }

    #endregion

    #region CanMoveOutside
    /// <summary>Gets or sets if children can be dragged outside of the canvas.</summary>
    public static readonly DependencyProperty CanMoveOutsideProperty = DependencyProperty.RegisterAttached("CanMoveOutside", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static bool GetCanMoveOutside(FrameworkElement i) => (bool)i.GetValue(CanMoveOutsideProperty);
    public static void SetCanMoveOutside(FrameworkElement i, bool input) => i.SetValue(CanMoveOutsideProperty, input);

    #endregion

    #region CanResize

    public static readonly DependencyProperty CanResizeProperty = DependencyProperty.RegisterAttached("CanResize", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnCanResizeChanged));
    public static bool GetCanResize(FrameworkElement i) => (bool)i.GetValue(CanResizeProperty);
    public static void SetCanResize(FrameworkElement i, bool input) => i.SetValue(CanResizeProperty, input);

    private static void OnCanResizeChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, CanResizeProperty, CanResize_Loaded, CanResize_Unloaded);
    }

    private static void CanResize_Loaded(FrameworkElement i)
    {
        var result = i.AddAdorner<ResizeAdorner>(() => new(i));
        SetResizeAdorner(i, result);
    }

    private static void CanResize_Unloaded(FrameworkElement i)
    {
        i.RemoveAdorners<ResizeAdorner>();
        SetResizeAdorner(i, null);
    }

    #endregion

    #region CanRotate

    public static readonly DependencyProperty CanRotateProperty = DependencyProperty.RegisterAttached("CanRotate", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnCanRotateChanged));
    public static bool GetCanRotate(FrameworkElement i) => (bool)i.GetValue(CanRotateProperty);
    public static void SetCanRotate(FrameworkElement i, bool input) => i.SetValue(CanRotateProperty, input);

    private static void OnCanRotateChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, CanRotateProperty, CanRotate_Loaded, CanRotate_Unloaded);
    }

    private static void CanRotate_Loaded(FrameworkElement i)
    {
        var result = i.AddAdorner<RotateAdorner>(() => new(i));
        SetRotateAdorner(i, result);
    }

    private static void CanRotate_Unloaded(FrameworkElement i)
    {
        i.RemoveAdorners<RotateAdorner>();
        SetRotateAdorner(i, null);
    }

    #endregion

    #region CanSelect

    public static readonly DependencyProperty CanSelectProperty = DependencyProperty.RegisterAttached("CanSelect", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnCanSelectChanged));
    public static bool GetCanSelect(FrameworkElement i) => (bool)i.GetValue(CanSelectProperty);
    public static void SetCanSelect(FrameworkElement i, bool input) => i.SetValue(CanSelectProperty, input);

    private static void OnCanSelectChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.AddHandlerAttached((bool)e.NewValue, CanSelectProperty, i =>
            {
                i.MouseDown
                    += CanSelect_MouseDown;
                i.MouseMove
                    += CanSelect_MouseMove;
                i.MouseUp
                    += CanSelect_MouseUp;

                SetSelectionAdorner(i, i.AddAdorner<SelectionAdorner>(() => new(i)));
            }, i =>
            {
                i.MouseDown
                    -= CanSelect_MouseDown;
                i.MouseMove
                    -= CanSelect_MouseMove;
                i.MouseUp
                    -= CanSelect_MouseUp;

                i.RemoveAdorners<SelectionAdorner>();
                SetSelectionAdorner(i, null);
            });
        }
    }

    ///

    private static void CanSelect_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.ChangedButton == GetSelectionButton(element))
            {
                element.CaptureMouse();

                var start = e.GetPosition(element);
                SetSelectionStart(element, start);

                var selection = GetSelection(element);
                selection.X = start.X; selection.Y = start.Y;
                selection.Height = selection.Width = 0;
            }
        }
    }

    private static void CanSelect_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (GetSelectionStart(element) != null)
            {
                var result = CalculateSelection(GetSelectionStart(element).Value, e.GetPosition(element));

                var selection
                    = GetSelection(element);
                selection.X
                    = result.X;
                selection.Y
                    = result.Y;
                selection.Height
                    = result.Height;
                selection.Width
                    = result.Width;
            }
        }
    }

    private static void CanSelect_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            SetSelectionStart(element, null);

            if (element.IsMouseCaptured)
                element.ReleaseMouseCapture();

            var selection = GetSelection(element);

            OnSelected(element, selection);
            GetSelectedCommand(element)?.Execute(selection);

            if (GetSelectionResets(element))
                selection.X = selection.Y = selection.Height = selection.Width = 0;
        }
    }

    ///

    private static Rect CalculateSelection(Point a, Point b)
    {
        Rect result = new();
        double
            x = a.X < b.X ? a.X : b.X,
            y = a.Y < b.Y ? a.Y : b.Y;

        result.Size = new Size(Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
        result.X = x;
        result.Y = y;
        return result;
    }

    ///

    private static void OnSelected(FrameworkElement element, MArea<double> selection)
        => element.RaiseEvent(new RoutedEventArgs<MArea<double>>(SelectedEvent, selection, element));

    #endregion

    #region Cursor

    public static readonly DependencyProperty CursorProperty = DependencyProperty.RegisterAttached("Cursor", typeof(Uri), typeof(XElement), new FrameworkPropertyMetadata(null, OnCursorChanged));
    public static Uri GetCursor(FrameworkElement i) => (Uri)i.GetValue(CursorProperty);
    public static void SetCursor(FrameworkElement i, Uri input) => i.SetValue(CursorProperty, input);

    private static void OnCursorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.NewValue is Uri uri)
            {
                var bitmap = XBitmap.Convert(new ImageSourceConverter().ConvertFromString(uri.OriginalString).As<ImageSource>(), BitmapEncoders.PNG);
                element.Cursor = XCursor.Convert(bitmap, bitmap.Width / 2, bitmap.Height / 2).Convert();
            }
        }
    }

    #endregion

    #region CursorBitmap

    public static readonly DependencyProperty CursorBitmapProperty = DependencyProperty.RegisterAttached("CursorBitmap", typeof(WriteableBitmap), typeof(XElement), new FrameworkPropertyMetadata(null, OnCursorBitmapChanged));
    public static WriteableBitmap GetCursorBitmap(FrameworkElement i) => (WriteableBitmap)i.GetValue(CursorBitmapProperty);
    public static void SetCursorBitmap(FrameworkElement i, WriteableBitmap input) => i.SetValue(CursorBitmapProperty, input);

    private static void OnCursorBitmapChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.NewValue is WriteableBitmap bitmap)
                element.Cursor = XCursor.Convert(XBitmap.Convert(bitmap, BitmapEncoders.PNG), bitmap.PixelWidth / 2, bitmap.PixelHeight / 2).Convert();
        }
    }

    #endregion

    #region DragMoveWindow

    public static readonly DependencyProperty DragMoveWindowProperty = DependencyProperty.RegisterAttached("DragMoveWindow", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnDragMoveWindowChanged));
    public static bool GetDragMoveWindow(FrameworkElement i) => (bool)i.GetValue(DragMoveWindowProperty);
    public static void SetDragMoveWindow(FrameworkElement i, bool input) => i.SetValue(DragMoveWindowProperty, input);

    private static void OnDragMoveWindowChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, DragMoveWindowProperty, i => i.MouseDown += DragMoveWindow_MouseDown, i => i.MouseDown -= DragMoveWindow_MouseDown);
    }

    private static void DragMoveWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                element.GetParent<Window>()?.DragMove();
        }
    }

    #endregion

    #region FadeIn

    public static readonly DependencyProperty FadeInProperty = DependencyProperty.RegisterAttached("FadeIn", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnFadeInChanged));
    public static bool GetFadeIn(FrameworkElement i) => (bool)i.GetValue(FadeInProperty);
    public static void SetFadeIn(FrameworkElement i, bool value) => i.SetValue(FadeInProperty, value);

    private static void OnFadeInChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.SetCurrentValue(UIElement.OpacityProperty, (bool)e.NewValue ? 0d : 1d);
            element.AddHandlerAttached((bool)e.NewValue, FadeInProperty, i =>
            {
                var duration = GetFadeInDuration(i);
                _ = i.FadeIn(duration == default ? new Duration(TimeSpan.FromSeconds(0.5)) : duration);
            }, null);
        }
    }

    #endregion

    #region FadeInDuration

    public static readonly DependencyProperty FadeInDurationProperty = DependencyProperty.RegisterAttached("FadeInDuration", typeof(Duration), typeof(XElement), new FrameworkPropertyMetadata(default(Duration)));
    public static Duration GetFadeInDuration(FrameworkElement i) => (Duration)i.GetValue(FadeInDurationProperty);
    public static void SetFadeInDuration(FrameworkElement i, Duration input) => i.SetValue(FadeInDurationProperty, input);

    #endregion

    #region FadeOut

    public static readonly DependencyProperty FadeOutProperty = DependencyProperty.RegisterAttached("FadeOut", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnFadeOutChanged));
    public static bool GetFadeOut(FrameworkElement i) => (bool)i.GetValue(FadeOutProperty);
    public static void SetFadeOut(FrameworkElement i, bool value) => i.SetValue(FadeOutProperty, value);

    private static void OnFadeOutChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.AddHandlerAttached((bool)e.NewValue, FadeOutProperty, null, i =>
            {
                var duration = GetFadeOutDuration(i);
                _ = i.FadeOut(duration == default ? new Duration(TimeSpan.FromSeconds(0.5)) : duration);
            });
        }
    }

    #endregion

    #region FadeOutDuration

    public static readonly DependencyProperty FadeOutDurationProperty = DependencyProperty.RegisterAttached("FadeOutDuration", typeof(Duration), typeof(XElement), new FrameworkPropertyMetadata(default(Duration)));
    public static Duration GetFadeOutDuration(FrameworkElement i) => (Duration)i.GetValue(FadeOutDurationProperty);
    public static void SetFadeOutDuration(FrameworkElement i, Duration input) => i.SetValue(FadeOutDurationProperty, input);

    #endregion

    #region FadeTrigger

    private static void FadeIn(FrameworkElement input)
    {
        var animation = new DoubleAnimation()
        {
            Duration
                = GetFadeTriggerDuration(input),
            From
                = input.Opacity,
            To
                = 1,
        };
        Storyboard.SetTarget(animation, input);
        Storyboard.SetTargetProperty(animation, new PropertyPath(nameof(UIElement.Opacity)));

        input.IsHitTestVisible = true;

        var result = new Storyboard();
        result.Children.Add(animation);
        result.Begin(input, HandoffBehavior.SnapshotAndReplace);
    }

    private static void FadeOut(FrameworkElement input)
    {
        var animation = new DoubleAnimation()
        {
            Duration
                = GetFadeTriggerDuration(input),
            From
                = input.Opacity,
            To
                = 0
        };
        Storyboard.SetTarget(animation, input);
        Storyboard.SetTargetProperty(animation, new PropertyPath(nameof(UIElement.Opacity)));

        input.IsHitTestVisible = false;

        var result = new Storyboard();
        result.Children.Add(animation);
        result.Begin(input, HandoffBehavior.SnapshotAndReplace);
    }

    public static readonly DependencyProperty FadeTriggerProperty = DependencyProperty.RegisterAttached("FadeTrigger", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnFadeTriggerChanged));
    public static bool GetFadeTrigger(FrameworkElement i) => (bool)i.GetValue(FadeTriggerProperty);
    public static void SetFadeTrigger(FrameworkElement i, bool value) => i.SetValue(FadeTriggerProperty, value);

    private static void OnFadeTriggerChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if ((bool)e.NewValue)
            {
                if (GetFadeTriggerSource(element))
                    FadeIn(element);

                else FadeOut(element);
            }
        }
    }

    #endregion

    #region FadeTriggerDuration

    public static readonly DependencyProperty FadeTriggerDurationProperty = DependencyProperty.RegisterAttached("FadeTriggerDuration", typeof(TimeSpan), typeof(XElement), new FrameworkPropertyMetadata(0.8.Seconds()));
    public static TimeSpan GetFadeTriggerDuration(FrameworkElement i) => (TimeSpan)i.GetValue(FadeTriggerDurationProperty);
    public static void SetFadeTriggerDuration(FrameworkElement i, TimeSpan value) => i.SetValue(FadeTriggerDurationProperty, value);

    #endregion

    #region FadeTriggerSource

    public static readonly DependencyProperty FadeTriggerSourceProperty = DependencyProperty.RegisterAttached("FadeTriggerSource", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnFadeTriggerSourceChanged));
    public static bool GetFadeTriggerSource(FrameworkElement i) => (bool)i.GetValue(FadeTriggerSourceProperty);
    public static void SetFadeTriggerSource(FrameworkElement i, bool value) => i.SetValue(FadeTriggerSourceProperty, value);

    private static void OnFadeTriggerSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (GetFadeTrigger(element))
            {
                if ((bool)e.NewValue)
                    FadeIn(element);

                else FadeOut(element);
            }
        }
    }

    #endregion

    #region Handler

    public static readonly DependencyProperty HandlerProperty = DependencyProperty.RegisterAttached("Handler", typeof(FrameworkElementHandler), typeof(XElement), new FrameworkPropertyMetadata(null));
    private static FrameworkElementHandler GetHandler(this FrameworkElement i) => i.GetValueOrSetDefault<FrameworkElementHandler>(HandlerProperty, () => new(i, null));

    #endregion

    #region InheritChild

    public static readonly DependencyProperty InheritChildProperty = DependencyProperty.RegisterAttached("InheritChild", typeof(Type), typeof(XElement), new FrameworkPropertyMetadata(null, OnInheritChildChanged));
    public static Type GetInheritChild(FrameworkElement i) => (Type)i.GetValue(InheritChildProperty);
    public static void SetInheritChild(FrameworkElement i, Type value) => i.SetValue(InheritChildProperty, value);

    private static void OnInheritChildChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, InheritChildProperty, InheritChild_Loaded, InheritChild_Unloaded);
    }

    private static void InheritChild_Loaded(FrameworkElement a)
    {
        var property = GetInheritChildProperty(a);
        if (property != null)
        {
            var b = Try.Do(() => a.GetChildOfType(GetInheritChild(a)));
            if (b != null)
                a.Bind(property, property.Name, b);
        }
    }

    private static void InheritChild_Unloaded(FrameworkElement input)
        => GetInheritChildProperty(input).IfNotNull(i => input.Unbind(i));

    #endregion

    #region InheritChildProperty

    public static readonly DependencyProperty InheritChildPropertyProperty = DependencyProperty.RegisterAttached("InheritChildProperty", typeof(DependencyProperty), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static DependencyProperty GetInheritChildProperty(FrameworkElement i) => (DependencyProperty)i.GetValue(InheritChildPropertyProperty);
    public static void SetInheritChildProperty(FrameworkElement i, DependencyProperty value) => i.SetValue(InheritChildPropertyProperty, value);

    #endregion

    #region (ReadOnly) IsLoaded

    private static readonly DependencyPropertyKey IsLoadedKey = DependencyProperty.RegisterAttachedReadOnly("IsLoaded", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsLoadedProperty = IsLoadedKey.DependencyProperty;
    public static bool GetIsLoaded(FrameworkElement i) => (bool)i.GetValue(IsLoadedProperty);
    internal static void SetIsLoaded(this FrameworkElement i, bool input) => i.SetValue(IsLoadedKey, input);

    #endregion

    #region IsMouseDown

    private static readonly DependencyPropertyKey IsMouseDownKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseDown", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsMouseDownProperty = IsMouseDownKey.DependencyProperty;
    public static bool GetIsMouseDown(FrameworkElement i) => (bool)i.GetValue(IsMouseDownProperty);
    private static void SetIsMouseDown(this FrameworkElement i, bool input) => i.SetValue(IsMouseDownKey, input);

    #endregion

    #region IsMouseLeftButtonDown

    private static readonly DependencyPropertyKey IsMouseLeftButtonDownKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseLeftButtonDown", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsMouseLeftButtonDownProperty = IsMouseLeftButtonDownKey.DependencyProperty;
    public static bool GetIsMouseLeftButtonDown(FrameworkElement i) => (bool)i.GetValue(IsMouseLeftButtonDownProperty);
    private static void SetIsMouseLeftButtonDown(this FrameworkElement i, bool input) => i.SetValue(IsMouseLeftButtonDownKey, input);

    #endregion

    #region IsMouseMiddleButtonDown

    private static readonly DependencyPropertyKey IsMouseMiddleButtonDownKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseMiddleButtonDown", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsMouseMiddleButtonDownProperty = IsMouseMiddleButtonDownKey.DependencyProperty;
    public static bool GetIsMouseMiddleButtonDown(FrameworkElement i) => (bool)i.GetValue(IsMouseMiddleButtonDownProperty);
    private static void SetIsMouseMiddleButtonDown(this FrameworkElement i, bool input) => i.SetValue(IsMouseMiddleButtonDownKey, input);

    #endregion

    #region IsMouseRightButtonDown

    private static readonly DependencyPropertyKey IsMouseRightButtonDownKey = DependencyProperty.RegisterAttachedReadOnly("IsMouseRightButtonDown", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsMouseRightButtonDownProperty = IsMouseRightButtonDownKey.DependencyProperty;
    public static bool GetIsMouseRightButtonDown(FrameworkElement i) => (bool)i.GetValue(IsMouseRightButtonDownProperty);
    private static void SetIsMouseRightButtonDown(this FrameworkElement i, bool input) => i.SetValue(IsMouseRightButtonDownKey, input);

    #endregion

    #region LostFocusCommand

    public static readonly DependencyProperty LostFocusCommandProperty = DependencyProperty.RegisterAttached("LostFocusCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null, OnLostFocusCommandChanged));
    public static ICommand GetLostFocusCommand(FrameworkElement i) => (ICommand)i.GetValue(LostFocusCommandProperty);
    public static void SetLostFocusCommand(FrameworkElement i, ICommand input) => i.SetValue(LostFocusCommandProperty, input);

    private static void OnLostFocusCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, LostFocusCommandProperty, i => i.LostFocus += LostFocusCommand_LostFocus, i => i.LostFocus -= LostFocusCommand_LostFocus);
    }

    private static void LostFocusCommand_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
            GetLostFocusCommand(element).Execute(GetLostFocusCommandParameter(element));
    }

    #endregion

    #region LostFocusCommandParameter

    public static readonly DependencyProperty LostFocusCommandParameterProperty = DependencyProperty.RegisterAttached("LostFocusCommandParameter", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static object GetLostFocusCommandParameter(FrameworkElement i) => i.GetValue(LostFocusCommandParameterProperty);
    public static void SetLostFocusCommandParameter(FrameworkElement i, object input) => i.SetValue(LostFocusCommandParameterProperty, input);

    #endregion

    ///

    #region (private) MoveAdorner

    private static readonly DependencyProperty MoveAdornerProperty = DependencyProperty.RegisterAttached("MoveAdorner", typeof(MoveAdorner), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static void SetMoveAdorner(FrameworkElement i, MoveAdorner input) => i.SetValue(MoveAdornerProperty, input);

    #endregion

    #region MoveButton

    public static readonly DependencyProperty MoveButtonProperty = DependencyProperty.RegisterAttached("MoveButton", typeof(MouseButton), typeof(XElement), new FrameworkPropertyMetadata(MouseButton.Left));
    public static MouseButton GetMoveButton(FrameworkElement i) => (MouseButton)i.GetValue(MoveButtonProperty);
    public static void SetMoveButton(FrameworkElement i, MouseButton input) => i.SetValue(MoveButtonProperty, input);

    #endregion

    #region (private) MoveOrigin

    private static readonly DependencyProperty MoveOriginProperty = DependencyProperty.RegisterAttached("MoveOrigin", typeof(Point?), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static Point? GetMoveOrigin(FrameworkElement i) => (Point?)i.GetValue(MoveOriginProperty);
    private static void SetMoveOrigin(FrameworkElement i, Point? input) => i.SetValue(MoveOriginProperty, input);

    #endregion

    #region MoveSnap

    public static readonly DependencyProperty MoveSnapProperty = DependencyProperty.RegisterAttached("MoveSnap", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(1.0));
    public static double GetMoveSnap(FrameworkElement i) => (double)i.GetValue(MoveSnapProperty);
    public static void SetMoveSnap(FrameworkElement i, double input) => i.SetValue(MoveSnapProperty, input);

    #endregion

    #region (private) MoveStart

    private static readonly DependencyProperty MoveStartProperty = DependencyProperty.RegisterAttached("MoveStart", typeof(Point?), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static Point? GetMoveStart(Canvas i) => (Point?)i.GetValue(MoveStartProperty);
    private static void SetMoveStart(Canvas i, Point? input) => i.SetValue(MoveStartProperty, input);

    #endregion

    #region MoveThumbStyle

    public static readonly DependencyProperty MoveThumbStyleProperty = DependencyProperty.RegisterAttached("MoveThumbStyle", typeof(Style), typeof(XElement), new FrameworkPropertyMetadata(default(Style)));
    public static Style GetMoveThumbStyle(FrameworkElement i) => (Style)i.GetValue(MoveThumbStyleProperty);
    public static void SetMoveThumbStyle(FrameworkElement i, Style input) => i.SetValue(MoveThumbStyleProperty, input);

    #endregion

    ///

    #region MouseDownCommand

    public static readonly DependencyProperty MouseDownCommandProperty = DependencyProperty.RegisterAttached("MouseDownCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null, OnMouseDownCommandChanged));
    public static ICommand GetMouseDownCommand(FrameworkElement i) => (ICommand)i.GetValue(MouseDownCommandProperty);
    public static void SetMouseDownCommand(FrameworkElement i, ICommand input) => i.SetValue(MouseDownCommandProperty, input);

    private static void OnMouseDownCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, MouseDownCommandProperty, i => i.MouseDown += MouseDownCommand_MouseDown, i => i.MouseDown -= MouseDownCommand_MouseDown);
    }

    private static void MouseDownCommand_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.ChangedButton == GetMouseDownCommandButton(element))
                GetMouseDownCommand(element).Execute(GetMouseDownCommandParameter(element));
        }
    }

    #endregion

    #region MouseDownCommandButton

    public static readonly DependencyProperty MouseDownCommandButtonProperty = DependencyProperty.RegisterAttached("MouseDownCommandButton", typeof(MouseButton), typeof(XElement), new FrameworkPropertyMetadata(MouseButton.Left));
    public static MouseButton GetMouseDownCommandButton(FrameworkElement i) => (MouseButton)i.GetValue(MouseDownCommandButtonProperty);
    public static void SetMouseDownCommandButton(FrameworkElement i, MouseButton input) => i.SetValue(MouseDownCommandButtonProperty, input);

    #endregion

    #region MouseDownCommandParameter

    public static readonly DependencyProperty MouseDownCommandParameterProperty = DependencyProperty.RegisterAttached("MouseDownCommandParameter", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static object GetMouseDownCommandParameter(FrameworkElement i) => i.GetValue(MouseDownCommandParameterProperty);
    public static void SetMouseDownCommandParameter(FrameworkElement i, object input) => i.SetValue(MouseDownCommandParameterProperty, input);

    #endregion

    ///

    #region MouseUpCommand

    public static readonly DependencyProperty MouseUpCommandProperty = DependencyProperty.RegisterAttached("MouseUpCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null, OnMouseUpCommandChanged));
    public static ICommand GetMouseUpCommand(FrameworkElement i) => (ICommand)i.GetValue(MouseUpCommandProperty);
    public static void SetMouseUpCommand(FrameworkElement i, ICommand input) => i.SetValue(MouseUpCommandProperty, input);

    private static void OnMouseUpCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, MouseUpCommandProperty, i => i.MouseUp += MouseUpCommand_MouseUp, i => i.MouseUp -= MouseUpCommand_MouseUp);
    }

    private static void MouseUpCommand_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.ChangedButton == GetMouseUpCommandButton(element))
                GetMouseUpCommand(element).Execute(GetMouseUpCommandParameter(element));
        }
    }

    #endregion

    #region MouseUpCommandButton

    public static readonly DependencyProperty MouseUpCommandButtonProperty = DependencyProperty.RegisterAttached("MouseUpCommandButton", typeof(MouseButton), typeof(XElement), new FrameworkPropertyMetadata(MouseButton.Left));
    public static MouseButton GetMouseUpCommandButton(FrameworkElement i) => (MouseButton)i.GetValue(MouseUpCommandButtonProperty);
    public static void SetMouseUpCommandButton(FrameworkElement i, MouseButton input) => i.SetValue(MouseUpCommandButtonProperty, input);

    #endregion

    #region MouseUpCommandParameter

    public static readonly DependencyProperty MouseUpCommandParameterProperty = DependencyProperty.RegisterAttached("MouseUpCommandParameter", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static object GetMouseUpCommandParameter(FrameworkElement i) => i.GetValue(MouseUpCommandParameterProperty);
    public static void SetMouseUpCommandParameter(FrameworkElement i, object input) => i.SetValue(MouseUpCommandParameterProperty, input);

    #endregion

    ///

    #region MouseEnterCommand

    public static readonly DependencyProperty MouseEnterCommandProperty = DependencyProperty.RegisterAttached("MouseEnterCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null, OnMouseEnterCommandChanged));
    public static ICommand GetMouseEnterCommand(FrameworkElement i) => (ICommand)i.GetValue(MouseEnterCommandProperty);
    public static void SetMouseEnterCommand(FrameworkElement i, ICommand input) => i.SetValue(MouseEnterCommandProperty, input);

    private static void OnMouseEnterCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, MouseEnterCommandProperty, i => i.MouseEnter += MouseEnterCommand_MouseEnter, i => i.MouseEnter -= MouseEnterCommand_MouseEnter);
    }

    private static void MouseEnterCommand_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
            GetMouseEnterCommand(element).Execute(GetMouseEnterCommandParameter(element));
    }

    #endregion

    #region MouseEnterCommandParameter

    public static readonly DependencyProperty MouseEnterCommandParameterProperty = DependencyProperty.RegisterAttached("MouseEnterCommandParameter", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static object GetMouseEnterCommandParameter(FrameworkElement i) => i.GetValue(MouseEnterCommandParameterProperty);
    public static void SetMouseEnterCommandParameter(FrameworkElement i, object input) => i.SetValue(MouseEnterCommandParameterProperty, input);

    #endregion

    #region MouseLeaveCommand

    public static readonly DependencyProperty MouseLeaveCommandProperty = DependencyProperty.RegisterAttached("MouseLeaveCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null, OnMouseLeaveCommandChanged));
    public static ICommand GetMouseLeaveCommand(FrameworkElement i) => (ICommand)i.GetValue(MouseLeaveCommandProperty);
    public static void SetMouseLeaveCommand(FrameworkElement i, ICommand input) => i.SetValue(MouseLeaveCommandProperty, input);

    private static void OnMouseLeaveCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, MouseLeaveCommandProperty, i => i.MouseLeave += MouseLeaveCommand_MouseLeave, i => i.MouseLeave -= MouseLeaveCommand_MouseLeave);
    }

    private static void MouseLeaveCommand_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
            GetMouseLeaveCommand(element).Execute(GetMouseLeaveCommandParameter(element));
    }

    #endregion

    #region MouseLeaveCommandParameter

    public static readonly DependencyProperty MouseLeaveCommandParameterProperty = DependencyProperty.RegisterAttached("MouseLeaveCommandParameter", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static object GetMouseLeaveCommandParameter(FrameworkElement i) => i.GetValue(MouseLeaveCommandParameterProperty);
    public static void SetMouseLeaveCommandParameter(FrameworkElement i, object input) => i.SetValue(MouseLeaveCommandParameterProperty, input);

    #endregion

    #region Name

    public static readonly DependencyProperty NameProperty = DependencyProperty.RegisterAttached("Name", typeof(IFrameworkElementKey), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static IFrameworkElementKey GetName(FrameworkElement i) => (IFrameworkElementKey)i.GetValue(NameProperty);
    public static void SetName(FrameworkElement i, IFrameworkElementKey input) => i.SetValue(NameProperty, input);

    #endregion

    #region OverrideHorizontalAlignment

    public static readonly DependencyProperty OverrideHorizontalAlignmentProperty = DependencyProperty.RegisterAttached("OverrideHorizontalAlignment", typeof(HorizontalAlignment?), typeof(XElement), new FrameworkPropertyMetadata(null, OnOverrideHorizontalAlignmentChanged));
    public static HorizontalAlignment? GetOverrideHorizontalAlignment(FrameworkElement i) => (HorizontalAlignment?)i.GetValue(OverrideHorizontalAlignmentProperty);
    public static void SetOverrideHorizontalAlignment(FrameworkElement i, HorizontalAlignment? input) => i.SetValue(OverrideHorizontalAlignmentProperty, input);

    private static void OnOverrideHorizontalAlignmentChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.NewValue is HorizontalAlignment alignment)
                element.HorizontalAlignment = alignment;
        }
    }

    #endregion

    #region OverrideMargin

    public static readonly DependencyProperty OverrideMarginProperty = DependencyProperty.RegisterAttached("OverrideMargin", typeof(Thickness?), typeof(XElement), new FrameworkPropertyMetadata(null, OnOverrideMarginChanged));
    public static Thickness? GetOverrideMargin(FrameworkElement i) => (Thickness?)i.GetValue(OverrideMarginProperty);
    public static void SetOverrideMargin(FrameworkElement i, Thickness? value) => i.SetValue(OverrideMarginProperty, value);

    private static void OnOverrideMarginChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.NewValue is Thickness thickness)
                element.Margin = thickness;
        }
    }

    #endregion

    #region OverrideVerticalAlignment

    public static readonly DependencyProperty OverrideVerticalAlignmentProperty = DependencyProperty.RegisterAttached("OverrideVerticalAlignment", typeof(VerticalAlignment?), typeof(XElement), new FrameworkPropertyMetadata(null, OnOverrideVerticalAlignmentChanged));
    public static VerticalAlignment? GetOverrideVerticalAlignment(FrameworkElement i) => (VerticalAlignment?)i.GetValue(OverrideVerticalAlignmentProperty);
    public static void SetOverrideVerticalAlignment(FrameworkElement i, VerticalAlignment? input) => i.SetValue(OverrideVerticalAlignmentProperty, input);

    private static void OnOverrideVerticalAlignmentChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.NewValue is VerticalAlignment alignment)
                element.VerticalAlignment = alignment;
        }
    }

    #endregion

    #region PreviewMouseLeftButtonUpCommand

    public static readonly DependencyProperty PreviewMouseLeftButtonUpCommandProperty = DependencyProperty.RegisterAttached("PreviewMouseLeftButtonUpCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null, OnPreviewMouseLeftButtonUpCommandChanged));
    public static ICommand GetPreviewMouseLeftButtonUpCommand(FrameworkElement i) => (ICommand)i.GetValue(PreviewMouseLeftButtonUpCommandProperty);
    public static void SetPreviewMouseLeftButtonUpCommand(FrameworkElement i, ICommand input) => i.SetValue(PreviewMouseLeftButtonUpCommandProperty, input);

    private static void OnPreviewMouseLeftButtonUpCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue != null, PreviewMouseLeftButtonUpCommandProperty, i => i.PreviewMouseLeftButtonUp += PreviewMouseLeftButtonUpCommand_PreviewMouseLeftButtonUp, i => i.PreviewMouseLeftButtonUp -= PreviewMouseLeftButtonUpCommand_PreviewMouseLeftButtonUp);
    }

    private static void PreviewMouseLeftButtonUpCommand_PreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
            GetPreviewMouseLeftButtonUpCommand(element).Execute(GetPreviewMouseLeftButtonUpCommandParameter(element));
    }

    #endregion

    #region PreviewMouseLeftButtonUpCommandParameter

    public static readonly DependencyProperty PreviewMouseLeftButtonUpCommandParameterProperty = DependencyProperty.RegisterAttached("PreviewMouseLeftButtonUpCommandParameter", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static object GetPreviewMouseLeftButtonUpCommandParameter(FrameworkElement i) => i.GetValue(PreviewMouseLeftButtonUpCommandParameterProperty);
    public static void SetPreviewMouseLeftButtonUpCommandParameter(FrameworkElement i, object input) => i.SetValue(PreviewMouseLeftButtonUpCommandParameterProperty, input);

    #endregion

    ///

    #region Reference

    public static readonly DependencyProperty ReferenceProperty = DependencyProperty.RegisterAttached("Reference", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null, OnReferenceChanged));
    public static object GetReference(FrameworkElement i) => i.GetValue(ReferenceProperty);
    public static void SetReference(FrameworkElement i, object input) => i.SetValue(ReferenceProperty, input);

    private static void OnReferenceChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateReference(element);
    }

    private static void UpdateReference(FrameworkElement element)
    {
        if (GetReferenceKey(element) is IFrameworkElementKey key)
        {
            if (GetReference(element) is object reference)
            {
                if (reference is object[] references)
                {
                    references.ForEach(i =>
                    {
                        if (i is IFrameworkElementReference x)
                            x.SetReference(key, element);
                    });
                }
                else if (reference is IFrameworkElementReference y)
                    y.SetReference(key, element);
            }
        }
    }

    #endregion

    #region ReferenceKey

    public static readonly DependencyProperty ReferenceKeyProperty = DependencyProperty.RegisterAttached("ReferenceKey", typeof(IFrameworkElementKey), typeof(XElement), new FrameworkPropertyMetadata(null, OnReferenceKeyChanged));
    public static IFrameworkElementKey GetReferenceKey(FrameworkElement i) => (IFrameworkElementKey)i.GetValue(ReferenceKeyProperty);
    public static void SetReferenceKey(FrameworkElement i, IFrameworkElementKey input) => i.SetValue(ReferenceKeyProperty, input);

    private static void OnReferenceKeyChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            UpdateReference(element);
    }

    #endregion

    ///

    #region RelativeContext

    public static readonly DependencyProperty RelativeContextProperty = DependencyProperty.RegisterAttached("RelativeContext", typeof(Type), typeof(XElement), new FrameworkPropertyMetadata(null, OnRelativeContextChanged));
    public static Type GetRelativeContext(FrameworkElement i) => (Type)i.GetValue(RelativeContextProperty);
    public static void SetRelativeContext(FrameworkElement i, Type input) => i.SetValue(RelativeContextProperty, input);

    private static void OnRelativeContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.UpdateRelative(FrameworkElement.DataContextProperty);
    }

    #endregion

    #region RelativeContextSource

    public static readonly DependencyProperty RelativeContextSourceProperty = DependencyProperty.RegisterAttached("RelativeContextSource", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null, OnRelativeContextSourceChanged));
    public static object GetRelativeContextSource(FrameworkElement i) => i.GetValue(RelativeContextSourceProperty);
    public static void SetRelativeContextSource(FrameworkElement i, object input) => i.SetValue(RelativeContextSourceProperty, input);

    private static void OnRelativeContextSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.UpdateRelative(FrameworkElement.DataContextProperty);
    }

    #endregion

    #region RelativeTag

    public static readonly DependencyProperty RelativeTagProperty = DependencyProperty.RegisterAttached("RelativeTag", typeof(Type), typeof(XElement), new FrameworkPropertyMetadata(null, OnRelativeTagChanged));
    public static Type GetRelativeTag(FrameworkElement i) => (Type)i.GetValue(RelativeTagProperty);
    public static void SetRelativeTag(FrameworkElement i, Type input) => i.SetValue(RelativeTagProperty, input);

    private static void OnRelativeTagChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.UpdateRelative(FrameworkElement.TagProperty);
    }

    #endregion

    #region RelativeTagSource

    public static readonly DependencyProperty RelativeTagSourceProperty = DependencyProperty.RegisterAttached("RelativeTagSource", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null, OnRelativeTagSourceChanged));
    public static object GetRelativeTagSource(FrameworkElement i) => i.GetValue(RelativeTagSourceProperty);
    public static void SetRelativeTagSource(FrameworkElement i, object input) => i.SetValue(RelativeTagSourceProperty, input);

    private static void OnRelativeTagSourceChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.UpdateRelative(FrameworkElement.TagProperty);
    }

    #endregion

    #region (private) ResizeAdorner

    private static readonly DependencyProperty ResizeAdornerProperty = DependencyProperty.RegisterAttached("ResizeAdorner", typeof(ResizeAdorner), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static void SetResizeAdorner(FrameworkElement i, ResizeAdorner input) => i.SetValue(ResizeAdornerProperty, input);

    #endregion

    #region ResizeCoerceAxis

    public static readonly DependencyProperty ResizeCoerceAxisProperty = DependencyProperty.RegisterAttached("ResizeCoerceAxis", typeof(Axis2?), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static Axis2? GetResizeCoerceAxis(FrameworkElement i) => (Axis2?)i.GetValue(ResizeCoerceAxisProperty);
    public static void SetResizeCoerceAxis(FrameworkElement i, Axis2? input) => i.SetValue(ResizeCoerceAxisProperty, input);

    #endregion

    #region ResizeCoerceDirection

    public static readonly DependencyProperty ResizeCoerceDirectionProperty = DependencyProperty.RegisterAttached("ResizeCoerceDirection", typeof(Direction?), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static Direction? GetResizeCoerceDirection(FrameworkElement i) => (Direction?)i.GetValue(ResizeCoerceDirectionProperty);
    public static void SetResizeCoerceDirection(FrameworkElement i, Direction? input) => i.SetValue(ResizeCoerceDirectionProperty, input);

    #endregion

    #region ResizeSnap

    public static readonly DependencyProperty ResizeSnapProperty = DependencyProperty.RegisterAttached("ResizeSnap", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(8d));
    public static double GetResizeSnap(FrameworkElement i) => (double)i.GetValue(ResizeSnapProperty);
    public static void SetResizeSnap(FrameworkElement i, double input) => i.SetValue(ResizeSnapProperty, input);

    #endregion

    #region ResizeThumbStyle

    public static readonly DependencyProperty ResizeThumbStyleProperty = DependencyProperty.RegisterAttached("ResizeThumbStyle", typeof(Style), typeof(XElement), new FrameworkPropertyMetadata(default(Style)));
    public static Style GetResizeThumbStyle(FrameworkElement i) => (Style)i.GetValue(ResizeThumbStyleProperty);
    public static void SetResizeThumbStyle(FrameworkElement i, Style input) => i.SetValue(ResizeThumbStyleProperty, input);

    #endregion

    #region (private) RotateAdorner

    private static readonly DependencyProperty RotateAdornerProperty = DependencyProperty.RegisterAttached("RotateAdorner", typeof(RotateAdorner), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static void SetRotateAdorner(FrameworkElement i, RotateAdorner input) => i.SetValue(RotateAdornerProperty, input);

    #endregion

    #region RotateLineColor

    public static readonly DependencyProperty RotateLineColorProperty = DependencyProperty.RegisterAttached("RotateLineColor", typeof(Brush), typeof(XElement), new FrameworkPropertyMetadata(Brushes.Black));
    public static Brush GetRotateLineColor(FrameworkElement i) => (Brush)i.GetValue(RotateLineColorProperty);
    public static void SetRotateLineColor(FrameworkElement i, Brush input) => i.SetValue(RotateLineColorProperty, input);

    #endregion

    #region RotateSnap

    public static readonly DependencyProperty RotateSnapProperty = DependencyProperty.RegisterAttached("RotateSnap", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(11.25));
    public static double GetRotateSnap(FrameworkElement i) => (double)i.GetValue(RotateSnapProperty);
    public static void SetRotateSnap(FrameworkElement i, double input) => i.SetValue(RotateSnapProperty, input);

    #endregion

    #region RotateThumbStyle

    public static readonly DependencyProperty RotateThumbStyleProperty = DependencyProperty.RegisterAttached("RotateThumbStyle", typeof(Style), typeof(XElement), new FrameworkPropertyMetadata(default(Style)));
    public static Style GetRotateThumbStyle(FrameworkElement i) => (Style)i.GetValue(RotateThumbStyleProperty);
    public static void SetRotateThumbStyle(FrameworkElement i, Style input) => i.SetValue(RotateThumbStyleProperty, input);

    #endregion

    ///

    #region Selected

    public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler<MArea<double>>), typeof(FrameworkElement));
    public static void AddSelectedHandler(FrameworkElement i, RoutedEventHandler<MArea<double>> handler)
        => i.AddHandler(SelectedEvent, handler);
    public static void RemoveSelectedHandler(FrameworkElement i, RoutedEventHandler<MArea<double>> handler)
        => i.RemoveHandler(SelectedEvent, handler);

    #endregion

    #region SelectedCommand

    public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.RegisterAttached("SelectedCommand", typeof(ICommand), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static ICommand GetSelectedCommand(FrameworkElement i) => (ICommand)i.GetValue(SelectedCommandProperty);
    public static void SetSelectedCommand(FrameworkElement i, ICommand input) => i.SetValue(SelectionProperty, input);

    #endregion

    #region Selection

    public static readonly DependencyProperty SelectionProperty = DependencyProperty.RegisterAttached("Selection", typeof(MArea<double>), typeof(XElement), new FrameworkPropertyMetadata(default(MArea<double>)));
    [TypeConverter(typeof(DoubleRegionTypeConverter))]
    public static MArea<double> GetSelection(FrameworkElement i) => i.GetValueOrSetDefault(SelectionProperty, () => new MArea<double>(0));
    public static void SetSelection(FrameworkElement i, MArea<double> input) => i.SetValue(SelectionProperty, input);

    #endregion

    #region (private) SelectionAdorner

    private static readonly DependencyProperty SelectionAdornerProperty = DependencyProperty.RegisterAttached("SelectionAdorner", typeof(SelectionAdorner), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static SelectionAdorner GetSelectionAdorner(FrameworkElement i) => (SelectionAdorner)i.GetValue(SelectionAdornerProperty);
    private static void SetSelectionAdorner(FrameworkElement i, SelectionAdorner input) => i.SetValue(SelectionAdornerProperty, input);

    #endregion

    #region SelectionButton

    public static readonly DependencyProperty SelectionButtonProperty = DependencyProperty.RegisterAttached("SelectionButton", typeof(MouseButton), typeof(XElement), new FrameworkPropertyMetadata(MouseButton.Left));
    public static MouseButton GetSelectionButton(FrameworkElement i) => (MouseButton)i.GetValue(SelectionButtonProperty);
    public static void SetSelectionButton(FrameworkElement i, MouseButton input) => i.SetValue(SelectionButtonProperty, input);

    #endregion

    #region SelectionResets

    public static readonly DependencyProperty SelectionResetsProperty = DependencyProperty.RegisterAttached("SelectionResets", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false));
    public static bool GetSelectionResets(FrameworkElement i) => (bool)i.GetValue(SelectionResetsProperty);
    public static void SetSelectionResets(FrameworkElement i, bool input) => i.SetValue(SelectionResetsProperty, input);

    #endregion

    #region (private) SelectionStart

    private static readonly DependencyProperty SelectionStartProperty = DependencyProperty.RegisterAttached("SelectionStart", typeof(Point?), typeof(XElement), new FrameworkPropertyMetadata(null));

    private static Point? GetSelectionStart(FrameworkElement i) => (Point?)i.GetValue(SelectionStartProperty);
    private static void SetSelectionStart(FrameworkElement i, Point? input) => i.SetValue(SelectionStartProperty, input);

    #endregion

    #region (ReadOnly) Self

    private static readonly DependencyPropertyKey SelfKey = DependencyProperty.RegisterAttachedReadOnly("Self", typeof(Reference), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SelfProperty = SelfKey.DependencyProperty;
    public static Reference GetSelf(FrameworkElement i) => i.IfNotNullGet(j => j.GetValueOrSetDefault(SelfProperty, () => new Reference(i)));

    #endregion

    #region ShellContextMenu

    public static readonly DependencyProperty ShellContextMenuProperty = DependencyProperty.RegisterAttached("ShellContextMenu", typeof(object), typeof(XElement), new FrameworkPropertyMetadata(null, OnShellContextMenuChanged));
    public static object GetShellContextMenu(FrameworkElement i) => i.GetValue(ShellContextMenuProperty);
    public static void SetShellContextMenu(FrameworkElement i, object input) => i.SetValue(ShellContextMenuProperty, input);

    private static void OnShellContextMenuChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached(e.NewValue?.ToString().IsEmpty() == false, ShellContextMenuProperty, i => i.PreviewMouseRightButtonUp += ShellContextMenu_PreviewMouseRightButtonUp, i => i.PreviewMouseRightButtonUp -= ShellContextMenu_PreviewMouseRightButtonUp);
    }

    private static void Add(List<FileSystemInfo> input, string path)
    {
        FileSystemInfo result = null;
        Try.Do(() => result = Storage.File.Exists(path) ? new FileInfo(path) : Folder.Exists(path) ? new DirectoryInfo(path) : null);
        if (result != null)
            input.Add(result);
    }

    private static void ShellContextMenu_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            List<FileSystemInfo> result = [];

            if (GetShellContextMenu(element) is string path)
                Add(result, path);

            else if (GetShellContextMenu(element) is string[] paths)
            {
                foreach (var i in paths)
                    Add(result, i);
            }

            else if (GetShellContextMenu(element) is IListObservable collect)
            {
                foreach (var i in collect)
                {
                    if (i is Item j)
                        Add(result, j.Path);
                }
            }

            if (result.Count > 0)
            {
                var point = element.PointToScreen(e.GetPosition(element));
                ShellContextMenu.Show(point.Int32(), result.ToArray());
            }
        }
    }

    #endregion

    /// <see cref="TriggersProperty"/>
    #region

    public static readonly DependencyProperty TriggersProperty = DependencyProperty.RegisterAttached("Triggers", typeof(Triggers), typeof(XElement), new FrameworkPropertyMetadata(null, OnTriggersChanged));
    public static Triggers GetTriggers(FrameworkElement i) => i.GetValueOrSetDefault<Triggers>(TriggersProperty, () => []);
    public static void SetTriggers(FrameworkElement i, Triggers value) => i.SetValue(TriggersProperty, value);
    private static void OnTriggersChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (e.OldValue is Triggers oldValue)
                oldValue.Element = null;

            if (e.NewValue is Triggers triggers)
                triggers.Element = element;
        }
    }

    #endregion

    #region Wheel

    public static readonly DependencyProperty WheelProperty = DependencyProperty.RegisterAttached("Wheel", typeof(bool), typeof(XElement), new FrameworkPropertyMetadata(false, OnWheelChanged));
    public static bool GetWheel(FrameworkElement i) => (bool)i.GetValue(WheelProperty);
    public static void SetWheel(FrameworkElement i, bool value) => i.SetValue(WheelProperty, value);

    private static void OnWheelChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, WheelProperty, i => i.PreviewMouseWheel += Wheel_MouseWheel, i => i.PreviewMouseWheel -= Wheel_MouseWheel);
    }

    private static void Wheel_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            if (ModifierKeys.Control.Pressed())
            {
                if (GetWheelValues(frameworkElement)?.Count > 0)
                {
                    var value
                        = GetWheelValue(frameworkElement);
                    var values
                        = GetWheelValues(frameworkElement);

                    var increment
                        = GetWheelIncrement(frameworkElement);
                    var maximum
                        = GetWheelIncrement(frameworkElement);
                    var minimum
                        = GetWheelIncrement(frameworkElement);

                    if (e.Delta > 0)
                    {
                        if (increment == 0)
                        {
                            var i = values.IndexOf(value) + 1;
                            if (i <= values.Count - 1)
                                value = (double)values[i];
                        }
                        else
                        {
                            if (value + increment <= maximum)
                                value += increment;
                        }
                    }
                    else
                    {
                        if (increment == 0)
                        {
                            var i = values.IndexOf(value) - 1;
                            if (i >= 0)
                                value = (double)values[i];
                        }
                        else
                        {
                            if (value - increment >= minimum)
                                value -= increment;
                        }
                    }
                    SetWheelValue(frameworkElement, value);
                    return;
                }

                if (e.Delta > 0)
                {
                    var i = GetWheelValue(frameworkElement) + GetWheelIncrement(frameworkElement);
                    if (i <= GetWheelMaximum(frameworkElement))
                        SetWheelValue(frameworkElement, i);
                }
                else
                {
                    var i = GetWheelValue(frameworkElement) - GetWheelIncrement(frameworkElement);
                    if (i >= GetWheelMinimum(frameworkElement))
                        SetWheelValue(frameworkElement, i);
                }
                e.Handled = true;
            }
        }
    }

    #endregion

    #region WheelIncrement

    public static readonly DependencyProperty WheelIncrementProperty = DependencyProperty.RegisterAttached("WheelIncrement", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(1.0));
    public static double GetWheelIncrement(FrameworkElement i) => (double)i.GetValue(WheelIncrementProperty);
    public static void SetWheelIncrement(FrameworkElement i, double value) => i.SetValue(WheelIncrementProperty, value);

    #endregion

    #region WheelMaximum

    public static readonly DependencyProperty WheelMaximumProperty = DependencyProperty.RegisterAttached("WheelMaximum", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(32.0));
    public static double GetWheelMaximum(FrameworkElement i) => (double)i.GetValue(WheelMaximumProperty);
    public static void SetWheelMaximum(FrameworkElement i, double value) => i.SetValue(WheelMaximumProperty, value);

    #endregion

    #region WheelMinimum

    public static readonly DependencyProperty WheelMinimumProperty = DependencyProperty.RegisterAttached("WheelMinimum", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(8.0));
    public static double GetWheelMinimum(FrameworkElement i) => (double)i.GetValue(WheelMinimumProperty);
    public static void SetWheelMinimum(FrameworkElement i, double value) => i.SetValue(WheelMinimumProperty, value);

    #endregion

    #region WheelValue

    public static readonly DependencyProperty WheelValueProperty = DependencyProperty.RegisterAttached("WheelValue", typeof(double), typeof(XElement), new FrameworkPropertyMetadata(8.0));
    public static double GetWheelValue(FrameworkElement i) => (double)i.GetValue(WheelValueProperty);
    public static void SetWheelValue(FrameworkElement i, double value) => i.SetValue(WheelValueProperty, value);

    #endregion

    #region WheelValues

    public static readonly DependencyProperty WheelValuesProperty = DependencyProperty.RegisterAttached("WheelValues", typeof(IList), typeof(XElement), new FrameworkPropertyMetadata(null));
    public static IList GetWheelValues(FrameworkElement i) => (IList)i.GetValue(WheelValuesProperty);
    public static void SetWheelValues(FrameworkElement i, IList value) => i.SetValue(WheelValuesProperty, value);

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    static XElement()
    {
        EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(FrameworkElement), UIElement.MouseDownEvent,
            new MouseButtonEventHandler(OnMouseDown), true);
        EventManager.RegisterClassHandler(typeof(FrameworkElement), UIElement.PreviewMouseUpEvent,
            new MouseButtonEventHandler(OnPreviewMouseUp), true);
    }

    /// <see cref="Region.Method.Private"/>
    #region

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (GetRelativeContext(element) != null)
                element.UpdateRelative(FrameworkElement.DataContextProperty);

            if (GetRelativeTag(element) != null)
                element.UpdateRelative(FrameworkElement.TagProperty);
        }
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.SetIsMouseDown(true);
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    element.SetIsMouseLeftButtonDown(true);
                    break;
                case MouseButton.Middle:
                    element.SetIsMouseMiddleButtonDown(true);
                    break;
                case MouseButton.Right:
                    element.SetIsMouseRightButtonDown(true);
                    break;
            }
        }
    }

    private static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.SetIsMouseDown
                (false);
            element.SetIsMouseLeftButtonDown
                (false);
            element.SetIsMouseMiddleButtonDown
                (false);
            element.SetIsMouseRightButtonDown
                (false);
        }
    }

    private static void UpdateRelative(this FrameworkElement element, DependencyProperty property)
    {
        object findSource;
        Type findType;

        if (property == FrameworkElement.DataContextProperty)
        {
            findSource
                = GetRelativeContextSource(element);
            findType
                = GetRelativeContext(element);
        }
        else if (property == FrameworkElement.TagProperty)
        {
            findSource
                = GetRelativeTagSource(element);
            findType
                = GetRelativeTag(element);
        }
        else return;

        var result = findSource is DependencyObject i ? i : element;
        if (result.GetParent(findType) is object parent)
            element.SetCurrentValue(property, parent);
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    /// <summary>Gets actual size of element.</summary>
    public static Size ActualSize(this FrameworkElement element)
        => new(element.ActualWidth, element.ActualHeight);

    /// <summary>Adds handler.</summary>
    public static void AddHandler(this FrameworkElement element, Action load, Action unload = null)
        => element.GetHandler().Add(load, unload);

    /// <summary>Adds handler.</summary>
    public static void AddHandler<T>(this T element, Action<T> load, Action<T> unload = null) where T : FrameworkElement
        => element.GetHandler().Add(load, unload);

    /// <summary>Adds handler for attached property.</summary>
    public static void AddHandlerAttached<T>(this T element, bool add, object key, Action<T> load, Action<T> unload = null) where T : FrameworkElement
        => element.GetHandler().AddAttached(add, key, load, unload);

    /// <summary>Gets if mouse is over or not.</summary>
    public static bool ContainsMouse(this FrameworkElement element)
    {
        var point = Mouse.GetPosition(element);
        return

            point.X >= 0
            &&
            point.X <= element.ActualWidth
            &&
            point.Y >= 0
            &&
            point.Y <= element.ActualHeight
        ;
    }

    /// <summary>Gets child of element.</summary>
    public static T GetChild<T>(this FrameworkElement element, ReferenceKey<T> key) where T : FrameworkElement
        => element.GetVisualChildren<T>().FirstOrDefault(i => ReferenceEquals(GetName(i), key)) ?? element.GetLogicalChildren<T>().FirstOrDefault(i => ReferenceEquals(GetName(i), key));

    /// <summary>Gets children of element.</summary>
    public static IEnumerable<T> GetChildren<T>(this FrameworkElement element, ReferenceKey<T> key) where T : FrameworkElement
        => element.GetVisualChildren<T>().Where(i => ReferenceEquals(GetName(i), key));

    /// <summary>Gets index of element from parenting <see cref="ItemsControl"/>.</summary>
    public static int Index(this FrameworkElement element, uint origin = 0)
    {
        var itemsControl = ItemsControl.ItemsControlFromItemContainer(element);
        var index = itemsControl.ItemContainerGenerator.IndexFromContainer(element);
        return Convert.ToInt32(origin) + index;
    }

    /// <summary>Renders element to <see cref="System.Drawing.Bitmap"/>.</summary>
    public static System.Drawing.Bitmap Render(this FrameworkElement element)
    {
        try
        {
            element.Measure(new Size(element.ActualWidth, element.ActualHeight));
            element.Arrange(new Rect(new Size(element.ActualWidth, element.ActualHeight)));

            var bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(element);

            var stream = new MemoryStream();

            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(stream);

            return new System.Drawing.Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    #endregion
}