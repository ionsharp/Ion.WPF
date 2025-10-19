using Ion.Colors;
using Ion.Core;
using Ion.Reflect;
using System;

namespace Ion.Controls;

public record class ColorViewModelGroup(Type input) : NamableGroup<Type>(input.Name, input.GetAttribute<ComponentGroupAttribute>()?.Group.ToString(), input)
{
    public string FirstLetter => Name[..1].ToUpper();
}