using System;
using System.Windows.Input;

namespace Ion.Controls;

/// <summary>
/// A key that generates token when pressed.
/// </summary>
[Flags]
public enum TokenBoxTrigger
{
    None = 0,
    /// <summary>
    /// = <see cref="Key.Enter"/>.
    /// </summary>
    Return = 1,
    /// <summary>
    /// = <see cref="Key.Tab"/>.
    /// </summary>
    Tab = 2,
    All = Return | Tab
}