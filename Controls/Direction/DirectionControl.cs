using Ion.Collect;
using Ion.Input;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public class DirectionControl : Control
{
    #region Properties

    private static readonly DependencyPropertyKey DirectionsKey = DependencyProperty.RegisterReadOnly(nameof(Directions), typeof(ListObservable<DirectionControlValue>), typeof(DirectionControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty DirectionsProperty = DirectionsKey.DependencyProperty;
    public ListObservable<DirectionControlValue> Directions
    {
        get => (ListObservable<DirectionControlValue>)GetValue(DirectionsProperty);
        private set => SetValue(DirectionsKey, value);
    }

    public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(nameof(Direction), typeof(Direction), typeof(DirectionControl), new FrameworkPropertyMetadata(Direction.Origin, OnDirectionChanged));
    public Direction Direction
    {
        get => (Direction)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    private static void OnDirectionChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => (i as DirectionControl).OnDirectionChanged(e.Convert<Direction>());

    public static readonly DependencyProperty ELabelProperty = DependencyProperty.Register(nameof(ELabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string ELabel
    {
        get => (string)GetValue(ELabelProperty);
        set => SetValue(ELabelProperty, value);
    }

    public static readonly DependencyProperty EIconProperty = DependencyProperty.Register(nameof(EIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource EIcon
    {
        get => (ImageSource)GetValue(EIconProperty);
        set => SetValue(EIconProperty, value);
    }

    public static readonly DependencyProperty NLabelProperty = DependencyProperty.Register(nameof(NLabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string NLabel
    {
        get => (string)GetValue(NLabelProperty);
        set => SetValue(NLabelProperty, value);
    }

    public static readonly DependencyProperty NIconProperty = DependencyProperty.Register(nameof(NIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource NIcon
    {
        get => (ImageSource)GetValue(NIconProperty);
        set => SetValue(NIconProperty, value);
    }

    public static readonly DependencyProperty NELabelProperty = DependencyProperty.Register(nameof(NELabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string NELabel
    {
        get => (string)GetValue(NELabelProperty);
        set => SetValue(NELabelProperty, value);
    }

    public static readonly DependencyProperty NEIconProperty = DependencyProperty.Register(nameof(NEIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource NEIcon
    {
        get => (ImageSource)GetValue(NEIconProperty);
        set => SetValue(NEIconProperty, value);
    }

    public static readonly DependencyProperty NWLabelProperty = DependencyProperty.Register(nameof(NWLabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string NWLabel
    {
        get => (string)GetValue(NWLabelProperty);
        set => SetValue(NWLabelProperty, value);
    }

    public static readonly DependencyProperty NWIconProperty = DependencyProperty.Register(nameof(NWIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource NWIcon
    {
        get => (ImageSource)GetValue(NWIconProperty);
        set => SetValue(NWIconProperty, value);
    }

    public static readonly DependencyProperty OriginLabelProperty = DependencyProperty.Register(nameof(OriginLabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string OriginLabel
    {
        get => (string)GetValue(OriginLabelProperty);
        set => SetValue(OriginLabelProperty, value);
    }

    public static readonly DependencyProperty OriginIconProperty = DependencyProperty.Register(nameof(OriginIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource OriginIcon
    {
        get => (ImageSource)GetValue(OriginIconProperty);
        set => SetValue(OriginIconProperty, value);
    }

    public static readonly DependencyProperty SLabelProperty = DependencyProperty.Register(nameof(SLabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string SLabel
    {
        get => (string)GetValue(SLabelProperty);
        set => SetValue(SLabelProperty, value);
    }

    public static readonly DependencyProperty SIconProperty = DependencyProperty.Register(nameof(SIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource SIcon
    {
        get => (ImageSource)GetValue(SIconProperty);
        set => SetValue(SIconProperty, value);
    }

    public static readonly DependencyProperty SELabelProperty = DependencyProperty.Register(nameof(SELabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string SELabel
    {
        get => (string)GetValue(SELabelProperty);
        set => SetValue(SELabelProperty, value);
    }

    public static readonly DependencyProperty SEIconProperty = DependencyProperty.Register(nameof(SEIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource SEIcon
    {
        get => (ImageSource)GetValue(SEIconProperty);
        set => SetValue(SEIconProperty, value);
    }

    public static readonly DependencyProperty SWLabelProperty = DependencyProperty.Register(nameof(SWLabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string SWLabel
    {
        get => (string)GetValue(SWLabelProperty);
        set => SetValue(SWLabelProperty, value);
    }

    public static readonly DependencyProperty SWIconProperty = DependencyProperty.Register(nameof(SWIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource SWIcon
    {
        get => (ImageSource)GetValue(SWIconProperty);
        set => SetValue(SWIconProperty, value);
    }

    public static readonly DependencyProperty WLabelProperty = DependencyProperty.Register(nameof(WLabel), typeof(string), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnLabelChanged));
    public string WLabel
    {
        get => (string)GetValue(WLabelProperty);
        set => SetValue(WLabelProperty, value);
    }

    public static readonly DependencyProperty WIconProperty = DependencyProperty.Register(nameof(WIcon), typeof(ImageSource), typeof(DirectionControl), new FrameworkPropertyMetadata(default(string), OnIconChanged));
    public ImageSource WIcon
    {
        get => (ImageSource)GetValue(WIconProperty);
        set => SetValue(WIconProperty, value);
    }

    private static void OnLabelChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => (i as DirectionControl).OnLabelChanged();

    private static void OnIconChanged(DependencyObject i, DependencyPropertyChangedEventArgs e) => (i as DirectionControl).OnIconChanged();

    #endregion

    #region DirectionControl

    /// <summary>
    /// Initializes an instance of <see cref="DirectionControl"/>.
    /// </summary>
    public DirectionControl()
    {
        Directions =
        [
            new DirectionControlValue(1, 1, 0),
            new DirectionControlValue(2, 1, 1),
            new DirectionControlValue(3, 1, 2),
            new DirectionControlValue(1, 2, 3),
            new DirectionControlValue(2, 2, 4),
            new DirectionControlValue(3, 2, 5),
            new DirectionControlValue(1, 3, 6),
            new DirectionControlValue(2, 3, 7),
            new DirectionControlValue(3, 3, 8)
        ];
    }

    #endregion

    #region Methods

    /// <summary>
    /// Shift all relative to the specified <see cref="Controls.Direction"/>.
    /// </summary>
    /// <param name="direction"></param>
    private void Shift(Direction direction)
    {
        var _direction = (int)direction;

        var shift = new int[,]
        {
            {-1, -1},
            {-1,  0},
            {-1,  1},
            { 0, -1},
            { 0,  0},
            { 0,  1},
            { 1, -1},
            { 1,  0},
            { 1,  1}
        };

        int x = shift[_direction, 1], y = shift[_direction, 0];

        if (x != 0 || y != 0)
        {
            foreach (var d in Directions)
            {
                d.Column += x;
                d.Row += y;
            }
        }
        else OnDirectionChanged(new(Direction.Origin));
    }

    /// <summary>
    /// Set <see cref="Direction"/> relative to the specified <see cref="DirectionControlValue"/>.
    /// </summary>
    /// <param name="button"></param>
    private void Set(DirectionControlValue button)
    {
        var origin = button.Direction == Direction.Origin ? button : Directions.First(x => x.Direction == Direction.Origin);
        var result = (Direction)new int[,]
        {
            { 0, 1, 2 },
            { 3, 4, 5 },
            { 6, 7, 8 }
        }
        [origin.Row - 1, origin.Column - 1];
        SetCurrentValue(DirectionProperty, result);
    }

    /// <summary>
    /// Occurs when <see cref="Direction"/> changes.
    /// </summary>
    /// <param name="value"></param>
    protected virtual void OnDirectionChanged(ValueChange<Direction> input)
    {
        var positions = new int[,]
        {
            {0, 0},
            {0, 1},
            {0, 2},
            {1, 0},
            {1, 1},
            {1, 2},
            {2, 0},
            {2, 1},
            {2, 2}
        };

        int srow = positions[(int)input.NewValue, 0],
            scolumn = positions[(int)input.NewValue, 1];

        int y = srow, x = scolumn;

        foreach (var d in Directions)
        {
            if (x < scolumn + 3)
            {
                d.Row = y;
                d.Column = x++;
                if (x == (scolumn + 3))
                {
                    x = scolumn;
                    y++;
                }
            }
        }
    }

    /// <summary>
    /// Occurs when <see cref="NWLabel"/>, <see cref="NLabel"/>, <see cref="NELabel"/>, <see cref="WLabel"/>, <see cref="OriginLabel"/>, <see cref="ELabel"/>, <see cref="SWLabel"/>, <see cref="SLabel"/>, or <see cref="SELabel"/> changes.
    /// </summary>
    protected virtual void OnLabelChanged()
    {
        foreach (var i in Directions)
        {
            switch (i.Direction)
            {
                case Direction.E:
                    i.Name = ELabel;
                    break;
                case Direction.N:
                    i.Name = NLabel;
                    break;
                case Direction.NE:
                    i.Name = NELabel;
                    break;
                case Direction.NW:
                    i.Name = NWLabel;
                    break;
                case Direction.Origin:
                    i.Name = OriginLabel;
                    break;
                case Direction.S:
                    i.Name = SLabel;
                    break;
                case Direction.SE:
                    i.Name = SELabel;
                    break;
                case Direction.SW:
                    i.Name = SWLabel;
                    break;
                case Direction.W:
                    i.Name = WLabel;
                    break;
            }
        }
    }

    /// <summary>
    /// Occurs when <see cref="NWIcon"/>, <see cref="NIcon"/>, <see cref="NEIcon"/>, <see cref="WIcon"/>, <see cref="OriginIcon"/>, <see cref="EIcon"/>, <see cref="SWIcon"/>, <see cref="SIcon"/>, or <see cref="SEIcon"/> changes.
    /// </summary>
    protected virtual void OnIconChanged()
    {
        foreach (var i in Directions)
        {
            switch (i.Direction)
            {
                case Direction.E:
                    i.Icon = EIcon;
                    break;
                case Direction.N:
                    i.Icon = NIcon;
                    break;
                case Direction.NE:
                    i.Icon = NEIcon;
                    break;
                case Direction.NW:
                    i.Icon = NWIcon;
                    break;
                case Direction.Origin:
                    i.Icon = OriginIcon;
                    break;
                case Direction.S:
                    i.Icon = SIcon;
                    break;
                case Direction.SE:
                    i.Icon = SEIcon;
                    break;
                case Direction.SW:
                    i.Icon = SWIcon;
                    break;
                case Direction.W:
                    i.Icon = WIcon;
                    break;
            }
        }
    }

    private ICommand shiftCommand;
    public ICommand ShiftCommand => shiftCommand ??= new RelayCommand<DirectionControlValue>(i =>
    {
        Shift(i.Direction);
        Set(i);
    },
    i => i is not null);

    #endregion
}