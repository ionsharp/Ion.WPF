namespace Ion.Input;

public interface IKeySelector
{
    bool Compare(object input, string query);
}