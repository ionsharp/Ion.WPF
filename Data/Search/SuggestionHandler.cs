namespace Ion.Data;

public class SuggestionHandler : ISuggest
{
    public virtual bool UpdateOnFocus => true;

    public virtual bool UpdateOnChange => default;

    public virtual string Convert(object input) => $"{input}";

    public virtual bool Handle(object input, string text) => true;
}

public class AnySuggestionHandler : SuggestionHandler
{
    public override bool UpdateOnChange => false;

    public override bool Handle(object input, string text) => true;
}

public class StartsWithSuggestionHandler : SuggestionHandler
{
    public override bool UpdateOnChange => true;

    public override bool Handle(object input, string text) => Convert(input).ToLower().StartsWith(text.ToLower());
}