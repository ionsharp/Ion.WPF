using Ion.Collect;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Ion.Data;

[ContentProperty(nameof(DefaultConverterSelector.Converters))]
public class ConverterSelector() : Freezable()
{
    public virtual ConverterTemplate SelectTemplate(object input) => null;

    public virtual IValueConverter SelectConverter(object input) => null;

    protected override Freezable CreateInstanceCore() => new ConverterSelector();
}

public class DefaultConverterSelector() : ConverterSelector()
{
    public ListObservable<ConverterTemplate> Converters { get; private set; } = [];

    public override ConverterTemplate SelectTemplate(object input)
    {
        foreach (var i in Converters)
        {
            if (Equals(input, i.DataType))
                return i;
        }
        return base.SelectTemplate(input);
    }

    public override IValueConverter SelectConverter(object input)
    {
        foreach (var i in Converters)
        {
            if (Equals(input, i.DataType))
                return ValueConverter.Cache[i.Converter];
        }
        return base.SelectConverter(input);
    }
}