namespace Ion.Input;

public class KeySelector : IKeySelector
{
    public bool Compare(object input, string query) 
        => input?.ToString().StartsWith(query, System.StringComparison.CurrentCultureIgnoreCase) == true;
}