using Ion.Storage;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ion.Validation;

public class FileExtensionRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            var b = "";
            foreach (var i in result)
            {
                if (FilePath.InvalidCharacters.Contains(i))
                    b += i;
            }

            if (b.Length > 0)
                return new ValidationResult(false, new FileNotValidExtension(b.ToList().ToString(", ", c => c.ToString())).Message);
        }
        return new ValidationResult(true, null);
    }
}