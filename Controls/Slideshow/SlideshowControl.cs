using Ion.Analysis;
using Ion.Input;
using Ion.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Ion.Controls;

public class SlideshowControl : Control
{
    #region Properties

    private readonly DispatcherTimer timer;

    private readonly ItemList items = new(string.Empty, new Ion.Storage.Filter(ItemType.File));

    ///

    public static readonly DependencyProperty DefaultBackgroundProperty = DependencyProperty.Register(nameof(DefaultBackground), typeof(Brush), typeof(SlideshowControl), new FrameworkPropertyMetadata(SystemColors.ControlBrush));
    public Brush DefaultBackground
    {
        get => (Brush)GetValue(DefaultBackgroundProperty);
        set => SetValue(DefaultBackgroundProperty, value);
    }

    public static readonly DependencyProperty BackgroundBlurProperty = DependencyProperty.Register(nameof(BackgroundBlur), typeof(bool), typeof(SlideshowControl), new FrameworkPropertyMetadata(true));
    public bool BackgroundBlur
    {
        get => (bool)GetValue(BackgroundBlurProperty);
        set => SetValue(BackgroundBlurProperty, value);
    }

    public static readonly DependencyProperty BackgroundBlurRadiusProperty = DependencyProperty.Register(nameof(BackgroundBlurRadius), typeof(double), typeof(SlideshowControl), new FrameworkPropertyMetadata(100.0));
    public double BackgroundBlurRadius
    {
        get => (double)GetValue(BackgroundBlurRadiusProperty);
        set => SetValue(BackgroundBlurRadiusProperty, value);
    }

    public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register(nameof(BackgroundOpacity), typeof(double), typeof(SlideshowControl), new FrameworkPropertyMetadata(1.0));
    public double BackgroundOpacity
    {
        get => (double)GetValue(BackgroundOpacityProperty);
        set => SetValue(BackgroundOpacityProperty, value);
    }

    public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(nameof(Interval), typeof(TimeSpan), typeof(SlideshowControl), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(3), OnIntervalChanged));
    public TimeSpan Interval
    {
        get => (TimeSpan)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    private static void OnIntervalChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<SlideshowControl>().OnIntervalChanged(e.Convert<TimeSpan>());

    public static readonly DependencyProperty PauseOnMouseOverProperty = DependencyProperty.Register(nameof(PauseOnMouseOver), typeof(bool), typeof(SlideshowControl), new FrameworkPropertyMetadata(true));
    public bool PauseOnMouseOver
    {
        get => (bool)GetValue(PauseOnMouseOverProperty);
        set => SetValue(PauseOnMouseOverProperty, value);
    }

    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(nameof(Path), typeof(string), typeof(SlideshowControl), new FrameworkPropertyMetadata(string.Empty, OnPathChanged));
    public string Path
    {
        get => (string)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    private static void OnPathChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<SlideshowControl>().OnPathChanged(e.Convert<string>());

    private static readonly DependencyPropertyKey PathTypeKey = DependencyProperty.RegisterReadOnly(nameof(PathType), typeof(SlideshowControlType), typeof(SlideshowControl), new FrameworkPropertyMetadata(SlideshowControlType.None));
    public static readonly DependencyProperty PathTypeProperty = PathTypeKey.DependencyProperty;
    public SlideshowControlType PathType
    {
        get => (SlideshowControlType)GetValue(PathTypeProperty);
        private set => SetValue(PathTypeKey, value);
    }

    public static readonly DependencyProperty ScalingModeProperty = DependencyProperty.Register(nameof(ScalingMode), typeof(BitmapScalingMode), typeof(SlideshowControl), new FrameworkPropertyMetadata(BitmapScalingMode.HighQuality));
    public BitmapScalingMode ScalingMode
    {
        get => (BitmapScalingMode)GetValue(ScalingModeProperty);
        set => SetValue(ScalingModeProperty, value);
    }

    private static readonly DependencyPropertyKey SelectedImageKey = DependencyProperty.RegisterReadOnly(nameof(SelectedImage), typeof(string), typeof(SlideshowControl), new FrameworkPropertyMetadata(default(string), OnSelectedImageChanged));
    public static readonly DependencyProperty SelectedImageProperty = SelectedImageKey.DependencyProperty;
    public string SelectedImage
    {
        get => (string)GetValue(SelectedImageProperty);
        private set => SetValue(SelectedImageKey, value);
    }

    private static void OnSelectedImageChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => i.As<SlideshowControl>().OnSelectedImageChanged(e.Convert<string>());

    private static readonly DependencyPropertyKey SelectedImageSourceKey = DependencyProperty.RegisterReadOnly(nameof(SelectedImageSource), typeof(ImageSource), typeof(SlideshowControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SelectedImageSourceProperty = SelectedImageSourceKey.DependencyProperty;
    public ImageSource SelectedImageSource
    {
        get => (ImageSource)GetValue(SelectedImageSourceProperty);
        private set => SetValue(SelectedImageSourceKey, value);
    }

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(System.Windows.Media.Stretch), typeof(SlideshowControl), new FrameworkPropertyMetadata(System.Windows.Media.Stretch.UniformToFill));
    public Stretch Stretch
    {
        get => (System.Windows.Media.Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register(nameof(Transition), typeof(Transitions), typeof(SlideshowControl), new FrameworkPropertyMetadata(Transitions.LeftReplace));
    public Transitions Transition
    {
        get => (Transitions)GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    #endregion

    #region SlideshowControl

    public SlideshowControl() : base()
    {
        this.AddHandler(OnLoaded, OnUnloaded);

        timer = new DispatcherTimer { Interval = Interval };
        timer.Tick += OnTick;
    }

    #endregion

    #region Methods

    private void OnLoaded()
    {
        items.Subscribe();
        _ = items.RefreshAsync(Path);
    }

    private void OnUnloaded()
    {
        items.Unsubscribe();
        items.Clear();
    }

    private void OnTick(object sender, object e)
    {
        if (PathType != SlideshowControlType.Folder)
        {
            timer.Stop();
            return;
        }

        NextCommand.Execute(null);
    }

    ///

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (PauseOnMouseOver)
            timer.Stop();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (PauseOnMouseOver)
            timer.Start();
    }

    ///

    protected virtual void OnIntervalChanged(ValueChange<TimeSpan> input)
    {
        timer.Interval = TimeSpan.FromSeconds(Math.Clamp(input.NewValue.TotalSeconds, 3, TimeSpan.MaxValue.TotalSeconds));
    }

    protected virtual void OnPathChanged(ValueChange<string> input)
    {
        timer.Stop();
        var type = XItemPath.GetType(input.NewValue);
        switch (type)
        {
            case ItemType.File:
                PathType = SlideshowControlType.File;
                SelectedImage = input.NewValue;
                break;

            case ItemType.Folder:
                PathType = SlideshowControlType.Folder;
                _ = items.RefreshAsync(input.NewValue);
                timer.Start();
                break;

            case ItemType.Shortcut:
                if (Shortcut.TargetsFolder(input.NewValue))
                {
                    PathType = SlideshowControlType.Folder;
                    _ = items.RefreshAsync(Shortcut.TargetPath(input.NewValue));
                    timer.Start();
                }

                goto default;

            default:
                PathType = SlideshowControlType.None;
                break;
        }
    }

    protected async virtual void OnSelectedImageChanged(ValueChange<string> input)
        => SelectedImageSource = await Open(input.NewValue);

    ///

    [Obsolete]
    private async Task<BitmapSource> Open(string filePath)
    {
        BitmapSource result = await Task.Run(() =>
        {
            try
            {
                return (BitmapSource)default;
                //return new MagickImage(filePath);
            }
            catch (Exception e)
            {
                Log.Write(e);
                return null;
            }
        });
        return result;
        //return result?.ToBitmapSource();
    }

    ///

    private void Next()
    {
        if (items.Count <= 1)
            return;

        var item = items.FirstOrDefault(i => i.Path == SelectedImage) ?? items[0];
        var index = items.IndexOf(item);

        index++;
        index = index > items.Count - 1 ? 0 : index;

        SelectedImage = items[index].Path;
    }

    private void Previous()
    {
        if (items.Count <= 1)
            return;

        var item = items.FirstOrDefault(i => i.Path == SelectedImage) ?? items[0];
        var index = items.IndexOf(item);

        index--;
        index = index < 0 ? items.Count - 1 : index;

        SelectedImage = items[index].Path;
    }

    ///

    private ICommand nextCommand;
    public ICommand NextCommand => nextCommand ??= new RelayCommand(Next, () => PathType == SlideshowControlType.Folder && items.Count > 1);

    private ICommand previousCommand;
    public ICommand PreviousCommand => previousCommand ??= new RelayCommand(Previous, () => PathType == SlideshowControlType.Folder && items.Count > 1);

    #endregion
}