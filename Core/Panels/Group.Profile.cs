using Ion;
using Ion.Collect;
using Ion.Colors;
using Ion.Core;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ion.Controls;

/// <inheritdoc/>
[Description("Manage groups of working profiles.")]
[Image(Images.Channels)]
[Name("Profile")]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
public record class ProfilePanel : DataGroupPanel<ColorProfile>
{
    [TabView(View = Ion.View.Main)]
    private new enum Tab { }

    /// <see cref="Region.Field"/>

    public static readonly new ResourceKey Template = new();

    public override string ItemName => "Profile";

    public ProfilePanel() : base() { }

    public ProfilePanel(IListWritable input) : base(input) { }

    protected override IEnumerable<ItemGroup<ColorProfile>> GetDefaultGroups()
    {
        var profiles = new List<ItemGroup<ColorProfile>>();
        typeof(ColorProfiles).GetProperties().GroupBy(i => i.GetGroup()).ForEach(i => profiles.Add(new ItemGroup<ColorProfile>(i.Key, i.Select(j => new Item<ColorProfile>(Instance.GetName(j), Instance.GetDescription(j), (ColorProfile)j.GetValue(null))))));
        foreach (var i in profiles)
            yield return i;
    }

    protected override object GetDefaultItem() => ColorProfile.Default;

    protected override Images GetItemIcon() => Images.Channels;
}