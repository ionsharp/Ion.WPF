using Ion.Imaging;
using Ion.Linq;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Ion.Colors;

[Description("An array of position-dependent colors.")]
[ContentProperty(nameof(Steps)), Name(nameof(GradientModel)), Image(Images.Gradient), Styles.Object(GroupName = GroupName.None), Serializable]
public class GradientModel() : Model(), ICloneable, IReset
{
    public static GradientModel Default => new(new GradientStep(0, VectorByte4.White), new GradientStep(1, VectorByte4.Black));

    public static LinearGradientBrush DefaultBrush => new(System.Windows.Media.Colors.White, System.Windows.Media.Colors.Black, new Point(Horizontal.X1, Horizontal.Y1), new Point(Horizontal.X2, Horizontal.Y2));

    public static GradientModel Rainbow => new
    (
        new GradientStep(0.000, new VectorByte4(255,   0,   0)),
        new GradientStep(0.166, new VectorByte4(255, 255,   0)),
        new GradientStep(0.332, new VectorByte4(  0, 255,   0)),
        new GradientStep(0.500, new VectorByte4(  0, 255, 255)),
        new GradientStep(0.666, new VectorByte4(  0,   0, 255)),
        new GradientStep(0.832, new VectorByte4(255,   0, 255)),
        new GradientStep(1.000, new VectorByte4(255,   0,   0))
    );

    ///

    public static Line<double> Horizontal => new(0, 0.5, 1, 0.5);

    public static Line<double> Vertical => new(0.5, 0, 0.5, 1);

    ///

    [Styles.List(NameHide = true, ItemTypes = [typeof(GradientStep)])]
    public GradientStepCollection Steps { get => Get(new GradientStepCollection()); private set => Set(value); }

    ///

    public GradientModel(params GradientStep[] i) : this()
        => i?.ForEach(Steps.Add);

    public GradientModel(GradientStepCollection i) : this()
        => i?.ForEach(j => Steps.Add(new GradientStep(j.Offset, j.Color)));

    public GradientModel(GradientStopCollection i) : this() => i?.ForEach(j =>
    {
        j.Color.Convert(out VectorByte4 k);
        Steps.Add(new GradientStep(j.Offset, k));
    });

    public GradientModel(LinearGradientBrush i) : this(i.GradientStops) { }

    public GradientModel(RadialGradientBrush i) : this(i.GradientStops) { }

    ///

    object ICloneable.Clone() => Clone();
    public virtual GradientModel Clone()
    {
        var result = new GradientModel();
        result.CopyFrom(this);
        return result;
    }

    public T Convert<T>() where T : GradientBrush
    {
        T result = default;
        if (typeof(T) == typeof(LinearGradientBrush))
        {
            result = new LinearGradientBrush()
            {
                EndPoint = new Point(Horizontal.X2, Horizontal.Y2),
                StartPoint = new Point(Horizontal.X1, Horizontal.Y1),
                Opacity = 1,
            } as T;
        }
        if (typeof(T) == typeof(RadialGradientBrush))
        {
            result = new RadialGradientBrush()
            {
                RadiusX = 0.5,
                RadiusY = 0.5,
                Opacity = 1,
            } as T;
        }
        Steps.ForEach(i => result.GradientStops.Add(new GradientStop(XColor.Convert(i.Color), i.Offset)));
        return result;
    }

    public void CopyFrom(GradientModel input)
    {
        Steps.Clear();
        foreach (var i in input.Steps)
            Steps.Add(new GradientStep(i.Offset, i.Color));
    }

    public virtual void Reset() => CopyFrom(Default);
}