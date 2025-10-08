using Ion.Collect;
using System.Windows.Controls;

namespace Ion.Controls;

public class DockPanelBar : ToolBar, IDockPanelSource
{
    public DockRootControl Root { get; private set; }

    public IListObservable Source => ItemsSource as IListObservable;

    public DockPanelBar() : base()
    {
        this.AddHandler(i => Root = this.GetParent<DockRootControl>());
    }
}