using System;

namespace Ion.Data;

public enum SearchWord
{
    [Description("The target text must match all source words.")]
    All,
    [Description("The target text must match at least one source word.")]
    Any,
    [Description("The target text must match the source text exactly.")]
    Exact,
    [Description("The target text cannot match any source word.")]
    None
}