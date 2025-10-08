using Ion;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using Ion.Imaging;
using Ion.Input;
using Ion.Media;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Validation;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

[PropertyPanel.Target]
[Styles.Object(Name = "Color document",
    Filter = Filter.None,
    GroupName = MemberGroupName.None,
    IgnoreNames = [nameof(Created), nameof(LastAccessed), nameof(LastModified), nameof(Name), nameof(Path), nameof(SaveCommand), nameof(SaveAsCommand), nameof(Size)])]
[Serializable]
public record class ColorDocument : FileDocument
{
    private enum Group { Background }

    /// <see cref="Region.Event"/>
    #region 

    [field: NonSerialized]
    public event EventHandle<Color> ColorChanged;

    [field: NonSerialized]
    public event EventHandle<Color> ColorSaved;

    #endregion

    /// <see cref="Region.Field"/>
    #region 

    public static readonly ResourceKey Template = new();

    public static readonly ResourceKey IconTemplate = new();

    public static readonly ResourceKey PreviewTemplate = new();

    public static readonly ResourceKey TitleTemplate = new();

    public static readonly IConvert<Color, string> ConvertColorToString = new ValueConverter<Color, string>(i => { i.Value.Convert(out ByteVector4 j); return j.ToString(); }, i => XColor.Convert(new ByteVector4(i.Value)));

    public static readonly Color DefaultOldColor = System.Windows.Media.Colors.Black;

    public static readonly Color DefaultNewColor = System.Windows.Media.Colors.White;

    public static readonly Type DefaultModel = typeof(HSB);

    public static readonly ColorViewModel[] DefaultModels =
    [
        new ColorViewModel3(new RGB()),
        new ColorViewModel3(new XYZ()),
        new ColorViewModel4(new CMYK())
    ];

    [NonSerialized]
    private ListCollectionView defaultColors;

    [NonSerialized]
    private readonly Handle handleColor = false;

    [NonSerialized]
    private ListObservable<ColorViewModelGroup> models = null;

    #endregion

    /// <see cref="Region.Property"/>
    #region 

    public byte Alpha { get => Get((byte)255); set => Set(value); }

    public ListCollectionView DefaultColors => defaultColors ??= GetColors();

    public Component4 Component => (Component4)Math.Clamp(SelectedComponentIndex, 0, 3);

    public override object Icon => NewColor;

    public bool Dimension12 => !Dimension3;

    public bool Dimension3 => Dimension == AxisType.Three;

    public bool Is4D => SelectedModel.As<ColorViewModelGroup>()?.Value.Implements<IColor4>() == true;

    public Type Model => SelectedModel.As<ColorViewModelGroup>()?.Value ?? DefaultModel;

    public bool Normalize { get => Get(false, false); set => Set(value, false); }

    public int Precision { get => Get(2, false); set => Set(value, false); }

    public ColorProfile Profile { get => Get(ColorProfile.Default); set => Set(value); }

    public IListWritable Profiles { get => Get<IListWritable>(null, false); set => Set(value, false); }

    public object SelectedComponent { get => Get<object>(null, false); set => Set(value, false); }

    public int SelectedComponentIndex { get => Get(0); set => Set(value); }

    public object SelectedModel { get => Get<object>(null, false); set => Set(value, false); }

    public int SelectedModelIndex { get => Get(0); set => Set(value); }

    public override string Title
    {
        get
        {
            NewColor.Convert(out ByteVector4 color);
            return $"#{color.XYZ}";
        }
    }

    public override object ToolTip => NewColor;

    public override string[] WritableExtensions => ["color"];

    #endregion

    /// <see cref="Region.Constructor"/>
    #region 

    public ColorDocument() : this(DefaultNewColor, DefaultModel, null) { }

    public ColorDocument(Color color, Type model, IListWritable profiles) : base()
    {
        NewColor = color; Profiles = profiles; SelectedModel = GetModel(model);
    }

    #endregion

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnColorChanged(Color color) => ColorChanged?.Invoke(this, new(color));

    private static void OnProfileChanged(Value<ColorProfile> input) { if (input.OldValue != input.NewValue) { } }

    private void OnSelectedProfileChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        if (sender is GroupItemForm result)
            Profile = result.SelectedItem?.As<Item<ColorProfile>>().Value ?? Profile;
    }

    private void OnValueChanged(object sender, EventArgs e)
    {
        ToColor(Color);
        UpdateColors();
    }

    private void UpdateModels()
    {
        models ??= [];

        var init = Models is null;
        Models ??= new(models);

        if (init)
        {
            Models.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ColorViewModelGroup.Group)));
            Models.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ColorViewModelGroup.Group), System.ComponentModel.ListSortDirection.Ascending));
            Models.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ColorViewModelGroup.Name), System.ComponentModel.ListSortDirection.Ascending));
        }

        models.Clear();
        IColor.GetTypes().Where(i => i.GetAttribute<ComponentGroupAttribute>() is ComponentGroupAttribute j && ModelClass.HasFlag(j.Group)).ForEach(i => models.Add(new ColorViewModelGroup(i)));

        Models.Refresh();
    }

    ///

    private ColorViewModelGroup GetModel(Type model)
        => Models?.SourceCollection.As<IList>().FirstOrDefault<ColorViewModelGroup>(i => i.Value == model);

    ///

    /// <summary>Converts from <see cref="Core.CommandModel"/> to <see cref="RGB"/> based on <see cref="Component"/>.</summary>
    private void ToColor(ColorViewModel input)
    {
        handleColor.DoInternal(() =>
        {
            var color = input.GetColor();
            color.To(out RGB rgb, Profile);

            NewColor = XColor.Convert(XColorVector.Convert(rgb));
            OnColorChanged(NewColor);
        });
    }

    private void FromColor(ColorViewModel input)
    {
        handleColor.DoInternal((Action)(() =>
        {
            NewColor.Convert(out ByteVector4 v);
            var rgb = XColorVector.Convert(v);

            var result = new RGB(rgb).To(Model, Profile);
            if (result is IColor3 result3)
            {
                var color3 = result3.XYZ.Normalize();
                input.If<ColorViewModel3>(i => { i.X = color3.X; i.Y = color3.Y; i.Z = color3.Z; });
            }

            else if (result is IColor4 result4)
            {
                var color4 = result4.XYZW.Normalize();
                input.If<ColorViewModel4>(i => { i.X = color4.X; i.Y = color4.Y; i.Z = color4.Z; i.W = color4.W; });
            }

            OnColorChanged(NewColor);
        }));
    }

    ///

    private void UpdateColors()
    {
        NewColor.Convert(out ByteVector4 v);
        var rgb = XColorVector.Convert(v);
        foreach (var i in Colors)
        {
            var j = new RGB(rgb).To(i.ColorType, ColorProfile.Default);
            if (i is ColorViewModel3 i3)
            {
                if (j is IColor3 j3)
                {
                    if (Normalize)
                    {
                        i3.X = new Range<double>(i3.Minimum[0], i3.Maximum[0]).ToRange(0, 1, j3.X);
                        i3.Y = new Range<double>(i3.Minimum[1], i3.Maximum[1]).ToRange(0, 1, j3.Y);
                        i3.Z = new Range<double>(i3.Minimum[2], i3.Maximum[2]).ToRange(0, 1, j3.Z);
                    }
                    else
                    {
                        i3.DisplayX = $"{j3.X}";
                        i3.DisplayY = $"{j3.Y}";
                        i3.DisplayZ = $"{j3.Z}";
                    }
                }
            }
            if (i is ColorViewModel4 i4)
            {
                if (j is IColor4 j4)
                {
                    if (Normalize)
                    {
                        i4.X = new Range<double>(i4.Minimum[0], i4.Maximum[0]).ToRange(0, 1, j4.X);
                        i4.Y = new Range<double>(i4.Minimum[1], i4.Maximum[1]).ToRange(0, 1, j4.Y);
                        i4.Z = new Range<double>(i4.Minimum[2], i4.Maximum[2]).ToRange(0, 1, j4.Z);
                        i4.W = new Range<double>(i4.Minimum[3], i4.Maximum[3]).ToRange(0, 1, j4.W);
                    }
                    else
                    {
                        i4.DisplayX = $"{j4.X}";
                        i4.DisplayY = $"{j4.Y}";
                        i4.DisplayZ = $"{j4.Z}";
                        i4.DisplayW = $"{j4.W}";
                    }
                }
            }
        }
    }

    #endregion

    /// <see cref="Region.Method.Protected.Override"/>
    #region

    protected override void OnConstructed()
    {
        base.OnConstructed();
        Components ??= [];
        UpdateModels();
    }

    protected override Task<bool> SaveAsync(string filePath) => throw new NotImplementedException();

    #endregion

    /// <see cref="Region.Method.Public.Override"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Alpha):
                NewColor = System.Windows.Media.Color.FromArgb(Alpha, NewColor.R, NewColor.G, NewColor.B);
                break;

            case nameof(Color):
                e.OldValue.If<ColorViewModel>(i => i.ValueChanged -= OnValueChanged);
                e.NewValue.If<ColorViewModel>(i => i.ValueChanged += OnValueChanged);
                break;

            case nameof(Dimension):
                Reset(() => Dimension12);
                Reset(() => Dimension3);
                break;

            case nameof(ModelClass):
                UpdateModels();
                break;

            case nameof(NewColor):
                Reset(() => Title);
                Reset(() => ToolTip);

                FromColor(Color);
                UpdateColors();
                break;

            case nameof(Normalize):
            case nameof(Precision):
                Color.IfNotNull(i =>
                {
                    i.Normalize = Normalize;
                    i.Precision = Precision;
                });
                Colors.ForEach(i => { i.Normalize = Normalize; i.Precision = Precision; });
                break;

            case nameof(Profile):
                OnProfileChanged(new((ColorProfile)e.OldValue, (ColorProfile)e.NewValue));
                break;

            case nameof(Profiles):
                SelectedProfile = Profiles != null ? new(Profiles, 0, 0) : null;
                break;

            case nameof(SelectedComponent):
                Reset(() => Component);
                break;

            case nameof(SelectedModel):
                if (SelectedModel is ColorViewModelGroup model)
                {
                    var cIndex = SelectedComponentIndex >= 0 ? SelectedComponentIndex : 0;

                    Components ??= [];
                    Components.Clear();

                    IColor.Components[model.Value].ForEach((i, j) => Components.Add(j));

                    SelectedComponentIndex = !Is4D && SelectedComponentIndex == 3 ? 2 : cIndex;

                    Reset(() => Is4D);
                    Color = Instance.CloneDeep(DefaultColors.SourceCollection.As<ListObservable<ColorViewModel>>().First(i => i.ColorType == model.Value)).As<ColorViewModel>();

                    Reset(() => Model);
                }
                break;

            case nameof(SelectedProfile):
                e.OldValue.If<GroupItemForm>(i => i.PropertySet -= OnSelectedProfileChanged);
                e.NewValue.If<GroupItemForm>(i => i.PropertySet += OnSelectedProfileChanged);
                break;
        }
    }

    /// <see cref="Region.Method.Static"/>
    #region

    public static ListCollectionView GetColors()
    {
        ListObservable<ColorViewModel> models = new(IColor.GetTypes()
            .Select(i => ColorViewModel.New(i.Create<IColor>())));

        var result = new ListCollectionView(models);
        result.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ColorViewModel.Group)));
        result.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ColorViewModel.Group), System.ComponentModel.ListSortDirection.Ascending));
        result.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ColorViewModel.Name), System.ComponentModel.ListSortDirection.Ascending));
        return result;
    }

    public static ListCollectionView GetColorGroups(string group = null)
    {
        group ??= nameof(ColorViewModelGroup.Group);

        var models = new ListObservable<ColorViewModelGroup>();
        IColor.GetTypes().ForEach(i => models.Add(new(i)));

        var result = new ListCollectionView(models);
        result.GroupDescriptions.Add(new PropertyGroupDescription(group));
        result.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ColorViewModelGroup.Group), System.ComponentModel.ListSortDirection.Ascending));
        result.SortDescriptions.Add(new System.ComponentModel.SortDescription(nameof(ColorViewModelGroup.Name), System.ComponentModel.ListSortDirection.Ascending));
        result.Refresh();
        return result;
    }

    #endregion

    /// <see cref="ICommand"/>
    #region 

    public ICommand PickCommand
        => Commands[nameof(PickCommand)]
        ??= new RelayCommand<Color>(i => NewColor = i);

    public ICommand SaveColorCommand
        => Commands[nameof(SaveColorCommand)]
        ??= new RelayCommand(() => ColorSaved?.Invoke(this, new(NewColor)), () => true);

    #endregion

    /// <see cref="View.Main"/>
    #region 

    [Styles.Object(Ion.Template.Object,
        Filter = Filter.None,
        GroupName = MemberGroupName.None,
        NameHide = true,
        Index = 1,
        Strict = MemberTypes.All)]
    public ColorViewModel Color { get => Get<ColorViewModel>(); set => Set(value); }

    [Styles.List(Ion.Template.List,
        NameHide = true,
        Index = 2,
        ItemBullet = Text.Bullet.None,
        ItemValues = nameof(DefaultColors),
        ItemAddIconVisible = false,
        ItemAddHeaderPath = nameof(ColorViewModel.Name),
        ItemValueToolTipTemplate = nameof(ColorView.ModelToolTipTemplate),
        ItemValueToolTipTemplateType = typeof(ColorView),
        ItemAddToolTipHeaderTemplate = nameof(ColorView.ModelToolTipHeaderTemplate),
        ItemAddToolTipHeaderTemplateType = typeof(ColorView))]
    [Styles.Object(Ion.Template.Object,
        Filter = Filter.None,
        GroupName = MemberGroupName.None,
        NameHide = true,
        TargetItem = typeof(ColorViewModel))]
    public ListObservable<ColorViewModel> Colors { get => Get(new ListObservable<ColorViewModel>(DefaultModels)); set => Set(value); }

    [Styles.List(Ion.Template.ListCombo,
        Description = "The color component.",
        Float = Sides.LeftOrTop,
        NameHide = true,
        Index = 1,
        ItemToolTipHeaderSource = typeof(ColorView),
        ItemToolTipHeaderSourceKey = nameof(ColorView.ComponentToolTipHeaderTemplate),
        ItemToolTipSource = typeof(ColorView),
        ItemToolTipSourceKey = nameof(ColorView.ComponentToolTipTemplate),
        Name = "Component",
        SelectedIndexProperty = nameof(SelectedComponentIndex),
        SelectedItemProperty = nameof(SelectedComponent),
        Validate = [typeof(RequireSelectionRule)])]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(Ion.Colors.Component.Name),
        TargetItem = typeof(Component))]
    public ListObservable<Component> Components { get => Get<ListObservable<Component>>(null, false); set => Set(value, false); }

    [Style(Ion.Template.EnumFlagButton,
        Description = "The color model class.",
        Float = Sides.LeftOrTop,
        NameHide = true,
        Index = -1,
        Name = "Model class")]
    public ComponentGroup ModelClass { get => Get(ComponentGroup.H | ComponentGroup.HC | ComponentGroup.HS); set => Set(value); }

    [Styles.List(Ion.Template.ListCombo, Float = Sides.LeftOrTop, NameHide = true,
        Index = 0,
        ItemToolTipPath = nameof(ColorViewModelGroup.Value),
        ItemToolTipSource = typeof(ColorView),
        ItemToolTipSourceKey = nameof(ColorView.ModelToolTipTemplate),
        Description = "The color model.",
        Name = "Model",
        SelectedIndexProperty = nameof(SelectedModelIndex),
        SelectedItemProperty = nameof(SelectedModel),
        Validate = [typeof(RequireSelectionRule)])]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(ColorViewModelGroup.Value) + "." + nameof(Type.Name),
        TargetItem = typeof(ColorViewModelGroup))]
    public ListCollectionView Models { get => Get<ListCollectionView>(null, false); set => Set(value, false); }

    [Style(NameHide = true,
        Index = int.MinValue + 1,
        Name = "Color",
        Pin = Sides.LeftOrTop,
        Options = Option.All,
        ToolTipSource = typeof(ColorView),
        ToolTipSourceKey = nameof(ColorView.ToolTipTemplate),
        Validate = [typeof(RequireRule)])]
    public Color NewColor { get => Get(DefaultNewColor, ConvertColorToString); set => Set(value, ConvertColorToString); }

    [Style(NameHide = true,
        Index = int.MinValue,
        Name = "Color",
        Pin = Sides.LeftOrTop,
        Template = nameof(PreviewTemplate),
        TemplateType = typeof(ColorDocument))]
    public Color OldColor { get => Get(DefaultOldColor, ConvertColorToString); set => Set(value, ConvertColorToString); }

    [Name("Profile")]
    [Style(Index = -1, Options = Option.All)]
    public GroupItemForm SelectedProfile { get => Get<GroupItemForm>(null, false); set => Set(value, false); }

    [Image(Images.Revert)]
    [Styles.Button(Ion.Template.ButtonCancel,
        CommandImage = Images.Revert,
        CommandText = "Revert",
        NameHide = true,
        Index = 1,
        Name = "Revert",
        Pin = Sides.RightOrBottom)]
    public ICommand RevertCommand
        => Commands[nameof(RevertCommand)] ??= new RelayCommand(() =>
        {
            (NewColor, OldColor) = (OldColor, NewColor);
        },
        () => true);

    [Styles.Button(Ion.Template.ButtonDefault,
        CommandImage = Images.Checkmark,
        CommandText = "Select",
        NameHide = true,
        Index = 0,
        Name = "Select",
        Pin = Sides.RightOrBottom)]
    public ICommand SelectCommand
        => Commands[nameof(SelectCommand)]
        ??= new RelayCommand(() => OldColor = NewColor, () => true);

    #endregion

    /// <see cref="View.Footer"/>
    #region 

    [Description("The depth of the color model.")]
    [Styles.Number(0.0, 128.0, 1.0,
        Pin = Sides.RightOrBottom,
        View = View.Footer,
        ValueFormat = NumberFormat.Default,
        Width = 128)]
    public double Depth { get => Get(.0); set => Set(value); }

    [Description("The dimension of the color model.")]
    [Style(NameHide = true,
        Index = 0,
        Pin = Sides.LeftOrTop,
        View = View.Footer)]
    public AxisType Dimension { get => Get(AxisType.Two); set => Set(value); }

    [Name("X°")]
    [Styles.Number(0.0, 360.0, 1.0,
        Index = 0,
        View = View.Footer,
        ValueFormat = NumberFormat.Default,
        Width = 86)]
    [VisibilityTrigger(nameof(Dimension3), true)]
    public double RotateX { get => Get(45.0); set => Set(value); }

    [Name("Y°")]
    [Styles.Number(0.0, 360.0, 1.0,
        Index = 1,
        View = View.Footer,
        ValueFormat = NumberFormat.Default,
        Width = 86)]
    [VisibilityTrigger(nameof(Dimension3), true)]
    public double RotateY { get => Get(45.0); set => Set(value); }

    [Name("Z°")]
    [Styles.Number(0.0, 360.0, 1.0,
        Index = 2,
        View = View.Footer,
        ValueFormat = NumberFormat.Default,
        Width = 86)]
    [VisibilityTrigger(nameof(Dimension3), true)]
    public double RotateZ { get => Get(.0); set => Set(value); }

    [Description("The shape of the color model.")]
    [Style(NameHide = true,
        Index = 1,
        Pin = Sides.LeftOrTop,
        View = View.Footer)]
    [VisibilityTrigger(nameof(Dimension12), true)]
    public Polygon2D Shape { get => Get(Polygon2D.Square); set => Set(value); }

    [Styles.Number(0.0, 5.0, 0.01,
        Index = 3,
        View = View.Footer,
        ValueFormat = NumberFormat.Percent,
        Width = 128)]
    [VisibilityTrigger(nameof(Dimension3), true)]
    public double Zoom { get => Get(1.8); set => Set(value); }

    #endregion
}