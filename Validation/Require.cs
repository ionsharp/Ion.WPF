using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class RequireRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();
        if (string.IsNullOrEmpty(result))
            return new ValidationResult(false, new Require().Message);

        return new ValidationResult(true, null);
    }
}