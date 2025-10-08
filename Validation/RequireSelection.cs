using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class RequireSelectionRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is int index)
        {
            if (index >= 0)
                return new ValidationResult(true, null);

            return new ValidationResult(false, new RequireSelection().Message);
        }
        return new ValidationResult(false, new RequireValidSelection().Message);
    }
}