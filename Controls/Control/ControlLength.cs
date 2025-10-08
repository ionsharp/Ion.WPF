using System;
using System.Windows;
using System.Xml.Serialization;

namespace Ion.Controls;

[Serializable]
public class ControlLength
{
    public static readonly ControlLength Auto = new(1, ControlLengthUnit.Auto);

    public static readonly ControlLength Default = Star;

    public static readonly ControlLength Star = new(1, ControlLengthUnit.Star);

    public static readonly ControlLength Zero = new(0, ControlLengthUnit.Pixel);

    [XmlAttribute]
    public ControlLengthUnit Unit { get; set; } = ControlLengthUnit.Star;

    [XmlAttribute]
    public double Value { get; set; } = 1;

    public ControlLength() { }

    public ControlLength(double value, ControlLengthUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    public static implicit operator GridLength(ControlLength input) => new(input.Value, (GridUnitType)Enum.Parse(typeof(GridUnitType), input.Unit.ToString()));

    public static implicit operator ControlLength(GridLength input) => new(input.Value, (ControlLengthUnit)Enum.Parse(typeof(ControlLengthUnit), input.GridUnitType.ToString()));

    public static implicit operator ControlLength(string input)
    {
        if (input.ToLower() == "auto")
            return Auto;

        if (input.IsNumeric())
        {
            double.TryParse(input, out double result);
            return new ControlLength(result, ControlLengthUnit.Pixel);
        }

        if (input == "*")
            return Star;

        if (input.EndsWith("*"))
        {
            var number = new char[input.Length - 1];
            for (var i = 0; i < input.Length - 1; i++)
                number[i] = input[i];

            double.TryParse(new string(number), out double result);
            return new ControlLength(result, ControlLengthUnit.Star);
        }

        return Default;
    }

    public static implicit operator string(ControlLength input)
    {
        return input.Unit switch
        {
            ControlLengthUnit.Auto => "Auto",
            ControlLengthUnit.Pixel => $"{input.Value}",
            ControlLengthUnit.Star => $"{input.Value}*",
            _ => throw new InvalidOperationException(),
        };
    }
}