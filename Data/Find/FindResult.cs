using Ion.Text;

namespace Ion.Core;

public record class FindResult : Model
{
    public int Column { get => Get(0); set => Set(value); }

    public int Index { get => Get(0); set => Set(value); }

    [Filter(Filter.Search | Filter.Sort)]
    public int Line { get => Get(0); set => Set(value); }

    [Filter(Filter.Group | Filter.Search | Filter.Sort)]
    public IFind File { get => Get<IFind>(); set => Set(value); }

    [Filter(Filter.Search | Filter.Sort)]
    public string Text { get => Get(""); set => Set(value); }

    public FindResult(IFind target, int index, string text, int line, int column) : base()
    {
        File
            = target;
        Index
            = index;
        Text
            = text;
        Line
            = line;
        Column
            = column;
    }
}