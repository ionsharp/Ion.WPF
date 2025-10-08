using Ion.Collect;

namespace Ion.Controls;

public interface IDockContentSource
{
    DockRootControl Root { get; }

    IListObservable Source { get; }
}