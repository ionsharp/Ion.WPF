using Ion.Numeral;
using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class NumberRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            foreach (var i in result)
            {
                if (i == '.' || i == '-' || Text.Characters.Numbers.Contains(i))
                    continue;

                return new ValidationResult(false, new ArgumentIsNotNumber(result).Message);
            }
        }
        return new ValidationResult(true, null);
    }
}