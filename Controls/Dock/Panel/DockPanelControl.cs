﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Controls;

public class DockPanelControl(DockRootControl root) : DockContentControl(root)
{
    public static readonly DependencyProperty CollapseProperty = DependencyProperty.Register(nameof(Collapse), typeof(bool), typeof(DockPanelControl), new FrameworkPropertyMetadata(false));
    public bool Collapse
    {
        get => (bool)GetValue(CollapseProperty);
        set => SetValue(CollapseProperty, value);
    }

    public static readonly DependencyProperty IsMenuVisibleProperty = DependencyProperty.Register(nameof(IsMenuVisible), typeof(bool), typeof(DockPanelControl), new FrameworkPropertyMetadata(false));
    public bool IsMenuVisible
    {
        get => (bool)GetValue(IsMenuVisibleProperty);
        set => SetValue(IsMenuVisibleProperty, value);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (Collapse)
            {
                if (e.OriginalSource.As<DependencyObject>().GetParent<TabItem>() is TabItem item)
                {
                    SetCurrentValue(IsMenuVisibleProperty, !item.IsSelected);
                    if (item.IsSelected)
                        SetCurrentValue(SelectedIndexProperty, -1);
                }
            }
        }
        base.OnPreviewMouseDown(e);
    }
}