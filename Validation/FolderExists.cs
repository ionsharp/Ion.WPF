using Ion.Storage;
using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class FolderExistsRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            if (!Folder.Exists(result))
                return new ValidationResult(false, new FolderNotFound(result).Message);
        }
        return new ValidationResult(true, null);
    }
}