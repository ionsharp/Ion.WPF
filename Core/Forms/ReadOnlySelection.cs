using Ion.Reflect;
using System.Collections;

namespace Ion.Core;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class ReadOnlySelectionForm : ViewModel
{
    [Styles.List(Template.ListCombo, NameHide = true, Pin = Sides.LeftOrTop,
        SelectedIndexProperty = nameof(SelectedIndex),
        SelectedItemProperty = nameof(SelectedItem))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IName.Name),
        TargetItem = typeof(IName))]
    public IList ItemSource { get => Get<IList>(); set => Set(value); }

    [Hide]
    public int SelectedIndex { get => Get(0); set => Set(value); }

    [Styles.Object(CanEdit = false,
        NameHide = true)]
    public object SelectedItem { get => Get<object>(); set => Set(value); }

    public ReadOnlySelectionForm(IList itemSource) : base() => ItemSource = itemSource;
}