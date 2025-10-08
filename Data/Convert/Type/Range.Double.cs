using Ion.Numeral;

namespace Ion.Data;

public class DoubleRangeTypeConverter : StringTypeConverter<double>
{
    protected override int? Length => 2;

    protected override double Convert(string input) => double.Parse(input);

    protected override object Convert(double[] input) => new Range<double>(input[0], input[1]);
}