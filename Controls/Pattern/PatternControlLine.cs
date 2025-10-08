using Ion.Numeral;

namespace Ion.Controls;

public sealed record class PatternControlLine : MLine<double>
{
    public bool IsOpen { get => this.Get(false); set => this.Set(value); }

    public PatternControlLine() : base(0) { }

    public PatternControlLine(bool isOpen, Numeral.Line<int> point) : this(isOpen, point.X1, point.Y1, point.X2, point.Y2) { }

    public PatternControlLine(bool isOpen, double x12, double y12) : this(isOpen, x12, y12, x12, y12) { }

    public PatternControlLine(bool isOpen, double x1, double y1, double x2, double y2) : this()
    {
        IsOpen = isOpen;
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }
}