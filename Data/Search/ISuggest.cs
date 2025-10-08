namespace Ion.Data;

public interface ISuggest
{
    bool UpdateOnFocus { get; }

    bool UpdateOnChange { get; }

    string Convert(object input);

    bool Handle(object input, string text);
}