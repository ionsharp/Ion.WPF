using Ion.Collect;
using System;
using System.Collections;

namespace Ion.Core;

[Styles.Object(Filter = Filter.None)]
[Serializable]
public record class GroupItemForm : Model
{
    public IList Groups { get => Get<IList>(null, false); set => Set(value, false); }

    public IList SelectedGroup => Groups != null && SelectedGroupIndex >= 0 && SelectedGroupIndex < Groups.Count ? (IList)Groups[SelectedGroupIndex] : null;

    public int SelectedGroupIndex { get => Get(0); set => Set(value); }

    public int SelectedIndex { get => Get(0); set => Set(value); }

    [Style(NameHide = true)]
    public object SelectedItem => SelectedGroup != null && SelectedIndex >= 0 && SelectedIndex < SelectedGroup.Count ? SelectedGroup[SelectedIndex] : null;

    private GroupItemForm() : base() { }

    public GroupItemForm(IListWritable groups, int selectedGroupIndex, int selectedIndex) : base()
    {
        Groups = groups as IList; SelectedGroupIndex = selectedGroupIndex; SelectedIndex = selectedIndex;
    }

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Groups):
            case nameof(SelectedGroupIndex):
                Reset(() => SelectedGroup);
                break;

            case nameof(SelectedIndex):
                Reset(() => SelectedItem);
                break;
        }
    }
}