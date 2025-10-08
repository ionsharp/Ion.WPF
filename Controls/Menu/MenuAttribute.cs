using Ion.Collect;
using Ion.Reflect;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Ion;

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MenuAttribute(params Type[] types) : Attribute()
{
    public object[] SortBy { get; set; }

    public bool SortByClass { get; set; } = true;

    public readonly Type[] Types = types;
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
#pragma warning disable CA1813 // Avoid unsealed attributes
public class MenuItemAttribute : Attribute
#pragma warning restore CA1813 // Avoid unsealed attributes
{
    /// <see cref="Region.Property"/>

    public object Group { get; set; }

    public int GroupIndex { get; set; }

    ///

    public string Header { get; set; }

    public string HeaderPrefix { get; set; }

    public string HeaderSuffix { get; set; }

    public Type HeaderBinding { get; set; }

    public Type HeaderConverter { get; set; }

    public string HeaderPath { get; set; }

    public bool HideIfDisabled { get; set; }

    ///

    public object Icon { get; set; }

    public bool IconCollapse { get; set; }

    public Type IconConverter { get; set; }

    public string IconPath { get; set; }

    public Type IconTemplate { get; set; }

    public string IconTemplateField { get; set; }

    public Guid Id { get; set; } = Guid.NewGuid();

    public int Index { get; set; }

    public string InputGestureText { get; set; }

    public Type InputGestureTextConverter { get; set; }

    public object InputGestureTextConverterParameter { get; set; }

    public string InputGestureTextPath { get; set; }

    public Type InputGestureTextTemplate { get; set; }

    public string InputGestureTextTemplateField { get; set; }

    ///

    public object Parent { get; set; }

    ///

    public bool CanSlide { get; set; }

    public string SlidePath { get; set; }

    public double SlideMaximum { get; set; }

    public double SlideMinimum { get; set; }

    public double SlideTick { get; set; }

    public string SlideHeader { get; set; }

    public int SubGroup { get; set; }

    public bool StaysOpenOnClick { get; set; }

    ///

    public string ToolTip { get; set; }

    public string ToolTipPath { get; set; }

    public Type ToolTipTemplate { get; set; }

    public string ToolTipTemplateField { get; set; }

    public string ToolTipHeaderPath { get; set; }

    public Type ToolTipHeaderTemplate { get; set; }

    public string ToolTipHeaderTemplateField { get; set; }

    public string ToolTipHeaderIconPath { get; set; }

    public Type ToolTipHeaderIconTemplate { get; set; }

    public string ToolTipHeaderIconTemplateField { get; set; }

    /// <see cref="Region.Constructor"/>

    public MenuItemAttribute() : base() { }

    public MenuItemAttribute(object name, object icon) : this(null)
    {
        Header = $"{name}";
        Icon = icon;
    }

    public MenuItemAttribute(object parent) : this() => Parent = parent;
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class MenuCommand(object name, object icon) : MenuItemAttribute(name, icon)
{
    public ICommand Command { get; set; }
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class MenuEnum(object parent) : MenuItemAttribute(parent)
{
    /// <see cref="Region.Property"/>
    #region

    public bool IsFlag { get; set; }

    public bool IsInline { get; set; }

    public bool ItemStaysOpenOnClick { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public MemberSortName SortName { get; set; } = MemberSortName.DeclarationOrder;

    #endregion

    /// <see cref="Region.Constructor"/>

    public MenuEnum() : this(null) { }
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class MenuList(object parent) : MenuItemAttribute(parent)
{
    /// <see cref="Region.Property"/>
    #region

    public bool CanClear { get; set; }

    ///

    public ListSortDirection GroupDirection { get; set; }

    public string GroupDirectionPath { get; set; }

    public string GroupName { get; set; }

    public string GroupNamePath { get; set; }

    public string GroupSource { get; set; }

    ///

    public bool IsInline { get; set; } = true;

    ///

    public bool ItemCheckable { get; set; }

    public object ItemCheckableMode { get; set; }

    public string ItemCheckablePath { get; set; }

    ///

    public string ItemCommand { get; set; }

    public string ItemCommandParameterPath { get; set; }

    ///

    public string ItemHeader { get; set; }

    public string ItemHeaderPrefix { get; set; }

    public string ItemHeaderSuffix { get; set; }

    public Type ItemHeaderBinding { get; set; }

    public Type ItemHeaderConverter { get; set; }

    public object ItemHeaderConverterParameter { get; set; }

    public string ItemHeaderPath { get; set; }

    ///

    public bool ItemIconCollapse { get; set; }

    public object ItemIcon { get; set; }

    public Type ItemIconConverter { get; set; }

    public string ItemIconPath { get; set; }

    public Type ItemIconTemplate { get; set; }

    public string ItemIconTemplateField { get; set; }

    ///

    public Type ItemInputGestureTextConverter { get; set; }

    public object ItemInputGestureTextConverterParameter { get; set; }

    public string ItemInputGestureTextPath { get; set; }

    public string ItemInputGestureTextTemplateField { get; set; }

    public Type ItemInputGestureTextTemplate { get; set; }

    ///

    public bool ItemStaysOpenOnClick { get; set; }

    ///

    public string ItemToolTipPath { get; set; }

    public Type ItemToolTipTemplate { get; set; }

    public string ItemToolTipTemplateField { get; set; }

    public Type ItemType { get; set; }

    ///

    public ListSortDirection SortDirection { get; set; }

    public string SortDirectionPath { get; set; }

    public string SortName { get; set; }

    public string SortNamePath { get; set; }

    public string SortSource { get; set; }

    #endregion

    /// <see cref="Region.Constructor"/>

    public MenuList() : this(null) { }

    public MenuList(object parent, Type itemType, string itemHeaderPath = null, string itemIconPath = null) : this(parent) { ItemType = itemType; ItemHeaderPath = itemHeaderPath; ItemIconPath = itemIconPath; }
}

/// <inheritdoc/>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class MenuObject(object name, object icon) : MenuItemAttribute(name, icon)
{
    public bool Unpack { get; set; }
}