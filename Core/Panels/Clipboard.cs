using Ion.Controls;
using Ion.Reflect;
using System;

namespace Ion.Core;

[Description("Store a single instance of any type.")]
[Image(Images.Paste), Name("Clipboard")]
public record class ClipboardPanel : DataGridPanel<CacheListItem>
{
    /// <see cref="Region.Key"/>

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="DataPanel"/>

    public override bool CanAdd => false;

    public override bool CanAddFromPreset => false;

    public override bool CanClone => false;

    public override bool CanCopy => false;

    public override bool CanCopyTo => false;

    public override bool CanCut => false;

    public override bool CanGroup => false;

    public override bool CanMoveTo => false;

    public override bool CanPaste => false;

    /// <see cref="DataGridPanel"/>

    public override bool CanAddRows => false;

    public override bool CanResizeRows => false;

    #endregion

    /// <see cref="Region.Constructor"/>

    public ClipboardPanel() : base() => Items = Appp.Cache;
}