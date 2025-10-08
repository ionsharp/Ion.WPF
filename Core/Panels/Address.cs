using Ion.Controls;
using Ion.Reflect;
using System;
using System.Reflection;

namespace Ion.Core;

[Name("Address")]
[Image(Images.Shutdown)]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab)]
[Serializable]
public record class AddressPanel : Panel
{
    [TabView(View = View.Main)]
    private enum Tab { }

    /// <see cref="Region.Key"/>

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Property"/>
    #region

    public override bool CanShare => false;

    public override bool HideIfNoActiveDocument => true;

    public override double MaxHeight => 36;

    public override bool TitleVisibility => false;

    #endregion

    /// <see cref="Region.Constructor"/>

    public AddressPanel() : base() { }
}