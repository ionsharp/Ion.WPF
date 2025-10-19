using Ion.Core;

namespace Ion.Collect;

public class GroupListWritable<T> : ListObservableWritable<ItemGroup<T>>
{
    public GroupListWritable() : base() { }

    public GroupListWritable(string folderPath, string fileName, string fileExtension, ListWritableLimit limit) : base()
    {
        Limit = limit;
        this.SetFile(folderPath, fileName, fileExtension);
    }
}