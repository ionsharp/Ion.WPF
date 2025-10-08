using System.Collections;

namespace Ion.Reflection;

public class MemberItem : Instance
{
    public IList List => Parent.Value as IList;

    public MemberItem(Instance parent, object item) : base() { Parent = parent; Value = item; }

    /*
    protected override StyleAttribute OnGetStyle(TypeData data)
    {
        return

            //a) Parent declaration
            Parent.Attributes?
                .FirstOrDefault<StyleAttribute>(i => i.TargetType is null && i.ItemTargetType is not null && (ValueType.Implements(i.ItemTargetType) || ValueType.Inherits(i.ItemTargetType)))

            //b) Type declaration
            ?? ValueType?.GetAttributes<StyleAttribute>()
                .FirstOrDefault(i => i.TargetType is null && i.ItemTargetType is null)

            //c) Default
            ?? new Styles.ObjectAttribute();
    }
    */

    protected override StyleAttribute FixStyle(StyleAttribute oldAttribute) => oldAttribute;
}