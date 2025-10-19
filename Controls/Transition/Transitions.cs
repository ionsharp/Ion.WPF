using System;

namespace Ion.Controls;

/// <summary>
/// Transitions of <see cref="TransitionControl"/>.
/// </summary>
public enum Transitions
{
    /// <summary>
    /// Use the <see cref="VisualState"/> DefaultTransition
    /// </summary>
    Default,
    /// <summary>
    /// Use the <see cref="VisualState"/> Normal
    /// </summary>
    Normal,
    /// <summary>
    /// Use the <see cref="VisualState"/> UpTransition
    /// </summary>
    Up,
    /// <summary>
    /// Use the <see cref="VisualState"/> DownTransition
    /// </summary>
    Down,
    /// <summary>
    /// Use the <see cref="VisualState"/> RightTransition
    /// </summary>
    Right,
    /// <summary>
    /// Use the <see cref="VisualState"/> RightReplaceTransition
    /// </summary>
    RightReplace,
    /// <summary>
    /// Use the <see cref="VisualState"/> LeftTransition
    /// </summary>
    Left,
    /// <summary>
    /// Use the <see cref="VisualState"/> LeftReplaceTransition
    /// </summary>
    LeftReplace,
    /// <summary>
    /// Use a custom <see cref="VisualState"/>, the name must be set using <see langword="CustomVisualStatesName"/> property
    /// </summary>
    Custom,
    /// <summary>
    /// Use a random (defined) <see cref="VisualState"/>.
    /// </summary>
    Random
}