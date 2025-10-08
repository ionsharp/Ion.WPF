using System;
using System.Collections.Generic;
using System.Windows;

namespace Ion.Controls;

public class FrameworkElementHandler : DependencyObject
{
    ///<see cref="Region.Field"/>
    #region

    private readonly FrameworkElement Element;

    private readonly List<Action<FrameworkElement>> Load = new();

    private readonly Dictionary<object, Action<FrameworkElement>> LoadAttached = new();

    private readonly Dictionary<object, bool> PreloadAttached = new();

    private readonly List<Action<FrameworkElement>> Unload = new();

    private readonly Dictionary<object, Action<FrameworkElement>> UnloadAttached = new();

    #endregion

    ///<see cref="Region.Constructor"/>

    DependencyProperty IsLoadedProperty = null;

    public FrameworkElementHandler(FrameworkElement i, DependencyProperty isLoaded)
    {
        Element = i;
        i.Loaded += OnLoaded;

        IsLoadedProperty = isLoaded;
    }

    ///<see cref="Region.Method.Internal"/>
    #region

    public void Add(Action load, Action unload)
    {
        if (load is not null)
            Load.Add(i => load());

        if (unload is not null)
            Unload.Add(i => unload());
    }

    public void Add<T>(Action<T> load, Action<T> unload) where T : FrameworkElement
    {
        if (load != null)
            Load.Add(i => load((T)i));

        if (unload != null)
            Unload.Add(i => unload((T)i));
    }

    public void AddAttached<T>(bool add, object key, Action<T> load, Action<T> unload) where T : FrameworkElement
    {
        if (UnloadAttached.ContainsKey(key))
            UnloadAttached[key](Element);

        else unload?.Invoke((T)Element);

        LoadAttached
            .Remove(key);
        UnloadAttached
            .Remove(key);

        PreloadAttached
            .Remove(key);

        if (add)
        {
            PreloadAttached.Add(key, false);
            if (load != null)
            {
                if (Element.IsLoaded)
                {
                    PreloadAttached[key] = true;
                    load((T)Element);
                }
                LoadAttached.Add(key, i => load((T)i));
            }

            if (unload != null)
                UnloadAttached.Add(key, i => unload((T)i));
        }
    }

    #endregion

    ///<see cref="Region.Method.Private"/>
    #region

    private void OnLoaded()
    {
        if (Element is Window window)
            window.Closed += OnWindowClosed;

        if (Element is not Window)
        {
            Element.Unloaded
                += OnUnloaded;
            Window.GetWindow(Element).If(i => i is not null, i => i.Closed += OnWindowClosed);
        }

        Load
            .ForEach(i => i(Element));
        LoadAttached
            .ForEach(i =>
            {
                if (!PreloadAttached[i.Key])
                    i.Value(Element);
            });

        if (IsLoadedProperty is not null)
            Element.SetValue(IsLoadedProperty, true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => OnLoaded();

    private void OnUnloaded()
    {
        if (Element is Window window)
            window.Closed -= OnWindowClosed;

        if (Element is not Window)
        {
            Element.Unloaded
                -= OnUnloaded;
            Window.GetWindow(Element).If(i => i is not null, i => i.Closed -= OnWindowClosed);
        }

        Unload
            .ForEach(i => i(Element));
        UnloadAttached
            .ForEach(i =>
            {
                PreloadAttached[i.Key] = false;
                i.Value(Element);
            });

        if (IsLoadedProperty is not null)
            Element.SetValue(IsLoadedProperty, false);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => OnUnloaded();

    private void OnWindowClosed(object sender, EventArgs e)
    {
        Element.Loaded -= OnLoaded;
        OnUnloaded();
    }

    #endregion
}