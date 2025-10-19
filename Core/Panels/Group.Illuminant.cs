using Ion;
using Ion.Collect;
using Ion.Colors;
using Ion.Core;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ion.Controls;

/// <inheritdoc/>
[Description("Manage groups of illuminants.")]
[Image(Images.LightBulb)]
[Name("Illuminant")]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
public record class IlluminantPanel : DataGroupPanel<Vector2>
{
    [TabView(View = Ion.View.Main)]
    private new enum Tab { }

    /// <see cref="Region.Field"/>

    public static readonly new ResourceKey Template = new();

    public override string ItemName => "Illuminant";

    public IlluminantPanel() : base() { }

    public IlluminantPanel(IListWritable input) : base(input) { }

    protected override IEnumerable<ItemGroup<Vector2>> GetDefaultGroups()
    {
        yield return new ItemGroup<Vector2>("Daylight (2°)",
            typeof(Illuminant2).GetProperties().Where(i => i.Name.StartsWith('D')).Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));
        yield return new ItemGroup<Vector2>("Daylight (10°)",
            typeof(Illuminant10).GetProperties().Where(i => i.Name.StartsWith('D')).Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));

        yield return new ItemGroup<Vector2>("Equal energy",
            typeof(Illuminant).GetProperties().Where(i => i.Name == "E").Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));

        yield return new ItemGroup<Vector2>("Flourescent (2°)",
            typeof(Illuminant2).GetProperties().Where(i => i.Name.StartsWith('F')).Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));
        yield return new ItemGroup<Vector2>("Flourescent (10°)",
            typeof(Illuminant10).GetProperties().Where(i => i.Name.StartsWith('F')).Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));

        yield return new ItemGroup<Vector2>("Incandescent (2°)",
            typeof(Illuminant2).GetProperties().Where(i => i.Name == "A" || i.Name == "B" || i.Name == "C").Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));
        yield return new ItemGroup<Vector2>("Incandescent (10°)",
            typeof(Illuminant10).GetProperties().Where(i => i.Name == "A" || i.Name == "B" || i.Name == "C").Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));

        yield return new ItemGroup<Vector2>("LED (2°)",
            typeof(Illuminant2).GetProperties().Where(i => i.Name.StartsWith("LED")).Select(i => new Item<Vector2>(i.Name, Instance.GetDescription(i), (Vector2)i.GetValue(null))));
    }

    protected override object GetDefaultItem() => Illuminant.E;

    protected override Images GetItemIcon() => Images.LightBulb;
}