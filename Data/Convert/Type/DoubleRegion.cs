using Ion.Numeral.Models;

namespace Ion.Data;

public class DoubleRegionTypeConverter : StringTypeConverter<double>
{
    protected override int? Length => 4;

    protected override double Convert(string input) => double.Parse(input);

    protected override object Convert(double[] input) => new MArea<double>(input[0], input[1], input[2], input[3]);
}