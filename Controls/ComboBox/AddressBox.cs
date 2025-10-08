using Ion.Analysis;
using Ion.Collect;
using Ion.Core;
using Ion.Input;
using Ion.Storage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public class AddressBox : ComboBox, IStorageControl
{
    /// <see cref="Region.Key"/>

    public static readonly ReferenceKey<TextBox> TextBoxKey = new();

    public static readonly ReferenceKey<ToolBar> ToolBarKey = new();

    /// <see cref="Region.Field"/>

    private readonly AddressBoxDropHandler DropHandler;

    private readonly Handle Handle = false;

    /// <see cref="Region.Property"/>
    #region

    new public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(AddressBox), new FrameworkPropertyMetadata(null, null, OnBackgroundCoerced));
    new public Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    private static object OnBackgroundCoerced(DependencyObject i, object value) => value ?? Brushes.Transparent;

    public static readonly DependencyProperty CrumbsProperty = DependencyProperty.Register(nameof(Crumbs), typeof(ListObservableOfString), typeof(AddressBox), new FrameworkPropertyMetadata(default(ListObservableOfString)));
    public ListObservableOfString Crumbs
    {
        get => (ListObservableOfString)GetValue(CrumbsProperty);
        set => SetValue(CrumbsProperty, value);
    }

    public static readonly DependencyProperty HistoryProperty = DependencyProperty.Register(nameof(History), typeof(HistoryOfString), typeof(AddressBox), new FrameworkPropertyMetadata(null));
    public HistoryOfString History
    {
        get => (HistoryOfString)GetValue(HistoryProperty);
        set => SetValue(HistoryProperty, value);
    }

    public string Path
    {
        get => XStorage.GetPath(this);
        set => XStorage.SetPath(this, value);
    }

    public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(nameof(RefreshCommand), typeof(ICommand), typeof(AddressBox), new FrameworkPropertyMetadata(null));
    public ICommand RefreshCommand
    {
        get => (ICommand)GetValue(RefreshCommandProperty);
        set => SetValue(RefreshCommandProperty, value);
    }

    #endregion

    /// <see cref="Region.Constructor"/>

    public AddressBox() : base()
    {
        DropHandler = new AddressBoxDropHandler(this);
        GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(this, DropHandler);

        SetCurrentValue(CrumbsProperty,
            new ListObservableOfString());
        SetCurrentValue(HistoryProperty,
            new HistoryOfString());

        this.AddHandler(OnLoad, OnUnload);
    }

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnLoad()
    {
        Update();
        this.GetChild(ToolBarKey).IfNotNull(i => i.PreviewMouseDown += OnPreviewMouseDown);
        this.AddPathChanged(OnPathChanged);
    }

    private void OnUnload()
    {
        this.GetChild(ToolBarKey).IfNotNull(i => i.PreviewMouseDown -= OnPreviewMouseDown);
        this.RemovePathChanged(OnPathChanged);
    }

    private void OnPathChanged(object sender, PathChangedEventArgs e)
    {
        Update();
        Handle.DoInternal(() => History?.Add(e.Path));
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource.As<DependencyObject>().GetParent<ButtonBase>() is null)
        {
            SetCurrentValue(IsEditableProperty, true);
            this.GetChild<TextBox>(TextBoxKey)?.Focus();
        }
    }

    private void Update()
    {
        Crumbs.Clear();
        Try.Do(() =>
        {
            var i = Path;
            while (!i.IsEmpty())
            {
                Crumbs.Insert(0, i);
                i = System.IO.Path.GetDirectoryName(i);
            }
        });
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        Path = SelectedItem.ToString();
    }

    /// <see cref="ICommand"/>
    #region

    private ICommand backCommand;
    public ICommand BackCommand => backCommand ??= new RelayCommand<object>(i => History.Undo(j => Handle.Do(() => Path = j)), i => History?.CanUndo() == true);

    private ICommand clearHistoryCommand;
    public ICommand ClearHistoryCommand => clearHistoryCommand ??= new RelayCommand<object>(i => History.Clear(), i => History?.Count > 0);

    private ICommand enterCommand;
    public ICommand EnterCommand => enterCommand ??= new RelayCommand(() => SetCurrentValue(IsEditableProperty, false));

    private ICommand forwardCommand;
    public ICommand ForwardCommand => forwardCommand ??= new RelayCommand<object>(i => History.Redo(j => Handle.Do(() => Path = j)), i => History?.CanRedo() == true);

    private ICommand goCommand;
    public ICommand GoCommand => goCommand ??= new RelayCommand<object>(i => SetCurrentValue(IsEditableProperty, false), i => true);

    private ICommand goUpCommand;
    public ICommand GoUpCommand => goUpCommand ??= new RelayCommand(() => Try.Do(() => Path = Folder.GetParent(Path), e => Log.Write(e)), () => Path != FilePath.Root);

    private ICommand setPathCommand;
    public ICommand SetPathCommand => setPathCommand ??= new RelayCommand<string>(i => Path = i, i => i is not null);

    #endregion
}