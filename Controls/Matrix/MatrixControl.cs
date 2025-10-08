using Ion;
using Ion.Collect;
using Ion.Input;
using Ion.Numeral;
using System;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Controls;

public class MatrixControl : Control
{
    /// <see cref="Region.Field"/>
    #region

    private readonly Handle Handle = false;

    #endregion

    /// <see cref="Region.Property"/>
    #region

    /// <see cref="Columns"/>
    #region

    private static readonly DependencyPropertyKey ColumnsKey = DependencyProperty.RegisterReadOnly(nameof(Columns), typeof(int), typeof(MatrixControl), new FrameworkPropertyMetadata(1));
    public static readonly DependencyProperty ColumnsProperty = ColumnsKey.DependencyProperty;
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        private set => SetValue(ColumnsKey, value);
    }

    #endregion

    /// <see cref="IsEditable"/>
    #region

    public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(MatrixControl), new FrameworkPropertyMetadata(true));
    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    #endregion

    /// <see cref="LabelVisibility"/>
    #region

    public static readonly DependencyProperty LabelVisibilityProperty = DependencyProperty.Register(nameof(LabelVisibility), typeof(Visibility), typeof(MatrixControl), new FrameworkPropertyMetadata(Visibility.Visible));
    public Visibility LabelVisibility
    {
        get => (Visibility)GetValue(LabelVisibilityProperty);
        set => SetValue(LabelVisibilityProperty, value);
    }

    #endregion

    /// <see cref="Matrix"/>
    #region

    public static readonly DependencyProperty MatrixProperty = DependencyProperty.Register(nameof(Matrix), typeof(Matrix<object>), typeof(MatrixControl), new FrameworkPropertyMetadata(null, OnMatrixChanged));
    public Matrix<object> Matrix
    {
        get => (Matrix<object>)GetValue(MatrixProperty);
        set => SetValue(MatrixProperty, value);
    }
    private static void OnMatrixChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<MatrixControl>().OnMatrixChanged(e);

    #endregion

    /// <see cref="MatrixValues"/>
    #region

    private static readonly DependencyPropertyKey MatrixValuesKey = DependencyProperty.RegisterReadOnly(nameof(MatrixValues), typeof(ListObservable<MatrixControlValue>), typeof(MatrixControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty MatrixValuesProperty = MatrixValuesKey.DependencyProperty;
    public ListObservable<MatrixControlValue> MatrixValues
    {
        get => (ListObservable<MatrixControlValue>)GetValue(MatrixValuesProperty);
        private set => SetValue(MatrixValuesKey, value);
    }

    #endregion

    /// <see cref="Rows"/>
    #region

    private static readonly DependencyPropertyKey RowsKey = DependencyProperty.RegisterReadOnly(nameof(Rows), typeof(int), typeof(MatrixControl), new FrameworkPropertyMetadata(1));
    public static readonly DependencyProperty RowsProperty = RowsKey.DependencyProperty;
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        private set => SetValue(RowsKey, value);
    }

    #endregion

    /// <see cref="ValueTemplate"/>
    #region

    public static readonly DependencyProperty ValueTemplateProperty = DependencyProperty.Register(nameof(ValueTemplate), typeof(DataTemplate), typeof(MatrixControl), new FrameworkPropertyMetadata(null));
    public DataTemplate ValueTemplate
    {
        get => (DataTemplate)GetValue(ValueTemplateProperty);
        set => SetValue(ValueTemplateProperty, value);
    }

    #endregion

    /// <see cref="ValueTemplateSelector"/>
    #region

    public static readonly DependencyProperty ValueTemplateSelectorProperty = DependencyProperty.Register(nameof(ValueTemplateSelector), typeof(DataTemplateSelector), typeof(MatrixControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector ValueTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ValueTemplateSelectorProperty);
        set => SetValue(ValueTemplateSelectorProperty, value);
    }

    #endregion

    /// <see cref="WeightBrush"/>
    #region

    public static readonly DependencyProperty WeightBrushProperty = DependencyProperty.Register(nameof(WeightBrush), typeof(Brush), typeof(MatrixControl), new FrameworkPropertyMetadata(Brushes.Black));
    public Brush WeightBrush
    {
        get => (Brush)GetValue(WeightBrushProperty);
        set => SetValue(WeightBrushProperty, value);
    }

    #endregion

    /// <see cref="WeightVisibility"/>
    #region

    public static readonly DependencyProperty WeightVisibilityProperty = DependencyProperty.Register(nameof(WeightVisibility), typeof(Visibility), typeof(MatrixControl), new FrameworkPropertyMetadata(Visibility.Visible));
    public Visibility WeightVisibility
    {
        get => (Visibility)GetValue(WeightVisibilityProperty);
        set => SetValue(WeightVisibilityProperty, value);
    }

    #endregion

    /// <see cref="Zoom"/>
    #region

    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(MatrixControl), new FrameworkPropertyMetadata(1d));
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    #endregion

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public MatrixControl() : base()
    {
        MatrixValues = [];
    }

    #endregion

    /// <see cref="Region.Method"/>
    #region

    private void Each(Action<int, int, MatrixControlValue> action)
    {
        int x = 0, y = 0;
        MatrixValues.ForEach(i =>
        {
            action.Invoke(y, x, i);
            if (x == Columns - 1)
            {
                x = 0;
                y++;
            }
            else x++;
        });
    }

    [NotStable]
    internal void Update() => Handle.Do(() =>
    {
        var result = new double[Rows, Columns];
        Each((x, y, i) =>
        {
            result[y, x] = i.Value;
            i.Weight = 0;
        });

        var matrix = new Matrix<double>(result.As());

        var normal = matrix.Normalize(RangeType.Weight);
        Each((x, y, i) => i.Weight = normal[y, x]);

        SetCurrentValue(MatrixProperty, matrix);
    });

    [NotStable]
    protected virtual void OnMatrixChanged(Value<IMatrix<double>> input)
    {
        Handle.DoInternal(() =>
        {
            MatrixValues.Clear();
            if (input.NewValue is IMatrix<double> newValue)
            {
                Columns = newValue.Columns; Rows = newValue.Rows;
                newValue.ForEach(i => MatrixValues.Add(new(this, i)));

                var normal = newValue.Normalize(RangeType.Weight);
                Each((x, y, i) => i.Weight = normal[y, x]);
            }
        });
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    private ICommand addColumnCommand;
    public ICommand AddColumnCommand
        => addColumnCommand ??= new RelayCommand(() =>
        {
            if (Matrix is IMatrix<object> matrix)
            {
                var result = new object[matrix.Rows, matrix.Columns + 1];
                matrix.ForEach((y, x, i) => result[y, x] = i);

                var x = new Matrix<object>(result.As());
                SetCurrentValue(MatrixProperty, x);
            }
        },
        () => Matrix != null);

    private ICommand addRowCommand;
    public ICommand AddRowCommand
        => addRowCommand ??= new RelayCommand(() =>
        {
            if (Matrix is IMatrix<object> matrix)
            {
                var result = new object[matrix.Rows + 1, matrix.Columns];
                matrix.ForEach((y, x, i) => result[y, x] = i);

                var x = new Matrix<object>(result.As());
                SetCurrentValue(MatrixProperty, x);
            }
        },
        () => Matrix != null);

    private ICommand invertCommand;
    public ICommand InvertCommand
        => invertCommand ??= new RelayCommand(() => SetCurrentValue(MatrixProperty, Matrix.NewType(i => (double)i).Invert()), () => Matrix != null);

    private ICommand removeColumnCommand;
    public ICommand RemoveColumnCommand
        => removeColumnCommand ??= new RelayCommand(() =>
        {
            if (Matrix is IMatrix matrix)
            {
                var result = new object[matrix.Rows, matrix.Columns - 1];
                Array2D.Do(matrix.Rows, matrix.Columns - 1, (y, x) => result[y, x] = matrix[y, x]);

                var x = new Matrix<object>(result.As());
                SetCurrentValue(MatrixProperty, x);
            }
        },
        () => Matrix is IMatrix i && i.Columns > 1);

    private ICommand removeRowCommand;
    public ICommand RemoveRowCommand
        => removeRowCommand ??= new RelayCommand(() =>
        {
            if (Matrix is IMatrix matrix)
            {
                var result = new object[matrix.Rows - 1, matrix.Columns];
                Array2D.Do(matrix.Rows - 1, matrix.Columns, (y, x) => result[y, x] = matrix[y, x]);

                var x = new Matrix<object>(result.As());
                SetCurrentValue(MatrixProperty, x);
            }
        },
        () => Matrix is IMatrix i && i.Rows > 1);

    #endregion
}