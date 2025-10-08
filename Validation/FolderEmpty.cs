using Ion.Storage;
using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class FolderEmptyRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string result = value?.ToString();
        if (!string.IsNullOrEmpty(result))
        {
            if (Folder.Exists(result))
            {
                try
                {
                    if (Folder.GetFiles(result).Count() == 0 && Folder.GetFolders(result).Count() == 0)
                        return new ValidationResult(false, new FolderEmpty(result).Message);
                }
                catch { }
            }
        }
        return new ValidationResult(true, null);
    }
}