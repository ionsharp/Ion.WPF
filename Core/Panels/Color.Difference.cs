using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Data;
using Ion.Numeral;
using Ion.Reflect;
using System;

namespace Ion.Core;

/// <inheritdoc/>
[Styles.Object(Image = Images.Ruler, Name = "Difference",
    Description = "Calcuate the distance of color.",
    Filter = Filter.None)]
public record class ColorDifferencePanel : Panel
{
    private enum Group { A, B, Difference }

    /// <see cref="Region.Key"/>

    public static readonly new ResourceKey Template = new();

    [Group(Group.A)]
    [Name("Color")]
    [Style(Index = 1, Orientation = Orient.Horizontal)]
    public ByteVector4 Color1 { get => Get(ByteVector4.Black); set => Set(value); }

    [Group(Group.A)]
    [Name("Profile")]
    [Style(Index = 0)]
    public GroupItemForm Profile1 { get => Get<GroupItemForm>(); set => Set(value); }

    [Group(Group.B)]
    [Name("Color")]
    [Style(Index = 1, Orientation = Orient.Horizontal)]
    public ByteVector4 Color2 { get => Get(ByteVector4.White); set => Set(value); }

    [Group(Group.B)]
    [Name("Profile")]
    [Style(Index = 0)]
    public GroupItemForm Profile2 { get => Get<GroupItemForm>(); set => Set(value); }

    [Group(Group.Difference)]
    [Style(CanEdit = false)]
    public double Difference { get => Get(.0); set => Set(value); }

    public IListWritable Profiles { get => Get<IListWritable>(); set => Set(value); }

    public int SelectedTypeIndex { get => Get(-1); set => Set(value); }

    public object SelectedType { get => Get<object>(); set => Set(value); }

    private IColorDifference Type;

    [Styles.List(Ion.Template.ListCombo, Pin = Sides.LeftOrTop,
        NameHide = true,
        Name = "Type",
        SelectedIndexProperty = nameof(SelectedTypeIndex),
        SelectedItemProperty = nameof(SelectedType))]
    [Styles.Text(CanEdit = false,
        ValueConvert = typeof(ConvertAttributeName),
        TargetItem = typeof(Type))]
    public static object Types => new ListObservable<Type>()
    {
        typeof(CIE76ColorDifference),
        typeof(CIE94ColorDifference),
        typeof(CIEDE2000ColorDifference),
        typeof(CMCColorDifference),
        typeof(EuclideanColorDifference),
        typeof(JzCzhzDEzColorDifference),
    };

    public ColorDifferencePanel() : base() { }

    public ColorDifferencePanel(IListWritable profiles) : this() => Profiles = profiles;

    private void Update()
    {
        if (Type != null)
        {
            var profile1 = Profile1?.SelectedItem?.As<ColorProfile>() ?? ColorProfile.Default;
            var profile2 = Profile2?.SelectedItem?.As<ColorProfile>() ?? ColorProfile.Default;

            Try.Do(() =>
            {
                var a = XColorVector.Convert(Color1);
                var b = XColorVector.Convert(Color2);

                if (Type is CIE76ColorDifference || Type is CIE94ColorDifference || Type is CIEDE2000ColorDifference || Type is CMCColorDifference)
                    Difference = Type.ComputeDifference(new RGB(a).To<Lab>(profile1), new RGB(b).To<Lab>(profile2));

                else if (Type is JzCzhzDEzColorDifference)
                    Difference = Type.ComputeDifference(new RGB(a).To<LCHabj>(profile1), new RGB(b).To<LCHabj>(profile2));

                else if (Type is EuclideanColorDifference)
                    Difference = Type.ComputeDifference(new RGB(a), new RGB(b));
            },
            e => Analysis.Log.Write(e));
        }
    }

    private void OnProfileChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => Update();

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Color1):
            case nameof(Color2):
            case nameof(Type):
                Update(); break;

            case nameof(Profile1):
            case nameof(Profile2):
                e.OldValue.If<IPropertySet>(i => i.PropertyChanged -= OnProfileChanged);
                e.NewValue.If<IPropertySet>(i => i.PropertyChanged += OnProfileChanged);
                Update();
                break;

            case nameof(Profiles):
                Profile1 = new(Profiles, 0, 0);
                Profile2 = new(Profiles, 0, 0);
                break;

            case nameof(SelectedType):
                SelectedType.If<Type>(i => Type = i.Create<IColorDifference>());
                Update();
                break;
        }
    }
}