using System;
using System.Collections;
using System.Collections.Generic;

namespace Ion.Reflect;

public class MemberSorter : IComparer
{
    private static readonly Dictionary<string, Func<Member, string>> Names = new()
    {
        { nameof(Member.DeclarationType),
            i => i.DeclarationType?.FullName },
        { nameof(IMember.DeclaringType),
            i => i.As<IMember>().DeclaringType?.FullName },
        { nameof(Member.Group),
            i => i.Group?.ToString() },
        { nameof(IMember.MemberType),
            i => i.As<IMember>().Info?.MemberType.ToString() },
        { nameof(Member.Name),
            i => i.Style?.GetValue(j => j.Name) },
        { nameof(Member.ValueType),
            i => i.ValueType?.FullName },
    };

    /// <see cref="Region.Constructor"/>

    public MemberSorter() : base() { }

    /// <see cref="Region.Method"/>

    private static int Compare(Member i, Member j, string name)
    {
        int result = 0;
        if (name is not null)
        {
            if (Names.ContainsKey(name))
                return Names[name](i)?.CompareTo(Names[name](j)) ?? 0;
        }
        return result;
    }

    /// <see cref="IComparer"/>

    int IComparer.Compare(object a, object b)
    {
        if (a is Member i && b is Member j)
        {
            int result = 0;

            //(a) GroupIndex
            result = i.GroupIndex.CompareTo(j.GroupIndex);
            if (result != 0) return result;

            //(b) GroupName
            var groupName = j.Parent.Style?.GetValue<Styles.ObjectAttribute, MemberGroupName>(x => x.GroupName).ToString();
            result = Compare(i, j, groupName);
            if (result != 0) return result;

            //(c) Index
            result = i.Index.CompareTo(j.Index);
            if (result != 0) return result;

            //(d) SortName
            var sortName = j.Parent.Style?.GetValue<Styles.ObjectAttribute, MemberSortName>(x => x.SortName).ToString();
            result = Compare(i, j, sortName);
            if (result != 0) return result;
        }
        return 0;
    }
}