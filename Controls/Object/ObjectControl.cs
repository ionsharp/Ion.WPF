using Ion.Reflect;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

public class ObjectControl() : Control(), IObjectControl<Control>
{
    /// <see cref="Region.Property"/>
    #region 

    /// <see cref="DescriptionLength"/>
    #region

    public static readonly DependencyProperty DescriptionLengthProperty = DependencyProperty.Register(nameof(DescriptionLength), typeof(GridLength), typeof(ObjectControl), new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Auto)));
    public GridLength DescriptionLength
    {
        get => (GridLength)GetValue(DescriptionLengthProperty);
        set => SetValue(DescriptionLengthProperty, value);
    }

    #endregion

    /// <see cref="DescriptionResize"/>
    #region

    public static readonly DependencyProperty DescriptionResizeProperty = DependencyProperty.Register(nameof(DescriptionResize), typeof(bool), typeof(ObjectControl), new FrameworkPropertyMetadata(false));
    public bool DescriptionResize
    {
        get => (bool)GetValue(DescriptionResizeProperty);
        set => SetValue(DescriptionResizeProperty, value);
    }

    #endregion

    /// <see cref="DescriptionTemplate"/>
    #region

    public static readonly DependencyProperty DescriptionTemplateProperty = DependencyProperty.Register(nameof(DescriptionTemplate), typeof(DataTemplate), typeof(ObjectControl), new FrameworkPropertyMetadata(null));
    public DataTemplate DescriptionTemplate
    {
        get => (DataTemplate)GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    #endregion

    /// <see cref="DescriptionTemplateSelector"/>
    #region

    public static readonly DependencyProperty DescriptionTemplateSelectorProperty = DependencyProperty.Register(nameof(DescriptionTemplateSelector), typeof(DataTemplateSelector), typeof(ObjectControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector DescriptionTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DescriptionTemplateSelectorProperty);
        set => SetValue(DescriptionTemplateSelectorProperty, value);
    }

    #endregion

    /// <see cref="DescriptionVisibility"/>
    #region

    public static readonly DependencyProperty DescriptionVisibilityProperty = DependencyProperty.Register(nameof(DescriptionVisibility), typeof(Visibility), typeof(ObjectControl), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public Visibility DescriptionVisibility
    {
        get => (Visibility)GetValue(DescriptionVisibilityProperty);
        set => SetValue(DescriptionVisibilityProperty, value);
    }

    #endregion

    /// <see cref="Orientation"/>
    #region

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ObjectControl), new FrameworkPropertyMetadata(Orientation.Vertical));
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion

    /// <see cref="SelectedMember"/>
    #region

    private static readonly DependencyPropertyKey SelectedMemberKey = DependencyProperty.RegisterReadOnly(nameof(SelectedMember), typeof(Member), typeof(ObjectControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty SelectedMemberProperty = SelectedMemberKey.DependencyProperty;
    public Member SelectedMember
    {
        get => (Member)GetValue(SelectedMemberProperty);
        private set => SetValue(SelectedMemberKey, value);
    }

    #endregion

    #endregion

    /// <see cref="Region.Method"/>
    #region 

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        if (e.OriginalSource is DependencyObject i)
        {
            var x = i.GetParent<MemberControlItem>()?.Content;
            if (x is Member y && y.Parent is MemberBase z)
            {
                if (z.StyleModel is TemplateModelObject m)
                {
                    foreach (var n in m.Members)
                    {
                        if (n is Member o)
                            o.IsSelected = ReferenceEquals(x, o);
                    }
                    SelectedMember = y;
                }
            }
        }
    }

    #endregion
}