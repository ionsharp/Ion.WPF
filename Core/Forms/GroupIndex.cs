using Ion.Collect;
using Ion.Reflect;

namespace Ion.Core;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class GroupIndexForm : Model
{
    [Hide]
    public object Group { get => Get<object>(); set => Set(value); }

    [Hide]
    public int GroupIndex { get => Get(0); set => Set(value); }

    [Name("Group")]
    [Styles.List(Template.ListCombo,
        Placeholder = "Select a group",
        SelectedIndexProperty = nameof(GroupIndex),
        SelectedItemProperty = nameof(Group))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItemGroup.Name),
        TargetItem = typeof(IItemGroup))]
    public IListWritable Groups { get; private set; }

    public GroupIndexForm(IListWritable groups, int selectedIndex) : base()
    {
        Groups = groups; GroupIndex = selectedIndex;
    }
}