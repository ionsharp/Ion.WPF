using Ion.Core;
using System;
using System.Windows.Controls;

namespace Ion.Controls;

public abstract record class ControlOptions<T> : Model, IControlOptions where T : Control
{
    protected ControlOptions() : base() { }
}