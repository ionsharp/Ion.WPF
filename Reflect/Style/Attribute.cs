using Ion.Core;
using Ion.Text;
using System;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Ion;

/// <summary>Indicates how to represent a <see cref="Type"/> in a user interface.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
#pragma warning disable CA1813 // Avoid unsealed attributes
public class StyleAttribute(object Template) : Attribute()
#pragma warning restore CA1813 // Avoid unsealed attributes
{
    /// <see cref="Region.Property"/>

    #region Align

    public object AlignX { get; set; } //= AlignX.Stretch;

    public object AlignY { get; set; } //= AlignY.Stretch;

    #endregion

    #region CanEdit

    public bool CanEdit { get; set; } = true;

    #endregion

    #region Caption

    public object Caption { get; set; }

    public Format CaptionFormat { get; set; } = Format.MarkUp;

    public bool CaptionFromDescription { get; set; }

    public string CaptionIcon { get; set; }

    public object CaptionSide { get; set; } //= Sides.RightOrBottom;

    public string CaptionTemplate { get; set; }

    public Type CaptionTemplateType { get; set; }

    #endregion

    #region Description

    public object Description { get; set; }

    public Format DescriptionFormat { get; set; } = Format.MarkUp;

    public bool DescriptionFromValue { get; set; }

    public bool DescriptionLocalize { get; set; } = true;

    public Type DescriptionTemplateType { get; set; }

    public string DescriptionTemplate { get; set; }

    #endregion

    #region Group

    public object Group { get; set; }

    public int GroupIndex { get; set; }

    #endregion

    #region Height

    public double MaximumHeight { get; set; } = double.NaN;

    public double MinimumHeight { get; set; } = double.NaN;

    public double Height { get; set; } = double.NaN;

    #endregion

    #region Image

    public object Image { get; set; }

    #endregion

    #region Index

    public object Index { get; set; }

    #endregion

    #region Is

    public bool IsEditingText { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsLockable { get; set; }

    #endregion

    #region LeftText

    public string LeftText { get; set; }

    public Format LeftTextFormat { get; set; } = Format.Default;

    #endregion

    #region Name

    public string Name { get; set; }

    public bool NameHide { get; set; }

    public object NameIcon { get; set; }

    public bool NameLocalize { get; set; } = true;

    public object NamePin { get; set; } //= Sides.LeftOrTop;

    public bool NameFromValue { get; set; }

    public string NameTemplate { get; set; }

    public Type NameTemplateType { get; set; }

    #endregion

    #region Null

    public bool HideNull { get; set; }

    public string NullTemplate { get; set; }

    public Type NullTemplateType { get; set; }

    public string NullText { get; set; }

    #endregion

    #region Options

    public Option Options { get; set; } = Option.None;

    #endregion

    #region Placeholder

    public object Placeholder { get; set; }

    public Format PlaceholderFormat { get; set; } = Format.MarkUp;

    public bool PlaceholderLocalize { get; set; } = true;

    public string PlaceholderTemplate { get; set; }

    public Type PlaceholderTemplateType { get; set; }

    #endregion

    #region Replace

    public string ReplaceCommand { get; set; }

    public string ReplaceItems { get; set; }

    public Type[] ReplaceTypes { get; set; }

    #endregion

    #region RightText

    public string RightText { get; set; }

    public Format RightTextFormat { get; set; } = Format.Default;

    #endregion

    #region Route

    public object RouteIcon { get; set; }

    public string RouteName { get; set; }

    #endregion

    #region Tab

    public object Tab { get; set; }

    #endregion

    #region Target

    /// <summary>Gets or sets the <see cref="Type"/> of members the style may apply to when declared on a <see langword="class"/> or <see langword="struct"/>.</summary>
    public Type TargetMember { get; set; }

    /// <summary>Gets or sets the <see cref="Type"/> of items the style may apply to when the declaring member inherits <see cref="INotifyCollectionChanged"/>.</summary>
    public Type TargetItem { get; set; }

    #endregion

    #region Template

    public object Template { get; set; } = Template;

    public Type TemplateModel { get; set; }

    public Type TemplateType { get; set; }

    #endregion

    #region ToolTip

    public Type ToolTipSource { get; set; }

    public string ToolTipSourceKey { get; set; }

    public string ToolTipPath { get; set; }

    #endregion

    #region Update

    public bool Update { get; set; }

    public int UpdateInterval { get; set; } = 1; //Seconds

    public string UpdateMethod { get; set; }

    #endregion

    #region Validate

    public Type[] Validate { get; set; }

    public object[] ValidateParameter { get; set; }

    public string ValidateTemplate { get; set; }

    public Type ValidateTemplateType { get; set; }

    #endregion

    #region Value

    public Type ValueConvert { get; set; }

    public object ValueConvertParameter { get; set; }

    public string ValueFormat { get; set; }

    public BindMode ValueMode { get; set; } = BindMode.TwoWay;

    public string ValuePath { get; set; }

    public BindTrigger ValueTrigger { get; set; } = BindTrigger.Default;

    #endregion

    #region View

    public View View { get; set; } = View.Main;

    public Sides Float { get; set; } = Sides.None;

    public Sides Pin { get; set; } = Sides.None;

    public Orient Orientation { get; set; } = Orient.Horizontal;

    #endregion

    #region Width

    public double MaximumWidth { get; set; } = double.NaN;

    public double MinimumWidth { get; set; } = double.NaN;

    public double Width { get; set; } = double.NaN;

    #endregion

    /// <see cref="Region.Constructor"/>

    public StyleAttribute()
        : this((object)null) { }

    public StyleAttribute(Images image)
        : this((object)null) => Image = image;

    public StyleAttribute(string name)
        : this((object)null) => Name = name;

    public StyleAttribute(Template template)
        : this((object)template) { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class StyleInheritAttribute() : TriggerAttribute() { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class StyleOverrideAttribute(string propertyName, object value) : TriggerAttribute()
{
    public string PropertyName { get; set; } = propertyName;

    public object Value { get; set; } = value;

    [NotComplete, Obsolete]
    public StyleOverrideAttribute(Expression<Func<StyleAttribute, object>> input, object value) : this(default(string), value)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Body is MemberExpression body)
            PropertyName = body.Member.Name;

        var result = input.Body.ToString();
        result = result[(result.IndexOf('.') + 1)..].TrimEnd([')']);
        PropertyName = result;
    }
}