using Ion.Numeral;
using System.Globalization;
using System.Windows.Controls;

namespace Ion.Validation;

public class Int32GreaterThanRule : Rule32
{
    public int Value { get; set; }

    protected override ValidationResult Validate(int value, CultureInfo cultureInfo)
    {
        if (value <= Value)
            return new ValidationResult(false, $"Number must be greater than {Value.ToString(NumberFormat.Default)}.");

        return new ValidationResult(true, null);
    }
}

public class Int32GreaterThanOrEqualToRule : Rule32
{
    public int Value { get; set; }

    protected override ValidationResult Validate(int value, CultureInfo cultureInfo)
    {
        if (value < Value)
            return new ValidationResult(false, $"Number must be greater than or equal to {Value.ToString(NumberFormat.Default)}.");

        return new ValidationResult(true, null);
    }
}

public class Int32LessThanRule : Rule32
{
    public int Value { get; set; }

    protected override ValidationResult Validate(int value, CultureInfo cultureInfo)
    {
        if (value >= Value)
            return new ValidationResult(false, $"Number must be less than {Value.ToString(NumberFormat.Default)}.");

        return new ValidationResult(true, null);
    }
}

public class Int32LessThanOrEqualToRule : Rule32
{
    public int Value { get; set; }

    protected override ValidationResult Validate(int value, CultureInfo cultureInfo)
    {
        if (value > Value)
            return new ValidationResult(false, $"Number must be less than or equal to {Value.ToString(NumberFormat.Default)}.");

        return new ValidationResult(true, null);
    }
}