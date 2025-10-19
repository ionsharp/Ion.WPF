using Ion.Collect;
using Ion.Colors;
using Ion.Core;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Reflection;

namespace Ion.Controls;

[Styles.Object(
    Filter = Filter.None,
    Orientation = Orient.Vertical,
    Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
public record class ColorFileDockViewModelData() : FileDockViewModelData()
{
    /// <see cref="Tab"/>
    #region

    private enum Group { Background, Default }

    [TabView(View = View.Main)]
    private enum Tab { Data, Document, Layout }

    /// <see cref="Tab.Data"/>
    #region

    public GroupListWritable<ByteVector4> Colors { get => Get<GroupListWritable<ByteVector4>>(null, false); set => Set(value, false); }

    public GroupListWritable<Gradient> Gradients { get => Get<GroupListWritable<Gradient>>(null, false); set => Set(value, false); }

    public GroupListWritable<Numeral.Vector2> Illuminants { get => Get<GroupListWritable<Numeral.Vector2>>(null, false); set => Set(value, false); }

    public GroupListWritable<IMatrix> Matrices { get => Get<GroupListWritable<IMatrix>>(null, false); set => Set(value, false); }

    public GroupListWritable<ColorProfile> Profiles { get => Get<GroupListWritable<ColorProfile>>(null, false); set => Set(value, false); }

    #endregion

    /// <see cref="Tab.Document"/>
    #region

    [Group(Group.Default)]
    [Name("Depth")]
    [Styles.Number(Tab = Tab.Document, Step = 1.0, Maximum = 128.0, Minimum = 0.0, ValueFormat = NumberFormat.Default)]
    public int DefaultDepth { get => Get(0); set => Set(value); }

    [Group(Group.Default)]
    [Name("Dimension")]
    [Style(Tab = Tab.Document)]
    public AxisType DefaultDimension { get => Get(AxisType.Two); set => Set(value); }

    /// <see cref="Group.Background"/>
    #region

    [Group(Group.Background)]
    [Name("Type")]
    [Style(Index = 0, Tab = Tab.Document)]
    public BackgroundType BackgroundType { get => Get(BackgroundType.Gradient); set => Set(value); }

    [Group(Group.Background)]
    [Name("Color")]
    [Style(Index = 1, Tab = Tab.Document)]
    [VisibilityTrigger(nameof(BackgroundType), BackgroundType.Color)]
    public System.Windows.Media.Color BackgroundColor { get => Get(System.Windows.Media.Colors.Black, false); set => Set(value, false); }

    public Gradient BackgroundGradient { get => Get(new Gradient(new GradientStep(0, ByteVector4.Black), new GradientStep(1, ColorPreset.CSS.DarkGray)), false); set => Set(value, false); }

    [Group(Group.Background)]
    [Name("Gradient")]
    [Style(Index = 2, Tab = Tab.Document)]
    [VisibilityTrigger(nameof(BackgroundType), BackgroundType.Gradient)]
    public GradientGroupForm BackgroundGradients { get => Get<GradientGroupForm>(null, false); set => Set(value, false); }

    [Group(Group.Background)]
    [Name("Gradient type")]
    [Style(Index = 1, Tab = Tab.Document)]
    [VisibilityTrigger(nameof(BackgroundType), BackgroundType.Gradient)]
    public GradientType BackgroundGradientType { get => Get(GradientType.Radial); set => Set(value); }

    #endregion

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private void OnBackgroundGradientsChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        if (e.PropertyName == nameof(GroupForm.SelectedItem))
            BackgroundGradient = BackgroundGradients.SelectedItem.As<Item<Gradient>>()?.Value;
    }

    protected override void OnConstructed()
    {
        base.OnConstructed();
        Colors ??= new GroupListWritable<ByteVector4>
            (FolderPath, nameof(Colors),
            "dat", ListWritableLimit.Default);

        Gradients ??= new GroupListWritable<Gradient>
            (FolderPath, nameof(Gradients),
            "dat", ListWritableLimit.Default);

        Illuminants ??= new GroupListWritable<Numeral.Vector2>
            (FolderPath, nameof(Illuminants),
            "dat", ListWritableLimit.Default);

        Matrices ??= new GroupListWritable<IMatrix>
            (FolderPath, nameof(Matrices),
            "dat", ListWritableLimit.Default);

        Profiles ??= new GroupListWritable<ColorProfile>
            (FolderPath, nameof(Profiles),
            "dat", ListWritableLimit.Default);
    }

    #endregion

    /// <see cref="IPropertySet"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(Gradients))
        {
            if (Gradients is not null)
            {
                BackgroundGradients = new GradientGroupForm(Gradients);
                BackgroundGradients.PropertySet += OnBackgroundGradientsChanged;
            }
            else if (BackgroundGradients is not null)
            {
                BackgroundGradients.PropertySet -= OnBackgroundGradientsChanged;
                BackgroundGradients = null;
            }
        }
    }
}