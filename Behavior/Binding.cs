using Ion.Collect;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Ion.Behavior;

public class BindingBehavior : Behavior<DependencyObject>
{
    public DependencyProperty DataProperty { get; set; }

    private static readonly DependencyPropertyKey ActualConverterKey = DependencyProperty.RegisterReadOnly(nameof(ActualConverter), typeof(IValueConverter), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public static readonly DependencyProperty ActualConverterProperty = ActualConverterKey.DependencyProperty;
    public IValueConverter ActualConverter
    {
        get => (IValueConverter)GetValue(ActualConverterProperty);
        private set => SetValue(ActualConverterKey, value);
    }

    public static readonly DependencyProperty ConverterProperty = DependencyProperty.Register(nameof(Converter), typeof(Type), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnConverterChanged));
    public Type Converter
    {
        get => (Type)GetValue(ConverterProperty);
        set => SetValue(ConverterProperty, value);
    }

    private static void OnConverterChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<BindingBehavior>().OnConverterChanged(e);

    public static readonly DependencyProperty ConverterParameterProperty = DependencyProperty.Register(nameof(ConverterParameter), typeof(object), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public object ConverterParameter
    {
        get => GetValue(ConverterParameterProperty);
        set => SetValue(ConverterParameterProperty, value);
    }

    public static readonly DependencyProperty ConverterSelectorProperty = DependencyProperty.Register(nameof(ConverterSelector), typeof(ConverterSelector), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public ConverterSelector ConverterSelector
    {
        get => (ConverterSelector)GetValue(ConverterSelectorProperty);
        set => SetValue(ConverterSelectorProperty, value);
    }

    public static readonly DependencyProperty ConverterSelectorKeyProperty = DependencyProperty.Register(nameof(ConverterSelectorKey), typeof(object), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnConverterSelectorKeyChanged));
    public object ConverterSelectorKey
    {
        get => (object)GetValue(ConverterSelectorKeyProperty);
        set => SetValue(ConverterSelectorKeyProperty, value);
    }

    private static void OnConverterSelectorKeyChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<BindingBehavior>().OnConverterSelectorKeyChanged(e);

    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(nameof(Mode), typeof(BindMode), typeof(BindingBehavior), new FrameworkPropertyMetadata(BindMode.OneWay, OnPropertyChanged));
    public BindMode Mode
    {
        get => (BindMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty NotifyOnValidationErrorProperty = DependencyProperty.Register(nameof(NotifyOnValidationError), typeof(bool), typeof(BindingBehavior), new FrameworkPropertyMetadata(false, OnPropertyChanged));
    public bool NotifyOnValidationError
    {
        get => (bool)GetValue(NotifyOnValidationErrorProperty);
        set => SetValue(NotifyOnValidationErrorProperty, value);
    }

    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(PropertyPath), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public PropertyPath Path
    {
        get => (PropertyPath)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(nameof(Property), typeof(DependencyProperty), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public DependencyProperty Property
    {
        get => (DependencyProperty)GetValue(PropertyProperty);
        set => SetValue(PropertyProperty, value);
    }

    public static readonly DependencyProperty PropertyGenericProperty = DependencyProperty.Register(nameof(PropertyGeneric), typeof(object), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnGenericPropertyChanged));
    public object PropertyGeneric
    {
        get => GetValue(PropertyGenericProperty);
        set => SetValue(PropertyGenericProperty, value);
    }
    private static void OnGenericPropertyChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        if (i is BindingBehavior j)
        {
            if (e.NewValue is DependencyPropertyGeneric k)
                j.SetCurrentValue(PropertyProperty, k.Property);
        }
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public object Source
    {
        get => (object)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty SourceTriggerProperty = DependencyProperty.Register(nameof(SourceTrigger), typeof(BindTrigger), typeof(BindingBehavior), new FrameworkPropertyMetadata(BindTrigger.Default, OnPropertyChanged));
    public BindTrigger SourceTrigger
    {
        get => (BindTrigger)GetValue(SourceTriggerProperty);
        set => SetValue(SourceTriggerProperty, value);
    }

    public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    public static readonly DependencyProperty ValidatesOnNotifyDataErrorsProperty = DependencyProperty.Register(nameof(ValidatesOnNotifyDataErrors), typeof(bool), typeof(BindingBehavior), new FrameworkPropertyMetadata(false, OnPropertyChanged));
    public bool ValidatesOnNotifyDataErrors
    {
        get => (bool)GetValue(ValidatesOnNotifyDataErrorsProperty);
        set => SetValue(ValidatesOnNotifyDataErrorsProperty, value);
    }

    public static readonly DependencyProperty ValidatesOnDataErrorsProperty = DependencyProperty.Register(nameof(ValidatesOnDataErrors), typeof(bool), typeof(BindingBehavior), new FrameworkPropertyMetadata(false, OnPropertyChanged));
    public bool ValidatesOnDataErrors
    {
        get => (bool)GetValue(ValidatesOnDataErrorsProperty);
        set => SetValue(ValidatesOnDataErrorsProperty, value);
    }

    public static readonly DependencyProperty ValidationRulesProperty = DependencyProperty.Register(nameof(ValidationRules), typeof(ListObservable<ValidationRule>), typeof(BindingBehavior), new FrameworkPropertyMetadata(null, OnPropertyChanged));
    public ListObservable<ValidationRule> ValidationRules
    {
        get => (ListObservable<ValidationRule>)GetValue(ValidationRulesProperty);
        set => SetValue(ValidationRulesProperty, value);
    }

    private static void OnPropertyChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<BindingBehavior>().OnPropertyChanged();

    public BindingBehavior() : base() => SetCurrentValue(ValidationRulesProperty, new ListObservable<ValidationRule>());

    protected override void OnAttached() => OnPropertyChanged();

    protected virtual void OnConverterChanged(Value<object> input)
    {
        if (Converter is not null)
            ActualConverter = ValueConverter.Cache[Converter];

        OnPropertyChanged();
    }

    protected virtual void OnConverterSelectorKeyChanged(Value<object> input)
    {
        var container = ConverterSelector?.SelectTemplate(input.NewValue);
        if (container != null)
        {
            this.Unbind(ConverterParameterProperty);

            ActualConverter = ValueConverter.Cache[container.Converter];
            this.Bind(ConverterParameterProperty, nameof(ConverterTemplate.ConverterParameter), container);
            SetCurrentValue(ConverterParameterProperty, container.ConverterParameter);
        }
        else
        {
            ActualConverter = ConverterSelector?.SelectConverter(input.NewValue);
        }

        OnPropertyChanged();
    }

    protected virtual void OnPropertyChanged()
    {
        if (AssociatedObject != null)
        {
            if (Mode == BindMode.TwoWay && Path is null)
                return;

            AssociatedObject.Unbind(Property);
            var result = new Binding()
            {
                Converter
                    = ActualConverter,
                ConverterParameter
                    = ConverterParameter,
                Mode
                    = Mode.ToString().Parse<BindingMode>(),
                NotifyOnValidationError
                    = NotifyOnValidationError,
                Path
                    = Path,
                Source
                    = Source,
                StringFormat
                    = StringFormat,
                UpdateSourceTrigger
                    = SourceTrigger.ToString().Parse<UpdateSourceTrigger>(),
                ValidatesOnNotifyDataErrors
                    = ValidatesOnNotifyDataErrors,
                ValidatesOnDataErrors
                    = ValidatesOnDataErrors
            };
            ValidationRules?.ForEach(result.ValidationRules.Add);
            AssociatedObject.Bind(Property, result);
        }
    }
}