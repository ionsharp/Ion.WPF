using System;
using System.Windows;

namespace Ion.Data;

public class GridLengthArrayTypeConverter : StringTypeConverter<GridLength>
{
    protected override int? Length => null;

    protected override GridLength Convert(string input)
    {
        switch (input.ToLower())
        {
            case "auto":
                return new GridLength(1, GridUnitType.Auto);

            case "*":
                return new GridLength(1, GridUnitType.Star);

            default:

                if (input.EndsWith("*"))
                {
                    var i = input.Replace("*", string.Empty);
                    if (i.IsNumeric())
                        return new GridLength(double.Parse(i), GridUnitType.Star);
                }

                if (input.IsNumeric())
                    return new GridLength(double.Parse(input), GridUnitType.Pixel);

                throw new NotSupportedException();
        }
    }

    protected override object Convert(GridLength[] input) => input;
}