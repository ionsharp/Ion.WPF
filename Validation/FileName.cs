using Ion.Storage;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ion.Validation;

public class FileNameRule : ValidationRule
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

            //Old
            //var result = Path.GetInvalidFileNameChars().Aggregate(result, (i, j) => i.Replace(j.ToString(), ""));
            //if (result != clean)

            if (b.Length > 0)
                return new ValidationResult(false, new FileNotValidName(b.ToList().ToString(", ", c => c.ToString())).Message);
        }
        return new ValidationResult(true, null);
    }
}