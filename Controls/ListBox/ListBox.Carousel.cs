using Ion.Analysis;
using Ion.Data;
using Ion.Input;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Controls;

public class CarouselVisibilityBinding : MultiBind
{
    public CarouselVisibilityBinding() : base()
    {
        Converter = CarouselBox.VisibilityConverter;
        //[0-5]
        Bindings.Add(new Binding(Paths.Dot));
        Bindings.Add(new Ancestor("ItemsSource",
            typeof(CarouselBox)));
        Bindings.Add(new Ancestor("ItemsSource.Count",
            typeof(CarouselBox)));
        Bindings.Add(new Ancestor("Columns",
            typeof(CarouselBox)));
        Bindings.Add(new Ancestor("Index",
            typeof(CarouselBox)));
        Bindings.Add(new Ancestor(typeof(CarouselBox)));
        //[*]
        Bindings.Add(new Ancestor("SortDirection",
            typeof(CarouselBox)));
        Bindings.Add(new Ancestor("SortName",
            typeof(CarouselBox)));
    }
}

public class CarouselBox : ListBox
{
    #region (IMultiValueConverter) ColumnConverter

    public static readonly IMultiValueConverter ColumnConverter = new MultiValueConverter<int>(i =>
    {
        if (i.Values?.Length == 2)
        {
            if (i.Values[0] is int columns)
            {
                if (i.Values[1] is int count)
                    return count < columns ? count : columns;
            }
        }
        return 0;
    });

    #endregion

    #region (IMultiValueConverter) VisibilityConverter

    public static readonly IMultiValueConverter VisibilityConverter = new MultiValueConverter<Visibility>(i =>
    {
        if (i.Values?.Length == 8)
        {
            if (i.Values[0] is object item)
            {
                if (i.Values[1] is IList itemsSource)
                {
                    if (i.Values[2] is int count)
                    {
                        if (i.Values[3] is int columns)
                        {
                            columns = columns > count ? count : columns;

                            if (columns == 0)
                                return Visibility.Collapsed;

                            if (i.Values[4] is int index)
                            {
                                if (i.Values[5] is CarouselBox carousel)
                                {
                                    if (carousel is ListBox listBox)
                                    {
                                        DependencyObject container = null;
                                        Try.Do(() => container = listBox.ItemContainerGenerator.ContainerFromItem(item), e => Log.Write(e));

                                        if (container is not null)
                                        {
                                            var actualIndex = listBox.ItemContainerGenerator.IndexFromContainer(container);
                                            if (actualIndex >= index)
                                            {
                                                if (actualIndex < index + columns)
                                                    return Visibility.Visible;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return Visibility.Collapsed;
    });

    #endregion

    /// <see cref="Region.Property"/>
    #region

    public int ActualColumns
        => ActualItemsSource?.Count < Columns ? ActualItemsSource.Count : Columns;

    public IList ActualItemsSource
        => ItemsSource as IList;

    private int Limit
        => ActualItemsSource?.Count - ActualColumns ?? 0;

    ///

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(CarouselBox), new FrameworkPropertyMetadata(7, null, OnColumnsCoerced));
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    private static object OnColumnsCoerced(DependencyObject i, object value) => i.As<CarouselBox>().OnColumnsCoerced((int)value);

    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(nameof(Index), typeof(int), typeof(CarouselBox), new FrameworkPropertyMetadata(0));
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public static readonly DependencyProperty LeftButtonTemplateProperty = DependencyProperty.Register(nameof(LeftButtonTemplate), typeof(DataTemplate), typeof(CarouselBox), new FrameworkPropertyMetadata(null));
    public DataTemplate LeftButtonTemplate
    {
        get => (DataTemplate)GetValue(LeftButtonTemplateProperty);
        set => SetValue(LeftButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty RightButtonTemplateProperty = DependencyProperty.Register(nameof(RightButtonTemplate), typeof(DataTemplate), typeof(CarouselBox), new FrameworkPropertyMetadata(null));
    public DataTemplate RightButtonTemplate
    {
        get => (DataTemplate)GetValue(RightButtonTemplateProperty);
        set => SetValue(RightButtonTemplateProperty, value);
    }

    public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register(nameof(SortDirection), typeof(ListSortDirection), typeof(CarouselBox), new FrameworkPropertyMetadata(ListSortDirection.Ascending));
    public ListSortDirection SortDirection
    {
        get => (ListSortDirection)GetValue(SortDirectionProperty);
        set => SetValue(SortDirectionProperty, value);
    }

    public static readonly DependencyProperty SortNameProperty = DependencyProperty.Register(nameof(SortName), typeof(object), typeof(CarouselBox), new FrameworkPropertyMetadata(null));
    public object SortName
    {
        get => GetValue(SortNameProperty);
        set => SetValue(SortNameProperty, value);
    }

    public static readonly DependencyProperty WrapProperty = DependencyProperty.Register(nameof(Wrap), typeof(bool), typeof(CarouselBox), new FrameworkPropertyMetadata(true));
    public bool Wrap
    {
        get => (bool)GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    #endregion

    /// <see cref="Region.Constructor"/>

    public CarouselBox() : base() { }

    /// <see cref="Region.Method"/>
    #region

    private void Move(SideX direction)
    {
        switch (direction)
        {
            case SideX.Left:
                Index--;
                Index = Index < 0 ? (!Wrap ? 0 : Limit) : Index;
                break;
            case SideX.Right:
                Index++;
                Index = Index > Limit ? (!Wrap ? Limit : 0) : Index;
                break;
        }
    }

    protected virtual object OnColumnsCoerced(int columns) => Math.Clamp(columns, 1, int.MaxValue);

    #endregion

    /// <see cref="ICommand"/>
    #region

    private ICommand nextCommand;
    public ICommand NextCommand => nextCommand ??= new RelayCommand(() => Move(SideX.Right), () => Wrap || Index < Limit);

    private ICommand previousCommand;
    public ICommand PreviousCommand => previousCommand ??= new RelayCommand(() => Move(SideX.Left), () => Wrap || Index > 0);

    #endregion
}