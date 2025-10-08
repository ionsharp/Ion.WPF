using Ion.Numeral;

namespace Ion.Data;

public class DegreeRangeTypeConverter : StringTypeConverter<Angle>
{
    protected override int? Length => 2;

    protected override Angle Convert(string input) => (Angle)double.Parse(input);

    protected override object Convert(Angle[] input) => new Range<Angle>(input[0], input[1]);
}