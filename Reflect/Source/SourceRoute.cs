using Ion.Collect;

namespace Ion.Reflect;

public class SourceRoute() : ListObservable<MemberBase>
{
    public MemberBase Back(object parameter)
    {
        var route = this;

        MemberBase target = default;
        if (parameter is MemberBase p)
        {
            for (var i = route.Count - 1; i >= 0; i--)
            {
                var next = route[i];
                route.RemoveAt(i);

                if (next == p)
                {
                    target = p;
                    break;
                }
            }
        }
        else
        {
            for (int i = route.Count - 1, j = 0; i >= 0; i--, j++)
            {
                var next = route[i];
                route.RemoveAt(i);

                if (j == 1)
                {
                    target = next;
                    break;
                }
            }
        }

        return target;
    }
}