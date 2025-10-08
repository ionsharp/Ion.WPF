using System;

namespace Ion.Core;

[AppLink(Name = "Reflect",
    Description = "Examine all compiled assembly code.",
    Icon = Images.Code)]
[Serializable]
public record class ReflectLink() : PanelLink<ReflectPanel>() { }