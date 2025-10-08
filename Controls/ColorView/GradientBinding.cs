using Ion.Data;
using Ion.Imaging;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ion.Colors;

public class GradientBinding : MultiBind
{
    public GradientBinding() : this(Paths.Dot) { }

    public GradientBinding(string path) : base(path)
    {
        Converter = new MultiValueConverter<GradientBrush>(data =>
        {
            if (data.Values?.Length > 0)
            {
                if (data.Values[0] is Gradient gradient)
                    return Convert<LinearGradientBrush>(gradient);

                else if (data.Values[0] is GradientStepCollection collection)
                    return Convert<LinearGradientBrush>(new Gradient(collection));
            }
            return null;
        });

        void Add(string a, string b) => Bindings.Add(new Binding($"{a}{b}"));

        var i = path == Paths.Dot ? "" : $"{path}.";

        Add(i, $"{nameof(Gradient.Steps)}");
        Add(i, $"{nameof(Gradient.Steps)}.{nameof(Gradient.Steps.Count)}");
    }

    public static T Convert<T>(Gradient gradient) where T : GradientBrush
    {
        T result = default;
        if (typeof(T) == typeof(LinearGradientBrush))
        {
            result = new LinearGradientBrush()
            {
                EndPoint = new Point(Gradient.Horizontal.X2, Gradient.Horizontal.Y2),
                StartPoint = new Point(Gradient.Horizontal.X1, Gradient.Horizontal.Y1),
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
        gradient.Steps.ForEach(i => result.GradientStops.Add(new GradientStop(XColor.Convert(i.Color), i.Offset)));
        return result;
    }
}

public class GradientStepBinding : MultiBind
{
    public GradientStepBinding() : base()
    {
        Converter = new MultiValueConverter<GradientBrush>(i =>
        {
            if (i.Values?.Length > 0)
            {
                if (i.Values[0] is GradientStepCollection collection)
                    return GradientBinding.Convert<LinearGradientBrush>(new Gradient(collection));
            }
            return null;
        });

        Bindings.Add(new Binding(Paths.Dot));
        Bindings.Add(new Binding($"{nameof(GradientStepCollection.Count)}"));
    }
}