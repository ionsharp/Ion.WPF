using Ion.Collect;

namespace Ion.Controls;

public interface IDockPanelSource
{
    DockRootControl Root { get; }

    IListObservable Source { get; }
}