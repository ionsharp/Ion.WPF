using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ion.Controls;

public class Display : ContentPresenter
{
    /// <see cref="Region.Property"/>
    #region

    /// <see cref="BackgroundProperty"/>
    #region

    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Display), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, OnBackgroundCoerced));
    public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    private static object OnBackgroundCoerced(DependencyObject sender, object input) => (sender as Display).OnBackgroundCoerced(input);

    #endregion

    /// <see cref="ConditionProperty"/>
    #region

    public static readonly DependencyProperty ConditionProperty = DependencyProperty.RegisterAttached("Condition", typeof(bool), typeof(Display), new FrameworkPropertyMetadata(false, OnConditionChanged));
    public static bool GetCondition(ContentPresenter i) => (bool)i.GetValue(ConditionProperty);
    public static void SetCondition(ContentPresenter i, bool input) => i.SetValue(ConditionProperty, input);
    private static void OnConditionChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ContentPresenter>(SetTemplate);

    #endregion

    /// <see cref="ConditionalTemplateProperty"/>
    #region

    public static readonly DependencyProperty ConditionalTemplateProperty = DependencyProperty.RegisterAttached("ConditionalTemplate", typeof(DataTemplate), typeof(Display), new FrameworkPropertyMetadata(null, OnConditionalTemplateChanged));
    public static DataTemplate GetConditionalTemplate(ContentPresenter i) => (DataTemplate)i.GetValue(ConditionalTemplateProperty);
    public static void SetConditionalTemplate(ContentPresenter i, DataTemplate input) => i.SetValue(ConditionalTemplateProperty, input);
    private static void OnConditionalTemplateChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ContentPresenter>(SetTemplate);

    #endregion

    /// <see cref="DefaultTemplateProperty"/>
    #region

    public static readonly DependencyProperty DefaultTemplateProperty = DependencyProperty.RegisterAttached("DefaultTemplate", typeof(DataTemplate), typeof(Display), new FrameworkPropertyMetadata(new DataTemplate(), OnDefaultTemplateChanged));
    public static DataTemplate GetDefaultTemplate(ContentPresenter i) => (DataTemplate)i.GetValue(DefaultTemplateProperty);
    public static void SetDefaultTemplate(ContentPresenter i, DataTemplate input) => i.SetValue(DefaultTemplateProperty, input);
    private static void OnDefaultTemplateChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ContentPresenter>(SetTemplate);

    #endregion

    /// <see cref="TemplateKeyProperty"/>
    #region

    public static readonly DependencyProperty TemplateKeyProperty = DependencyProperty.RegisterAttached("TemplateKey", typeof(object), typeof(Display), new FrameworkPropertyMetadata(null, OnTemplateKeyChanged));
    public static object GetTemplateKey(ContentPresenter i) => (object)i.GetValue(TemplateKeyProperty);
    public static void SetTemplateKey(ContentPresenter i, object input) => i.SetValue(TemplateKeyProperty, input);
    private static void OnTemplateKeyChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ContentPresenter>(SetTemplate);

    #endregion

    /// <see cref="TemplateSelectorProperty"/>
    #region

    public static readonly DependencyProperty TemplateSelectorProperty = DependencyProperty.RegisterAttached("TemplateSelector", typeof(DataTemplateSelector), typeof(Display), new FrameworkPropertyMetadata(null, OnTemplateSelectorChanged));
    public static DataTemplateSelector GetTemplateSelector(ContentPresenter i) => (DataTemplateSelector)i.GetValue(TemplateSelectorProperty);
    public static void SetTemplateSelector(ContentPresenter i, DataTemplateSelector input) => i.SetValue(TemplateSelectorProperty, input);
    private static void OnTemplateSelectorChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.If<ContentPresenter>(SetTemplate);

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    public Display() : base() => this.AddHandler(OnLoaded, OnUnloaded);

    /// <see cref="Region.Method"/>
    #region

    private static void SetTemplate(ContentPresenter i)
    {
        if (GetTemplateKey(i) is object key)
        {
            if (GetTemplateSelector(i) is DataTemplateSelector selector)
            {
                DataTemplate result = selector.SelectTemplate(key, null);
                if (result is not null)
                {
                    i.ContentTemplate = result;
                    return;
                }
            }
        }

        if (GetConditionalTemplate(i) is DataTemplate conditionalTemplate)
        {
            if (GetCondition(i))
            {
                i.ContentTemplate = conditionalTemplate;
                return;
            }
        }

        i.ContentTemplate = GetDefaultTemplate(i) ?? new DataTemplate();
    }

    protected override void OnRender(DrawingContext context)
    {
        base.OnRender(context);
        context.DrawRectangle(Background, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
    }

    protected virtual object OnBackgroundCoerced(object input) => input;

    #endregion

    protected virtual void OnLoaded() { }

    protected virtual void OnUnloaded() { }
}

public abstract class Display<T>() : Display() where T : DependencyObject
{
    public T Control { get; protected set; }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        Control = this.GetParent<T>();
        if (Control is null)
            throw new ControlMissingParent<Display<T>, T>();
    }
}