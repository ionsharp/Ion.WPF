using Ion.Collect;
using Ion.Controls;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Text;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Ion;

[Flags]
public enum ItemAction
{
    None = 0,
    Add = 1,
    Clear = 2,
    Clone = 4,
    Copy = 8,
    Move = 16,
    Paste = 32,
    Remove = 64,
    All = Add | Clear | Clone | Copy | Move | Paste | Remove
}

[Flags]
public enum NumberLayout
{
    None = 0,
    Slider = 1,
    Text = 2,
    All = Slider | Text
}

public static partial class Styles
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ButtonAttribute() : StyleAttribute(Ion.Template.Button)
    {
        /// <see cref="Region.Property"/>

        public ButtonColor Color { get; set; } = ButtonColor.White;

        public object Command { get; set; }

        public object CommandImage { get; set; }

        public Type CommandImageColorType { get; set; }

        public object CommandParameter { get; set; }

        public string CommandText { get; set; }

        /// <see cref="Region.Constructor"/>

        public ButtonAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class CheckAttribute() : StyleAttribute(Ion.Template.Check)
    {
        /// <see cref="Region.Property"/>

        public object CheckImage { get; set; }

        public object CheckImageColor { get; set; }

        public Type CheckImageColorType { get; set; }

        public object CheckImageToggle { get; set; }

        public string CheckText { get; set; }

        /// <see cref="Region.Constructor"/>

        public CheckAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ColorAttribute() : StyleAttribute(Ion.Template.Color)
    {
        /// <see cref="Region.Property"/>
        #region

        public bool Alpha
        { get; set; }

        public Type ColorModel
        { get; set; }

        public bool Normalize
        { get; set; }

        public int Precision
        { get; set; } = 3;

        #endregion

        public ColorAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class EnumAttribute() : StyleAttribute(Ion.Template.Enum)
    {
        /// <see cref="Region.Property"/>

        public object ItemTemplate { get; set; }

        /// <see cref="Region.Constructor"/>

        public EnumAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ImageAttribute() : StyleAttribute(Ion.Template.Image)
    {
        /// <see cref="Region.Property"/>

        public string ImageColor { get; set; }

        public double ImageHeight { get; set; } = 16;

        public double ImageWidth { get; set; } = 16;

        /// <see cref="Region.Constructor"/>

        public ImageAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ImageSlideAttribute() : StyleAttribute(Ion.Template.ImageSlide)
    {
        /// <see cref="Region.Property"/>

        public bool ImageBackBlur { get; set; } = true;

        public double ImageBackBlurRadius { get; set; } = 100;

        public double ImageBackOpacity { get; set; } = 1;

        public TimeSpan ImageInterval { get; set; } = TimeSpan.FromSeconds(3);

        public bool ImagePauseOver { get; set; } = true;

        public object ImageScaling { get; set; }

        public object ImageStretch { get; set; }

        public object ImageTransition { get; set; }

        /// <see cref="Region.Constructor"/>

        public ImageSlideAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ListAttribute() : StyleAttribute(Ion.Template.List)
    {
        /// <see cref="Region.Property"/>
        #region

        public ItemAction ItemAction { get; set; } = ItemAction.Add | ItemAction.Clear | ItemAction.Move | ItemAction.Remove;

        ///

        public Type ItemAddHandle { get; set; }

        public string ItemAddMethod { get; set; }

        ///

        public object ItemAddIconTemplate { get; set; }

        public Type ItemAddIconTemplateType { get; set; }

        public bool ItemAddIconVisible { get; set; } = true;

        ///

        public object ItemAddGestureTemplate { get; set; }

        public Type ItemAddGestureTemplateType { get; set; }

        public Type ItemAddGestureConvert { get; set; }

        public object ItemAddGestureConvertParameter { get; set; }

        public string ItemAddGestureFormat { get; set; }

        public string ItemAddGesturePath { get; set; }

        ///

        public object ItemAddHeaderTemplate { get; set; }

        public Type ItemAddHeaderTemplateType { get; set; }

        public Type ItemAddHeaderConvert { get; set; }

        public object ItemAddHeaderConvertParameter { get; set; }

        public string ItemAddHeaderFormat { get; set; }

        public string ItemAddHeaderPath { get; set; }

        ///

        public Bullet ItemBullet { get; set; } = Bullet.Square;

        public Type ItemCloneHandle { get; set; }

        public string ItemCommand { get; set; }

        ///

        public ListSortDirection ItemGroupDirection { get; set; } = ListSortDirection.Ascending;

        public string ItemGroupName { get; set; }

        public string ItemNoGroupName { get; set; } = "None";

        public string ItemNoSortName { get; set; } = "None";

        public ListSortDirection ItemSortDirection { get; set; } = ListSortDirection.Ascending;

        public string ItemSortName { get; set; }

        ///

        public object ItemTemplate { get; set; }

        public Type ItemTemplateType { get; set; }

        ///

        public string ItemToolTipHeaderPath { get; set; }

        public Type ItemToolTipHeaderSource { get; set; }

        public string ItemToolTipHeaderSourceKey { get; set; }

        public string ItemToolTipPath { get; set; }

        public Type ItemToolTipSource { get; set; }

        public string ItemToolTipSourceKey { get; set; }

        ///

        public Type[] ItemTypes { get; set; }

        public bool ItemTypeFind { get; set; }

        public string ItemValues { get; set; }

        ///

        public string ItemAddToolTipHeaderTemplate { get; set; }

        public Type ItemAddToolTipHeaderTemplateType { get; set; }

        public string ItemValueToolTipTemplate { get; set; }

        public Type ItemValueToolTipTemplateType { get; set; }

        ///

        public string SelectedIndexProperty { get; set; }

        public string SelectedItemProperty { get; set; }

        #endregion

        public ListAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ListButtonAttribute() : StyleAttribute(Ion.Template.ListButton)
    {
        /// <see cref="Region.Property"/>
        #region

        public object ButtonImage { get; set; }

        public object ButtonText { get; set; }

        #endregion

        public ListButtonAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class MatrixAttribute() : StyleAttribute(Ion.Template.Matrix)
    {
        /// <see cref="Region.Property"/>
        #region

        public bool CanInvert { get; set; }

        public bool ShowLabel { get; set; } = true;

        public bool ShowWeight { get; set; } = true;

        public double Zoom { get; set; } = 128;

        #endregion

        public MatrixAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class NumberAttribute() : StyleAttribute(Ion.Template.Number)
    {
        /// <see cref="Region.Property"/>
        #region

        public bool CanUpDown { get; set; } = true;

        public string[] Gradient { get; set; }

        public NumberLayout Layout { get; set; } = NumberLayout.All;

        public object LayoutOrientation { get; set; } //= Orient.Horizontal;

        public object Maximum { get; set; }

        public object Minimum { get; set; }

        public object Step { get; set; }

        #endregion

        /// <see cref="Region.Constructor"/>

        private NumberAttribute(object minimum, object maximum, object increment,
            Template template = Ion.Template.Number) : this()
        {
            Template = template;
            Step = increment;
            Minimum = minimum;
            Maximum = maximum;
        }

        public NumberAttribute(NumberLayout layout)
            : this(null, null, null) => Layout = layout;

        public NumberAttribute(Template template)
            : this() => Template = template;

        public NumberAttribute(byte minimum, byte maximum, byte increment)
            : this((object)minimum, maximum, increment) { }

        public NumberAttribute(decimal minimum, decimal maximum, decimal increment)
            : this((object)minimum, maximum, increment) { }

        public NumberAttribute(double minimum, double maximum, double increment, Template template = Ion.Template.Number)
            : this((object)minimum, maximum, increment, template) { }

        public NumberAttribute(short minimum, short maximum, short increment)
            : this((object)minimum, maximum, increment) { }

        public NumberAttribute(int minimum, int maximum, int increment)
            : this((object)minimum, maximum, increment) { }

        public NumberAttribute(long minimum, long maximum, long increment)
            : this((object)minimum, maximum, increment) { }

        public NumberAttribute(sbyte minimum, sbyte maximum, sbyte increment)
            : this((object)minimum, maximum, increment) { }

        public NumberAttribute(float minimum, float maximum, float increment)
            : this((object)minimum, maximum, increment) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ObjectAttribute() : StyleAttribute(Ion.Template.Object)
    {
        /// <see cref="Region.Property"/>
        #region

        /// <summary>Types of filtering supported.</summary>
        public Filter Filter { get; set; } = Filter.All;

        /// <summary>Access of members to get.</summary>
        public Access FilterAccess { get; set; } = Access.All;

        /// <summary>Types of members to get.</summary>
        public MemberInstanceType FilterType { get; set; } = MemberInstanceType.All;

        ///

        public GroupDirection GroupDirection { get; set; } = GroupDirection.Ascending;

        public MemberGroupName GroupName { get; set; } = MemberGroupName.Group;

        ///

        /// <summary>Names of members to ignore.</summary>
        public string[] IgnoreNames { get; set; }

        /// <summary>Types of members to ignore.</summary>
        public Type[] IgnoreTypes { get; set; }

        ///

        /// <summary>Access of members to include.</summary>
        public Access IncludeAccess { get; set; } = Access.Public;

        /// <summary>Types of members to include.</summary>
        public MemberInstanceType IncludeType { get; set; } = MemberInstanceType.All;

        ///

        public MemberSearchName SearchName { get; set; } = MemberSearchName.Name;

        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

        public MemberSortName SortName { get; set; } = MemberSortName.Name;

        ///

        /// <summary>Types of members that are hidden unless <see cref="StyleAttribute"/> is specified. All other member types are automatically visible unless <see cref="HideAttribute"/> is specified.</summary>
        public MemberTypes Strict { get; set; } = MemberTypes.All;

        ///

        public object TabPlacement { get; set; } //= Side.Top;

        ///

        public View MemberView { get; set; } = View.All;

        public MemberViewType MemberViewType { get; set; } = MemberViewType.All;

        #endregion

        public ObjectAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class PathAttribute() : StyleAttribute(Ion.Template.Path)
    {
        /// <see cref="Region.Property"/>

        public bool PathButtonBrowseVisible { get; set; } = true;

        public bool PathButtonClearVisible { get; set; } = true;

        public bool PathIconVisible { get; set; } = true;

        /// <see cref="Region.Constructor"/>

        public PathAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ProgressAttribute() : StyleAttribute(Ion.Template.Progress)
    {
        /// <see cref="Region.Property"/>

        public double ProgressMaximum { get; set; } = 1;

        public double ProgressMinimum { get; set; }

        /// <see cref="Region.Constructor"/>

        public ProgressAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class TextAttribute() : StyleAttribute(Ion.Template.Text)
    {
        /// <see cref="Region.Property"/>
        #region

        public bool CanClear { get; set; }

        public bool CanClearSuggestions { get; set; }

        public bool CanSelect { get; set; } = true;

        public string EnterCommand { get; set; }

        public object EnterImage { get; set; }

        public Type SuggestionHandler { get; set; }

        public string SuggestionProperty { get; set; }

        public string SuggestionCommandProperty { get; set; }

        public string TextExpression { get; set; }

        public Format TextFormat { get; set; } = Format.Default;

        public int TextLength { get; set; }

        public int TextLineMinimum { get; set; } = 1;

        public bool TextLineMultiple { get; set; }

        public Trim TextTrim { get; set; } = Trim.None;

        public Wrap TextWrap { get; set; } = Wrap.None;

        #endregion

        public TextAttribute(Template template) : this() => Template = template;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class TokenAttribute() : StyleAttribute(Ion.Template.Token)
    {
        /// <see cref="Region.Property"/>

        public char Delimiter { get; set; } = ';';

        /// <see cref="Region.Constructor"/>

        public TokenAttribute(Template i) : this() => Template = i;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class UnitAttribute() : StyleAttribute(Ion.Template.Unit)
    {
        /// <see cref="Region.Property"/>

        public string UnitFormat { get; set; } = NumberFormat.Default;

        public float UnitResolution { get; set; } = 72;

        public UnitType UnitType { get; set; } = UnitType.Pixel;

        /// <see cref="Region.Constructor"/>

        public UnitAttribute(Template i) : this() => Template = i;
    }
}