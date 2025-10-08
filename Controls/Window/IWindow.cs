using System.Collections.Generic;

namespace Ion.Controls;

public interface IWindow
{
    IEnumerable<ButtonModel> FooterButtons { get; }

    IEnumerable<ButtonModel> HeaderButtons { get; }
}