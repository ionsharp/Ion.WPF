namespace Ion.Controls;

public class ItemViewControl : DataViewControl, IStorageControl
{
    public const double HiddenOpacity = 0.5;

    /// <see cref="Region.Field"/>
    #region

    public static readonly ResourceKey DriveDescriptionTemplate = new();

    public static readonly ResourceKey DriveDetail1Template = new();

    public static readonly ResourceKey DriveDetail2Template = new();

    public static readonly ResourceKey FileDescriptionTemplate = new();

    public static readonly ResourceKey FileDetail1Template = new();

    public static readonly ResourceKey FileDetail2Template = new();

    public static readonly ResourceKey ItemNameCenterTemplate = new();

    public static readonly ResourceKey ItemNameLeftTemplate = new();

    public static readonly ResourceKey ItemNameLabelTemplate = new();

    public static readonly ResourceKey ItemNameStyle = new();

    public static readonly ResourceKey HiddenIconTemplate = new();

    public static readonly ResourceKey ReadOnlyIconTemplate = new();

    public static readonly ResourceKey ShortcutIconTemplate = new();

    #endregion

    /// <see cref="Region.Property"/>

    public string Path
    {
        get => XStorage.GetPath(this);
        set => XStorage.SetPath(this, value);
    }

    /// <see cref="Region.Constructor"/>

    public ItemViewControl() : base() { }
}