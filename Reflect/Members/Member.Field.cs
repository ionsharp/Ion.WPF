using System;
using System.Reflection;

namespace Ion.Reflect;

public record class MemberField : MemberAssignable<FieldInfo>, IField
{
    public MemberField(MemberBase parent, MemberData data) : base(parent, data) { }

    public override bool CanSet => Info.IsSettable();

    public override Type DeclarationType => Info.FieldType;

    protected override object GetValue(object obj) => Info.GetValue(obj);

    protected override void SetValue(object obj, object value) => Info.SetValue(obj, value);
}