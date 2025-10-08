using System;

namespace Ion.Controls;

[Serializable]
public enum DataViews
{
    [Image(Images.ViewBlock)]
    Block,
    [Image(Images.ViewDetail)]
    Detail,
    [Image(Images.ViewPage)]
    Page,
    [Image(Images.ViewSlide)]
    Slide,
    [Image(Images.ViewThumb)]
    Thumb,
    [Image(Images.ViewTile)]
    Tile,
}