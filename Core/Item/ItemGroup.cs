using Ion.Collect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ion.Core;

public class ItemGroup<T> : ListObservable<Item<T>>, IItemGroup, IName
{
    public string Name { get => this.Get(""); set => this.Set(value); }

    public ItemGroup() : this(default, (T[])[]) { }

    public ItemGroup(string name) : this(name, (T[])[]) { }

    public ItemGroup(string name, IEnumerable<T> items) : this(name, items?.Select(i => new Item<T>(i)) ?? []) { }

    public ItemGroup(string name, IEnumerable<Item<T>> items) : base(items) => Name = name;

    ///

    public void Add(string name, T item) => Add(new Item<T>(name, item));

    public void Add(string name, string description, T item) => Add(new Item<T>(name, description, item));

    public override string ToString() => Name;
}