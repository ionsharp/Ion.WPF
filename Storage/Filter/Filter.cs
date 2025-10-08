using System.Collections.Generic;
using System.Linq;

namespace Ion.Storage;

public sealed class Filter(ItemType types = ItemType.All, params string[] extensions)
{
    public static readonly Filter Default = new(ItemType.All);

    public readonly IEnumerable<string> Extensions = extensions?.Length > 0 ? Enumerable.Select(extensions, i => i.ToLower()) : null;

    public readonly ItemType Types = types;

    public bool Evaluate(string path, ItemType type)
    {
        if (type == ItemType.File)
        {
            if (Extensions is null)
                return true;

            var fileExtension = FilePath.GetExtension(path);
            //Arbitrarily exclude files with no extensions when at least one extension is specified
            if (Extensions.Any())
            {
                if (fileExtension.TrimWhite().IsEmpty())
                    return false;
            }

            return Extensions.Contains(fileExtension) && Types.HasFlag(ItemType.File);
        }
        return Types.HasFlag(type);
    }
}