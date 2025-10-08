using Ion.Core;
using System.Windows;

namespace Ion.Controls;

public sealed record class PatternControlDot : Model
{
    private readonly bool isConnected;
    public bool IsConnected { get => Get(false); set => Set(value); }

    private Point position;
    public Point Position { get => Get<Point>(); set => Set(value); }

    public PatternControlDot() : base() { }

    public PatternControlDot(Point position) : base()
    {
        Position = position;
    }
}