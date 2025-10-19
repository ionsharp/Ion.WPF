using System;

namespace Ion.Core;

[AppLink(Name = "Random",
    Description = "Generate a sequence of random characters with an arbitrary length.",
    Icon = Images.Dice)]
public record class RandomLink() : PanelLink<RandomPanel>() { }