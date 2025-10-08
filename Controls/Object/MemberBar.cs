using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class MemberBar() : ToolBar(), IMemberControl
{
    /// <see cref="Region.Method"/>
    #region

    protected override DependencyObject GetContainerForItemOverride() => new MemberControlItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is MemberControlItem;

    #endregion
}