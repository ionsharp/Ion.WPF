using Ion.Collect;
using Ion.Reflect;
using System;

namespace Ion.Core;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class GroupValueForm<T> : Model<object>, IGeneric
{
    public object Group { get => Get<object>(); set => Set(value); }

    public int GroupIndex { get => Get(-1); set => Set(value); }

    [Styles.List(Template.ListCombo,
        Pin = Sides.LeftOrTop,
        Placeholder = "Select a group",
        SelectedIndexProperty = nameof(GroupIndex),
        SelectedItemProperty = nameof(Group))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItemGroup.Name),
        TargetItem = typeof(IItemGroup))]
    [Name("Group")]
    public IListWritable Groups { get; private set; }

    [StyleInherit]
    [StyleOverride(nameof(StyleAttribute.NameHide), true)]
    [StyleOverride(nameof(StyleAttribute.NameFromValue), true)]
    public override object Value { get => base.Value; set => base.Value = value; }

    public GroupValueForm(IListWritable groups, object value, int groupIndex) : base(value)
    {
        Groups = groups; GroupIndex = groupIndex;
    }

    Type IGeneric.GetGenericType() => typeof(T);
}