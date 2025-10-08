namespace Ion.Core;

[Image(Images.Dot)]
public record class ItemPreset(string name, object value = null) : Namable<object>(name, value)
{
    public const string DefaultGroup = "Default";

    public string Group { get => Get(DefaultGroup); set => Set(value); }

    public ItemPreset() : this(DefaultName, null) { }

    public ItemPreset(object value) : this(DefaultName, value) { }
}