using System.Reflection;

namespace Ion.Reflect;

public record class MemberEvent(IMemberInfo parent, MemberData data) : MemberUnassignable<EventInfo>(parent, data) { }