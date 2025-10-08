using Ion.Data;
using Ion.Input;
using Ion.Storage;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Controls;

public class PathBox : TextBox
{
    #region Properties

    private readonly PathBoxDropHandler DropHandler;

    ///

    public static readonly DependencyProperty BrowseButtonTemplateProperty = DependencyProperty.Register(nameof(BrowseButtonTemplate), typeof(DataTemplate), typeof(PathBox), new FrameworkPropertyMetadata(default(DataTemplate)));
    public DataTemplate BrowseButtonTemplate
    {
        get => (DataTemplate)GetValue(BrowseButtonTemplateProperty);
        set => SetValue(BrowseButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty BrowseButtonToolTipProperty = DependencyProperty.Register(nameof(BrowseButtonToolTip), typeof(string), typeof(PathBox), new FrameworkPropertyMetadata(default(string)));
    public string BrowseButtonToolTip
    {
        get => (string)GetValue(BrowseButtonToolTipProperty);
        set => SetValue(BrowseButtonToolTipProperty, value);
    }

    public static readonly DependencyProperty BrowseButtonVisibilityProperty = DependencyProperty.Register(nameof(BrowseButtonVisibility), typeof(bool), typeof(PathBox), new FrameworkPropertyMetadata(true));
    public bool BrowseButtonVisibility
    {
        get => (bool)GetValue(BrowseButtonVisibilityProperty);
        set => SetValue(BrowseButtonVisibilityProperty, value);
    }

    public static readonly DependencyProperty BrowseFileExtensionsProperty = DependencyProperty.Register(nameof(BrowseFileExtensions), typeof(FileExtensions), typeof(PathBox), new FrameworkPropertyMetadata(FileExtensions.Empty));
    [TypeConverter(typeof(ExtensionsTypeConverter))]
    public FileExtensions BrowseFileExtensions
    {
        get => (FileExtensions)GetValue(BrowseFileExtensionsProperty);
        set => SetValue(BrowseFileExtensionsProperty, value);
    }

    public static readonly DependencyProperty BrowseModeProperty = DependencyProperty.Register(nameof(BrowseMode), typeof(StorageDialogMode), typeof(PathBox), new FrameworkPropertyMetadata(StorageDialogMode.OpenFolder, null, OnBrowseModeCoerced));
    public StorageDialogMode BrowseMode
    {
        get => (StorageDialogMode)GetValue(BrowseModeProperty);
        set => SetValue(BrowseModeProperty, value);
    }

    private static object OnBrowseModeCoerced(DependencyObject i, object input) => input is StorageDialogMode mode && mode != StorageDialogMode.SaveFile ? input : throw new NotSupportedException();

    public static readonly DependencyProperty BrowseTitleProperty = DependencyProperty.Register(nameof(BrowseTitle), typeof(string), typeof(PathBox), new FrameworkPropertyMetadata(default(string)));
    public string BrowseTitle
    {
        get => (string)GetValue(BrowseTitleProperty);
        set => SetValue(BrowseTitleProperty, value);
    }

    public static readonly DependencyProperty CanBrowseProperty = DependencyProperty.Register(nameof(CanBrowse), typeof(bool), typeof(PathBox), new FrameworkPropertyMetadata(true));
    public bool CanBrowse
    {
        get => (bool)GetValue(CanBrowseProperty);
        set => SetValue(CanBrowseProperty, value);
    }

    public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(nameof(IconVisibility), typeof(Visibility), typeof(PathBox), new FrameworkPropertyMetadata(Visibility.Visible));
    public Visibility IconVisibility
    {
        get => (Visibility)GetValue(IconVisibilityProperty);
        set => SetValue(IconVisibilityProperty, value);
    }

    #endregion

    #region PathBox

    public PathBox() : base()
    {
        this.AddHandler(OnLoaded, OnUnloaded);

        DropHandler = new(this);
        GongSolutions.Wpf.DragDrop.DragDrop.SetDropHandler(this, DropHandler);
    }

    #endregion

    #region Methods

    private void OnDriveInserted(RemovableDriveEventArgs e)
        => BindingOperations.GetBindingExpressionBase(this, TextProperty)?.UpdateTarget();

    private void OnDriveRemoved(RemovableDriveEventArgs e)
        => BindingOperations.GetBindingExpressionBase(this, TextProperty)?.UpdateTarget();

    private void OnLoaded()
    {
        RemovableDrive.Inserted -= OnDriveInserted; RemovableDrive.Removed -= OnDriveRemoved;
        RemovableDrive.Inserted += OnDriveInserted; RemovableDrive.Removed += OnDriveRemoved;
    }

    private void OnUnloaded()
    {
        RemovableDrive.Inserted -= OnDriveInserted; RemovableDrive.Removed -= OnDriveRemoved;
    }

    ///

    public void Browse()
    {
        Focus();
        if (StorageDialog.Show(out string path, BrowseTitle, BrowseMode, BrowseFileExtensions.ToArray(), Text))
            SetCurrentValue(TextProperty, path);

        MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    private ICommand browseCommand;
    public ICommand BrowseCommand => browseCommand ??= new RelayCommand(Browse, () => CanBrowse);

    #endregion
}