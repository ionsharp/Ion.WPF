using Ion.Collect;
using Ion.Data;
using Ion.Reflect;
using System;
using System.Collections.Generic;

namespace Ion.Core;

public static class Appp
{
    /// <see cref="Region.Property"/>

    private static readonly Dictionary<Type, IAppElement> elements = [];

    public static CacheByTypeList Cache { get; private set; } = [];

    public static IAppModel Model { get; internal set; }

    /// <see cref="Region.Method.Internal"/>

    internal static void AddElement<T>(T instance) where T : IAppElement
    {
        var type = instance.GetType();
        if (elements.ContainsKey(type))
        {
            elements[type] = instance;
            return;
        }
        elements.Add(type, instance);
    }

    /// <see cref="Region.Method.Public"/>

    public static T Get<T>() where T : IAppElement
    {
        if (elements.ContainsKey(typeof(T)))
            return (T)elements[typeof(T)];

        return elements.FirstOrDefault<Type, IAppElement, T>(i => i.Key.Inherits(typeof(T)) || (typeof(T).IsInterface && i.Key.Implements<T>()));
    }

    public static object GetSource(AppSource i)
    {
        return i switch
        {
            AppSource.App
                => App.Current,
            AppSource.Data
                => Model?.Data,
            AppSource.Model
                => Model,
            AppSource.Theme
                => Model?.Theme,
            AppSource.View
                => Model?.View,
            _ => null,
        };
    }
}