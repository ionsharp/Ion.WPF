using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public abstract class Rule32 : Rule<int>
{
    protected sealed override int Parse(string i)
    {
        var result = i?.ToString();
        if (!result.IsEmpty())
        {
            _ = int.TryParse(i, out int j);
            return j;
        }
        return -1;
    }

    public sealed override ValidationResult Validate(object value, CultureInfo cultureInfo)
        => Validate(Parse(value?.ToString()), cultureInfo);

    protected abstract ValidationResult Validate(int value, CultureInfo cultureInfo);
}