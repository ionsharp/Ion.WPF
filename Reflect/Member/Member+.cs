using System.Reflection;

namespace Ion.Reflect;

/// <inheritdoc/>
public abstract record class Member<T>(IMemberInfo parent, MemberData data) : Member(parent, data) where T : MemberInfo
{
    new public T Info => (T)base.Info;
}