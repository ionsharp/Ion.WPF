using Ion.Collect;
using Ion.Core;

namespace Ion.Storage;

public record class FileExtensionGroup : Model
{
    public const string Wild = "*";

    public bool IsWild => FileExtensions.Contains("*");

    public ListObservableOfString FileExtensions { get => Get(new ListObservableOfString()); private set => Set(value); }

    public FileExtensionGroup(params string[] fileExtensions)
    {
        if (fileExtensions?.Length > 0)
        {
            foreach (var i in fileExtensions)
                FileExtensions.Add(i);
        }
    }
}

public class FileExtensionGroups : ListObservable<FileExtensionGroup>
{
    public void Add(params string[] fileExtensions) => Add(new FileExtensionGroup(fileExtensions));
}