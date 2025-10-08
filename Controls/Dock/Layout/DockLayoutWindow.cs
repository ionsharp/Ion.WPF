using Ion.Numeral;
using System;
using System.Windows;

namespace Ion.Controls;

[Serializable]
public record class DockLayoutWindow : DockLayoutRoot
{
    public Vector2M<double> Position { get => Get<Vector2M<double>>(); set => Set(value); }

    public MSize<double> Size { get => Get<MSize<double>>(); set => Set(value); }

    public virtual WindowState State { get => Get(WindowState.Normal); set => Set(value); }

    public DockLayoutWindow() : base() { }
}