using Ion.Core;
using System;
using System.ComponentModel;

namespace Ion.Controls;

[Serializable]
public record class FolderOptions : Model
{
    public ListSortDirection GroupDirection { get => Get(ListSortDirection.Ascending); set => Set(value); }

    public object GroupName { get => Get<object>(); set => Set(value); }

    public ListSortDirection SortDirection { get => Get(ListSortDirection.Ascending); set => Set(value); }

    public object SortName { get => Get<object>(); set => Set(value); }

    public DataViews View { get => Get(DataViewPanel.DefaultView); set => Set(value); }
}