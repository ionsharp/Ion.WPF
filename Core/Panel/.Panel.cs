using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Reflection;
using System.Timers;

namespace Ion.Core;

/// <summary><see cref="Content"/> that gets created multiple times.</summary>
[Image(Images.Panel)]
[Name("Panel")]
[Styles.Object(Strict = MemberTypes.All,
    MemberViewType = MemberViewType.Tab,
    MemberView = View.Option)]
[Serializable]
public abstract record class Panel : Content
{
    [TabView(View = View.Option)]
    private enum Tab
    {
        [TabStyle(Image = Images.General)]
        General
    }

    /// <see cref="Region.Field"/>
    #region

    public const SecondaryDocks DefaultDockPreference = SecondaryDocks.Center;

    public const System.Windows.Controls.Orientation DefaultOrientationPreference = System.Windows.Controls.Orientation.Horizontal;

    #endregion

    /// <see cref="Region.Delegate"/>

    internal delegate void SizeRequestHandler(Panel sender, double length);

    /// <see cref="Region.Event"/>
    #region

    internal event SizeRequestHandler HeightRequested;

    internal event SizeRequestHandler WidthRequested;

    #endregion

    /// <see cref="Region.Field"/>

    private readonly Updatable update;

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="Region.Property.Protected"/>
    #region

    /// <summary></summary>
    protected virtual bool CanUpdate => false;

    protected virtual TimeSpan UpdateInterval => Updatable.DefaultInterval;

    #endregion

    /// <see cref="Region.Property.Public"/>
    #region

    public double Height { set => HeightRequested?.Invoke(this, value); }

    [NonSerializable]
    public bool IsSelected { get => Get(false); set => Set(value); }

    [NonSerializable]
    public bool IsVisible { get => Get(true); set => Set(value); }

    public string Name => GetType().Name;

    public string NameRaw => Name[..^nameof(Panel).Length];

    [NonSerializable]
    public ControlLength PinHeight { get => Get<ControlLength>(); set => Set(value); }

    [NonSerializable]
    public ControlLength PinWidth { get => Get<ControlLength>(); set => Set(value); }

    public double Width { set => WidthRequested?.Invoke(this, value); }

    #endregion

    /// <see cref="Region.Property.Public.Override"/>
    #region

    public override string Title
        => Instance.GetName(this) is string result
        ?
        (
            " panel" is string suffix && result.EndsWith(suffix)
            ? result[..^suffix.Length]
            : result
        )
        : null;

    public override object ToolTip => this;

    #endregion

    /// <see cref="Region.Property.Public.Virtual"/>
    #region

    /// <summary>Gets whether or not the panel can be hidden.</summary>
    public virtual bool CanHide { get; } = true;

    /// <summary>Gets whether or not the panel can live with other panels.</summary>
    public virtual bool CanShare { get; } = true;

    /// <summary>Gets whether or not the panel hides if no active document exists.</summary>
    public virtual bool HideIfNoActiveDocument => false;

    /// <summary>Gets the direction in which the panel prefers to dock.</summary>
    public virtual SecondaryDocks DockPreference { get; } = DefaultDockPreference;

    [NonSerializable]
    public double Progress { get => Get<double>(); protected set => Set(value); }

    /// <summary>Gets if progress displays in steps.</summary>
    public bool IsProgressIndeterminate { get => Get(false, false); protected set => Set(value, false); }

    /// <summary>Gets if progress is displayed.</summary>
    public bool IsProgressVisible { get => Get(false, false); protected set => Set(value, false); }

    public virtual double MaxHeight { get; }

    public virtual double MaxWidth { get; }

    public virtual double MinHeight { get; }

    public virtual double MinWidth { get; }

    public virtual bool TitleLocalized => true;

    public virtual bool TitleVisibility => true;

    #endregion

    /// <see cref="View.Option"/>
    #region

    /// <see cref="Tab.General"/>
    #region

    [Group(GroupDefault.Show)]
    [Name("Footer")]
    [Style(View = View.Option,
        Tab = Tab.General)]
    public bool ShowFooter { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Show)]
    [Name("Header")]
    [Style(View = View.Option,
        Tab = Tab.General)]
    public bool ShowHeader { get => Get(true); set => Set(value); }

    #endregion

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>

    /// <inheritdoc/>
    protected Panel() : base()
    {
        update = new(UpdateInterval, CanUpdate);
    }

    /// <see cref="Region.Method"/>
    #region

    public static T ActiveContent<T>() where T : Content => Appp.Get<IDockAppModel>()?.ViewModel?.ActiveContent.As<T>();

    public static T ActiveDocument<T>() where T : Document => Appp.Get<IDockAppModel>()?.ViewModel?.ActiveDocument.As<T>();

    public static T ActivePanel<T>() where T : Panel => Appp.Get<IDockAppModel>()?.ViewModel?.ActivePanel.As<T>();

    ///

    private void OnActiveContentChanged(object sender, ChangeEventArgs<Content> e) => OnActiveContentChanged(new(e.OldValue, e.NewValue));

    protected virtual void OnActiveContentChanged(Value<Content> newValue) { }

    private void OnActiveDocumentChanged(object sender, ChangeEventArgs<Document> e) => OnActiveDocumentChanged(e.OldValue, e.NewValue);

    protected virtual void OnActiveDocumentChanged(Document oldValue, Document newValue)
    {
        if (IsSubscribed)
        {
            Appp.Get<IDockAppModel>()?.ViewModel?.ActiveDocument.IfNotNull(i =>
            {
                i.PropertySet -= OnActiveDocumentPropertyChanged;
                i.PropertySet += OnActiveDocumentPropertyChanged;
            });
        }
    }

    private void OnActiveDocumentPropertyChanged(IPropertySet sender, PropertySetEventArgs e)
        => OnActiveDocumentPropertyChanged(e);

    protected virtual void OnActiveDocumentPropertyChanged(PropertySetEventArgs e) { }

    private void OnActivePanelChanged(object sender, ChangeEventArgs<Panel> e) => OnActivePanelChanged(e.NewValue);

    protected virtual void OnActivePanelChanged(Panel input) { }

    private void OnDocumentAdded(object sender, EventArgs<Document> e) => OnDocumentAdded(e.A);

    protected virtual void OnDocumentAdded(Document input) { }

    private void OnDocumentRemoved(object sender, EventArgs<Document> e) => OnDocumentRemoved(e.A);

    protected virtual void OnDocumentRemoved(Document input) { }

    ///

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        if (e.PropertyName == nameof(Title))
            Reset(() => ToolTip);
    }

    ///

    public override void Subscribe()
    {
        base.Subscribe();
        update.Updated -= OnUpdate; update.Updated += OnUpdate;
        Appp.Get<IDockAppModel>()?.ViewModel.IfNotNull(i =>
        {
            i.ActiveContentChanged
                += OnActiveContentChanged;
            i.ActiveDocumentChanged
                += OnActiveDocumentChanged;
            i.ActivePanelChanged
                += OnActivePanelChanged;

            i.DocumentAdded
                += OnDocumentAdded;
            i.DocumentRemoved
                += OnDocumentRemoved;

            if (i.ActiveDocument != null)
            {
                i.ActiveDocument.PropertySet
                    -= OnActiveDocumentPropertyChanged;
                i.ActiveDocument.PropertySet
                    += OnActiveDocumentPropertyChanged;
            }
        });
    }

    public override void Unsubscribe()
    {
        base.Subscribe();
        update.Updated -= OnUpdate;
        Appp.Get<IDockAppModel>()?.ViewModel.IfNotNull(i =>
        {
            i.ActiveContentChanged
                -= OnActiveContentChanged;
            i.ActiveDocumentChanged
                -= OnActiveDocumentChanged;
            i.ActivePanelChanged
                -= OnActivePanelChanged;

            i.DocumentAdded
                -= OnDocumentAdded;
            i.DocumentRemoved
                -= OnDocumentRemoved;

            if (i.ActiveDocument != null)
                i.ActiveDocument.PropertySet -= OnActiveDocumentPropertyChanged;
        });
    }

    ///

    private void OnUpdate(object sender, ElapsedEventArgs e) => OnUpdate(e);

    protected virtual void OnUpdate(ElapsedEventArgs e) { }

    #endregion
}