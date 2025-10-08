using System.Windows;
using System.Windows.Markup;

namespace Ion.Controls;

[ContentProperty(nameof(Setters))]
public class Trigger() : FrameworkElement()
{
    /// <see cref="Region.Property"/>
    #region

    /// <see cref="Binding"/>
    #region

    public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(nameof(Binding), typeof(object), typeof(Trigger), new FrameworkPropertyMetadata(null, OnBindingChanged));
    public object Binding
    {
        get => (object)GetValue(BindingProperty);
        set => SetValue(BindingProperty, value);
    }
    private static void OnBindingChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<Trigger>(i => i.OnBindingChanged(e));

    #endregion

    /// <see cref="Element"/>
    #region

    internal static readonly DependencyProperty ElementProperty = DependencyProperty.Register(nameof(Element), typeof(FrameworkElement), typeof(Trigger), new FrameworkPropertyMetadata(null, OnElementChanged));
    internal FrameworkElement Element
    {
        get => (FrameworkElement)GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }
    private static void OnElementChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<Trigger>(i => i.OnElementChanged(e));

    #endregion

    /// <see cref="Setters"/>
    #region

    public static readonly DependencyProperty SettersProperty = DependencyProperty.Register(nameof(Setters), typeof(Setters), typeof(Trigger), new FrameworkPropertyMetadata(null, OnSettersChanged));
    public Setters Setters
    {
        get => this.GetValueOrSetDefault<Setters>(SettersProperty, () => []);
        set => SetValue(SettersProperty, value);
    }
    private static void OnSettersChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<Trigger>(i => i.OnSettersChanged(e));

    #endregion

    /// <see cref="Value"/>
    #region

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(object), typeof(Trigger), new FrameworkPropertyMetadata(null, OnValueChanged, OnValueCoerced));
    public object Value
    {
        get => (object)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    private static void OnValueChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<Trigger>(i => i.OnValueChanged(e));
    private static object OnValueCoerced(DependencyObject sender, object value)
    {
        if (value?.ToString().ToLower() == "false")
            return false;

        if (value?.ToString().ToLower() == "true")
            return true;

        return value;
    }

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private void Update()
    {
        if (Equals(Binding, Value))
        {
            Setters?.ForEach(i =>
            {
                if (i.Property is not null)
                    Element?.SetCurrentValue(i.Property, i.Value);
            });
        }
    }

    protected virtual void OnBindingChanged(Value<object> value)
        => Update();

    protected virtual void OnElementChanged(Value<FrameworkElement> value)
        => Update();

    protected virtual void OnSettersChanged(Value<Setters> value)
        => Update();

    protected virtual void OnValueChanged(Value<object> value)
        => Update();

    #endregion
}