﻿using Ion.Collect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

/// <summary>
/// Similar to <see cref="WrapPanel"/>, stacks items horizontally or vertically where a maximum number of columns or rows can be specified.
/// </summary>
public class StackControl : Control
{
    public static readonly ReferenceKey<StackPanel> StackPanelKey;

    ///

    private IList Items => Source as IList;

    private StackPanel Panel => this.GetChild(StackPanelKey);

    ///

    public static readonly DependencyProperty ColumnsOrRowsProperty = DependencyProperty.Register(nameof(ColumnsOrRows), typeof(int), typeof(StackControl), new PropertyMetadata(1, OnColumnsOrRowsChanged));
    public int ColumnsOrRows
    {
        get => (int)GetValue(ColumnsOrRowsProperty);
        set => SetValue(ColumnsOrRowsProperty, Math.Max(value, 1));
    }

    private static void OnColumnsOrRowsChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.As<StackControl>().OnColumnsOrRowsChanged(e);

    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(StackControl), new PropertyMetadata(null));
    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(StackControl), new PropertyMetadata(null));
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orient), typeof(StackControl), new PropertyMetadata(Orient.Horizontal, OnOrientationChanged));
    public Orient Orientation
    {
        get => (Orient)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    private static void OnOrientationChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.As<StackControl>().OnOrientationChanged(e);

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(object), typeof(StackControl), new PropertyMetadata(null, OnSourceChanged));
    public object Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private static void OnSourceChanged(object sender, DependencyPropertyChangedEventArgs e) => sender.As<StackControl>().OnSourceChanged(e);

    ///

    public StackControl() : base() => this.AddHandler(OnLoaded, OnUnloaded);

    ///

    private void OnItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => Update();

    ///

    private void OnLoaded()
    {
        Update();
        if (Source is IListObservable collection)
            collection.CollectionChanged += OnItemsChanged;
    }

    private void OnUnloaded()
    {
        if (Source is IListObservable collection)
            collection.CollectionChanged -= OnItemsChanged;
    }

    ///

    private StackPanel CreateColumnOrRow(IEnumerable<object> children)
    {
        var result = new StackPanel();
        result.Bind(StackPanel.HorizontalAlignmentProperty, nameof(HorizontalContentAlignment), this);
        result.Bind(StackPanel.OrientationProperty, nameof(Orientation), this);
        result.Bind(StackPanel.VerticalAlignmentProperty, nameof(VerticalContentAlignment), this);
        foreach (var i in children)
        {
            var content = new ContentPresenter() { Content = i };
            content.Bind(ContentPresenter.ContentTemplateProperty, nameof(ItemTemplate), this);
            content.Bind(ContentPresenter.StyleProperty, nameof(ItemContainerStyle), this);
            result.Children.Add(content);
        }
        return result;
    }

    private void Update()
    {
        var panel = Panel;
        if (panel is null)
            return;

        panel.Children.Clear();
        var items = new List<object>();

        var index = 0;
        foreach (var i in Items)
        {
            items.Add(i);
            index++;

            if (index == ColumnsOrRows)
            {
                panel.Children.Add(CreateColumnOrRow(items));
                items.Clear();
                index = 0;
            }
        }
        if (index != 0)
            panel.Children.Add(CreateColumnOrRow(items));
    }

    ///

    protected virtual void OnColumnsOrRowsChanged(Value<int> input)
        => Update();

    protected virtual void OnOrientationChanged(Value<Orient> input)
        => Update();

    protected virtual void OnSourceChanged(Value<object> input)
    {
        Update();
        if (IsLoaded)
        {
            if (Source is IListObservable collection)
                collection.CollectionChanged += OnItemsChanged;
        }
    }
}