using System.Reflection;

namespace Ion.Reflect;

public record class MemberMethod(IMemberInfo parent, MemberData data) : MemberUnassignable<MethodInfo>(parent, data)
{
    protected override bool CanInvokeAll() => true;

    protected override void InvokeAll() => Try.Do(() => Info.Invoke(Parent.Value, []));

    protected override StyleAttribute FixStyle(StyleAttribute style)
    {
        var i = base.FixStyle(style);
        i.Template ??= Template.ButtonDefault;
        return i;
    }
}