using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Data;
using Ion.Input;
using Ion.Numeral;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

namespace Ion.Core;

[Image(Images.Bell)]
[Name("Notifications")]
[Serializable]
public record class NotificationPanel : XmlDataGridPanel<Notification>
{
    private enum Category { Text }

    public static readonly new ResourceKey Template = new();

    /// <see cref="Region.Field"/>

    private readonly Updatable update = new(1.Seconds());

    /// <see cref="Region.Property"/>
    #region

    public override bool CanAdd => false;

    public override bool CanCopyTo => false;

    public override bool CanEdit => false;

    public override bool CanMoveTo => false;

    public override bool CanPaste => false;

    public override bool CanAddFromPreset => false;

    [Styles.NumberAttribute(int.MinValue, int.MaxValue, 1, CanUpDown = true,
        RightText = "seconds",
        View = Ion.View.Option)]
    public int ClearAfter { get => Get(0); set => Set(value); }

    public IList<Notification> Notifications => Items as IList<Notification>;

    [Group(Category.Text)]
    [Image(Images.ArrowDownLeft)]
    [Style(Ion.Template.Check, NameHide = true, Index = int.MaxValue - 1,
        View = Ion.View.Header)]
    public bool TextWrap { get => Get(true); set => Set(value); }

    public override int TitleCount => Items?.Count<Notification>(i => !i.IsRead) ?? 0;

    #endregion

    /// <see cref="Region.Constructor"/>

    public NotificationPanel(IList<Notification> input) : base(input as IList)
    {
        Notifications.ForEach(i => { Unsubscribe(i); Subscribe(i); });
        update.Updated += OnUpdate;
    }

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnNotificationChanged(IPropertySet sender, PropertySetEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Notification.IsRead):
                Reset(() => Title);
                break;
        }
    }

    private void Subscribe(Notification input) => input.PropertySet += OnNotificationChanged;

    private void Unsubscribe(Notification input) => input.PropertySet -= OnNotificationChanged;

    private void OnUpdate(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (ClearAfter > 0)
        {
            for (var i = Notifications.Count - 1; i >= 0; i--)
            {
                var j = Notifications[i].As<Notification>();
                if (DateTime.Now > j.Added + TimeSpan.FromSeconds(ClearAfter))
                    Notifications.RemoveAt(i);
            }
        }
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>

    protected override void OnItemAdded(object input)
        => Subscribe(input as Notification);

    protected override void OnItemRemoved(object input)
        => Unsubscribe(input as Notification);

    /// <see cref="ICommand"/>


    public ICommand MarkCommand => Commands[nameof(MarkCommand)] ??= new RelayCommand<Notification>(i => i.IsRead = true, i => i is not null);

    private ICommand markAllCommand;
    [Group(GroupDefault.General)]
    [Image(Images.Read)]
    [Name("MarkAll")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand MarkAllCommand => markAllCommand
        ??= new RelayCommand(() => Notifications.ForEach(i => i.IsRead = true), () => Notifications?.Any<Notification>(i => !i.IsRead) == true);

    private ICommand unmarkAllCommand;
    [Group(GroupDefault.General)]
    [Image(Images.Unread)]
    [Name("UnmarkAll")]
    [Style(View = Ion.View.HeaderOption)]
    public ICommand UnmarkAllCommand => unmarkAllCommand
        ??= new RelayCommand(() => Notifications.ForEach(i => i.IsRead = false), () => Notifications?.Any<Notification>(i => i.IsRead) == true);
}