using Ion.Controls;
using Ion.Imaging;
using Ion.Input.Global;
using Ion.Reflect;
using System;
using System.Windows.Media;

namespace Ion.Core;

[Name("Mouse"), Image(Images.Arrow)]
[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class MousePanel() : Panel()
{
    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Field"/>

    private ListenerMouse listener;

    /// <see cref="Region.Property"/>

    /// <see cref="View.Main"/>

    [Style(CanEdit = false)]
    public Color Color { get => Get<Color>(); set => Set(value); }

    /// <see cref="View.Header"/>

    [Style(Ion.Template.CheckSwitch,
        View = View.Header)]
    public bool Enable { get => Get(true); set => Set(value); }

    /// <see cref="View.Footer"/>

    [Style(CanEdit = false,
        View = View.Footer)]
    public int X { get => Get(0); set => Set(value); }

    [Style(CanEdit = false,
        View = View.Footer)]
    public int Y { get => Get(0); set => Set(value); }

    /// <see cref="Region.Method"/>

    private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        XCursor.GetColor().Convert(out Color color);
        Color = color;

        X = e.X; Y = e.Y;
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(Enable))
            listener.IfNotNull(i => i.Enabled = Enable);
    }

    public override void Subscribe()
    {
        base.Subscribe();
        listener = new(new Input.Global.WinApi.GlobalHooker())
        {
            Enabled = Enable
        };
        listener.MouseMove += OnMouseMove;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        if (listener != null)
        {
            listener.Enabled = false;
            listener.MouseMove -= OnMouseMove;
            listener.Dispose();
            listener = null;
        }
    }
}