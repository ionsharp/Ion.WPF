using Ion.Colors;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ion.Validation;

public class ColorModelNameRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();

        if (string.IsNullOrEmpty(result))
            return new ValidationResult(false, $"No color models were specified.");

        var models = result.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var i in models)
        {
            if (!IColor.GetTypes().Any(j => j.Name == i))
                return new ValidationResult(false, $"The color model '{i}' isn't valid.");
        }

        return new ValidationResult(true, null);
    }
}