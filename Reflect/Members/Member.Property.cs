using System;
using System.Reflection;

namespace Ion.Reflect;

public record class MemberProperty : MemberAssignable<PropertyInfo>
{
    public MemberProperty(MemberBase parent, MemberData data) : base(parent, data) { }

    public override bool CanSet => Info.IsSettable();

    public override Type DeclarationType => Info.PropertyType;

    protected override object GetValue(object obj) => Info.GetValue(obj);

    protected override void SetValue(object obj, object value) => Info.SetValue(obj, value, null);
}