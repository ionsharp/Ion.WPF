using Ion.Core;
using Ion.Numeral;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Ion.Controls;

[Extend<UIElement>]
public static class XUIElement
{
    public static T AddAdorner<T>(this UIElement input, Func<T> create) where T : Adorner
    {
        var layer = AdornerLayer.GetAdornerLayer
        (
            input is Window window
            ? window.GetChild(XWindow.BorderKey)
            : input
        );

        if (layer != null)
        {
            var result = create();
            layer.Add(result);
            if (result is ISubscribe i)
                i.Subscribe();

            return result;
        }

        return default;
    }

    public static void RemoveAdorners<T>(this UIElement input) where T : Adorner
    {
        if (AdornerLayer.GetAdornerLayer(input) is AdornerLayer layer)
        {
            layer.GetAdorners(input)?.ForEach(i =>
            {
                if (i is T)
                {
                    if (i is ISubscribe j)
                        j.Unsubscribe();

                    layer.Remove(i);
                }
            });
        }
    }

    ///

    public static void AddOnce(this UIElement input, CommandBinding binding)
    {
        if (!input.CommandBindings.Any<CommandBinding>(i => i.Command == binding.Command))
            input.CommandBindings.Add(binding);
    }

    public static Task Animate(this UIElement input, Storyboard story)
    {
        var result = new TaskCompletionSource<object>();

        void handler(object s, EventArgs e)
        {
            story.Completed -= handler;
            result.TrySetResult(null);
        }

        story.Completed += handler;
        story.Begin();
        return result.Task;
    }

    public async static Task FadeIn(this UIElement input, Duration duration = default)
    {
        var animation = new DoubleAnimation()
        {
            From = 0.0,
            To = 1.0,
            Duration = duration == default ? 0.3.Seconds().Duration() : duration
        };
        Storyboard.SetTarget(animation, input);
        Storyboard.SetTargetProperty(animation, new PropertyPath(nameof(UIElement.Opacity)));

        var result = new Storyboard();
        result.Children.Add(animation);
        await input.Animate(result);
    }

    public async static Task FadeOut(this UIElement input, Duration duration = default, EventHandler Callback = null)
    {
        var animation = new DoubleAnimation()
        {
            From = 1.0,
            To = 0.0,
            Duration = duration == default ? 0.5.Seconds().Duration() : duration
        };
        Storyboard.SetTarget(animation, input);
        Storyboard.SetTargetProperty(animation, new PropertyPath(nameof(UIElement.Opacity)));

        var result = new Storyboard();
        result.Children.Add(animation);
        await input.Animate(result);
    }

    ///

    public static double GetLeft(this UIElement input) => Canvas.GetLeft(input);

    public static double GetTop(this UIElement input) => Canvas.GetTop(input);

    public static Point GetTopLeft(this UIElement input) => new(input.GetLeft(), input.GetTop());

    ///

    public static void SetLeft(this UIElement input, double x) => Canvas.SetLeft(input, x);

    public static void SetTop(this UIElement input, double y) => Canvas.SetTop(input, y);

    public static void SetTopLeft(this UIElement input, Point point) { input.SetLeft(point.X); input.SetTop(point.Y); }

    public static void SetTopLeft(this UIElement input, double x, double y) => input.SetTopLeft(new(x, y));
}