using Ion.Collect;
using Ion.Core;

namespace Ion.Controls;

public class DockAnchorDocumentCollection(DockRootControl root) : ListObservable<Document>, IDockContentSource
{
    public DockRootControl Root { get; private set; } = root;

    IListObservable IDockContentSource.Source => this;
}