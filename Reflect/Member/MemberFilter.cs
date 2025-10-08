using System;

namespace Ion.Reflect;

public class MemberFilter(Type attribute, bool ignore, View section) : object()
{
    public readonly Type Attribute = attribute;

    public readonly bool Ignore = ignore;

    public readonly View Section = section;
}