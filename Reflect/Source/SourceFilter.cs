namespace Ion.Reflect;

public class SourceFilter(object source, View section) : object()
{
    public readonly View Section = section;

    public readonly object Source = source;
}