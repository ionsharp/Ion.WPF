using Ion.Imaging;
using Ion.Reflect;
using System;
using System.Windows;

namespace Ion.Controls;

public class CursorExtension : UriExtension
{
    public System.Drawing.Point Point { get; set; } = new System.Drawing.Point(0, 0);

    public CursorExtension(string relativePath) : base(relativePath, AssemblyProject.Main) { }

    public CursorExtension(string relativePath, AssemblyProject assembly) : base(relativePath, assembly) { }

    public CursorExtension(string relativePath, int x, int y) : this(relativePath, x, y, AssemblyProject.Main) { }

    public CursorExtension(string relativePath, int x, int y, AssemblyProject assembly) : this(relativePath, assembly) => Point = new(x, y);

    public override object ProvideValue(IServiceProvider serviceProvider)
        => Try.Get(() => XCursor.Convert(XBitmap.Convert(XImageSource.Convert(base.ProvideValue(serviceProvider).To<System.Uri>()), BitmapEncoders.PNG), Point.X, Point.Y).Convert());
}