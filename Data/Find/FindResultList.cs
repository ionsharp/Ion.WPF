using Ion.Collect;

namespace Ion.Core;

public class FindResultList(string findText) : ListObservable<FindResult>
{
    public string FindText { get; private set; } = findText;
}