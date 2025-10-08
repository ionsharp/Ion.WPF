using Ion.Extend;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Ion;

public class ElementHandler : DependencyObject
{
    /// <see cref="Region.Field"/>
    #region

    private readonly FrameworkElement Element;

    private readonly List<Action<FrameworkElement>> Load = [];

    private readonly Dictionary<object, Action<FrameworkElement>> LoadAttached = [];

    private readonly Dictionary<object, bool> PreloadAttached = [];

    private readonly List<Action<FrameworkElement>> Unload = [];

    private readonly Dictionary<object, Action<FrameworkElement>> UnloadAttached = [];

    #endregion

    /// <see cref="Region.Constructor"/>

    public ElementHandler(FrameworkElement i)
    {
        Element = i;
        i.Loaded += OnLoaded;
    }

    /// <see cref="Region.Method.Internal"/>
    #region

    internal void Add(Action load, Action unload)
    {
        if (load is not null)
            Load.Add(i => load());

        if (unload is not null)
            Unload.Add(i => unload());
    }

    internal void Add<T>(Action<T> load, Action<T> unload) where T : FrameworkElement
    {
        if (load != null)
            Load.Add(i => load((T)i));

        if (unload != null)
            Unload.Add(i => unload((T)i));
    }

    internal void AddAttached<T>(bool add, object key, Action<T> load, Action<T> unload) where T : FrameworkElement
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

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnLoaded()
    {
        if (Element is Window window)
            window.Closed += OnWindowClosed;

        if (Element is not Window)
        {
            Element.Unloaded
                += OnUnloaded;
            Window.GetWindow(Element).IfNotNull(i => i.Closed += OnWindowClosed);
        }

        Load
            .ForEach(i => i(Element));
        LoadAttached
            .ForEach(i =>
            {
                if (!PreloadAttached[i.Key])
                    i.Value(Element);
            });

        XElement.SetIsLoaded(Element, true);
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
            Window.GetWindow(Element).IfNotNull(i => i.Closed -= OnWindowClosed);
        }

        Unload
            .ForEach(i => i(Element));
        UnloadAttached
            .ForEach(i =>
            {
                PreloadAttached[i.Key] = false;
                i.Value(Element);
            });

        XElement.SetIsLoaded(Element, false);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => OnUnloaded();

    private void OnWindowClosed(object sender, EventArgs e)
    {
        Element.Loaded -= OnLoaded;
        OnUnloaded();
    }

    #endregion
}