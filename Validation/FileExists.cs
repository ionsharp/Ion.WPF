using Ion.Storage;
using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class FileExistsRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            if (!File.Exists(result))
                return new ValidationResult(false, new FileNotFound(result).Message);
        }
        return new ValidationResult(true, null);
    }
}