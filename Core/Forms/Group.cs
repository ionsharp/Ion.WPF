using Ion.Collect;
using System.Collections;
using System.Reflection;

namespace Ion.Core;

[Styles.Object(Filter = Filter.None, GroupName = Reflect.MemberGroupName.None, Strict = MemberTypes.All)]
public record class GroupForm : Form
{
    [Styles.List(Template.ListCombo,
        NameHide = true,
        SelectedIndexProperty = nameof(SelectedGroupIndex),
        SelectedItemProperty = nameof(SelectedGroup))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItemGroup.Name),
        TargetItem = typeof(IItemGroup))]
    public virtual IList Groups { get => Get<IList>(null, false); set => Set(value, false); }

    [Styles.List(Template.ListCombo,
        NameHide = true,
        SelectedIndexProperty = nameof(SelectedItemIndex),
        SelectedItemProperty = nameof(SelectedItem))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItem.Name),
        TargetItem = typeof(IItem))]
    public virtual IList SelectedGroup { get => Get<IList>(null, false); set => Set(value, false); }

    public int SelectedGroupIndex { get => Get(0); set => Set(value); }

    public object SelectedItem { get => Get<object>(null, false); set => Set(value, false); }

    public int SelectedItemIndex { get => Get(0); set => Set(value); }

    private GroupForm() : this(null) { }

    public GroupForm(IListWritable groups, int selectedGroupIndex = 0, int selectedItemIndex = 0) : base()
    {
        Groups = groups as IList; SelectedGroupIndex = selectedGroupIndex; SelectedItemIndex = selectedItemIndex;
    }
}

[Styles.Object(Filter = Filter.None, GroupName = Reflect.MemberGroupName.None, Strict = MemberTypes.All)]
public record class GradientGroupForm(IListWritable groups, int selectedGroupIndex = 0, int selectedItemIndex = 0) : GroupForm(groups, selectedGroupIndex, selectedItemIndex)
{
    [Styles.List(Template.ListCombo,
        NameHide = true,
        SelectedIndexProperty = nameof(SelectedGroupIndex),
        SelectedItemProperty = nameof(SelectedGroup))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItemGroup.Name),
        TargetItem = typeof(IItemGroup))]
    public override IList Groups { get => base.Groups; set => base.Groups = value; }

    [Styles.List(Template.ListCombo,
        NameHide = true,
        HideNull = true,
        SelectedIndexProperty = nameof(SelectedItemIndex),
        SelectedItemProperty = nameof(SelectedItem))]
    [Styles.Text(CanEdit = false,
        ValuePath = nameof(IItem.Name),
        TargetItem = typeof(IItem))]
    public override IList SelectedGroup { get => base.SelectedGroup; set => base.SelectedGroup = value; }

    private GradientGroupForm() : this(null) { }
}