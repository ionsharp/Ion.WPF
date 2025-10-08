using Ion.Core;
using Ion.Reflect;

namespace Ion;

[Styles.Object(GroupName = MemberGroupName.None, Filter = Filter.None)]
public record class NameForm : Form
{
    public string Name { get => Get(Namable.DefaultName); set => Set(value); }

    public NameForm() : this(Namable.DefaultName) { }

    public NameForm(string name) : base() => Name = name;
}