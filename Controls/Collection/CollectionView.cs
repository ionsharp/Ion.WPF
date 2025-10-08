using Ion.Collect;
using System.Collections;
using System.Windows.Data;

namespace Ion.Controls;

public class CollectionView<T>() : object()
{
    public ListObservable<T> Source { get; private set; }

    public ListCollectionView View { get; private set; }

    public CollectionView(ListObservable<T> items, IComparer sort = null) : this()
    {
        Source = items ?? [];
        View = new(Source) { CustomSort = sort };
    }
}