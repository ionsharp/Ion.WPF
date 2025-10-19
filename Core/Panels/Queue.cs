using Ion.Controls;
using Ion.Storage;
using System;

namespace Ion.Core;

[Description("A queue of operations to run asynchronously (first one in, first one out).")]
[Image(Images.Queue)]
[Name("Queue")]
public record class QueuePanel : DataGridPanel<FileTask>
{
    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="DataPanel"/>

    public override bool CanAdd => false;

    public override bool CanClear => false;

    public override bool CanClone => false;

    public override bool CanCopy => false;

    public override bool CanCopyTo => false;

    public override bool CanCut => false;

    public override bool CanEdit => false;

    public override bool CanMoveTo => false;

    public override bool CanPaste => false;

    public override bool CanRefresh => false;

    public override bool CanRemove => false;

    public override bool CanAddFromPreset => false;

    /// <see cref="DataGridPanel"/>

    public override bool CanAddRows => false;

    public override bool CanDeleteRows => false;

    public override bool CanResizeColumns => false;

    public override bool CanResizeRows => false;

    #endregion

    /// <see cref="Region.Constructor"/>

    public QueuePanel() : base() { }
}