using System;

namespace Ion.Storage;

[Serializable]
public enum CopyDirection
{
    [Image(Images.ArrowPrevious)]
    Left,
    [Image(Images.ArrowNext)]
    Right,
    [Image(Images.ArrowBoth)]
    Both
}