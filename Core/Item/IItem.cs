using System;

namespace Ion.Core;

public interface IItem
{
    string Description { get; }

    string Name { get; }

    object Value { get; }

    Type ValueType { get; }
}