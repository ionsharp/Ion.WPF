using Ion.Core;

namespace Ion.Collect;

public class GroupListWritable<T> : ListWritable<ItemGroup<T>>
{
    public GroupListWritable() : base() { }

    public GroupListWritable(string folderPath, string fileName, string fileExtension, ListWritableLimit limit) : base(folderPath, fileName, fileExtension) => Limit = limit;
}