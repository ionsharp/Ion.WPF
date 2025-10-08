using System.Windows.Data;

namespace Ion.Data;

[Extend<Binding>]
public static class XBinding
{
    public static Binding Clone(this Binding i) => new Binding()
    {
        IsAsync
            = i.IsAsync,
        AsyncState
            = i.AsyncState,
        BindingGroupName
            = i.BindingGroupName,
        BindsDirectlyToSource
            = i.BindsDirectlyToSource,
        Converter
            = i.Converter,
        ConverterCulture
            = i.ConverterCulture,
        ConverterParameter
            = i.ConverterParameter,
        Delay
            = i.Delay,
        ElementName
            = i.ElementName,
        FallbackValue
            = i.FallbackValue,
        Mode
            = i.Mode,
        NotifyOnSourceUpdated
            = i.NotifyOnSourceUpdated,
        NotifyOnTargetUpdated
            = i.NotifyOnTargetUpdated,
        NotifyOnValidationError
            = i.NotifyOnValidationError,
        Path
            = i.Path,
        RelativeSource
            = i.RelativeSource,
        Source
            = i.Source,
        StringFormat
            = i.StringFormat,
        TargetNullValue
            = i.TargetNullValue,
        UpdateSourceExceptionFilter
            = i.UpdateSourceExceptionFilter,
        UpdateSourceTrigger
            = i.UpdateSourceTrigger,
        ValidatesOnDataErrors
            = i.ValidatesOnDataErrors,
        ValidatesOnExceptions
            = i.ValidatesOnExceptions,
        ValidatesOnNotifyDataErrors
            = i.ValidatesOnNotifyDataErrors,
        XPath
            = i.XPath,
    };
}