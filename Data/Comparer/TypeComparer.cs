using Ion.Reflect;
using System;
using System.Collections;
using System.ComponentModel;

namespace Ion.Data;

public class TypeComparer : IComparer
{
    public static readonly TypeComparer Default = new();

    public TypeComparer() : base() { }

    int IComparer.Compare(object a, object b)
    {
        if (a is Type i)
        {
            if (b is Type j)
            {
                int result;

                var aCategory = $"{i.GetAttribute<GroupAttribute>()?.Name}" ?? "General";
                var bCategory = $"{j.GetAttribute<GroupAttribute>()?.Name}" ?? "General";

                result = aCategory?.ToString()?.CompareTo(bCategory) ?? 0;
                if (result != 0) return result;

                var aName = i.GetAttribute<NameAttribute>()?.Name ?? i.Name;
                var bName = j.GetAttribute<NameAttribute>()?.Name ?? j.Name;

                result = aName?.CompareTo(bName) ?? 0;
                if (result != 0) return result;
            }
        }
        return 0;
    }
}