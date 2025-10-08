using Ion.Numeral;
using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace Ion.Controls;

public class PathGeometryExtension() : MarkupExtension()
{
    public double Angle { get; set; }

    public double Height { get; set; } = 100;

    public double Width { get; set; } = 100;

    public uint Sides { get; set; } = 3;

    private static PathGeometry Render(uint sides, double angle, double height, double width)
    {
        var shape = new ShapePoint(Shape.GetPolygon(new Angle(angle).Convert(AngleType.Radian), new Vector2(0, 0), new Size<double>(width, height), sides, 0));
        shape.Translate();
        shape.Normalize();
        shape.Scale(new Vector2(width, height));

        var figure = new PathFigure { StartPoint = new(shape.Points[0].X, shape.Points[0].Y) };
        var segment = new PolyLineSegment();

        for (var i = 0; i < shape.Points.Count; i++)
            segment.Points.Add(new(shape.Points[i].X, shape.Points[i].Y));

        segment.Points.Add(new(shape.Points[0].X, shape.Points[0].Y));

        var segments = new PathSegmentCollection() { segment };
        figure.Segments = segments;

        return new PathGeometry() { Figures = [figure] };
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => Render(Sides, Angle, Height, Width);
}