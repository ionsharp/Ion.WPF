using System;

namespace Ion.Local;

[Description("A language.")]
[Serializable]
public enum Language
{
    [Culture("en")]
    [Description("The english language.")]
    [Image(Images.FlagUnitedStates,
        Mask = false)]
    English,
    [Culture("fr-FR")]
    [Description("The french language.")]
    [Image(Images.FlagFrance,
        Mask = false)]
    French,
    [Culture("it-IT")]
    [Description("The italian language.")]
    [Image(Images.FlagItaly,
        Mask = false)]
    Italian,
    [Culture("ja-JP")]
    [Description("The japanese language.")]
    [Image(Images.FlagJapan,
        Mask = false)]
    Japanese
}