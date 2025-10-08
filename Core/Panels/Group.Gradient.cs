using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Input;
using Ion.Numeral;
using Ion.Reflect;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Description("Manage groups of gradients.")]
[Name("Gradient"), Image(Images.Gradient)]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
public record class GradientPanel(IListWritable input) : DataGroupPanel<Gradient>(input)
{
    private enum Group { Add }

    [TabView(View = Ion.View.Main)]
    private new enum Tab { }

    /// <see cref="Region.Field"/>

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Property"/>

    public override string ItemName => "Gradient";

    /// <see cref="Region.Constructor"/>

    public GradientPanel() : this(default(IListWritable)) { }

    /// <see cref="Region.Method"/>

    protected override IEnumerable<ItemGroup<Gradient>> GetDefaultGroups()
    {
        yield return new ItemGroup<Gradient>("Neutral",
            Gradient.New(ByteVector4.Neutral));
        yield return new ItemGroup<Gradient>("Primary",
            Gradient.New(typeof(Colors1)));
        yield return new ItemGroup<Gradient>("Secondary",
            Gradient.New(typeof(Colors2)));
        yield return new ItemGroup<Gradient>("Tertiary",
            Gradient.New(typeof(Colors3)));
    }

    protected override Images GetItemIcon() => Images.Gradient;

    private ICommand generateCommand;
    [Group(Group.Add)]
    [Image(Images.Plus), Name("Generate")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand GenerateCommand => generateCommand ??= new RelayCommand(() =>
    {
        var form1 = new GradientForm([ByteVector4.White, ByteVector4.Black]);
        var form2 = new GroupValueForm<object>(Groups, form1, 0);

        Dialog.ShowObject("Generate gradients", form2, Resource.GetImageUri(Images.Plus), i =>
        {
            if (i == 0 && form2.Group is ItemGroup<Gradient> group)
                Gradient.New(form1.Colors).ForEach(j => group.Add(NewItemName, j));
        },
        Buttons.SaveCancel);
    },
    () => Groups?.Count > 0);

    private ICommand resetCommand;
    public ICommand ResetCommand => resetCommand ??= new RelayCommand<Gradient>(i => i.Reset(), i => i != null);
}