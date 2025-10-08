using Ion.Data;
using Ion.Storage;
using Ion.Windows;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Ion.Controls;

[Extend<IStorageControl>]
public static class XStorage
{
    public static readonly ResourceKey IconTemplateKey = new();

    public static readonly ResourceKey ToolTipTemplateKey = new();

    #region (IValueConverter) LegacyToolTipConverter

    public static readonly IValueConverter LegacyToolTipConverter = new ValueConverter<string, string>(i =>
    {
        var result = new StringBuilder();

        var type = XItemPath.GetType(i.Value);
        switch (type)
        {
            case ItemType.Drive:
                foreach (var j in Drive.Get())
                {
                    if (j.Name == i.Value)
                    {
                        result.AppendLine($"Available space: {new FileSize(j.AvailableFreeSpace).ToString(FileSizeFormat.BinaryUsingSI)}");
                        result.Append($"Total space: {new FileSize(j.TotalSize).ToString(FileSizeFormat.BinaryUsingSI)}");
                        break;
                    }
                }
                break;

            case ItemType.File:
                var fileInfo = new FileInfo(i.Value);
                result.AppendLine($"Type: {ShellProperties.GetDescription(i.Value)}");
                result.Append($"Size: {new FileSize(fileInfo.Length).ToString(FileSizeFormat.BinaryUsingSI)}");
                break;

            case ItemType.Folder:
                var directoryInfo = new DirectoryInfo(i.Value);
                result.Append($"Date created: {directoryInfo.CreationTime}");
                break;

            case ItemType.Shortcut:
                if (!Try.Do(() => result.Append($"Location: {Shortcut.TargetPath(i.Value)}")))
                    goto case ItemType.File;

                break;
        }
        return result.ToString();
    });

    #endregion

    #region DefaultPath

    public static readonly DependencyProperty DefaultPathProperty = DependencyProperty.RegisterAttached("DefaultPath", typeof(string), typeof(XStorage), new FrameworkPropertyMetadata(FilePath.Root));
    public static string GetDefaultPath(IStorageControl i) => (string)i.As<DependencyObject>().GetValue(DefaultPathProperty);
    public static void SetDefaultPath(IStorageControl i, string input) => i.As<DependencyObject>().SetValue(DefaultPathProperty, input);

    #endregion

    #region CopyWarningTitle

    public static readonly DependencyProperty CopyWarningTitleProperty = DependencyProperty.RegisterAttached("CopyWarningTitle", typeof(string), typeof(XStorage), new FrameworkPropertyMetadata(string.Empty));
    public static string GetCopyWarningTitle(IStorageControl i) => (string)i.As<DependencyObject>().GetValue(CopyWarningTitleProperty);
    public static void SetCopyWarningTitle(IStorageControl i, string input) => i.As<DependencyObject>().SetValue(CopyWarningTitleProperty, input);

    #endregion

    #region CopyWarningMessage

    public static readonly DependencyProperty CopyWarningMessageProperty = DependencyProperty.RegisterAttached("CopyWarningMessage", typeof(string), typeof(XStorage), new FrameworkPropertyMetadata(string.Empty));
    public static string GetCopyWarningMessage(IStorageControl i) => (string)i.As<DependencyObject>().GetValue(CopyWarningMessageProperty);
    public static void SetCopyWarningMessage(IStorageControl i, string input) => i.As<DependencyObject>().SetValue(CopyWarningMessageProperty, input);

    #endregion

    #region MoveWarningTitle

    public static readonly DependencyProperty MoveWarningTitleProperty = DependencyProperty.RegisterAttached("MoveWarningTitle", typeof(string), typeof(XStorage), new FrameworkPropertyMetadata(string.Empty));
    public static string GetMoveWarningTitle(IStorageControl i) => (string)i.As<DependencyObject>().GetValue(MoveWarningTitleProperty);
    public static void SetMoveWarningTitle(IStorageControl i, string input) => i.As<DependencyObject>().SetValue(MoveWarningTitleProperty, input);

    #endregion

    #region MoveWarningMessage

    public static readonly DependencyProperty MoveWarningMessageProperty = DependencyProperty.RegisterAttached("MoveWarningMessage", typeof(string), typeof(XStorage), new FrameworkPropertyMetadata(string.Empty));
    public static string GetMoveWarningMessage(IStorageControl i) => (string)i.As<DependencyObject>().GetValue(MoveWarningMessageProperty);
    public static void SetMoveWarningMessage(IStorageControl i, string input) => i.As<DependencyObject>().SetValue(MoveWarningMessageProperty, input);

    #endregion

    #region Path

    public static readonly DependencyProperty PathProperty = DependencyProperty.RegisterAttached("Path", typeof(string), typeof(XStorage), new FrameworkPropertyMetadata(string.Empty, OnPathChanged));
    public static string GetPath(IStorageControl i) => (string)i.As<DependencyObject>().GetValue(PathProperty);
    public static void SetPath(IStorageControl i, string input) => i.As<DependencyObject>().SetValue(PathProperty, input);

    private static void OnPathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is IStorageControl i)
        {
            if (i is FrameworkElement j)
                j.RaiseEvent(new PathChangedEventArgs(PathChangedEvent, (string)e.NewValue));
        }
    }

    #endregion

    #region PathChanged

    public static readonly RoutedEvent PathChangedEvent = EventManager.RegisterRoutedEvent("PathChanged", RoutingStrategy.Direct, typeof(PathChangedEventHandler), typeof(XStorage));
    public static void AddPathChangedHandler(DependencyObject i, PathChangedEventHandler handler)
    {
        if (i is UIElement j)
            j.AddHandler(PathChangedEvent, handler);
    }
    public static void AddPathChanged(this IStorageControl i, PathChangedEventHandler handler) => AddPathChangedHandler(i as DependencyObject, handler);
    public static void RemovePathChangedHandler(DependencyObject i, PathChangedEventHandler handler)
    {
        if (i is UIElement j)
            j.RemoveHandler(PathChangedEvent, handler);
    }
    public static void RemovePathChanged(this IStorageControl i, PathChangedEventHandler handler) => RemovePathChangedHandler(i as DependencyObject, handler);

    #endregion

    #region WarnBeforeDrop

    public static readonly DependencyProperty WarnBeforeDropProperty = DependencyProperty.RegisterAttached("WarnBeforeDrop", typeof(bool), typeof(XStorage), new FrameworkPropertyMetadata(true));
    public static bool GetWarnBeforeDrop(IStorageControl i) => (bool)i.As<DependencyObject>().GetValue(WarnBeforeDropProperty);
    public static void SetWarnBeforeDrop(IStorageControl i, bool input) => i.As<DependencyObject>().SetValue(WarnBeforeDropProperty, input);

    #endregion
}