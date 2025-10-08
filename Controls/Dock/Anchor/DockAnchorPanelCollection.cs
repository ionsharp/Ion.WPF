using Ion.Collect;
using Ion.Core;
using System.Collections;

namespace Ion.Controls;

public class DockAnchorPanelCollection(DockRootControl root) : ListObservable<Panel>, IDockContentSource, IDockPanelSource
{
    public DockRootControl Root { get; private set; } = root;

    IListObservable IDockContentSource.Source => this;

    IListObservable IDockPanelSource.Source => this;
}