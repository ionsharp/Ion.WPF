using Ion.Data;
using Ion.Reflect;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Controls;

[Extend<IObjectControl>]
public static class XObjectControl
{
    /// <see cref="Region.Property"/>
    #region

    #region (private) HandleSource

    private static readonly DependencyProperty HandleSourceProperty = DependencyProperty.RegisterAttached("HandleSource", typeof(object), typeof(XObjectControl), new FrameworkPropertyMetadata(null, OnSourceInternalChanged));
    private static Handle GetHandleSource(IObjectControl i) => i.As<DependencyObject>()?.GetValueOrSetDefault(HandleSourceProperty, () => new Handle());

    #endregion

    #region Log

    public static readonly DependencyProperty LogProperty = DependencyProperty.RegisterAttached("Log", typeof(bool), typeof(XObjectControl), new FrameworkPropertyMetadata(false, OnLogChanged));
    public static bool GetLog(IObjectControl i) => (bool)i.GetValue(LogProperty);
    public static void SetLog(IObjectControl i, bool input) => i.SetValue(LogProperty, input);
    private static void OnLogChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
        => i.If<IObjectControl>(j => e.NewValue.If<bool>(k => GetSourceModel(j).Log = k));

    #endregion

    #region LogType

    public static readonly DependencyProperty LogTypeProperty = DependencyProperty.RegisterAttached("LogType", typeof(MemberLogType), typeof(XObjectControl), new FrameworkPropertyMetadata(MemberLogType.All, OnLogTypeChanged));
    public static MemberLogType GetLogType(IObjectControl i) => (MemberLogType)i.GetValue(LogTypeProperty);
    public static void SetLogType(IObjectControl i, MemberLogType input) => i.SetValue(LogTypeProperty, input);
    private static void OnLogTypeChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
        => i.If<IObjectControl>(j => e.NewValue.If<MemberLogType>(k => GetSourceModel(j).LogType = k));

    #endregion

    #region (readonly) Route

    private static readonly DependencyPropertyKey RouteKey = DependencyProperty.RegisterAttachedReadOnly("Route", typeof(SourceRoute), typeof(XObjectControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty RouteProperty = RouteKey.DependencyProperty;
    public static SourceRoute GetRoute(IObjectControl i) => i.IfGet<DependencyObject, SourceRoute>(j => j.GetValueOrSetDefault(RouteKey, () => new SourceRoute()));

    #endregion

    #region Source(External)

    public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(object), typeof(XObjectControl), new FrameworkPropertyMetadata(null, OnSourceChanged));
    public static object GetSource(IObjectControl i) => (object)i.GetValue(SourceProperty);
    public static void SetSource(IObjectControl i, object input) => i.SetValue(SourceProperty, input);

    #endregion

    #region (private) SourceInternal

    private static readonly DependencyProperty SourceInternalProperty = DependencyProperty.RegisterAttached("SourceInternal", typeof(object), typeof(XObjectControl), new FrameworkPropertyMetadata(null, OnSourceInternalChanged));
    private static void OnSourceInternalChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        if (i is IObjectControl j)
        {
            GetHandleSource(j)
                .DoInternal(() => i.SetCurrentValue(SourceProperty, e.NewValue));
        }
    }

    #endregion

    #region (readonly) SourceModel

    private static readonly DependencyPropertyKey SourceModelKey = DependencyProperty.RegisterAttachedReadOnly("SourceModel", typeof(MemberBase), typeof(XObjectControl), new FrameworkPropertyMetadata(null, OnSourceModelChanged));
    public static readonly DependencyProperty SourceModelProperty = SourceModelKey.DependencyProperty;
    public static MemberBase GetSourceModel(IObjectControl i)
        => i.IfGet<DependencyObject, MemberBase>(j => j.GetValueOrSetDefault(SourceModelKey, () => new MemberBase() { Log = GetLog(i) }));
    private static void OnSourceModelChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        i.Unbind(SourceInternalProperty);
        i.Bind(SourceInternalProperty, new PropertyPath("(0)", SourceModelProperty), i, BindingMode.OneWay, new ValueConverter<MemberBase, object>(false, i => i.Value.Value));
    }

    #endregion

    #region (private) SourceTarget

    private static readonly DependencyProperty SourceTargetProperty = DependencyProperty.RegisterAttached("SourceTarget", typeof(object), typeof(XObjectControl), new FrameworkPropertyMetadata(null, OnSourceTargetChanged));

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    static XObjectControl()
    {
        EventManager.RegisterClassHandler(typeof(Control), Control.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(DataGrid), DataGrid.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
        EventManager.RegisterClassHandler(typeof(MenuBase), MenuBase.LoadedEvent,
            new RoutedEventHandler(OnLoaded), true);
    }

    /// <see cref="Region.Method"/>
    #region

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement i)
        {
            i.AddOnce(new CommandBinding(BackCommand, BackCommand_Executed, BackCommand_CanExecute));
        }
    }

    private static void OnSourceChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.If<IObjectControl>(j =>
    {
        GetRoute(j).Clear();
        i.SetCurrentValue(SourceTargetProperty, e.NewValue);
    });

    private static void OnSourceTargetChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        if (i is IObjectControl j)
        {
            var model = GetSourceModel(j);

            GetRoute(j).Add(model);
            GetHandleSource(j).DoInternal(() => model.Value = e.NewValue);
        }
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    /// <see cref="BackCommand"/>
    #region

    public static readonly RoutedUICommand BackCommand = new(nameof(BackCommand), nameof(BackCommand), typeof(XObjectControl));
    private static void BackCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is IObjectControl control)
        {
            var target = GetRoute(control).Back(e.Parameter);
            target.IfNotNull(i => control.If<DependencyObject>(j => j.SetCurrentValue(SourceTargetProperty, i)));
        }
    }
    private static void BackCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is IObjectControl control)
            e.CanExecute = e.Parameter is not null || GetRoute(control).Count > 1;
    }

    #endregion

    #endregion
}