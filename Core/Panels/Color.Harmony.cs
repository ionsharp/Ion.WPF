using Ion;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Imaging;
using Ion.Input;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Threading;
using Ion.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using static System.Math;

namespace Ion.Core;

/// <inheritdoc/>
[Styles.Object(Image = Images.ColorWheel, Name = "Harmony",
    Description = "Explore color harmony.",
    MemberViewType = MemberViewType.Tab)]
[Serializable]
public record class ColorHarmonyPanel : DataViewPanel<Color>
{
    private enum Group
    {
        [GroupStyle(Index = -3)]
        Harmony,
        [GroupStyle(Index = -2)]
        Model,
        [GroupStyle(Index = -1)]
        Colors
    }

    [TabView(View = Ion.View.Main)]
    private new enum Tab { }

    [Serializable]
    public enum Steps { Increase, Decrease, Both }

    [Flags, Serializable]
    public enum TargetComponents { [Hide]None = 0, Y = 1, Z = 2, [Hide]Both = Y | Z }

    /// <see cref="Region.Event"/>
    #region

    [field: NonSerialized]
    public event EventHandle<Color> Picked;

    [field: NonSerialized]
    public event EventHandle<Color[]> Saved;

    #endregion

    /// <see cref="Region.Field"/>
    #region

    public static readonly new ResourceKey Template = new();

    private readonly Taskable update;

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public override bool CanAdd => false;

    public override bool CanAddFromPreset => false;

    public override bool CanClear => false;

    public override bool CanClone => false;

    public override bool CanCut => false;

    public override bool CanEdit => false;

    public override bool CanMoveTo => false;

    public override bool CanPaste => false;

    public override bool CanRemove => false;

    [NonSerializable]
    public ColorDocument ActiveDocument { get; private set; }

    [NonSerializable]
    public ListObservable<Color> Colors { get => Get(new ListObservable<Color>(), false); set => Set(value, false); }

    [Group(Group.Model)]
    [Style(Ion.Template.EnumFlag, CaptionSide = Sides.RightOrBottom, NameHide = true, Index = 1, Name = "Components",
        View = Ion.View.Header)]
    [StyleTrigger(nameof(StyleAttribute.Caption), nameof(ComponentNames))]
    public TargetComponents Components { get => Get(TargetComponents.Y); set => Set(value); }

    public string ComponentNames
    {
        get
        {
            var result = "None";
            if (Models != null && SelectedModelIndex >= 0 && SelectedModelIndex < Models.Count)
            {
                var components = Models[SelectedModelIndex].GetAttributes<ComponentAttribute>().Select(i => i.Info).Where(i => i.Name != "Hue").ToArray();
                if (Components == TargetComponents.Both)
                    result = $"{components[0].Name}, {components[1].Name}";

                else if (Components == TargetComponents.None)
                {
                }

                else if (Components.HasFlag(TargetComponents.Y))
                    result = $"{components[0].Name}";

                else if (Components.HasFlag(TargetComponents.Z))
                    result = $"{components[1].Name}";
            }
            return $"({result})";
        }
    }

    [Group(Group.Colors)]
    [Styles.NumberAttribute(2, 256, 1, Name = "Colors",
        View = Ion.View.Header, CanUpDown = true)]
    public int Count { get => Get(7); set => Set(value); }

    [Group(Group.Harmony)]
    [Style(NameHide = true, Index = -11,
        View = Ion.View.Header)]
    public Harmony Harmony { get => Get(Harmony.Monochromatic); set => Set(value); }

    private readonly ListObservable<Type> models = new(typeof(IColor).Assembly.GetDerivedTypes<IColor>().Where(i => i.GetAttribute<ComponentGroupAttribute>()?.Group.HasFlag(ComponentGroup.H) == true));
    [Group(Group.Model)]
    [Styles.List(Ion.Template.ListCombo, NameHide = true,
        Index = 0,
        Name = "Model",
        View = Ion.View.Header,
        SelectedIndexProperty = nameof(SelectedModelIndex),
        SelectedItemProperty = nameof(SelectedModel),
        Validate = [typeof(RequireSelectionRule)])]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(Type.Name),
        TargetItem = typeof(Type))]
    public ListObservable<Type> Models => models;

    [Style(View = Ion.View.Header)]
    public bool Randomize { get => Get(false); set => Set(value); }

    [Group(Group.Colors)]
    [Styles.NumberAttribute(0.0, 100.0, 1.0, CanUpDown = true,
        RightText = "%",
        View = Ion.View.Header)]
    public double Range { get => Get(100.0); set => Set(value); }

    [Style(View = Ion.View.Header)]
    public bool Reverse { get => Get(false); set => Set(value); }

    public int SelectedModelIndex { get => Get(0); set => Set(value); }

    public object SelectedModel { get => Get<object>(); set => Set(value); }

    [Group(Group.Model)]
    [Style(NameHide = true, Index = 2,
        View = Ion.View.Header)]
    public Steps Step { get => Get(Steps.Both); set => Set(value); }

    [Style(View = Ion.View.Header)]
    public bool Sync { get => Get(true); set => Set(value); }

    #endregion

    /// <see cref="Region.Constructor"/>

    public ColorHarmonyPanel() : base()
    {
        Items = Colors;

        update = new(Update, TaskStrategy.FinishAndRestart);
        ShowName = false;
    }

    /// <see cref="Region.Method"/>
    #region

    private void OnColorChanged(object sender, EventArgs<Color> e)
    {
        if (Sync)
            _ = update.Start();
    }

    private void ApplySteps(Color oldColor, double x, int count)
    {
        if (!(SelectedModelIndex >= 0 && SelectedModelIndex < Models.Count))
            return;

        var colorProfile = ColorProfile.Default;

        var colorModel = Models[SelectedModelIndex];

        oldColor.Convert(out ByteVector4 v);
        var rgb = new RGB(v);

        var targetColor = colorModel.Create<IColor3>();
        targetColor.From(rgb, colorProfile);

        var nColor = targetColor.XYZ.Normalize();

        x = x < 0 ? 359 - Abs(x) : x;
        x = x > 359 ? x - 359 : x;
        x /= 359.0;

        double y = nColor.Y, z = nColor.Z;

        Func<double, double> increment = Step switch
        {
            //[Original, 0]
            Steps.Decrease => i => -(i / Convert.ToDouble(count)),
            //[Original, 1]
            Steps.Increase => i => (1.0 - i) / Convert.ToDouble(count),
            //[0, 1]
            Steps.Both => i => 1.0 / Convert.ToDouble(count),
        };

        double randomize(double i) => Randomize ? Step switch
        {
            //[Original, 0]
            Steps.Decrease => Number.Random<double>() * y,
            //[Original, 1]
            Steps.Increase => 1 - Number.Random<double>() * (1 - y),
            //[0, 1]
            Steps.Both => Number.Random<double>(),
        } : i;

        double iY = increment(y);
        double iZ = increment(z);

        for (var i = 0; i < count; i++)
        {
            var rX = x;
            var rY = Components.HasFlag(TargetComponents.Y) ? randomize(y) : y;
            var rZ = Components.HasFlag(TargetComponents.Z) ? randomize(z) : z;

            var firstColor = new Vector<Double1>(Enumerable.Select(IColor.New(colorModel, rX, rY, rZ).ToArray(), xxx => new Double1(xxx)).ToArray()).Denormalize<double>();

            var newColor = IColor.New(colorModel, firstColor[0], firstColor[1], firstColor[2]);
            newColor.To(out RGB result, ColorProfile.Default);

            Colors.Add(XColor.Convert(XColorVector.Convert(result)));

            if (Components.HasFlag(TargetComponents.Y))
                y += iY;

            if (Components.HasFlag(TargetComponents.Z))
                z += iZ;
        }
    }

    private async Task Update(CancellationToken token)
    {
        await Task.Run(() =>
        {
            Try.Do(() =>
            {
                Colors.Clear();
                if (ActiveDocument is null)
                    return;

                ActiveDocument.NewColor.Convert(out System.Drawing.Color color);
                double h = color.GetHue();

                var range = Range / 100.0;

                double[] hues = null;
                switch (Harmony)
                {
                    case Harmony.Analogous:
                        hues = Reverse ? [h, h - range * 30, h - range * 60] : [h, h + range * 30, h + range * 60];
                        break;

                    case Harmony.Diad:
                        hues = Reverse ? [h, h + range * 180] : [h, h + range * 180];
                        break;

                    case Harmony.Monochromatic:
                        hues = Reverse ? [h] : [h];
                        break;

                    case Harmony.DoubleComplementary:
                        hues = Reverse ? [h, h - range * 30, h - range * 180, h - range * 210] : [h, h + range * 30, h + range * 180, h + range * 210];
                        break;

                    case Harmony.SplitComplementary:
                        hues = Reverse ? [h, h + range * 150, h - range * 150] : [h, h + range * 150, h - range * 150];
                        break;

                    case Harmony.Square:
                        hues = Reverse ? [h, h - range * 90, h - range * 180, h - range * 270] : [h, h + range * 90, h + range * 180, h + range * 270];
                        break;

                    case Harmony.Tetrad:
                        hues = Reverse ? [h, h - range * 60, h - range * 180, h - range * 240] : [h, h + range * 60, h + range * 180, h + range * 240];
                        break;

                    case Harmony.Triad:
                        hues = Reverse ? [h, h - range * 120, h - range * 240] : [h, h + range * 120, h + range * 240];
                        break;

                    case Harmony.CoolWarm:

                        double hx = h;
                        double hy = range * 360; hy = hy > Count ? Count : hy;
                        double hz = hy / Count;

                        List<double> other = [];
                        for (double i = 0; i < Count; i++)
                        {
                            other.Add(hx);
                            hx = Reverse ? hx - hz : hx + hz;

                            int w = 135, c = 315, m = 359;
                            bool warm = h < w || h > c; bool cool = !warm;

                            if (cool)
                                hx = hx < w ? c : hx > c ? w : hx;

                            if (warm)
                            {
                                hx = hx < 0 ? m : hx > m ? 0 : (Reverse ? hx < c && hx > w ? w : hx : hx > w && hx < c ? c : hx);
                            }
                        }
                        hues = [.. other];
                        break;
                }

                var portion = Count / hues.Length;

                foreach (var i in hues)
                    ApplySteps(ActiveDocument.NewColor, i, portion);
            });
        },
        token);
    }

    ///

    protected override void OnActiveDocumentChanged(Document oldValue, Document newValue)
    {
        base.OnActiveDocumentChanged(oldValue, newValue);
        ActiveDocument.IfNotNull(i => i.ColorChanged -= OnColorChanged);

        if (newValue != null)
        {
            ActiveDocument = (ColorDocument)newValue;
            ActiveDocument.ColorChanged += OnColorChanged;
        }

        _ = update.Start();
    }

    protected virtual void OnPicked(Color input) => Picked?.Invoke(this, new(input));

    ///

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Components):
            case nameof(SelectedModelIndex):
                Reset(() => ComponentNames);
                _ = update.Start();
                break;

            case nameof(Count):
            case nameof(Harmony):
            case nameof(Randomize):
            case nameof(Range):
            case nameof(Reverse):
            case nameof(Step):
            case nameof(Sync):
                _ = update.Start();
                break;
        }
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    public ICommand SelectColorCommand => Commands[nameof(SelectColorCommand)] ??= new RelayCommand<Color>(OnPicked, i => i != null);

    [Image(Images.Save)]
    [Name("Save")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand SaveCommand => Commands[nameof(SaveCommand)] ??= new RelayCommand(() => Saved?.Invoke(this, new([.. Colors])), () => Colors.Count > 0);

    #endregion
}