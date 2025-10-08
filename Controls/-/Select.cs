using System;

namespace Ion.Controls;

[Serializable]
public enum Select
{
    /// <summary>One item must be selected.</summary>
    One,
    /// <summary>One item can be selected.</summary>
    OneOrNone,
    /// <summary>One or more items must be selected.</summary>
    OneOrMore,
    /// <summary>One or more items can be selected.</summary>
    OneOrMoreOrNone
}