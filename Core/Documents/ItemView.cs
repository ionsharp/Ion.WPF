using System;
using System.Reflection;

namespace Ion.Core;

[Styles.Object(Strict = MemberTypes.All)]
[Serializable]
public record class ItemViewDocument : Document
{
    public ItemViewPanel Panel { get => Get(new ItemViewPanel() { IsAddressVisible = false }); set => Set(value); }

    public ItemViewDocument() : base() { }

    public override void Save() { }

    public override void Subscribe()
    {
        base.Subscribe();
        Panel.Subscribe();
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        Panel.Unsubscribe();
    }
}