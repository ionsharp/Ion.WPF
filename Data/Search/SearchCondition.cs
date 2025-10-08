using System;

namespace Ion.Data;

[Serializable]
public enum SearchCondition
{
    [Description("The target text contains the source text.")]
    Contains,
    [Description("The target text ends with the source text.")]
    EndsWith,
    [Description("The target text starts with the source text.")]
    StartsWith
}