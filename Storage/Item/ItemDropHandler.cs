using Ion.Analysis;
using Ion.Controls;
using Ion.Core;
using Ion.Input;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ion.Storage;

///(abstract) <see cref="ItemDropHandler{Element}"/>
/// <see cref="Region.Constructor"/>
#region

public abstract class ItemDropHandler<Element>(Element parent) : DropHandler<Element, Item>(parent) where Element : UIElement
{
    /// <see cref="Region.Property"/>

    private static bool Modified => ModifierKeys.Control.Pressed() || ModifierKeys.Shift.Pressed();

    /// <see cref="Region.Method"/>
    #region

    protected override void Drop(IEnumerable<Item> source, UIElement target)
    {
        var parent = GetParent();
        if (parent is not null && XStorage.GetWarnBeforeDrop(parent))
        {
            var title
            = Modified
                ? XStorage.GetCopyWarningTitle(parent)
                : XStorage.GetMoveWarningTitle(parent);
            var message
                = Modified
                ? XStorage.GetCopyWarningMessage(parent)
                : XStorage.GetMoveWarningMessage(parent);

            Dialog.ShowResult(title, new Warning(message), i => i.IfEqual(0, () => Execute(source, GetTargetPath(target))), Buttons.YesNo);
            return;
        }
        Execute(source, GetTargetPath(target));
    }

    protected override bool Droppable(UIElement target)
    {
        if (target is FrameworkElement f)
        {
            if (f.DataContext is Item item)
            {
                switch (item.Type)
                {
                    case ItemType.Drive:
                        return false;

                    case ItemType.File:
                        return false;

                    case ItemType.Folder:
                        return true;

                    case ItemType.Shortcut:
                        return Shortcut.TargetsFolder(item.Path);
                }
            }
            else if (f.DataContext is string path)
                return Folder.Exists(path);
        }
        return false;
    }

    protected override IEnumerable<Item> From(DataObject dataObject)
    {
        if (dataObject.ContainsFileDropList())
        {
            foreach (var i in dataObject.GetFileDropList())
                yield return XItemPath.GetItem(i);
        }
    }

    protected virtual IStorageControl GetParent() => Parent as IStorageControl;

    protected virtual string GetTargetPath(UIElement target)
    {
        if (target is FrameworkElement element)
        {
            if (element.DataContext is Item i)
                return i.Path;

            if (element.DataContext is string j && Folder.Exists(j))
                return j;
        }
        return default;
    }

    protected void Execute(IEnumerable<Item> source, string targetPath)
    {
        if (targetPath is not null)
        {
            source.ForEach(i =>
            {
                if (Modified)
                    XItemPath.Copy(i.Path, targetPath);

                else XItemPath.Move(i.Path, targetPath, null);
            });
        }
    }

    #endregion
}

#endregion

/// <see cref="AddressBox"/>
#region

public class AddressBoxDropHandler(AddressBox parent) : ItemDropHandler<AddressBox>(parent)
{
    protected override bool Droppable(UIElement target) => base.Droppable(target) || target is ToolBar;
}

#endregion

/// <see cref="FolderBox"/>
#region

public class FolderBoxDropHandler(FolderBox parent) : ItemDropHandler<FolderBox>(parent)
{
    protected override bool Droppable(UIElement target)
    {
        var result = false;
        Try.Do(() =>
        {
            var targetPath = GetTargetPath(target);
            result = Shortcut.TargetsFolder(targetPath) || Folder.Exists(targetPath);
        });
        return result;
    }

    protected override string GetTargetPath(UIElement target)
    {
        if (Parent.SelectedItem is null)
            return Parent.Path;

        return Parent.SelectedItem.As<Folder>()?.Path;
    }
}

#endregion

/// <see cref="FolderButton"/>
#region

public class FolderButtonDropHandler(FolderButton parent) : ItemDropHandler<FolderButton>(parent)
{
    protected override bool Droppable(UIElement target)
    {
        var result = false;
        Try.Do(() =>
        {
            var targetPath = GetTargetPath(target);
            result = Shortcut.TargetsFolder(targetPath) || Folder.Exists(targetPath);
        });
        return result;
    }

    protected override string GetTargetPath(UIElement target) => Parent.Path;
}

#endregion

/// <see cref="ItemViewControl"/>
#region

public class ItemViewDropHandler(ItemViewPanel panel, System.Windows.Controls.Primitives.Selector parent) : ItemDropHandler<System.Windows.Controls.Primitives.Selector>(parent)
{
    private readonly ItemViewPanel Panel = panel;

    protected override bool Droppable(UIElement target) => base.Droppable(target) || target is ListBox;

    protected override IStorageControl GetParent() => Panel.Control as IStorageControl;

    protected override string GetTargetPath(UIElement target)
    {
        if (target is ListBox)
            return Panel.Path;

        return base.GetTargetPath(target);
    }
}

#endregion

/// <see cref="PathBox"/>
/// <see cref="Region.Constructor"/>
#region

public class PathBoxDropHandler(PathBox parent) : ItemDropHandler<PathBox>(parent)
{

    /// <see cref="Region.Method"/>

    protected override DragDropEffects DragOver(IEnumerable<Item> source, UIElement target)
        => DragDropEffects.Copy;

    protected override void Drop(IEnumerable<Item> source, UIElement target)
        => Parent.SetCurrentValue(PathBox.TextProperty, source.First<Item>().Path);

    protected override bool Droppable(UIElement target) => target is PathBox;
}

#endregion