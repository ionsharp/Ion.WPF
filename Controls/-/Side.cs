using System;

namespace Ion;

[Flags]
public enum Side { None = 0, Top = 1, Bottom = 2, Left = 4, Right = 8, All = Top | Bottom | Left | Right }

public enum Sides { None = 0, LeftOrTop, RightOrBottom }

public enum SideX { Left, Right }

public enum SideY { Top, Bottom }