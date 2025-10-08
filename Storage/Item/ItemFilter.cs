using Ion.Core;
using Ion.Reflect;

namespace Ion.Storage;

[Name("Filter")]
public record class ItemFilter : Model
{
    private enum Category { File, Folder }

    [Group(Category.File)]
    [Description("Whether or not to include or exclude files with the given attributes.")]
    [Name("Attributes")]
    [Style(Template.EnumFlag)]
    public ItemAttributes FileAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Group(Category.File)]
    [Description("The file extensions to include or exclude.")]
    [Name("Extensions")]
    [Styles.Object(GroupName = MemberGroupName.None)]
    public FilterExtensions FileExtensions { get => Get(new FilterExtensions()); set => Set(value); }

    [Group(Category.Folder)]
    [Description("Whether or not to include or exclude folders with the given attributes.")]
    [Name("Attributes")]
    [Style(Template.EnumFlag)]
    public ItemAttributes FolderAttributes { get => Get(ItemAttributes.All); set => Set(value); }

    [Group(Category.Folder)]
    [Description("The folder extensions to include or exclude.")]
    [Name("Extensions")]
    [Styles.Object(GroupName = MemberGroupName.None)]
    public FilterExtensions FolderExtensions { get => Get(new FilterExtensions()); set => Set(value); }
}