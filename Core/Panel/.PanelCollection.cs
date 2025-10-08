using Ion.Collect;
using System.Collections.Specialized;

namespace Ion.Core;

public class PanelCollection() : ListObservable<Panel>()
{
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                e.NewItems?.ForEach<Panel>(i => { i.Unsubscribe(); i.Subscribe(); });
                break;

            case NotifyCollectionChangedAction.Move:
                break;

            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                e.OldItems?.ForEach<Panel>(i => { i.Unsubscribe(); });
                break;

            case NotifyCollectionChangedAction.Replace:
                break;
        }
    }
}