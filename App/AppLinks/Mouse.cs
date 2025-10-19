using System;

namespace Ion.Core;

[AppLink(Name = "Mouse",
    Description = "Shows information about the mouse anywhere on the screen.",
    Icon = Images.Arrow)]
public record class MouseLink() : PanelLink<MousePanel>() { }