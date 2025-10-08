using Ion.Data;
using System.Windows.Data;

namespace Ion.Storage;

public class ItemGroupConverterSelector : ConverterSelector
{
    public ItemGroupConverterSelector() : base() { }

    public override IValueConverter SelectConverter(object input)
    {
        return $"{input}" switch
        {
            nameof(Item.IsHidden)
                => new ValueConverter<Item, string>(i => i.Value.IsHidden ? "Hidden" : "Visible"),
            nameof(Item.IsReadOnly)
                => new ValueConverter<Item, string>(i => i.Value.IsReadOnly ? "Read-only" : "Not read-only"),
            nameof(Item.Name)
                => new ValueConverter<Item, string>(i => ValueConverter.Cache.Get<ConvertToStringWithFirstLetter>().Convert(i.Value.Name, null, null, null)?.ToString()),
            nameof(Item.Type)
                => new ValueConverter<Item, string>(i => XItemPath.GetFriendlyDescription(i.Value.Path)),
            _ => default,
        };
    }
}