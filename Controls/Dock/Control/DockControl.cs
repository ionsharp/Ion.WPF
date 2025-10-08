using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using Ion.Imaging;
using Ion.Input;
using Ion.Input.Global;
using Ion.Numeral;
using Ion.Numeral.Models;
using Ion.Text;
using Ion.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Controls;

//Layouts?.Save(e.Value, Convert())

/// <see cref="DockControl"/>
#region

public class DockControl : Control
{
    #region Keys

    public static readonly ReferenceKey<Border> BorderKey = new();

    ///

    public static readonly ResourceKey FindTemplateKey = new();

    ///

    public static readonly ResourceKey EmptyMarkerStyleKey = new();

    public static readonly ResourceKey PrimaryMarkerStyleKey = new();

    public static readonly ResourceKey SecondaryMarkerStyleKey = new();

    public static readonly ResourceKey SelectionStyleKey = new();

    ///


    public static readonly ResourceKey DocumentHeaderTemplate = new();

    public static readonly ResourceKey DocumentMenu = new();

    public static readonly ResourceKey DocumentStyle = new();

    public static readonly ResourceKey DocumentTemplateKey = new();

    public static readonly ResourceKey PanelStyle = new();

    ///

    public static readonly ResourceKey GridSplitterStyleKey = new();

    ///

    public static readonly ResourceKey PanelMenuKey = new();

    public static readonly ResourceKey PanelHeaderPatternKey = new();

    public static readonly ResourceKey PanelHeaderTemplateKey = new();

    public static readonly ResourceKey PanelBodyContentTemplate = new();

    public static readonly ResourceKey PanelBodyTemplateKey = new();

    public static readonly ResourceKey PanelTemplateKey = new();

    public static readonly ResourceKey PanelTitleHeaderTemplateKey = new();

    public static readonly ResourceKey CollapsedPanelTitleHeaderTemplateKey = new();

    ///

    public static readonly ResourceKey ContentFooterTemplate = new();

    public static readonly ResourceKey ContentHeaderTemplate = new();

    public static readonly ResourceKey ContentOptionsTemplate = new();

    #endregion

    #region (IMultiValueConverter) SearchVisibilityConverter

    public static readonly IMultiValueConverter SearchVisibilityConverter = new MultiValueConverter<Visibility>(i =>
    {
        if (i.Values?.Length == 2)
        {
            if (i.Values[0] is IFind a)
            {
                if (i.Values[1] is IFind b)
                {
                    if (ReferenceEquals(a, b))
                        return Visibility.Visible;
                }
            }
        }
        return Visibility.Collapsed;
    });

    #endregion

    #region Events

    public event EventHandler<EventArgs> LayoutChanged;

    #endregion

    #region Fields

    private readonly ListenerMouse mouseListener = new(new Input.Global.WinApi.GlobalHooker());

    ///

    private readonly DockRootControl Root;

    ///

    private readonly Handle handleActive
        = false;
    private readonly Handle handleAnchor
        = false;
    private readonly Handle handleClosing
        = false;

    ///

    private readonly List<DockWindow> Floating
        = [];

    ///

    private readonly Dictionary<Core.Panel, IDockPanelSource> Hidden
        = [];
    private readonly Dictionary<Core.Panel, IDockPanelSource> Pins
        = [];

    ///

    private readonly Dictionary<Document, DockDocumentControl> Minimized
        = [];

    ///

    internal List<DockDocumentControl> DocumentControls
        = [];

    internal List<DockPanelControl> PanelControls
        = [];

    ///

    private readonly Taskable refreshTask;

    internal DockDragEvent LastDrag;

    #endregion

    #region Properties

    #region ActivateButton

    public static readonly DependencyProperty ActivateButtonProperty = DependencyProperty.Register(nameof(ActivateButton), typeof(MouseButton), typeof(DockControl), new FrameworkPropertyMetadata(MouseButton.Left));
    public MouseButton ActivateButton
    {
        get => (MouseButton)GetValue(ActivateButtonProperty);
        set => SetValue(ActivateButtonProperty, value);
    }

    #endregion

    #region ActivateButtonState

    public static readonly DependencyProperty ActivateButtonStateProperty = DependencyProperty.Register(nameof(ActivateButtonState), typeof(MouseButtonState), typeof(DockControl), new FrameworkPropertyMetadata(MouseButtonState.Pressed));
    public MouseButtonState ActivateButtonState
    {
        get => (MouseButtonState)GetValue(ActivateButtonStateProperty);
        set => SetValue(ActivateButtonStateProperty, value);
    }

    #endregion

    #region ActiveContent

    public static readonly DependencyProperty ActiveContentProperty = DependencyProperty.Register(nameof(ActiveContent), typeof(Content), typeof(DockControl), new FrameworkPropertyMetadata(null, OnActiveContentChanged));
    public Content ActiveContent
    {
        get => (Content)GetValue(ActiveContentProperty);
        set => SetValue(ActiveContentProperty, value);
    }

    private static void OnActiveContentChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnActiveContentChanged(e.NewValue as Content);

    #endregion

    #region ActiveDocument

    public static readonly DependencyProperty ActiveDocumentProperty = DependencyProperty.Register(nameof(ActiveDocument), typeof(Document), typeof(DockControl), new FrameworkPropertyMetadata(null, OnActiveDocumentChanged));
    public Document ActiveDocument
    {
        get => (Document)GetValue(ActiveDocumentProperty);
        set => SetValue(ActiveDocumentProperty, value);
    }

    private static void OnActiveDocumentChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnActiveDocumentChanged(e.Convert<Document>());

    #endregion

    #region ActivePanel

    public static readonly DependencyProperty ActivePanelProperty = DependencyProperty.Register(nameof(ActivePanel), typeof(Core.Panel), typeof(DockControl), new FrameworkPropertyMetadata(null, OnActivePanelChanged));
    public Core.Panel ActivePanel
    {
        get => (Core.Panel)GetValue(ActivePanelProperty);
        set => SetValue(ActivePanelProperty, value);
    }

    private static void OnActivePanelChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnActivePanelChanged(e.Convert<Core.Panel>());

    #endregion

    #region (readonly) ActiveFind

    private static readonly DependencyPropertyKey ActiveFindKey = DependencyProperty.RegisterReadOnly(nameof(ActiveFind), typeof(IFind), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ActiveFindProperty = ActiveFindKey.DependencyProperty;
    public IFind ActiveFind
    {
        get => (IFind)GetValue(ActiveFindProperty);
        private set => SetValue(ActiveFindKey, value);
    }

    #endregion

    #region (readonly) ActiveRoot

    private static readonly DependencyPropertyKey ActiveRootKey = DependencyProperty.RegisterReadOnly(nameof(ActiveRoot), typeof(DockRootControl), typeof(DockControl), new FrameworkPropertyMetadata(null, OnActiveRootChanged));
    public static readonly DependencyProperty ActiveRootProperty = ActiveRootKey.DependencyProperty;
    public DockRootControl ActiveRoot
    {
        get => (DockRootControl)GetValue(ActiveRootProperty);
        private set => SetValue(ActiveRootKey, value);
    }

    private static void OnActiveRootChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnActiveRootChanged(e.Convert<DockRootControl>());

    #endregion

    #region AutoSave

    public static readonly DependencyProperty AutoSaveProperty = DependencyProperty.Register(nameof(AutoSave), typeof(bool), typeof(DockControl), new FrameworkPropertyMetadata(true));
    public bool AutoSave
    {
        get => (bool)GetValue(AutoSaveProperty);
        set => SetValue(AutoSaveProperty, value);
    }

    #endregion

    #region DefaultPanelTemplate

    public static readonly DependencyProperty DefaultPanelTemplateProperty = DependencyProperty.Register(nameof(DefaultPanelTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DefaultPanelTemplate
    {
        get => (DataTemplate)GetValue(DefaultPanelTemplateProperty);
        set => SetValue(DefaultPanelTemplateProperty, value);
    }

    #endregion

    #region DocumentIconTemplate

    public static readonly DependencyProperty DocumentIconTemplateProperty = DependencyProperty.Register(nameof(DocumentIconTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DocumentIconTemplate
    {
        get => (DataTemplate)GetValue(DocumentIconTemplateProperty);
        set => SetValue(DocumentIconTemplateProperty, value);
    }

    #endregion

    #region DocumentIconTemplateSelector

    public static readonly DependencyProperty DocumentIconTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentIconTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DocumentIconTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DocumentIconTemplateSelectorProperty);
        set => SetValue(DocumentIconTemplateSelectorProperty, value);
    }

    #endregion

    #region Documents

    public static readonly DependencyProperty DocumentsProperty = DependencyProperty.Register(nameof(Documents), typeof(DocumentCollection), typeof(DockControl), new FrameworkPropertyMetadata(null, OnDocumentsChanged));
    public DocumentCollection Documents
    {
        get => (DocumentCollection)GetValue(DocumentsProperty);
        set => SetValue(DocumentsProperty, value);
    }

    private static void OnDocumentsChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnDocumentsChanged(e.Convert<DocumentCollection>());

    #endregion

    #region DocumentTemplate

    public static readonly DependencyProperty DocumentTemplateProperty = DependencyProperty.Register(nameof(DocumentTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DocumentTemplate
    {
        get => (DataTemplate)GetValue(DocumentTemplateProperty);
        set => SetValue(DocumentTemplateProperty, value);
    }

    #endregion

    #region DocumentTemplateSelector

    public static readonly DependencyProperty DocumentTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DocumentTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DocumentTemplateSelectorProperty);
        set => SetValue(DocumentTemplateSelectorProperty, value);
    }

    #endregion

    #region DocumentTitleTemplate

    public static readonly DependencyProperty DocumentTitleTemplateProperty = DependencyProperty.Register(nameof(DocumentTitleTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DocumentTitleTemplate
    {
        get => (DataTemplate)GetValue(DocumentTitleTemplateProperty);
        set => SetValue(DocumentTitleTemplateProperty, value);
    }

    #endregion

    #region DocumentTitleTemplateSelector

    public static readonly DependencyProperty DocumentTitleTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentTitleTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DocumentTitleTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DocumentTitleTemplateSelectorProperty);
        set => SetValue(DocumentTitleTemplateSelectorProperty, value);
    }

    #endregion

    #region DocumentToolTipHeaderTemplate

    public static readonly DependencyProperty DocumentToolTipHeaderTemplateProperty = DependencyProperty.Register(nameof(DocumentToolTipHeaderTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DocumentToolTipHeaderTemplate
    {
        get => (DataTemplate)GetValue(DocumentToolTipHeaderTemplateProperty);
        set => SetValue(DocumentToolTipHeaderTemplateProperty, value);
    }

    #endregion

    #region DocumentToolTipHeaderTemplateSelector

    public static readonly DependencyProperty DocumentToolTipHeaderTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentToolTipHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DocumentToolTipHeaderTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DocumentToolTipHeaderTemplateSelectorProperty);
        set => SetValue(DocumentToolTipHeaderTemplateSelectorProperty, value);
    }

    #endregion

    #region DocumentToolTipHeaderIconTemplate

    public static readonly DependencyProperty DocumentToolTipHeaderIconTemplateProperty = DependencyProperty.Register(nameof(DocumentToolTipHeaderIconTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DocumentToolTipHeaderIconTemplate
    {
        get => (DataTemplate)GetValue(DocumentToolTipHeaderIconTemplateProperty);
        set => SetValue(DocumentToolTipHeaderIconTemplateProperty, value);
    }

    #endregion

    #region DocumentToolTipHeaderIconTemplateSelector

    public static readonly DependencyProperty DocumentToolTipHeaderIconTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentToolTipHeaderIconTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DocumentToolTipHeaderIconTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DocumentToolTipHeaderIconTemplateSelectorProperty);
        set => SetValue(DocumentToolTipHeaderIconTemplateSelectorProperty, value);
    }

    #endregion

    #region DocumentToolTipTemplate

    public static readonly DependencyProperty DocumentToolTipTemplateProperty = DependencyProperty.Register(nameof(DocumentToolTipTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DocumentToolTipTemplate
    {
        get => (DataTemplate)GetValue(DocumentToolTipTemplateProperty);
        set => SetValue(DocumentToolTipTemplateProperty, value);
    }

    #endregion

    #region DocumentToolTipTemplateSelector

    public static readonly DependencyProperty DocumentToolTipTemplateSelectorProperty = DependencyProperty.Register(nameof(DocumentToolTipTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DocumentToolTipTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DocumentToolTipTemplateSelectorProperty);
        set => SetValue(DocumentToolTipTemplateSelectorProperty, value);
    }

    #endregion

    #region (readonly) Drag

    private static readonly DependencyPropertyKey DragKey = DependencyProperty.RegisterReadOnly(nameof(Drag), typeof(DockDragEvent), typeof(DockControl), new FrameworkPropertyMetadata(null, OnDragChanged));
    public static readonly DependencyProperty DragProperty = DragKey.DependencyProperty;
    public DockDragEvent Drag
    {
        get => (DockDragEvent)GetValue(DragProperty);
        private set => SetValue(DragKey, value);
    }

    private static void OnDragChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<DockControl>().OnDragChanged(e.Convert<DockDragEvent>());

    #endregion

    #region DragDistance

    public static readonly DependencyProperty DragDistanceProperty = DependencyProperty.Register(nameof(DragDistance), typeof(double), typeof(DockControl), new FrameworkPropertyMetadata(10.0));
    public double DragDistance
    {
        get => (double)GetValue(DragDistanceProperty);
        set => SetValue(DragDistanceProperty, value);
    }

    #endregion

    #region (readonly) Dragging

    private static readonly DependencyPropertyKey DraggingKey = DependencyProperty.RegisterReadOnly(nameof(Dragging), typeof(bool), typeof(DockControl), new FrameworkPropertyMetadata(false, OnDraggingChanged));
    public static readonly DependencyProperty DraggingProperty = DraggingKey.DependencyProperty;
    public bool Dragging
    {
        get => (bool)GetValue(DraggingProperty);
        private set => SetValue(DraggingKey, value);
    }

    private static void OnDraggingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<DockControl>().OnDraggingChanged(e.Convert<bool>());

    #endregion

    #region EmptyDocumentGroupVisibility

    public static readonly DependencyProperty EmptyDocumentGroupVisibilityProperty = DependencyProperty.Register(nameof(EmptyDocumentGroupVisibility), typeof(Visibility), typeof(DockControl), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public Visibility EmptyDocumentGroupVisibility
    {
        get => (Visibility)GetValue(EmptyDocumentGroupVisibilityProperty);
        set => SetValue(EmptyDocumentGroupVisibilityProperty, value);
    }

    #endregion

    #region EmptyPanelGroupVisibility

    public static readonly DependencyProperty EmptyPanelGroupVisibilityProperty = DependencyProperty.Register(nameof(EmptyPanelGroupVisibility), typeof(Visibility), typeof(DockControl), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public Visibility EmptyPanelGroupVisibility
    {
        get => (Visibility)GetValue(EmptyPanelGroupVisibilityProperty);
        set => SetValue(EmptyPanelGroupVisibilityProperty, value);
    }

    #endregion

    #region (readonly) ActualFindPanel

    private static readonly DependencyPropertyKey ActualFindPanelKey = DependencyProperty.RegisterReadOnly(nameof(ActualFindPanel), typeof(FindPanel), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty ActualFindPanelProperty = ActualFindPanelKey.DependencyProperty;
    public FindPanel ActualFindPanel
    {
        get => (FindPanel)GetValue(ActualFindPanelProperty);
        private set => SetValue(ActualFindPanelKey, value);
    }

    #endregion

    #region FloatingWindowDragOffset

    public static readonly DependencyProperty FloatingWindowDragOffsetProperty = DependencyProperty.Register(nameof(FloatingWindowDragOffset), typeof(double), typeof(DockControl), new FrameworkPropertyMetadata(2.0));
    public double FloatingWindowDragOffset
    {
        get => (double)GetValue(FloatingWindowDragOffsetProperty);
        set => SetValue(FloatingWindowDragOffsetProperty, value);
    }

    #endregion

    #region InvalidLayoutTemplate

    public static readonly DependencyProperty InvalidLayoutTemplateProperty = DependencyProperty.Register(nameof(InvalidLayoutTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate InvalidLayoutTemplate
    {
        get => (DataTemplate)GetValue(InvalidLayoutTemplateProperty);
        set => SetValue(InvalidLayoutTemplateProperty, value);
    }

    #endregion

    #region (readonly) IsLayoutInvalid

    private static readonly DependencyPropertyKey IsLayoutInvalidKey = DependencyProperty.RegisterReadOnly(nameof(IsLayoutInvalid), typeof(bool), typeof(DockControl), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsLayoutInvalidProperty = IsLayoutInvalidKey.DependencyProperty;
    public bool IsLayoutInvalid
    {
        get => (bool)GetValue(IsLayoutInvalidProperty);
        private set => SetValue(IsLayoutInvalidKey, value);
    }

    #endregion

    #region (readonly) IsRefreshing

    private static readonly DependencyPropertyKey RefreshingKey = DependencyProperty.RegisterReadOnly(nameof(Refreshing), typeof(bool), typeof(DockControl), new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty RefreshingProperty = RefreshingKey.DependencyProperty;
    public bool Refreshing
    {
        get => (bool)GetValue(RefreshingProperty);
        private set => SetValue(RefreshingKey, value);
    }

    #endregion

    #region Layout

    public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout), typeof(object), typeof(DockControl), new FrameworkPropertyMetadata(null, OnLayoutChanged));
    public object Layout
    {
        get => (object)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    private static void OnLayoutChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnLayoutChanged(e.Convert<object>());

    #endregion

    #region MinimizedDocumentPlacement

    public static readonly DependencyProperty MinimizedDocumentPlacementProperty = DependencyProperty.Register(nameof(MinimizedDocumentPlacement), typeof(SideY), typeof(DockControl), new FrameworkPropertyMetadata(SideY.Bottom));
    public SideY MinimizedDocumentPlacement
    {
        get => (SideY)GetValue(MinimizedDocumentPlacementProperty);
        set => SetValue(MinimizedDocumentPlacementProperty, value);
    }

    #endregion

    #region NoActiveDocumentTemplate

    public static readonly DependencyProperty NoActiveDocumentTemplateProperty = DependencyProperty.Register(nameof(NoActiveDocumentTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate NoActiveDocumentTemplate
    {
        get => (DataTemplate)GetValue(NoActiveDocumentTemplateProperty);
        set => SetValue(NoActiveDocumentTemplateProperty, value);
    }

    #endregion

    #region PanelHeaderPlacement

    public static readonly DependencyProperty PanelHeaderPlacementProperty = DependencyProperty.Register(nameof(PanelHeaderPlacement), typeof(SideY), typeof(DockControl), new FrameworkPropertyMetadata(SideY.Top));
    public SideY PanelHeaderPlacement
    {
        get => (SideY)GetValue(PanelHeaderPlacementProperty);
        set => SetValue(PanelHeaderPlacementProperty, value);
    }

    #endregion

    #region PanelHeaderTemplate

    public static readonly DependencyProperty PanelHeaderTemplateProperty = DependencyProperty.Register(nameof(PanelHeaderTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate PanelHeaderTemplate
    {
        get => (DataTemplate)GetValue(PanelHeaderTemplateProperty);
        set => SetValue(PanelHeaderTemplateProperty, value);
    }

    #endregion

    #region PanelHeaderTemplateSelector

    public static readonly DependencyProperty PanelHeaderTemplateSelectorProperty = DependencyProperty.Register(nameof(PanelHeaderTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector PanelHeaderTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(PanelHeaderTemplateSelectorProperty);
        set => SetValue(PanelHeaderTemplateSelectorProperty, value);
    }

    #endregion

    #region PanelIconTemplate

    public static readonly DependencyProperty PanelIconTemplateProperty = DependencyProperty.Register(nameof(PanelIconTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate PanelIconTemplate
    {
        get => (DataTemplate)GetValue(PanelIconTemplateProperty);
        set => SetValue(PanelIconTemplateProperty, value);
    }

    #endregion

    #region PanelIconTemplateSelector

    public static readonly DependencyProperty PanelIconTemplateSelectorProperty = DependencyProperty.Register(nameof(PanelIconTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector PanelIconTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(PanelIconTemplateSelectorProperty);
        set => SetValue(PanelIconTemplateSelectorProperty, value);
    }

    #endregion

    #region Panels

    public static readonly DependencyProperty PanelsProperty = DependencyProperty.Register(nameof(Panels), typeof(PanelCollection), typeof(DockControl), new FrameworkPropertyMetadata(null, OnPanelsChanged));
    public PanelCollection Panels
    {
        get => (PanelCollection)GetValue(PanelsProperty);
        set => SetValue(PanelsProperty, value);
    }

    private static void OnPanelsChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<DockControl>().OnPanelsChanged(e.Convert<PanelCollection>());

    #endregion

    #region PanelTemplate

    public static readonly DependencyProperty PanelTemplateProperty = DependencyProperty.Register(nameof(PanelTemplate), typeof(KeyTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public KeyTemplate PanelTemplate
    {
        get => (KeyTemplate)GetValue(PanelTemplateProperty);
        set => SetValue(PanelTemplateProperty, value);
    }

    #endregion

    #region PanelTemplateSelector

    public static readonly DependencyProperty PanelTemplateSelectorProperty = DependencyProperty.Register(nameof(PanelTemplateSelector), typeof(DockPanelTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DockPanelTemplateSelector PanelTemplateSelector
    {
        get => (DockPanelTemplateSelector)GetValue(PanelTemplateSelectorProperty);
        set => SetValue(PanelTemplateSelectorProperty, value);
    }

    #endregion

    #region PanelTitleHeaderTemplate

    public static readonly DependencyProperty PanelTitleHeaderTemplateProperty = DependencyProperty.Register(nameof(PanelTitleHeaderTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate PanelTitleHeaderTemplate
    {
        get => (DataTemplate)GetValue(PanelTitleHeaderTemplateProperty);
        set => SetValue(PanelTitleHeaderTemplateProperty, value);
    }

    #endregion

    #region PanelTitleTemplate

    public static readonly DependencyProperty PanelTitleTemplateProperty = DependencyProperty.Register(nameof(PanelTitleTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate PanelTitleTemplate
    {
        get => (DataTemplate)GetValue(PanelTitleTemplateProperty);
        set => SetValue(PanelTitleTemplateProperty, value);
    }

    #endregion

    #region PanelTitleTemplateSelector

    public static readonly DependencyProperty PanelTitleTemplateSelectorProperty = DependencyProperty.Register(nameof(PanelTitleTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector PanelTitleTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(PanelTitleTemplateSelectorProperty);
        set => SetValue(PanelTitleTemplateSelectorProperty, value);
    }

    #endregion

    #region PanelToolTipTemplate

    public static readonly DependencyProperty PanelToolTipTemplateProperty = DependencyProperty.Register(nameof(PanelToolTipTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate PanelToolTipTemplate
    {
        get => (DataTemplate)GetValue(PanelToolTipTemplateProperty);
        set => SetValue(PanelToolTipTemplateProperty, value);
    }

    #endregion

    #region PanelToolTipTemplateSelector

    public static readonly DependencyProperty PanelToolTipTemplateSelectorProperty = DependencyProperty.Register(nameof(PanelToolTipTemplateSelector), typeof(DataTemplateSelector), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector PanelToolTipTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(PanelToolTipTemplateSelectorProperty);
        set => SetValue(PanelToolTipTemplateSelectorProperty, value);
    }

    #endregion

    #region PanelToolTipHeaderTemplate

    public static readonly DependencyProperty PanelToolTipHeaderTemplateProperty = DependencyProperty.Register(nameof(PanelToolTipHeaderTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate PanelToolTipHeaderTemplate
    {
        get => (DataTemplate)GetValue(PanelToolTipHeaderTemplateProperty);
        set => SetValue(PanelToolTipHeaderTemplateProperty, value);
    }

    #endregion

    #region ParentWindow

    private Window parentWindow;

    private Window ParentWindow => parentWindow ??= this.GetParent<Window>();

    #endregion

    #region ProgressTemplate

    public static readonly DependencyProperty ProgressTemplateProperty = DependencyProperty.Register(nameof(ProgressTemplate), typeof(DataTemplate), typeof(DockControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ProgressTemplate
    {
        get => (DataTemplate)GetValue(ProgressTemplateProperty);
        set => SetValue(ProgressTemplateProperty, value);
    }

    #endregion

    #endregion

    #region DockControl

    public DockControl() : base()
    {
        Root = new DockRootControl(this, null);
        ActiveRoot = Root;

        this.AddHandler(OnLoaded, OnUnloaded);
        refreshTask = new Taskable(Refresh, TaskStrategy.Ignore);
    }

    #endregion

    #region Methods

    #region Private

    #region Activate/Deactivate

    private void Activate(Content input)
    {
        var control = FindControl(input);
        (control?.Root.Window ?? ParentWindow).Activate();
        control.IfNotNull(i => i.SelectedIndex = i.Items.IndexOf(input));
    }

    internal void Activate(DockRootControl input) => handleActive.DoInternal(() =>
    {
        SetCurrentValue(ActiveContentProperty, input.ActiveContent);

        if (input.ActiveContent is Document document)
            SetCurrentValue(ActiveDocumentProperty, document);

        if (input.ActiveContent is Core.Panel panel)
            SetCurrentValue(ActivePanelProperty, panel);
    });

    internal void Deactivate(DockRootControl input)
    {
        if (ActiveRoot?.Equals(input) == true)
        {
            handleActive.DoInternal(() =>
            {
                SetCurrentValue
                    (ActiveContentProperty, null);
                SetCurrentValue
                    (ActiveDocumentProperty, null);
                SetCurrentValue
                    (ActivePanelProperty, null);
            });
        }
    }

    #endregion

    #region Add/Remove

    private void Add(Document document)
    {
        Subscribe(document);

        if (document.IsMinimized)
        {
            Root.Minimized.Add(document);
            return;
        }

        var control = DocumentControls.FirstOrDefault<DockDocumentControl>();
        if (control != null)
            control.Source.Add(document);

        else DockContent(Root, document.DockPreference, null, document);

        SetCurrentValue(ActiveContentProperty, document);
    }

    private void Remove(Document document)
    {
        Unsubscribe(document);

        var control = FindControl(document);
        if (control != null)
            control.Source.Remove(document);

        else if (FindRoot(document) is DockRootControl root)
        {
            root.Minimized.Remove(document);
            Minimized.Remove(document);
        }

        if (ReferenceEquals(ActiveDocument, document) || Documents.Count == 0)
            SetCurrentValue(ActiveDocumentProperty, null);

    }

    ///

    private void Add(Core.Panel newPanel)
    {
        Subscribe(newPanel);

        var control = ActivePanel != null && PanelControls.FirstOrDefault(i => i.Source.Contains(ActivePanel)) is DockPanelControl result ? result : PanelControls.FirstOrDefault<DockPanelControl>();
        if (control != null)
            control.Source.Add(newPanel);

        else DockContent(Root, newPanel.DockPreference, null, newPanel);
    }

    private IDockPanelSource Remove(Core.Panel panel)
    {
        Unsubscribe(panel);

        if (ReferenceEquals(ActivePanel, panel))
            SetCurrentValue(ActivePanelProperty, null);

        Hidden.Remove(panel);

        var control = FindControl(panel);
        if (control != null)
        {
            control.Source.Remove(panel);
            return control;
        }
        else if (FindAnchor(panel) is DockAnchorPanelCollection anchor)
        {
            anchor.Remove(panel);

            Pins.Remove(panel);
            return anchor;
        }
        return null;
    }

    #endregion

    #region Collapse/Expand

    /// <summary>
    /// Makes important changes to parenting <see cref="DockGroupControl"/> when <see cref="DockPanelControl"/> is collapsed/expanded.
    /// </summary>
    private static void CollapseExpand(DockGroupControl parent, DockPanelControl child, bool expand)
    {
        var index = parent.Children.IndexOf(child);
        var lastIndex = index - 1;
        var nextIndex = index + 1;

        if (lastIndex >= 0)
        {
            if (parent.Children[lastIndex] is GridSplitter lastGridSplitter)
                lastGridSplitter.IsEnabled = expand;
        }
        if (nextIndex < parent.Children.Count)
        {
            if (parent.Children[nextIndex] is GridSplitter nextGridSplitter)
                nextGridSplitter.IsEnabled = expand;
        }
    }

    #endregion

    ///

    #region Convert (IDockControl)

    public DockLayoutElement Convert(IDockControl input, DockGroupControl parent = null, int index = -1)
    {
        DockLayoutElement result = null;

        if (input is DockGroupControl groupControl)
            result = Convert(groupControl);

        if (input is DockDocumentControl documentControl)
            result = Convert(documentControl);

        if (input is DockPanelControl panelControl)
            result = Convert(panelControl);

        if (parent != null)
        {
            if (parent.Orientation == Orient.Horizontal)
            {
                var columnDefinition = parent.ColumnDefinitions.ElementAt(index);
                result.MinWidth = columnDefinition.MinWidth;
                result.Width = (ControlLength)columnDefinition.Width;
                result.MaxWidth = columnDefinition.MaxWidth;
            }
            if (parent.Orientation == Orient.Vertical)
            {
                var rowDefinition = parent.RowDefinitions.ElementAt(index);
                result.MinHeight = rowDefinition.MinHeight;
                result.Height = (ControlLength)rowDefinition.Height;
                result.MaxHeight = rowDefinition.MaxHeight;
            }
        }
        return result;
    }

    #endregion

    #region Convert (DockGroupControl)

    private DockLayoutElement Convert(DockGroupControl input)
    {
        var layoutGroup = new DockLayoutGroup(input.Orientation);

        var k = 0;
        foreach (var i in input.Children)
        {
            if (i is IDockControl j)
            {
                layoutGroup.Elements.Add(Convert(j, input, k));
                k += 2;
            }
        }

        return layoutGroup;
    }

    #endregion

    #region Convert (DockDocumentControl)

    private static DockLayoutElement Convert(DockDocumentControl input)
        => new DockLayoutDocumentGroup(input.Source) { Default = input.Persist };

    #endregion

    #region Convert (DockPanelControl)

    private static DockLayoutElement Convert(DockPanelControl input)
    {
        var result = new DockLayoutPanelGroup();
        foreach (var i in input.Items)
        {
            if (i is Core.Panel panel)
                result.Panels.Add(new DockLayoutPanel(panel));
        }
        result.Collapse = input.Collapse;
        return result;
    }

    #endregion

    #region Convert (DockControl)

    public DockLayout Convert()
    {
        var layout = new DockLayout
        {
            Root = Convert(Root.Child)
        };

        Root.BottomPanels
            .ForEach(j => layout.Bottom
            .Add(new DockLayoutPanel(j)));
        Root.LeftPanels
            .ForEach(j => layout.Left
            .Add(new DockLayoutPanel(j)));
        Root.RightPanels
            .ForEach(j => layout.Right
            .Add(new DockLayoutPanel(j)));
        Root.TopPanels
            .ForEach(j => layout.Top
            .Add(new DockLayoutPanel(j)));

        foreach (var i in Floating)
        {
            var window = new DockLayoutWindow
            {
                Root = Convert(i.Root.Child),
                Position = new Vector2M<double>(i.Left, i.Top),
                Size = new MSize<double>(i.ActualHeight, i.ActualWidth),
                State = i.WindowState
            };

            i.Root.BottomPanels
                .ForEach(j => window.Bottom
                .Add(new DockLayoutPanel(j)));
            i.Root.LeftPanels
                .ForEach(j => window.Left
                .Add(new DockLayoutPanel(j)));
            i.Root.RightPanels
                .ForEach(j => window.Right
                .Add(new DockLayoutPanel(j)));
            i.Root.TopPanels
                .ForEach(j => window.Top
                .Add(new DockLayoutPanel(j)));

            layout.Floating.Add(window);
        }

        if (layout.First<DockLayoutDocumentGroup>() is DockLayoutDocumentGroup layoutDocumentGroup)
            layoutDocumentGroup.Default = true;

        return layout;
    }

    #endregion

    ///

    #region Convert (DockLayoutElement)

    private IDockControl Convert(DockRootControl root, DockLayoutElement input)
    {
        if (input is DockLayoutGroup group)
            return Convert(root, group);

        if (input is DockLayoutDocumentGroup documentGroup)
            return Convert(root, documentGroup);

        if (input is DockLayoutPanelGroup panelGroup)
            return Convert(root, panelGroup);

        return null;
    }

    #endregion

    #region Convert (DockLayoutGroup)

    private DockGroupControl Convert(DockRootControl root, DockLayoutGroup input)
    {
        var result = new DockGroupControl(root, input.Orientation);

        var j = 0;
        foreach (var i in input.Elements)
        {
            var child = Convert(root, i);
            if (child is null)
                continue;

            DockPanelControl collapsedControl = child is DockPanelControl cp && cp.Collapse ? cp : null;

            if (input.Orientation == Orient.Horizontal)
            {
                var columnDefinition = new ColumnDefinition();
                if (!double.IsNaN(i.MinWidth))
                    columnDefinition.MinWidth = i.MinWidth;

                columnDefinition.Width = (ControlLength)i.Width;

                if (!double.IsNaN(i.MaxWidth))
                    columnDefinition.MaxWidth = i.MaxWidth;

                if (collapsedControl != null)
                {
                    columnDefinition.Width
                        = new(1, GridUnitType.Auto);
                }

                result.ColumnDefinitions.Add(columnDefinition);
                Grid.SetColumn(child as UIElement, j * 2);
            }
            if (input.Orientation == Orient.Vertical)
            {
                var rowDefinition = new RowDefinition();
                if (!double.IsNaN(i.MinHeight))
                    rowDefinition.MinHeight = i.MinHeight;

                rowDefinition.Height = (ControlLength)i.Height;

                if (!double.IsNaN(i.MaxHeight))
                    rowDefinition.MaxHeight = i.MaxHeight;

                if (collapsedControl != null)
                {
                    rowDefinition.Height
                        = new(1, GridUnitType.Auto);
                }

                result.RowDefinitions.Add(rowDefinition);
                Grid.SetRow(child as UIElement, j * 2);
            }
            result.Children.Add(child as UIElement);

            //If the previous child is a GridSplitter and the current child is a (collapsed) DockPanelControl, the GridSplitter must be be disabled!
            if (collapsedControl != null)
            {
                var lastIndex = result.Children.IndexOf(collapsedControl) - 1;
                if (lastIndex >= 0)
                {
                    if (result.Children[lastIndex] is GridSplitter lastGridSplitter)
                        lastGridSplitter.IsEnabled = false;
                }
            }

            GridSplitter nextGridSplitter = null;
            if (j < input.Elements.Count - 1)
            {
                nextGridSplitter = CreateGridSplitter(input.Orientation);

                if (collapsedControl != null)
                    nextGridSplitter.IsEnabled = false;

                if (input.Orientation == Orient.Horizontal)
                {
                    result.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    Grid.SetColumn(nextGridSplitter, (j * 2) + 1);
                }
                if (input.Orientation == Orient.Vertical)
                {
                    result.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(nextGridSplitter, (j * 2) + 1);
                }
                result.Children.Add(nextGridSplitter);
            }

            j++;
        }
        return result;
    }

    #endregion

    #region Convert (DockLayoutDocumentGroup)

    private DockDocumentControl Convert(DockRootControl root, DockLayoutDocumentGroup input)
    {
        var result = new DockDocumentControl(root) { Persist = input.Default };

        if (input.Documents?.Any<Document>() == true)
        {
            input.Documents.ForEach(j =>
            {
                if (j.IsMinimized)
                {
                    root.Minimized.Add(j);
                    return;
                }
                result.Source.Add(j);
            });
        }
        FindPanels(input.Panels).ForEach(i => result.Source.Add(i));

        DocumentControls.Add(result);
        Subscribe(result);

        return result;
    }

    #endregion

    #region Convert (DockLayoutPanelGroup)

    private DockPanelControl Convert(DockRootControl root, DockLayoutPanelGroup input)
    {
        var result = new DockPanelControl(root) { Collapse = input.Collapse };
        FindPanels(input.Panels).ForEach(i =>
        {
            if (input.Collapse)
                i.IsSelected = false;

            result.Source.Add(i);
        });

        result.SelectedIndex = result.Collapse
            ? -1 : result.Source.IndexOf<Core.Panel>(i => i.IsSelected);

        PanelControls.Add(result);
        Subscribe(result);

        return result;
    }

    #endregion

    #region Convert (DockLayoutWindow)

    private DockWindow Convert(DockLayoutWindow input)
    {
        DockWindow result = new();

        var root = new DockRootControl(this, result);
        root.Child = Convert(root, input.Root);
        result.Content = root;

        ParentWindow.InputBindings.ForEach(i => result.InputBindings.Add(i as InputBinding));

        if (input.Position != null)
        {
            result.WindowStartupLocation = WindowStartupLocation.Manual;
            result.Left = input.Position.X;
            result.Top = input.Position.Y;
        }

        if (input.Size is not null)
        {
            result.Height = input.Size.Height;
            result.Width = input.Size.Width;
        }

        result.WindowState = input.State;
        result.Show();

        Subscribe(result);
        Floating.Add(result);

        return result;
    }

    #endregion

    #region Convert (DockLayout)

    private void Convert(DockLayout layout)
    {
        Deset();

        var result = Convert(Root, layout.Root);
        Root.Child = result;

        FindPanels(layout.Bottom)
            .ForEach(i => Root.BottomPanels.Add(i));
        FindPanels(layout.Left)
            .ForEach(i => Root.LeftPanels.Add(i));
        FindPanels(layout.Right)
            .ForEach(i => Root.RightPanels.Add(i));
        FindPanels(layout.Top)
            .ForEach(i => Root.TopPanels.Add(i));

        foreach (var i in layout.Floating)
        {
            var window = Convert(i);
            FindPanels(i.Bottom)
                .ForEach(j => window.Root.BottomPanels.Add(j));
            FindPanels(i.Left)
                .ForEach(j => window.Root.LeftPanels.Add(j));
            FindPanels(i.Right)
                .ForEach(j => window.Root.RightPanels.Add(j));
            FindPanels(i.Top)
                .ForEach(j => window.Root.TopPanels.Add(j));
        }

        Reset();
    }

    #endregion

    #region Create

    private static GridSplitter CreateGridSplitter(Orient orientation)
    {
        var result = new GridSplitter()
        {
            Background = System.Windows.Media.Brushes.Transparent,
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            ShowsPreview = true
        };

        if (orientation == Orient.Horizontal)
        {
            result.Height = double.NaN;
            result.ResizeDirection = GridResizeDirection.Columns;
            result.Width = 6;
        }
        else
        {
            result.Height = 6;
            result.ResizeDirection = GridResizeDirection.Rows;
            result.Width = double.NaN;
        }

        result.SetResourceReference(GridSplitter.StyleProperty, GridSplitterStyleKey);
        return result;
    }

    #endregion

    #region Deset/Reset

    private bool Deset(DockRootControl root, bool preserve = true, bool close = true)
    {
        bool cancel = false;

        if (root.Window != null)
            Unsubscribe(root.Window);

        Unsubscribe(Documents);

        //1. Minimized documents
        root.Minimized.Clear();
        for (var i = Minimized.Count - 1; i >= 0; i--)
        {
            var j = Minimized.ElementAt(i);
            if (j.Value is DockDocumentControl k && ReferenceEquals(k.Root, root))
            {
                if (!preserve)
                {
                    if (cancel)
                        break;

                    cancel = !Documents.Remove(j.Key);
                    if (!cancel)
                    {
                        Unsubscribe(j.Key);
                        Minimized.Remove(j.Key);
                    }
                }
                else
                {
                    Unsubscribe(j.Key); j.Key.IsMinimized = false; Subscribe(j.Key);
                    Minimized.Remove(j.Key);
                }
            }
        }

        if (cancel)
        {
            Subscribe(Documents);
            Subscribe(root.Window);
            return false;
        }

        //2. Unminimized documents/panels docked as documents
        for (var i = DocumentControls.Count - 1; i >= 0; i--)
        {
            var j = DocumentControls[i];
            if (j is DockDocumentControl k && ReferenceEquals(k.Root, root))
            {
                Unsubscribe(j);
                if (!preserve)
                {
                    //2a. Unminimized documents
                    for (var l = j.Source.Count - 1; l >= 0; l--)
                    {
                        if (cancel)
                            break;

                        if (j.Source[l] is Document)
                        {
                            cancel = !Documents.Remove(j.Source[l] as Document);
                            if (!cancel)
                                j.Source.RemoveAt(l);
                        }
                    }

                    if (cancel)
                    {
                        Subscribe(j);
                        break;
                    }

                    //2b. Panels docked as documents
                    for (var l = j.Source.Count - 1; l >= 0; l--)
                    {
                        if (j.Source[l] is Core.Panel p)
                        {
                            //Hide panel
                            p.IsVisible = false;
                            j.Source.RemoveAt(l);
                        }
                    }

                    DocumentControls.RemoveAt(i);
                }
                else
                {
                    //2a. Unminimized documents
                    j.Source.Clear();

                    //2b. Panels docked as documents
                    for (var l = j.Source.Count - 1; l >= 0; l--)
                    {
                        if (j.Source[l] is Core.Panel p)
                        {
                            //Hide panel
                            Unsubscribe(p); p.IsVisible = true; Subscribe(p);
                            j.Source.RemoveAt(l);
                        }
                    }
                }
                DocumentControls.RemoveAt(i);
            }
        }

        Subscribe(Documents);
        if (cancel)
        {
            if (root.Window != null)
                Subscribe(root.Window);

            return false;
        }

        if (root.Window != null)
            Floating.Remove(root.Window);

        //All documents and panels docked as documents are handled at this point

        //1. Pinned panels
        root.EachPin(i =>
        {
            i.ForEach(j =>
            {
                if (!preserve)
                    j.IsVisible = false;

                if (preserve)
                    Unsubscribe(j); j.IsVisible = true; Subscribe(j);

                Pins.Remove(j);
            });

            i.Clear();
            return true;
        });

        //2. Hidden panels
        for (var i = Hidden.Count - 1; i >= 0; i--)
        {
            var j = Hidden.ElementAt(i);
            if (j.Value is IDockPanelSource k && ReferenceEquals(k.Root, root))
            {
                if (preserve)
                {
                    Unsubscribe(j.Key); j.Key.IsVisible = true; Subscribe(j.Key);
                    Hidden.Remove(j.Key);
                }
                if (!preserve)
                    Hidden[j.Key] = null;
            }
        }

        //3. Visible panels
        for (var i = PanelControls.Count - 1; i >= 0; i--)
        {
            var j = PanelControls[i];
            if (j is DockPanelControl k && ReferenceEquals(k.Root, root))
            {
                Unsubscribe(j);

                if (preserve)
                    j.Source.ForEach<Core.Panel>(k => { Unsubscribe(k); k.IsVisible = true; Subscribe(k); });

                if (!preserve)
                    j.Source.ForEach<Core.Panel>(k => k.IsVisible = false);

                j.Source.Clear();
                PanelControls.RemoveAt(i);
            }
        }

        if (close)
            root.Window?.Close();

        return true;
    }

    private void Deset()
    {
        for (var i = Floating.Count - 1; i >= 0; i--)
            Deset(Floating[i].Root);

        Deset(Root);
    }

    private void Reset()
    {
        //Remove each empty DockPanelControl, if any
        for (var i = PanelControls.Count - 1; i >= 0; i--)
        {
            if (PanelControls[i].Source.Count == 0)
                PanelControls[i].Delete();
        }

        ///

        //Isolate panels that aren't allowed to share; isolated panels require a dedicated control
        var isolated = new List<Core.Panel>();
        foreach (var i in PanelControls)
        {
            if (i.Source.Count > 1)
            {
                if (i.Source.Any<Core.Panel>(j => !j.CanShare))
                {
                    for (var j = i.Source.Count - 1; j >= 0; j--)
                    {
                        if (i.Source[j] is Core.Panel panel)
                        {
                            if (!panel.CanShare)
                            {
                                isolated.Add(panel);
                                i.Source.RemoveAt(j);
                            }
                        }
                    }
                }
            }
        }
        foreach (var i in isolated)
            Show(i);

        ///

        //Make sure panels that are actually visible are marked as such
        Panels.ForEach(i => { Unsubscribe(i); i.IsVisible = FindPanel(i); Subscribe(i); });

        ///

        //Make sure at least one DockDocumentControl in the main root persists
        if (!DocumentControls.Any(i => i.Persist && ReferenceEquals(i.Root, Root)))
            DocumentControls.FirstOrDefault(i => ReferenceEquals(i.Root, Root)).IfNotNull(i => i.Persist = true);

        //Make sure all documents are subscribed and added where they need to go
        Documents.ForEach(Add);
    }

    #endregion

    #region Dock

    internal void DockObject(DockRootControl root, SecondaryDocks docks, IDockControl a, object b)
    {
        if (b is Content[] c)
            DockContent(root, docks, a, c);

        else if (b is DockLayoutElement d)
            DockElement(root, docks, a, d);

        else throw new NotSupportedException();
    }

    internal void DockContent(DockRootControl root, SecondaryDocks docks, IDockControl a, Content b)
    {
        if (b is Document)
        {
            if (docks == SecondaryDocks.Center)
                docks = Document.DefaultDockPreference;
        }
        else if (b is Core.Panel)
        {
            if (docks == SecondaryDocks.Center)
                docks = Core.Panel.DefaultDockPreference;
        }

        DockContent(root, docks, a, [b]);
    }

    internal void DockContent(DockRootControl root, SecondaryDocks docks, IDockControl a, Content[] b)
    {
        DockLayoutElement layout = b.First() is Document
            ? (DockLayoutElement)new DockLayoutDocumentGroup(b.Cast<Document>())
            : new DockLayoutPanelGroup(b.Cast<Core.Panel>());

        DockElement(root, docks, a, layout);
    }

    internal void DockContent(DockRootControl root, TertiaryDocks docks, IDockControl a, Content[] b)
    {
        DockElement(root, (SecondaryDocks)Enum.Parse(typeof(SecondaryDocks), $"{docks}"), a, new DockLayoutDocumentGroup(b));
    }

    internal void DockElement(DockRootControl root, SecondaryDocks docks, IDockControl a, DockLayoutElement b)
    {
        docks = docks == SecondaryDocks.Center ? SecondaryDocks.Left : docks;

        DockGroupControl result = null;
        switch (docks)
        {
            case SecondaryDocks.Left:
            case SecondaryDocks.Right:
                result = new DockGroupControl(root, Orient.Horizontal);
                break;
            case SecondaryDocks.Top:
            case SecondaryDocks.Bottom:
                result = new DockGroupControl(root, Orient.Vertical);
                break;
        }

        IDockControl aElement = a ?? root.Child;
        IDockControl bElement = Convert(root, b);

        if (aElement is null)
        {
            if (a is null)
                root.Child = bElement;

            return;
        }

        var gridSplitter = CreateGridSplitter(result.Orientation);

        if (a is null)
            root.Child = null;

        var index = a?.GetIndex();
        var parent = a?.GetParent();

        if (a is not null)
            parent.Children.RemoveAt(index.Value);

        switch (docks)
        {
            case SecondaryDocks.Left:
            case SecondaryDocks.Top:
                result.Children.Add(bElement as UIElement);
                result.Children.Add(gridSplitter);
                result.Children.Add(aElement as UIElement);
                break;
            case SecondaryDocks.Right:
            case SecondaryDocks.Bottom:
                result.Children.Add(aElement as UIElement);
                result.Children.Add(gridSplitter);
                result.Children.Add(bElement as UIElement);
                break;
        }

        GridLength[] lengths =
        [
            new(1, GridUnitType.Star),
            GridLength.Auto,
            new(1, GridUnitType.Star)
        ];

        for (var i = 0; i < 3; i++)
        {
            if (result.Orientation == Orient.Horizontal)
            {
                result.ColumnDefinitions.Add(new ColumnDefinition() { Width = lengths[i] });
                Grid.SetColumn(result.Children[i], i);
            }
            else
            {
                result.RowDefinitions.Add(new RowDefinition() { Height = lengths[i] });
                Grid.SetRow(result.Children[i], i);
            }
        }

        if (a is null)
            root.Child = result;

        if (a is not null)
        {
            parent.Children.Insert(index.Value, result);
            if (parent.Orientation == Orient.Horizontal)
                Grid.SetColumn(result, index.Value);

            if (parent.Orientation == Orient.Vertical)
                Grid.SetRow(result, index.Value);
        }
    }

    ///

    internal void DockCenter(DockDragEvent e, int index = -1)
    {
        void Close() => handleClosing.Do(() => LastDrag.Window.Close());
        if (e.Content?.Length > 0)
        {
            if (e.MouseOver is DockDocumentControl documents)
            {
                Close();

                index = index == -1 ? 0 : index;
                for (var i = e.Content.Length - 1; i >= 0; i--)
                    documents.Source.Insert(index, e.Content[i]);

                documents.SelectedIndex = index;
            }
            else if (e.MouseOver is DockPanelControl panels)
            {
                //Assuming there is only one panel already present, does it share?
                if (panels.Source.First<Core.Panel>().CanShare)
                {
                    if (e.Content.First() is Core.Panel panel)
                    {
                        if (panel.CanShare)
                        {
                            Close();

                            index = index == -1 ? 0 : index;
                            for (var i = e.Content.Length - 1; i >= 0; i--)
                                panels.Source.Insert(index, e.Content[i]);

                            panels.SelectedIndex = index;
                        }
                    }
                }
            }
        }
    }

    internal void DockPin(DockPanelBar target, DockDragEvent e, int index = -1)
    {
        if (e.Content?.Length > 0)
        {
            handleClosing.Do(() => LastDrag.Window.Close());

            index = index == -1 ? 0 : index;
            for (var i = e.Content.Length - 1; i >= 0; i--)
                target.Source.Insert(index, e.Content[i]);
        }
    }

    ///

    internal void DockPrimary(SecondaryDocks docks)
    {
        DockObject(LastDrag.MouseOver.Root, docks, null, LastDrag.ActualContent);
        handleClosing.Do(() => LastDrag.Window.Close());
    }

    internal void DockSecondary(SecondaryDocks docks)
    {
        if (docks == SecondaryDocks.Center)
        {
            DockCenter(LastDrag);
            return;
        }

        var parent = LastDrag.MouseOver.GetParent();
        if (parent is null)
        {
            DockObject(LastDrag.MouseOver.Root, docks, null, LastDrag.ActualContent);
        }
        else
        {
            var index = LastDrag.MouseOver.GetIndex();
            if (index == -1) return;

            DockObject(LastDrag.MouseOver.Root, docks, LastDrag.MouseOver, LastDrag.ActualContent);
        }
        handleClosing.Do(() => LastDrag.Window.Close());
    }

    /// <summary>
    /// How panels become documents...
    /// </summary>
    internal void DockTertiary(TertiaryDocks docks)
    {
        if (LastDrag.ActualContent is Content[] content)
        {
            var parent = LastDrag.MouseOver.GetParent();
            if (parent is null)
            {
                DockContent(LastDrag.MouseOver.Root, docks, null, content);
            }
            else
            {
                var index = LastDrag.MouseOver.GetIndex();
                if (index == -1) return;

                DockContent(LastDrag.MouseOver.Root, docks, LastDrag.MouseOver, content);
            }
            handleClosing.Do(() => LastDrag.Window.Close());
        }
    }

    #endregion

    #region Each

    internal void EachControl(Predicate<DockContentControl> @continue)
    {
        foreach (var i in DocumentControls)
        {
            if (!@continue(i))
                return;
        }
        foreach (var i in PanelControls)
        {
            if (!@continue(i))
                return;
        }
    }

    internal void EachRoot(Predicate<DockRootControl> @continue)
    {
        if (!@continue(Root))
            return;

        foreach (var i in Floating)
        {
            if (!@continue(i.Root))
                break;
        }
    }

    #endregion

    #region Find

    private bool FindAnchor(DockAnchorPanelCollection input)
    {
        var result = false;
        EachRoot(i =>
        {
            result = false;
            i.EachPin(j =>
            {
                if (j == input)
                {
                    result = true;
                    return false;
                }
                return true;
            });
            return !result;
        });
        return result;
    }

    private DockAnchorDocumentCollection FindAnchor(Document input)
    {
        DockAnchorDocumentCollection result = null;
        EachRoot(i =>
        {
            if (i.Minimized.Contains(input))
            {
                result = i.Minimized;
                return false;
            }
            return true;
        });
        return result;
    }

    private DockAnchorPanelCollection FindAnchor(Core.Panel input)
    {
        DockAnchorPanelCollection result = null;
        EachRoot(i =>
        {
            i.EachPin(j =>
            {
                foreach (var k in j)
                {
                    if (k.Equals(input))
                    {
                        result = j;
                        return false;
                    }
                }
                return true;
            });
            return result is null;
        });
        return result;
    }

    private static DockAnchorPanelCollection FindAnchor(DockContentControl control)
    {
        DockAnchorPanelCollection result;
        if (control.Parent is DockGroupControl)
        {
            //If it's horizontal, it should go top or bottom (whichever is closest)
            if (control.ActualHeight < control.ActualWidth)
            {
                //Get the y position of the old home
                var yTop = control.TranslatePoint(new Point(0, 0), control.Root).Y;
                var yBottom = yTop + control.ActualHeight;

                //Get the absolute y position
                var aBottom = control.Root.ActualHeight;

                //Is the bottom y closer to the absolute bottom than the top y is to the absolute top? If so, go bottom. If not, go top.
                result
                = aBottom - yBottom < yTop
                ? control.Root.BottomPanels
                : control.Root.TopPanels;
            }
            //If it's vertical
            else
            {
                //Get the x position of the old home
                var xLeft = control.TranslatePoint(new Point(0, 0), control.Root).X;
                var xRight = xLeft + control.ActualWidth;

                //Get the absolute x position
                var aRight = control.Root.ActualWidth;

                //Is the left x closer to the absolute left than the right x is to the absolute right? If so, go left. If not, go right.
                result
                    = aRight - xRight < xLeft
                    //It's closer to the right
                    ? control.Root.RightPanels
                    //It's closer to the left
                    : control.Root.LeftPanels;
            }
        }
        //Horizontal
        else if (control.ActualHeight < control.ActualWidth)
        {
            result = control.Root.TopPanels;
        }
        //Vertical
        else
        {
            result = control.Root.LeftPanels;
        }
        return result;
    }

    ///

    private bool IsCompatible(DockContentControl input)
        => !input.Source.Any<Core.Panel>(i => !i.CanShare);

    private DockContentControl FindCompatible(DockRootControl root, Core.Panel input)
    {
        if (input.CanShare)
        {
            var result = PanelControls.Where(IsCompatible);
            return result.FirstOrDefault(i => ReferenceEquals(i.Root, root));
        }
        return null;
    }

    ///

    private DockContentControl FindControl(Content input)
        => (DockContentControl)FindControl(input as Document)
        ?? FindControl(input as Core.Panel);

    private DockDocumentControl FindControl(Document input)
    {
        foreach (var i in DocumentControls)
        {
            foreach (var j in i.Source)
            {
                if (ReferenceEquals(j, input))
                    return i;
            }
        }
        return null;
    }

    private DockContentControl FindControl(Core.Panel input)
    {
        foreach (var i in DocumentControls)
        {
            foreach (var j in i.Source)
            {
                if (ReferenceEquals(j, input))
                    return i;
            }
        }
        foreach (var i in PanelControls)
        {
            foreach (var j in i.Source)
            {
                if (ReferenceEquals(j, input))
                    return i;
            }
        }
        return null;
    }

    ///

    private bool FindPanel(Core.Panel input)
        => FindAnchor(input) != null || FindControl(input) != null;

    private IEnumerable<Core.Panel> FindPanels(List<DockLayoutPanel> input)
    {
        foreach (var i in input)
        {
            if (Panels.FirstOrDefault(j => i.Name == j.Name) is Core.Panel panel)
                yield return panel;
        }
        yield break;
    }

    ///

    private DockRootControl FindRoot(Document input)
        => FindAnchor(input)?.Root ?? FindControl(input)?.Root;

    #endregion

    #region Float

    internal DockWindow Float(Content[] content, Point? position)
    {
        if (content?.Length > 0)
        {
            var result = new DockLayoutWindow();
            if (position != null)
                result.Position = new(position.Value.X, position.Value.Y);

            result.Root = content.Any<Document>()
                ? new DockLayoutDocumentGroup(content)
                : new DockLayoutPanelGroup(content.Cast<Core.Panel>());

            var control = FindControl(content[0]);
            if (control != null)
            {
                result.Size = control is DockPanelControl x && x.Collapse
                    ? null : new MSize<double>(control.ActualHeight, control.ActualWidth);
            }

            foreach (var i in content)
            {
                var a = FindControl(i);
                if (a != null)
                    a.Source.Remove(i);

                else if (i is Core.Panel j && FindAnchor(j) is DockAnchorPanelCollection b)
                {
                    b.Remove(j);
                    Pins.Remove(j);
                }
                else if (i is Document k && FindRoot(k) is DockRootControl c)
                {
                    c.Minimized.Remove(k);

                    Unsubscribe(k); k.IsMinimized = false; Subscribe(k);
                    Minimized.Remove(k);
                }
            }
            return Convert(result);
        }
        return null;
    }

    private void Float(Content i) => Float([i], FindControl(i)?.PointToScreen(new Point(0, 0)));

    private void FloatAll(Content i)
    {
        var control = FindControl(i);

        var result = new List<Content>();
        foreach (Content j in control.Source)
        {
            if (j.CanFloat)
                result.Add(j);
        }
        if (result.Count > 0)
            Float([.. result], control.PointToScreen(new Point(0, 0)));
    }

    #endregion

    #region Group

    private void NewGroup(Content i, SecondaryDocks j)
    {
        var control = FindControl(i);
        if (control != null)
        {
            control.Source.Remove(i);
            DockContent(control.Root, j, control, i);
        }
    }

    private void NewHorizontalGroup(Content i) => NewGroup(i, SecondaryDocks.Top);

    private void NewVerticalGroup(Content i) => NewGroup(i, SecondaryDocks.Right);

    #endregion

    #region Handlers

    private void OnContentItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is IListObservable source)
        {
            DockContentControl control = null;
            EachControl(i => { control = ReferenceEquals(i.Source, source) ? i : null; return control is null; });

            if (control is DockDocumentControl a)
            {
                if (source.Count == 0)
                {
                    if (a.Persist)
                        return;
                }
            }

            if (source.Count == 0)
            {
                if (control is DockPanelControl b)
                {
                    var skip = b.Root.AllPins().Any(i => i.Count > 0);
                    b.Delete();

                    if (skip) return;
                }

                if (control.Root.Window != null)
                {
                    if (control.Root.IsEmpty)
                        control.Root.Window.Close();
                }
            }
        }
    }

    private void OnContentMouseEnter(object sender, MouseEventArgs e) => Mark(sender as DockContentControl);

    private void OnContentMouseLeave(object sender, MouseEventArgs e) { }

    ///

    private void OnDocumentsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add((Document)e.NewItems[0]);
                break;

            case NotifyCollectionChangedAction.Remove:
                Remove((Document)e.OldItems[0]);
                break;

            case NotifyCollectionChangedAction.Replace:
                break;
        }
    }

    private void OnDocumentPropertyChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        if (sender is Document document)
        {
            switch (e.PropertyName)
            {
                case nameof(Document.IsChanged):

                    if (!document.IsMinimized)
                        Unminimize(document);

                    else if (document.CanMinimize)
                        Minimize(document);

                    else document.IsMinimized = false;
                    break;
                case nameof(Document.IsMinimized):

                    if (!document.IsMinimized)
                        Unminimize(document);

                    else if (document.CanMinimize)
                        Minimize(document);

                    else document.IsMinimized = false;
                    break;
            }
        }
    }

    ///

    private void OnFloatingAnchorChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        handleAnchor.DoInternal(() =>
        {
            if (sender is IDockContentSource items)
            {
                if (items.Root.IsEmpty)
                {
                    Unsubscribe(items.Root.Window);
                    items.Root.Window.Close();

                    Floating.Remove(items.Root.Window);
                }
            }
        });
    }

    private void OnFloatingClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is DockWindow window)
            handleClosing.DoInternal(() => e.Cancel = !Deset(window.Root, false, false), () => Deset(window.Root, true, false));
    }

    ///

    private void OnGlobalMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (Dragging)
        {
            if (Drag.Window != null)
            {
                var position = XCursor.GetPosition();
                Drag.Window.Left
                    = position.X + FloatingWindowDragOffset;
                Drag.Window.Top
                    = position.Y + FloatingWindowDragOffset;
            }
        }
    }

    private void OnGlobalMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        LastDrag = Drag;
        Drag = null;
    }

    ///

    private void OnLoaded()
    {
        mouseListener.MouseMove
            += OnGlobalMouseMove;
        mouseListener.MouseUp
            += OnGlobalMouseUp;

        ParentWindow.Activated
            += OnParentOrFloatingWindowActivated;
        ParentWindow.Closing
            += OnParentWindowClosing;
    }

    private void OnUnloaded()
    {
        mouseListener.MouseMove
            -= OnGlobalMouseMove;
        mouseListener.MouseUp
            -= OnGlobalMouseUp;

        ParentWindow.Activated
            -= OnParentOrFloatingWindowActivated;
        ParentWindow.Closing
            -= OnParentWindowClosing;

        //Deset(); This needs tested!
    }

    ///

    private void OnPanelHeightRequested(Core.Panel sender, double length)
    {
        var rowDefinition = FindControl(sender)?.RowDefinition;
        if (rowDefinition != null)
            rowDefinition.Height = new GridLength(length, GridUnitType.Pixel);
    }

    private void OnPanelPropertyChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        if (sender is Core.Panel panel)
        {
            switch (e.PropertyName)
            {
                case nameof(Core.Panel.IsVisible):
                    if (panel.IsVisible)
                        Show(panel);

                    else if (panel.CanHide)
                        Hide(panel);

                    else panel.IsVisible = true;
                    break;
            }
        }
    }

    private void OnPanelsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Add((Core.Panel)e.NewItems[0]);
                break;

            case NotifyCollectionChangedAction.Remove:
                Remove((Core.Panel)e.OldItems[0]);
                break;

            case NotifyCollectionChangedAction.Replace:
                break;
        }
    }

    private void OnPanelWidthRequested(Core.Panel sender, double length)
    {
        var columnDefinition = FindControl(sender)?.ColumnDefinition;
        if (columnDefinition != null)
            columnDefinition.Width = new GridLength(length, GridUnitType.Pixel);
    }

    ///

    private void OnParentOrFloatingWindowActivated(object sender, EventArgs e)
    {
        ActiveRoot = sender is DockWindow window ? window.Root : Root;
    }

    private void OnParentWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is Window window)
        {
            if (XWindow.GetDisableCancel(window))
                return;
        }

        if (Documents.Any(i => i.IsChanged))
        {
            Dialog.ShowResult("Close", new Warning("One or more documents have unsaved changes. Close anyway?"), i =>
            {
                e.Cancel = i == 1;
            },
            Buttons.YesNo);
        }

        if (!e.Cancel)
        {
            if (AutoSave)
            {
                /* Something causes infinite layouts to save (1 save each time app opens). Disable feature for now...
                if (Layouts != null)
                {
                    if (Layouts.Layout.NullOrEmpty())
                        Layouts.Update(Storage.File.Long.ClonePath($@"{Layouts.Path}\Default.xml"));

                    Layouts.Save();
                }
                */
            }
            for (var i = Floating.Count - 1; i >= 0; i--)
            {
                Unsubscribe(Floating[i]);
                Floating[i].Close();
            }
        }
    }

    #endregion

    #region Hide/Show

    private void Hide(Core.Panel panel)
    {
        if (!Hidden.ContainsKey(panel))
        {
            IDockPanelSource source = null;

            var control = FindControl(panel);
            if (control != null)
            {
                control.Source.Remove(panel);
                source = control;
            }
            else if (FindAnchor(panel) is DockAnchorPanelCollection anchor)
            {
                anchor.Remove(panel);
                source = anchor;
            }

            if (source != null)
                Hidden.Add(panel, source);
        }
    }

    private void Show(Core.Panel panel)
    {
        IDockPanelSource source = null;
        if (Hidden.ContainsKey(panel))
        {
            if (Hidden[panel] is DockContentControl a)
            {
                if (DocumentControls.Contains(a) || PanelControls.Contains(a))
                    source = a;
            }
            else if (Hidden[panel] is DockAnchorPanelCollection b)
                source = FindAnchor(b) ? b : Root.LeftPanels;

            if (source is null)
            {
                if (panel.DockPreference == SecondaryDocks.Center)
                    source = FindCompatible(Root, panel);
            }

            Hidden.Remove(panel);
        }
        else if (panel.DockPreference == SecondaryDocks.Center)
            source = FindCompatible(Root, panel);

        if (source != null)
            source.Source.Add(panel);

        else DockContent(Root, panel.DockPreference, null, panel);
    }

    #endregion

    #region Mark

    private void Mark(DockContentControl input)
    {
        if (Dragging)
        {
            if (input != null)
            {
                Drag.MouseOver
                    = input;
                Drag.MousePosition
                    = input.GetPosition();

                input.Root.MarkSecondary(Drag.MousePosition.X - 50, Drag.MousePosition.Y - 50);
            }
        }
    }

    #endregion

    #region Minimize/Unminimize

    private void Minimize(Document input)
    {
        if (FindAnchor(input) is null)
        {
            if (FindControl(input) is DockDocumentControl control)
            {
                if (control.Root.Window != null)
                    Unsubscribe(control.Root.Window);

                Unsubscribe(control); control.Source.Remove(input); Subscribe(control);
                if (!control.Persist && DocumentControls.Count(i => i.Root == control.Root) > 1)
                {
                    control.Delete();
                    Minimized.Add(input, null);
                }
                else Minimized.Add(input, control);

                handleAnchor
                    .Do(() => control.Root.Minimized.Add(input));

                if (control.Root.Window != null)
                    Subscribe(control.Root.Window);
            }
        }

    }

    private void Unminimize(Document input)
    {
        if (FindControl(input) is not DockDocumentControl control)
        {
            var root = FindRoot(input);
            handleAnchor.Do(() => root.Minimized.Remove(input));

            control = Minimized.ContainsKey(input) ? Minimized[input] : null;
            Minimized.Remove(input);

            control = DocumentControls.FirstOrDefault(i => i.Root.Equals(root)) ?? DocumentControls.FirstOrDefault<DockDocumentControl>();
            if (control != null)
            {
                Unsubscribe(control); control.Source.Add(input); Subscribe(control);
                return;
            }

            DockContent(Root, input.DockPreference, null, input);
        }
    }

    #endregion

    #region Move

    private void MoveDocumentToPreviousGroup(Document i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() - 2;
        if (XDockControl.GetParent(a).Children[index] is DockDocumentControl b)
        {
            a.Source.Remove(i);
            b.Source.Add(i);
        }
    }

    private void MoveAllDocumentsToPreviousGroup(Document i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() - 2;
        if (XDockControl.GetParent(a).Children[index] is DockDocumentControl b)
        {
            for (var j = a.Source.Count - 1; j >= 0; j--)
            {
                var c = a.Source[j];
                a.Source.RemoveAt(j);
                b.Source.Add(c);
            }
        }
    }

    private void MoveDocumentToNextGroup(Document i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() + 2;
        if (XDockControl.GetParent(a).Children[index] is DockDocumentControl b)
        {
            a.Source.Remove(i);
            b.Source.Add(i);
        }
    }

    private void MoveAllDocumentsToNextGroup(Document i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() + 2;
        if (XDockControl.GetParent(a).Children[index] is DockDocumentControl b)
        {
            for (var j = a.Source.Count - 1; j >= 0; j--)
            {
                var c = a.Source[j];
                a.Source.RemoveAt(j);
                b.Source.Add(c);
            }
        }
    }

    ///

    private void MovePanelToPreviousGroup(Core.Panel i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() - 2;
        if (XDockControl.GetParent(a).Children[index] is DockContentControl b)
        {
            if (a.GetType() == b.GetType())
            {
                a.Source.Remove(i);
                b.Source.Add(i);
            }
        }
    }

    private void MoveAllPanelsToPreviousGroup(Core.Panel i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() - 2;
        if (XDockControl.GetParent(a).Children[index] is DockContentControl b)
        {
            if (a.GetType() == b.GetType())
            {
                for (var j = a.Source.Count - 1; j >= 0; j--)
                {
                    var c = a.Source[j];
                    a.Source.RemoveAt(j);
                    b.Source.Add(c);
                }
            }
        }
    }

    private void MovePanelToNextGroup(Core.Panel i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() + 2;
        if (XDockControl.GetParent(a).Children[index] is DockContentControl b)
        {
            if (a.GetType() == b.GetType())
            {
                a.Source.Remove(i);
                b.Source.Add(i);
            }
        }
    }

    private void MoveAllPanelsToNextGroup(Core.Panel i)
    {
        var a = FindControl(i);

        var index = a.GetIndex() + 2;
        if (XDockControl.GetParent(a).Children[index] is DockContentControl b)
        {
            if (a.GetType() == b.GetType())
            {
                for (var j = a.Source.Count - 1; j >= 0; j--)
                {
                    var c = a.Source[j];
                    a.Source.RemoveAt(j);
                    b.Source.Add(c);
                }
            }
        }
    }

    #endregion

    #region Pin/Unpin

    private void Pin(Core.Panel input)
    {
        handleAnchor.Do(() =>
        {
            if (input.IsVisible)
            {
                if (!Pins.ContainsKey(input))
                {
                    if (FindAnchor(input) is null)
                    {
                        if (FindControl(input) is DockContentControl control)
                        {
                            input.PinHeight
                                = new ControlLength(control.ActualHeight, ControlLengthUnit.Pixel);
                            input.PinWidth
                                = new ControlLength(control.ActualWidth, ControlLengthUnit.Pixel);

                            var newSource = FindAnchor(control);
                            newSource.Add(input);

                            control.Source.Remove(input);
                            Pins.Add(input, control);
                        }
                    }
                }
            }
        });
    }

    private void Unpin(Core.Panel input)
    {
        handleAnchor.Do(() =>
        {
            if (input.IsVisible)
            {
                if (Pins.ContainsKey(input))
                {
                    if (FindAnchor(input) is DockAnchorPanelCollection anchor)
                    {
                        IDockPanelSource oldSource = Pins[input];
                        if (oldSource != null && !DocumentControls.Contains(oldSource) && !PanelControls.Contains(oldSource))
                        {
                            oldSource = null;
                            if (input.DockPreference == SecondaryDocks.Center)
                                oldSource = FindCompatible(anchor.Root, input);
                        }

                        anchor.Remove(input);

                        if (oldSource is null)
                        {
                            DockContent(anchor.Root, input.DockPreference, null, input);
                        }
                        else
                        {
                            oldSource.Source.Add(input);
                        }

                        Pins.Remove(input);
                    }
                }
            }
        });
    }

    #endregion

    #region Refresh

    private bool CanRefresh() => Documents is not null && Layout is not null && Panels is not null;

    private async Task Refresh(CancellationToken token)
    {
        if (!CanRefresh())
            return;

        Refreshing = true;

        var result = await ContentSerializer.Deserialize(Layout);
        if (result is Success<DockLayout> success)
        {
            IsLayoutInvalid = false;
            await Task.Run(() => { }/*Thread.Sleep(1500)*/, token);

            Convert(success.Data);
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }
        else IsLayoutInvalid = true;

        Refreshing = false;
    }

    #endregion

    #region Subscribe/Unsubscribe

    private void Subscribe(Document i)
    {
        Unsubscribe(i);
        i.PropertySet += OnDocumentPropertyChanged;
    }

    private void Unsubscribe(Document i)
    {
        i.PropertySet -= OnDocumentPropertyChanged;
    }

    ///

    private void Subscribe(DocumentCollection input)
    {
        Unsubscribe(input);
        input.CollectionChanged += OnDocumentsChanged;
        foreach (var i in input)
            Subscribe(i);
    }

    private void Unsubscribe(DocumentCollection input)
    {
        input.CollectionChanged -= OnDocumentsChanged;
        foreach (var i in input)
            Unsubscribe(i);
    }

    ///

    private void Subscribe(Core.Panel i)
    {
        Unsubscribe(i);
        i.HeightRequested
            += OnPanelHeightRequested;
        i.PropertySet
            += OnPanelPropertyChanged;
        i.WidthRequested
            += OnPanelWidthRequested;
    }

    private void Unsubscribe(Core.Panel i)
    {
        i.HeightRequested
            -= OnPanelHeightRequested;
        i.PropertySet
            -= OnPanelPropertyChanged;
        i.WidthRequested
            -= OnPanelWidthRequested;
    }


    ///

    private void Subscribe(PanelCollection input)
    {
        Unsubscribe(input);
        input.CollectionChanged += OnPanelsChanged;
        foreach (var i in input)
            Subscribe(i);
    }

    private void Unsubscribe(PanelCollection input)
    {
        input.CollectionChanged -= OnPanelsChanged;
        foreach (var i in input)
            Unsubscribe(i);
    }

    ///

    internal void Subscribe(DockContentControl input)
    {
        Unsubscribe(input);
        input.MouseEnter
            += OnContentMouseEnter;
        input.MouseLeave
            += OnContentMouseLeave;
        input.Source.CollectionChanged
            += OnContentItemsChanged;
    }

    internal void Unsubscribe(DockContentControl input)
    {
        input.MouseEnter
            -= OnContentMouseEnter;
        input.MouseLeave
            -= OnContentMouseLeave;
        input.Source.CollectionChanged
            -= OnContentItemsChanged;
    }

    ///

    private void Subscribe(DockWindow input)
    {
        Unsubscribe(input);
        input.Activated
            += OnParentOrFloatingWindowActivated;
        input.Closing
            += OnFloatingClosing;

        input.Root.Minimized.CollectionChanged
            += OnFloatingAnchorChanged;
        input.Root.EachPin(i =>
        {
            i.CollectionChanged += OnFloatingAnchorChanged;
            return true;
        });
    }

    private void Unsubscribe(DockWindow input)
    {
        input.Activated
            -= OnParentOrFloatingWindowActivated;
        input.Closing
            -= OnFloatingClosing;

        input.Root.Minimized.CollectionChanged
            -= OnFloatingAnchorChanged;
        input.Root.EachPin(i =>
        {
            i.CollectionChanged -= OnFloatingAnchorChanged;
            return true;
        });
    }

    #endregion

    #endregion

    #region Protected

    protected virtual void OnActiveContentChanged(Content input) => handleActive.DoInternal(() =>
    {
        if (input != null)
        {
            if (input is Document document)
                SetCurrentValue(ActiveDocumentProperty, document);

            if (input is Core.Panel panel)
                SetCurrentValue(ActivePanelProperty, panel);

            Activate(input);
        }
        else
        {
            SetCurrentValue
                (ActiveDocumentProperty, null);
            SetCurrentValue
                (ActivePanelProperty, null);
        }
    });

    protected virtual void OnActiveDocumentChanged(ValueChange<Document> input) => handleActive.DoInternal(() =>
    {
        if (input.NewValue != null)
        {
            SetCurrentValue(ActiveContentProperty, input.NewValue);
            Activate(input.NewValue);
        }
        else if (ReferenceEquals(input.OldValue, ActiveContent))
            SetCurrentValue(ActiveContentProperty, null);
    });

    protected virtual void OnActivePanelChanged(ValueChange<Core.Panel> input) => handleActive.DoInternal(() =>
    {
        if (input.NewValue != null)
        {
            SetCurrentValue(ActiveContentProperty, input.NewValue);
            Activate(input.NewValue);
        }
        else if (ReferenceEquals(input.OldValue, ActiveContent))
            SetCurrentValue(ActiveContentProperty, null);
    });

    protected virtual void OnActiveRootChanged(ValueChange<DockRootControl> input)
    {
        Activate(input.NewValue);
    }

    protected virtual void OnDocumentsChanged(ValueChange<DocumentCollection> input)
    {
        if (input.OldValue != null)
            Unsubscribe(input.OldValue);

        if (input.NewValue != null)
            Subscribe(input.NewValue);

        _ = refreshTask.Start();
    }

    protected virtual void OnDragChanged(ValueChange<DockDragEvent> input)
    {
        Dragging = input.NewValue != null;
        var e = input.NewValue;
        if (e?.Source?.Source.Count > 0)
            Mark(e.Source);
    }

    protected virtual void OnDraggingChanged(ValueChange<bool> input)
    {
        mouseListener.Enabled = input.NewValue;
    }

    protected virtual void OnLayoutChanged(ValueChange<object> input)
    {
        _ = refreshTask.Start();
    }

    protected virtual void OnPanelsChanged(ValueChange<PanelCollection> input)
    {
        if (input.OldValue != null)
            Unsubscribe(input.OldValue);

        if (input.NewValue != null)
            Subscribe(input.NewValue);

        _ = refreshTask.Start();
    }

    #endregion

    #region Internal 

    internal void StartDrag(DockDragEvent e) => Drag = e;

    #endregion

    #region Public 

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.GetChild(BorderKey).Child = Root;
    }

    public void Find(IFind document)
    {
        if (document != null)
        {
            ActiveFind = document;
            Panels.FirstOrDefault<FindPanel>().IfNotNull(j => j.FindText = Clipboard.GetText());
        }
    }

    #endregion

    #region Commands

    private ICommand closeDocumentCommand;
    public ICommand CloseDocumentCommand => closeDocumentCommand ??= new RelayCommand<Document>(i => Documents.Remove(i), i => i?.CanClose == true);

    private ICommand closeAllDocumentsCommand;
    public ICommand CloseAllDocumentsCommand => closeAllDocumentsCommand ??= new RelayCommand(() => Documents.Clear(), () => Documents?.Any<Document>() == true);

    private ICommand closeAllDocumentsButThisCommand;
    public ICommand CloseAllDocumentsButThisCommand => closeAllDocumentsButThisCommand ??= new RelayCommand<Document>(i =>
    {
        for (var j = Documents.Count - 1; j >= 0; j--)
        {
            if (!i.Equals(Documents[j]))
                Documents.RemoveAt(j);
        }
    },
    i => Documents?.Count > 0 && i != null);

    private ICommand closeFindCommand;
    public ICommand CloseFindCommand => closeFindCommand ??= new RelayCommand(() => ActiveFind = null, () => ActiveFind != null);

    ///

    private ICommand collapsePanelsCommand;
    public ICommand CollapsePanelsCommand
        => collapsePanelsCommand ??= new RelayCommand<Core.Panel>(i =>
        {
            var control = FindControl(i) as DockPanelControl;
            control.Collapse = true;

            if (control.Parent is DockGroupControl parent)
            {
                if (parent.Orientation == Orient.Horizontal)
                {
                    control.ColumnDefinition.Width
                        = new(1, GridUnitType.Auto);
                }
                if (parent.Orientation == Orient.Vertical)
                {
                    control.RowDefinition.Height
                        = new(1, GridUnitType.Auto);
                }
                CollapseExpand(parent, control, false);
            }
            control.SelectedIndex = -1;
        },
        i => FindControl(i) is DockPanelControl j && !j.Collapse);

    private ICommand expandPanelsCommand;
    public ICommand ExpandPanelsCommand
        => expandPanelsCommand ??= new RelayCommand<Core.Panel>(i =>
        {
            var control = FindControl(i) as DockPanelControl;
            control.Collapse = false;

            if (control.Parent is DockGroupControl parent)
            {
                if (parent.Orientation == Orient.Horizontal)
                {
                    control.ColumnDefinition.Width
                        = new(1, GridUnitType.Star);
                }
                if (parent.Orientation == Orient.Vertical)
                {
                    control.RowDefinition.Height
                        = new(1, GridUnitType.Star);
                }
                CollapseExpand(parent, control, true);
            }
        },
        i => FindControl(i) is DockPanelControl j && j.Collapse);

    ///

    private ICommand dockCommand;
    public ICommand DockCommand => dockCommand ??= new RelayCommand<DockWindow>(i =>
    {
        DockElement(Root, SecondaryDocks.Left, null, Convert(i.Root.Child));
        handleClosing.Do(() => i.Close());
    },
    i => i != null);

    private ICommand dockDocumentCommand;
    public ICommand DockDocumentCommand
        => dockDocumentCommand ??= new RelayCommand<Document>(i => { }, i => true);

    private ICommand dockAllDocumentsCommand;
    public ICommand DockAllDocumentsCommand
        => dockAllDocumentsCommand ??= new RelayCommand(() => { }, () => true);

    private ICommand dockPanelCommand;
    public ICommand DockPanelCommand
        => dockPanelCommand ??= new RelayCommand<Core.Panel>(i => { }, i => true);

    private ICommand dockAllPanelsCommand;
    public ICommand DockAllPanelsCommand
        => dockAllPanelsCommand ??= new RelayCommand(() => { }, () => true);

    private ICommand dockPinCommand;
    public ICommand DockPinCommand => dockPinCommand ??= new RelayCommand<DockPanelButton>(i =>
    {
        if (LastDrag != null)
        {
            var control = i.GetParent<DockPanelBar>();
            if (control != null)
            {
                var index = control.ItemContainerGenerator.IndexFromContainer(i.GetParent<ContentPresenter>());
                DockPin(control, LastDrag, index);
            }
        }
    },
    i => i != null);

    private ICommand dockTabCommand;
    public ICommand DockTabCommand => dockTabCommand ??= new RelayCommand<TabItem>(i =>
    {
        if (LastDrag != null)
        {
            var control = i.GetParent<TabControl>();
            if (control != null)
            {
                var index = control.ItemContainerGenerator.IndexFromContainer(i);
                DockCenter(LastDrag, index);
            }
        }
    },
    i => i != null);

    private ICommand findResultsCommand;
    public ICommand FindResultsCommand => findResultsCommand ??= new RelayCommand<FindResultList>(i =>
    {
        if (Panels.FirstOrDefault<FindResultPanel>(j => !j.KeepResults) is FindResultPanel oldPanel)
            oldPanel.Results = i;

        else Panels.Add(new FindResultPanel(i));
    },
    i => i != null);

    ///

    private ICommand floatCommand;
    public ICommand FloatCommand
        => floatCommand ??= new RelayCommand<Content>(i => Float(i), i => i?.CanFloat == true);

    private ICommand floatAllDocumentsCommand;
    public ICommand FloatAllDocumentsCommand
        => floatAllDocumentsCommand ??= new RelayCommand<Document>(i => FloatAll(i), i => FindControl(i)?.Source.Count > 1);

    private ICommand floatAllPanelsCommand;
    public ICommand FloatAllPanelsCommand
        => floatAllPanelsCommand ??= new RelayCommand<Core.Panel>(i => FloatAll(i), i => FindControl(i)?.Source.Count > 1);

    ///

    private ICommand hideCommand;
    public ICommand HideCommand
        => hideCommand ??= new RelayCommand<Core.Panel>(i => i.IsVisible = false, i => i?.CanHide == true && i.IsVisible);

    private ICommand hideAllCommand;
    public ICommand HideAllCommand
        => hideAllCommand ??= new RelayCommand(() => Panels.ForEach(i => i.IsVisible = false), () => Panels?.Any(j => j?.CanHide == true && j.IsVisible) == true);

    ///

    private ICommand moveDocumentToPreviousGroupCommand;
    public ICommand MoveDocumentToPreviousGroupCommand => moveDocumentToPreviousGroupCommand ??= new RelayCommand<Document>(i => MoveDocumentToPreviousGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            var index = a.GetIndex() - 2;
            if (index >= 0)
            {
                if (XDockControl.GetParent(a)?.Children[index] is DockDocumentControl)
                    return true;
            }
        }
        return false;
    });

    private ICommand moveAllDocumentsToPreviousGroupCommand;
    public ICommand MoveAllDocumentsToPreviousGroupCommand => moveAllDocumentsToPreviousGroupCommand ??= new RelayCommand<Document>(i => MoveAllDocumentsToPreviousGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            if (a.Source.Count > 1)
            {
                var index = a.GetIndex() - 2;
                if (index >= 0)
                {
                    if (XDockControl.GetParent(a)?.Children[index] is DockDocumentControl)
                        return true;
                }
            }
        }
        return false;
    });

    private ICommand moveDocumentToNextGroupCommand;
    public ICommand MoveDocumentToNextGroupCommand => moveDocumentToNextGroupCommand ??= new RelayCommand<Document>(i => MoveDocumentToNextGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            var index = a.GetIndex() + 2;
            if (index < XDockControl.GetParent(a)?.Children.Count)
            {
                if (XDockControl.GetParent(a).Children[index] is DockDocumentControl)
                    return true;
            }
        }
        return false;
    });

    private ICommand moveAllDocumentsToNextGroupCommand;
    public ICommand MoveAllDocumentsToNextGroupCommand => moveAllDocumentsToNextGroupCommand ??= new RelayCommand<Document>(i => MoveAllDocumentsToNextGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            if (a.Source.Count > 1)
            {
                var index = a.GetIndex() + 2;
                if (index < XDockControl.GetParent(a)?.Children.Count)
                {
                    if (XDockControl.GetParent(a).Children[index] is DockDocumentControl)
                        return true;
                }
            }
        }
        return false;
    });

    ///

    private ICommand movePanelToPreviousGroupCommand;
    public ICommand MovePanelToPreviousGroupCommand => movePanelToPreviousGroupCommand ??= new RelayCommand<Core.Panel>(i => MovePanelToPreviousGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            var index = a.GetIndex() - 2;
            if (index >= 0)
            {
                if (XDockControl.GetParent(a)?.Children[index] is DockContentControl b)
                {
                    if (a.GetType() == b.GetType())
                        return true;
                }
            }
        }
        return false;
    });

    private ICommand moveAllPanelsToPreviousGroupCommand;
    public ICommand MoveAllPanelsToPreviousGroupCommand => moveAllPanelsToPreviousGroupCommand ??= new RelayCommand<Core.Panel>(i => MoveAllPanelsToPreviousGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            if (a.Source.Count > 1)
            {
                var index = a.GetIndex() - 2;
                if (index >= 0)
                {
                    if (XDockControl.GetParent(a)?.Children[index] is DockContentControl b)
                    {
                        if (a.GetType() == b.GetType())
                            return true;
                    }
                }
            }
        }
        return false;
    });

    private ICommand movePanelToNextGroupCommand;
    public ICommand MovePanelToNextGroupCommand => movePanelToNextGroupCommand ??= new RelayCommand<Core.Panel>(i => MovePanelToNextGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            var index = a.GetIndex() + 2;
            if (index < XDockControl.GetParent(a)?.Children.Count)
            {
                if (XDockControl.GetParent(a).Children[index] is DockContentControl b)
                {
                    if (a.GetType() == b.GetType())
                        return true;
                }
            }
        }
        return false;
    });

    private ICommand moveAllPanelsToNextGroupCommand;
    public ICommand MoveAllPanelsToNextGroupCommand => moveAllPanelsToNextGroupCommand ??= new RelayCommand<Core.Panel>(i => MoveAllPanelsToNextGroup(i), i =>
    {
        var a = FindControl(i);
        if (a != null)
        {
            if (a.Source.Count > 1)
            {
                var index = a.GetIndex() + 2;
                if (index < XDockControl.GetParent(a)?.Children.Count)
                {
                    if (XDockControl.GetParent(a).Children[index] is DockContentControl b)
                    {
                        if (a.GetType() == b.GetType())
                            return true;
                    }
                }
            }
        }
        return false;
    });

    ///

    private ICommand newDocumentHorizontalGroupCommand;
    public ICommand NewDocumentHorizontalGroupCommand
        => newDocumentHorizontalGroupCommand ??= new RelayCommand<Document>(i => NewHorizontalGroup(i), i => FindControl(i)?.Source.Count > 1);

    private ICommand newDocumentVerticalGroupCommand;
    public ICommand NewDocumentVerticalGroupCommand
        => newDocumentVerticalGroupCommand ??= new RelayCommand<Document>(i => NewVerticalGroup(i), i => FindControl(i)?.Source.Count > 1);

    ///

    private ICommand newPanelHorizontalGroupCommand;
    public ICommand NewPanelHorizontalGroupCommand
        => newPanelHorizontalGroupCommand ??= new RelayCommand<Core.Panel>(i => NewHorizontalGroup(i), i => FindControl(i)?.Source.Count > 1);

    private ICommand newPanelVerticalGroupCommand;
    public ICommand NewPanelVerticalGroupCommand
        => newPanelVerticalGroupCommand ??= new RelayCommand<Core.Panel>(i => NewVerticalGroup(i), i => FindControl(i)?.Source.Count > 1);

    ///

    private ICommand minimizeCommand;
    public ICommand MinimizeCommand
        => minimizeCommand ??= new RelayCommand<Document>(i => i.IsMinimized = true, i => i?.CanMinimize == true && !i.IsMinimized);

    private ICommand minimizeAllCommand;
    public ICommand MinimizeAllCommand
        => minimizeAllCommand ??= new RelayCommand(() => Documents.ForEach(i => i.IsMinimized = true), () => true);

    private ICommand pinCommand;
    public ICommand PinCommand
        => pinCommand ??= new RelayCommand<Core.Panel>(i => Pin(i), i => i?.IsVisible == true && !Pins.ContainsKey(i));

    private ICommand pinAllCommand;
    public ICommand PinAllCommand => pinAllCommand ??= new RelayCommand(() =>
    {
        foreach (var i in Panels)
            PinCommand.Execute(i);
    });

    private ICommand restoreCommand;
    public ICommand RestoreCommand
        => restoreCommand ??= new RelayCommand<Document>(i => i.IsMinimized = false, i => i?.CanMinimize == true && i.IsMinimized);

    private ICommand restoreAllCommand;
    public ICommand RestoreAllCommand
        => restoreAllCommand ??= new RelayCommand(() => Documents.ForEach(i => i.IsMinimized = false), () => true);

    private ICommand selectCommand;
    public ICommand SelectCommand => selectCommand ??= new RelayCommand<Content>(i =>
    {
        var control = FindControl(i);
        control.SelectedIndex = control.Source.IndexOf(i);
    },
    i => i is not null);

    private ICommand showAllCommand;
    public ICommand ShowAllCommand
        => showAllCommand ??= new RelayCommand(() => Panels.ForEach(i => i.IsVisible = true), () => Panels?.Any(j => j?.CanHide == true && !j.IsVisible) == true);

    private ICommand unpinCommand;
    public ICommand UnpinCommand
        => unpinCommand ??= new RelayCommand<Core.Panel>(i => Unpin(i), i => i?.IsVisible == true && Pins.ContainsKey(i));

    private ICommand unpinAllCommand;
    public ICommand UnpinAllCommand
        => unpinAllCommand ??= new RelayCommand(() => Panels.Where(i => Pins.ContainsKey(i)).ForEach(i => UnpinCommand.Execute(i)), () => Panels?.Any(i => Pins.ContainsKey(i)) == true);

    #endregion

    #endregion
}

#endregion

/// <see cref="XDockControl"/>
#region

public static class XDockControl
{
    #region Drag

    public static readonly DependencyProperty DragProperty = DependencyProperty.RegisterAttached("Drag", typeof(bool), typeof(XDockControl), new FrameworkPropertyMetadata(false, OnDragChanged));
    public static bool GetDrag(FrameworkElement i) => (bool)i.GetValue(DragProperty);
    public static void SetDrag(FrameworkElement i, bool input) => i.SetValue(DragProperty, input);

    private static void OnDragChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, DragProperty, Drag_Loaded, Drag_Unloaded);
    }

    ///

    private static void Drag_Loaded(FrameworkElement input)
    {
        input.MouseLeftButtonDown
            += OnMouseLeftButtonDown;
        input.MouseMove
            += OnMouseMove;
        input.MouseLeftButtonUp
            += OnMouseLeftButtonUp;
    }

    private static void Drag_Unloaded(FrameworkElement input)
    {
        input.MouseLeftButtonDown
            -= OnMouseLeftButtonDown;
        input.MouseMove
            -= OnMouseMove;
        input.MouseLeftButtonUp
            -= OnMouseLeftButtonUp;
    }

    #endregion

    #region (internal) DragReference

    internal static readonly DependencyProperty DragReferenceProperty = DependencyProperty.RegisterAttached("DragReference", typeof(DockDragReference), typeof(XDockControl), new FrameworkPropertyMetadata(null));
    internal static DockDragReference GetDragReference(FrameworkElement i) => (DockDragReference)i.GetValue(DragReferenceProperty);
    internal static void SetDragReference(FrameworkElement i, DockDragReference input) => i.SetValue(DragReferenceProperty, input);

    #endregion

    #region (internal) DragRoot

    internal static readonly DependencyProperty DragRootProperty = DependencyProperty.RegisterAttached("DragRoot", typeof(DockRootControl), typeof(XDockControl), new FrameworkPropertyMetadata(null));
    internal static DockRootControl GetDragRoot(FrameworkElement i) => (DockRootControl)i.GetValue(DragRootProperty);
    internal static void SetDragRoot(FrameworkElement i, DockRootControl input) => i.SetValue(DragRootProperty, input);

    #endregion

    #region (internal) DragTarget

    internal static readonly DependencyProperty DragTargetProperty = DependencyProperty.RegisterAttached("DragTarget", typeof(Content), typeof(XDockControl), new FrameworkPropertyMetadata(null));
    internal static Content GetDragTarget(FrameworkElement i) => (Content)i.GetValue(DragTargetProperty);
    internal static void SetDragTarget(FrameworkElement i, Content input) => i.SetValue(DragTargetProperty, input);

    #endregion

    #region PreviewDrag

    public static readonly DependencyProperty PreviewDragProperty = DependencyProperty.RegisterAttached("PreviewDrag", typeof(bool), typeof(XDockControl), new FrameworkPropertyMetadata(false, OnPreviewDragChanged));
    public static bool GetPreviewDrag(FrameworkElement i) => (bool)i.GetValue(PreviewDragProperty);
    public static void SetPreviewDrag(FrameworkElement i, bool input) => i.SetValue(DragProperty, input);

    private static void OnPreviewDragChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
            element.AddHandlerAttached((bool)e.NewValue, PreviewDragProperty, PreviewDrag_Loaded, PreviewDrag_Unloaded);
    }

    ///

    private static void PreviewDrag_Loaded(FrameworkElement input)
    {
        input.PreviewMouseLeftButtonDown
            += OnMouseLeftButtonDown;
        input.PreviewMouseMove
            += OnMouseMove;
        input.PreviewMouseLeftButtonUp
            += OnMouseLeftButtonUp;
    }

    private static void PreviewDrag_Unloaded(FrameworkElement input)
    {
        input.PreviewMouseLeftButtonDown
            -= OnMouseLeftButtonDown;
        input.PreviewMouseMove
            -= OnMouseMove;
        input.PreviewMouseLeftButtonUp
            -= OnMouseLeftButtonUp;
    }

    #endregion

    #region Methods

    private static Content[] GetDragTarget(DependencyObject input)
    {
        if (input.GetParent<DockPanelControl>() is DockPanelControl control)
            return control.Source.Where<Content>(i => i.CanFloat).ToArray();

        if (input.GetParent<DockPopup>() is DockPopup popup)
            return [popup.Child.As<TransitionControl>().Content as Content];

        return null;
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            var root = element.GetParent<DockRootControl>();
            root.DockControl.LastDrag = null;

            Content[] content = null;
            if (GetDragTarget(element) is Content dragTarget)
                content = [dragTarget];

            content ??= GetDragTarget(e.OriginalSource as DependencyObject);
            if (content?.Length > 0)
            {
                SetDragRoot
                    (element, root);
                SetDragReference
                    (element, new DockDragReference(content, e.GetPosition(element)));

                element.CaptureMouse();
            }
        }
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            var reference = GetDragReference(element);
            if (reference != null)
            {
                var root = GetDragRoot(element);
                if (e.GetPosition(element).Distance(reference.Start) > root.DockControl.DragDistance)
                {
                    var content = reference.Content;
                    SetDragReference(element, null);

                    var p0 = XCursor.GetPosition();
                    var p1 = new Point(p0.X + root.DockControl.FloatingWindowDragOffset, p0.Y + root.DockControl.FloatingWindowDragOffset);

                    var window = root.DockControl.Float(content, p1);
                    root.DockControl.StartDrag(new(null, root, content, window));
                }
            }
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if (GetDragReference(element) != null)
            {
                if (element is DockPanelButton button)
                {
                    button.IsChecked = !button.IsChecked;
                    button.Command.Execute(button.CommandParameter);
                }
            }

            SetDragReference(element, null);
            SetDragRoot(element, null);

            element.ReleaseMouseCapture();
        }
    }

    ///

    private static List<T> GetAll<T>(IDockControl i) where T : Content
    {
        var result = new List<T>();

        if (i is null)
            return result;

        if (typeof(T).Equals(typeof(Document)))
        {
            if (i is DockDocumentControl a)
            {
                foreach (T j in a.Source)
                    result.Add(j);
            }
        }
        if (typeof(T).Equals(typeof(Core.Panel)))
        {
            if (i is DockPanelControl b)
            {
                foreach (T j in b.Source)
                    result.Add(j);
            }
        }
        if (i is DockGroupControl c)
        {
            foreach (var j in GetAll<T>(i, c))
                result.Add(j);
        }
        return result;
    }

    public static List<T> GetAll<T>(this IDockControl input, DockGroupControl parent = null) where T : Content
    {
        var result = new List<T>();
        if (parent is null)
        {
            foreach (var i in GetAll<T>(input))
                result.Add(i);

            return result;
        }

        foreach (var i in parent.Children)
        {
            foreach (var j in GetAll<T>(i as IDockControl))
                result.Add(j);
        }

        return result;
    }

    ///

    private static DockGroupControl GetParent(this IDockControl input, DockGroupControl parent = null)
    {
        if (parent is null)
        {
            if (ReferenceEquals(input.Root.Child, input))
                return null;

            if (input.Root.Child is DockGroupControl i)
                return input.GetParent(i);
        }

        foreach (var i in parent.Children)
        {
            if (ReferenceEquals(i, input))
                return parent;

            if (i is DockGroupControl j)
            {
                var result = input.GetParent(j);
                if (result != null)
                    return result;
            }
        }
        return null;
    }

    public static DockGroupControl GetParent(this IDockControl input) => input.GetParent(null);

    ///

    public static Point GetPosition(this IDockControl input)
    {
        var result = (input as UIElement).TranslatePoint(new Point(0, 0), input.Root);
        return new Point(result.X + (input.ActualWidth / 2), result.Y + (input.ActualHeight / 2));
    }

    ///

    public static int GetIndex(this IDockControl input) => input.GetParent()?.Children.IndexOf(input as UIElement) ?? -1;

    ///

    private static void Unsubscribe(IDockControl i)
    {
        if (i is DockDocumentControl a)
        {
            a.DockControl.Unsubscribe(a);
            a.DockControl.DocumentControls.Remove(a);
        }
        if (i is DockPanelControl b)
        {
            b.DockControl.Unsubscribe(b);
            b.DockControl.PanelControls.Remove(b);
        }
    }

    ///

    public static void Delete(this IDockControl input)
    {
        var parent = input.GetParent();
        if (parent is null)
        {
            Unsubscribe(input);
            input.Root.Child = null;
            return;
        }

        var index = input.GetIndex();
        if (index == -1)
            throw new IndexOutOfRangeException();

        Unsubscribe(input);
        parent.Children.RemoveAt(index);

        //Remove the column or row definition
        if (parent.Orientation == Orient.Horizontal)
            parent.ColumnDefinitions.RemoveAt(index);

        if (parent.Orientation == Orient.Vertical)
            parent.RowDefinitions.RemoveAt(index);

        if (parent.Children.Count > 0)
        {
            if (index >= parent.Children.Count)
                index = parent.Children.Count - 1;

            //Remove the grid splitter
            if (index < parent.Children.Count)
            {
                if (parent.Children[index] is GridSplitter)
                {
                    parent.Children.RemoveAt(index);
                    //Remove the column or row definition again
                    if (parent.Orientation == Orient.Horizontal)
                        parent.ColumnDefinitions.RemoveAt(index);

                    if (parent.Orientation == Orient.Vertical)
                        parent.RowDefinitions.RemoveAt(index);
                }
            }
        }

        //Reassign all column or row values
        for (var i = 0; i < parent.Children.Count; i++)
        {
            if (parent.Orientation == Orient.Horizontal)
                Grid.SetColumn(parent.Children[i], i);

            if (parent.Orientation == Orient.Vertical)
                Grid.SetRow(parent.Children[i], i);
        }

        //Remove the parent from it's own parent to avoid empty LayoutGroupControls
        if (parent.Children.Count == 0)
            parent.Delete();
    }

    #endregion
}

#endregion

/// <see cref="WindowsServices"/>
#region

/*
protected override void OnSourceInitialized(EventArgs e)
{
    base.OnSourceInitialized(e);
    var hwnd = new WindowInteropHelper(this).Handle;
    WindowsServices.SetWindowExTransparent(hwnd);
}
*/

public static class WindowsServices
{
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int GWL_EXSTYLE = (-20);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    public static void SetWindowExTransparent(IntPtr hwnd)
    {
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
    }
}

#endregion