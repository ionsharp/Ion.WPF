using Ion.Analysis;
using Ion.Controls;
using System;

namespace Ion.Core;

[Name("Options")]
[Image(Images.Options)]
public record class OptionPanel : ObjectPanel
{
    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Constructor"/>

    public OptionPanel() : base() { Source = Appp.Model.Data; }

    /// <see cref="Region.Method"/>

    [Image(Images.Save), Name("Save")]
    [Style(View = View.HeaderOption)]
    public static Result Save() => Appp.Model.Data.Serialize();
}