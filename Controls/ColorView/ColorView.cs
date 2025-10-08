using Ion.Core;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public partial class ColorView : Control
{
    public static readonly ResourceKey ComponentDescriptionTemplate = new();

    public static readonly ResourceKey ComponentToolTipTemplate = new();

    public static readonly ResourceKey ComponentToolTipHeaderTemplate = new();

    public static readonly ResourceKey ModelToolTipTemplate = new();

    public static readonly ResourceKey ModelToolTipHeaderTemplate = new();

    public static readonly ResourceKey HeaderTemplate = new();

    public static readonly ResourceKey IconTemplate = new();

    public static readonly ResourceKey ToolTipHeaderTemplate = new();

    public static readonly ResourceKey ToolTipTemplate = new();

    /// <see cref="Region.Property"/>
    #region 

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel), typeof(ColorFileDockViewModel), typeof(ColorView), new FrameworkPropertyMetadata(null, OnViewModelChanged));
    public ColorFileDockViewModel ViewModel
    {
        get => (ColorFileDockViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    private static void OnViewModelChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<ColorView>().OnViewModelChanged(e);

    #endregion

    /// <see cref="Region.Constructor"/>

    public ColorView() : base()
    {
        var viewModel = new ColorFileDockViewModel()
        {
            DataFileExtension
                = "data",
            DataFileName
                = nameof(ColorView),
            DataFolderPath
                = @$"{Appp.Model.DataFolderPathShared}\{nameof(Control)}",
        };

        viewModel.Load();
        viewModel.Subscribe();

        SetCurrentValue(ViewModelProperty, viewModel);
    }

    /// <see cref="Region.Method"/>

    protected virtual void OnViewModelChanged(Value<ColorFileDockViewModel> input) { }
}