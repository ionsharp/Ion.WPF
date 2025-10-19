using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Reflect;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ion.Core;

/// <see cref="DataViewPanel"/>
#region

/// <inheritdoc/>
[Styles.Object(MemberViewType = MemberViewType.Tab)]
public abstract record class DataViewPanel() : DataPanel()
{
    [TabView(View = Ion.View.Main)]
    protected enum Tab
    {
        [TabStyle(Image = Images.Group)]
        Group,
        [TabStyle(Image = Images.View)]
        View
    }

    /// <see cref="Region.Field"/>
    #region

    public const double DefaultItemSize = 32.0;

    public const double DefaultItemSizeIncrement = 1.0;

    public const double DefaultItemSizeMaximum = 512.0;

    public const double DefaultItemSizeMinimum = 16.0;

    ///

    public const double DefaultBlockIconSize = 36.0;

    public const double DefaultBlockIconSizeIncrement = 1.0;

    public const double DefaultBlockIconSizeMaximum = 512.0;

    public const double DefaultBlockIconSizeMinimum = 36.0;

    ///

    public const double DefaultDetailIconSize = 20.0;

    public const double DefaultDetailIconSizeIncrement = 1.0;

    public const double DefaultDetailIconSizeMaximum = 512.0;

    public const double DefaultDetailIconSizeMinimum = 20.0;

    ///

    public const double DefaultTileIconSize = 48.0;

    public const double DefaultTileIconSizeIncrement = 1.0;

    public const double DefaultTileIconSizeMaximum = 512.0;

    public const double DefaultTileIconSizeMinimum = 48.0;

    public const double DefaultTileWidth = 300.0;

    ///

    public const DataViews DefaultView = DataViews.Tile;

    #region SlideVisibilityConverter

    public static readonly IMultiValueConverter SlideVisibilityConverter = new MultiValueConverter<Visibility>(3, i =>
    {
        if (i.Values[0] is object item)
        {
            if (i.Values[1] is ListView listView)
            {
                if (i.Values[2] is DataViewControl control)
                {
                    var model
                        = control.Model as DataViewPanel;
                    var columns
                        = listView.View is PageView
                        ? 1
                        : model.ViewSlideColumns > model.Items.Count ? model.Items.Count : model.ViewSlideColumns;

                    if (columns > 0)
                    {
                        if (listView.GetContainer(item) is DependencyObject container)
                        {
                            var slideView = listView.View as SlideView;

                            var actualIndex = listView.ItemContainerGenerator.IndexFromContainer(container);
                            if (actualIndex >= slideView.Index)
                            {
                                if (actualIndex < slideView.Index + columns)
                                    return Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }
        return Visibility.Collapsed;
    });

    #endregion

    #endregion

    /// <see cref="Region.Property.Public.Virtual"/>
    #region

    public virtual bool CanAddRows => false;

    public virtual bool CanDeleteRows => false;

    public virtual bool CanReorderColumns => true;

    public virtual bool CanResizeColumns => true;

    public virtual bool CanResizeRows => false;

    public virtual bool CanSortColumns => false;

    public virtual string ItemNameLabel => "Name";

    public virtual string ItemDescriptionLabel => "Description";

    public virtual string ItemDetail1Label => "Detail 1";

    public virtual string ItemDetail2Label => "Detail 2";

    #endregion

    /// <see cref="View.Footer"/>
    #region

    [Group(GroupDefault.View)]
    [Description("The type of view.")]
    [Style(Float = Sides.RightOrBottom,
        Index = int.MaxValue,
        View = Ion.View.Footer)]
    public virtual DataViews View { get => Get(DefaultView); set => Set(value); }

    [Group(GroupDefault.View)]
    [Name("Size")]
    [Styles.NumberAttribute(DefaultBlockIconSizeMinimum, DefaultBlockIconSizeMaximum, DefaultBlockIconSizeIncrement,
        Float = Sides.RightOrBottom,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(View), DataViews.Block)]
    public virtual double ViewBlockIconSize { get => Get(DefaultBlockIconSize); set => Set(value); }

    [Group(GroupDefault.View)]
    [Name("Size")]
    [Styles.NumberAttribute(DefaultDetailIconSizeMinimum, DefaultDetailIconSizeMaximum, DefaultDetailIconSizeIncrement,
        Float = Sides.RightOrBottom,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(View), DataViews.Detail)]
    public virtual double ViewDetailIconSize { get => Get(DefaultDetailIconSize); set => Set(value); }

    [Group(GroupDefault.View)]
    [Name("Columns")]
    [Styles.NumberAttribute(1, 16, 1,
        Float = Sides.RightOrBottom,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(View), DataViews.Slide)]
    public virtual int ViewSlideColumns { get => Get(7); set => Set(value); }

    [Group(GroupDefault.View)]
    [Name("Size")]
    [Styles.NumberAttribute(DefaultItemSizeMinimum, DefaultItemSizeMaximum, DefaultItemSizeIncrement,
        Float = Sides.RightOrBottom,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(View), DataViews.Thumb)]
    public virtual double ViewThumbSize { get => Get(DefaultItemSize); set => Set(value); }

    [Group(GroupDefault.View)]
    [Name("Size")]
    [Styles.NumberAttribute(DefaultTileIconSizeMinimum, DefaultTileIconSizeMaximum, DefaultTileIconSizeIncrement,
        Float = Sides.RightOrBottom,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(View), DataViews.Tile)]
    public virtual double ViewTileIconSize { get => Get(DefaultTileIconSize); set => Set(value); }

    [Group(GroupDefault.View)]
    [Name("Width")]
    [Styles.NumberAttribute(DefaultItemSizeMinimum, DefaultItemSizeMaximum, DefaultItemSizeIncrement,
        Float = Sides.RightOrBottom,
        Index = 1,
        View = Ion.View.Footer)]
    [VisibilityTrigger(nameof(View), DataViews.Tile)]
    public virtual double ViewTileWidth { get => Get(DefaultTileWidth); set => Set(value); }

    #endregion

    /// <see cref="View.Option"/>
    #region

    /// <see cref="Tab.View"/>
    #region

    /// <see cref="GroupDefault.Option"/>
    #region

    [Group(nameof(GroupDefault.Option))]
    [Name("Page wrap")]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [VisibilityTrigger(nameof(View), DataViews.Page)]
    public virtual bool ViewPageWrap { get => Get(true); set => Set(value); }

    [Group(nameof(GroupDefault.Option))]
    [Name("Slide orientation")]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [VisibilityTrigger(nameof(View), DataViews.Slide)]
    public virtual Orient ViewSlideOrientation { get => Get(Orient.Horizontal); set => Set(value); }

    [Group(nameof(GroupDefault.Option))]
    [Name("Slide wrap")]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [VisibilityTrigger(nameof(View), DataViews.Slide)]
    public virtual bool ViewSlideWrap { get => Get(true); set => Set(value); }

    [Group(nameof(GroupDefault.Option))]
    [Name("Thumb orientation")]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [VisibilityTrigger(nameof(View), DataViews.Thumb)]
    public virtual Orient ViewThumbOrientation { get => Get(Orient.Horizontal); set => Set(value); }

    [Group(nameof(GroupDefault.Option))]
    [Name("Tile orientation")]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [VisibilityTrigger(nameof(View), DataViews.Tile)]
    public virtual Orient ViewTileOrientation { get => Get(Orient.Horizontal); set => Set(value); }

    #endregion

    /// <see cref="GroupDefault.Show"/>
    #region

    [Group(GroupDefault.Show)]
    [Style(View = Ion.View.Option)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(ItemDescriptionLabel))]
    public bool ShowDescription { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Show)]
    [Style(View = Ion.View.Option)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(ItemDetail1Label))]
    public bool ShowDetail1 { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Show)]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(ItemDetail2Label))]
    public bool ShowDetail2 { get => Get(true); set => Set(value); }

    [Group(GroupDefault.Show)]
    [Style(Tab = Tab.View,
        View = Ion.View.Option)]
    [StyleTrigger(nameof(StyleAttribute.Name), nameof(ItemNameLabel))]
    public bool ShowName { get => Get(true); set => Set(value); }

    #endregion

    #endregion

    #endregion

    /// <see cref="ICommand"/>
    #region

    public ICommand SlideBackwardCommand => Commands[nameof(SlideBackwardCommand)] ??= new RelayCommand(() =>
    {
        var view = Selector.As<ListView>().View as SlideView;

        var index = view.Index;

        var columns = view is PageView ? 1 : ViewSlideColumns;
        var limit = Items.Count - columns;

        index--;
        index = index < 0 ? (!(view is PageView ? ViewPageWrap : ViewSlideWrap) ? 0 : limit) : index;

        view.Index = index;
    },
    () => Selector is ListView listView && listView.View is SlideView && Items is not null);

    public ICommand SlideForewardCommand => Commands[nameof(SlideForewardCommand)] ??= new RelayCommand(() =>
    {
        var view = Selector.As<ListView>().View as SlideView;

        var index = view.Index;

        var columns = view is PageView ? 1 : ViewSlideColumns;
        var limit = Items.Count - columns;

        index++;
        index = index > limit ? (!(view is PageView ? ViewPageWrap : ViewSlideWrap) ? limit : 0) : index;

        Selector.As<ListView>().View.As<SlideView>().Index = index;
    },
    () => Selector is ListView listView && listView.View is SlideView && Items is not null);

    #endregion
}

#endregion

/// <see cref="DataViewPanel{T}"/>
#region

/// <inheritdoc/>
public abstract record class DataViewPanel<T>() : DataViewPanel()
{
    [Hide, NonSerializable]
    public override Type ItemType => typeof(T);
}

#endregion