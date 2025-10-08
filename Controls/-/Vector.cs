using Ion.Numeral;
using System;
using System.Windows;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<System.Windows.Vector>]
public static class XVector
{
    public static Point BoundSize(this System.Windows.Vector i, Point? origin, Point offset, Size mSize, Size size, double snap, bool limit = true)
    {
        var x = Math.Round(offset.X + i.X);
        var y = Math.Round(offset.Y + i.Y);
        if (origin != null)
        {
            x = origin.Value.X + x;
            y = origin.Value.Y + y;
        }

        if (limit)
        {
            x = x < 0 ? 0 : x;
            y = y < 0 ? 0 : y;

            x = x + size.Width > mSize.Width ? mSize.Width - size.Width : x;
            y = y + size.Height > mSize.Height ? mSize.Height - size.Height : y;
        }

        x = x.Nearest(snap);
        y = y.Nearest(snap);

        if (origin != null)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                y = origin.Value.Y;
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                x = origin.Value.X;
        }

        return new Point(x, y);
    }
}