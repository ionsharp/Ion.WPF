using Ion.Core;
using System.Windows.Media;

namespace Ion.Controls;

public record class DirectionControlValue : Namable
{
    public int DefaultColumn { get; }

    public int DefaultRow { get; }

    public Direction Direction { get; }

    public int Column { get => Get(0); set => Set(value); }

    public int Row { get => Get(0); set => Set(value); }

    public ImageSource Icon { get => Get<ImageSource>(); set => Set(value); }

    internal DirectionControlValue(int column, int row, int direction = (int)Direction.Unknown) : base()
        => (Column, DefaultColumn, DefaultRow, Row, Direction) = (column, column, row, row, (Direction)direction);
}