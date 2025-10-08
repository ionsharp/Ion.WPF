using System.Windows.Controls;

namespace Ion.Validation;

public abstract class Rule<T> : ValidationRule
{
    protected abstract T Parse(string i);
}