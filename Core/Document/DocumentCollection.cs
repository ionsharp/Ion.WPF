using Ion.Collect;
using System;
using System.Collections.Specialized;

namespace Ion.Core;

public class DocumentCollection : ListObservable<Document>
{
    public DocumentCollection() : base() { }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                e.NewItems?.ForEach<Document>(i => { i.Unsubscribe(); i.Subscribe(); });
                break;

            case NotifyCollectionChangedAction.Move:
                break;

            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Reset:
                e.OldItems?.ForEach<Document>(i => { i.Unsubscribe(); });
                break;

            case NotifyCollectionChangedAction.Replace:
                break;
        }
    }
}