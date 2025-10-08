using Ion.Storage;

namespace Ion.Controls;

public class LayoutList(string folderPath) : PathList(folderPath, new Storage.Filter(ItemType.File, "xml"))
{
    public override string ItemName => "Layout";
}