using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Input;
using Ion.Media;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Ion.Core;

/// <inheritdoc/>
[Description("Manage groups of colors.")]
[Image(Images.Colors)]
[Name("Color")]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
public record class ColorPanel : DataGroupPanel<ByteVector4>
{
    private enum Group { Detail }

    [TabView(View = Ion.View.Main)]
    private new enum Tab { }

    /// <see cref="Region.Event"/>

    [field: NonSerialized]
    public event EventHandler<EventArgs<ByteVector4>> Selected;

    /// <see cref="Region.Field"/>

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Property"/>

    public override string ItemName => "Color";

    /// <see cref="Region.Constructor"/>

    public ColorPanel() : base() => ShowDescription = false;

    public ColorPanel(IListWritable input) : base(input) { }

    /// <see cref="Region.Method"/>

    protected override IEnumerable<ItemGroup<ByteVector4>> GetDefaultGroups()
    {
        yield return new PrimaryColors();
        yield return new SecondaryColors();
        yield return new TertiaryColors();
        yield return new QuaternaryColors();
        yield return new QuinaryColors();

        yield return new ColorGroupCollection("CSS",
            typeof(ColorPreset.CSS));
        yield return new ColorGroupCollection("Web (Basic)",
            typeof(ColorPreset.WebBasic));
        yield return new ColorGroupCollection("Web (Safe)",
            ColorPreset.WebSafe.Colors.Select(i => new Item<ByteVector4>(new ByteVector4(i).GetName(), new ByteVector4(i))));
        yield return new ColorGroupCollection("Web (Safest)",
            typeof(ColorPreset.WebSafest));
    }

    protected override Images GetItemIcon() => Images.Color;

    protected virtual void OnSelected(ByteVector4 input) => Selected?.Invoke(this, new EventArgs<ByteVector4>(input));

    /// <see cref="ICommand"/>

    private ICommand selectColorCommand;
    public ICommand SelectColorCommand => selectColorCommand ??= new RelayCommand<ByteVector4>(OnSelected);
}