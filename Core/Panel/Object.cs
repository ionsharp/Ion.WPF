using Ion.Controls;
using System;

namespace Ion.Core;

/// <inheritdoc/>
[Serializable]
public abstract record class ObjectPanel() : Panel()
{
    /// <see cref="Region.Field"/>

    public static readonly ResourceKey Template = new();

    /// <see cref="Region.Property"/>

    [Style(Ion.Template.Check, NameHide = true,
        Image = Images.Info,
        Name = "Show description",
        View = View.HeaderItem)]
    public override bool IsOptionDescriptionVisible { get => base.IsOptionDescriptionVisible; set => base.IsOptionDescriptionVisible = value; }

    [NonSerializable]
    public virtual object Source { get => Get<object>(null, false); set => Set(value, false); }
}