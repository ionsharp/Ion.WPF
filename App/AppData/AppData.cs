using Ion.Analysis;
using Ion.Collect;
using Ion.Data;
using Ion.Input;
using Ion.Local;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Core;

/// <inheritdoc/>
[Image(Images.Options)]
[Styles.Object(
    Filter = Filter.All,
    Options = Option.All,
    TabPlacement = Side.Left,
    MemberViewType = MemberViewType.Tab)]
public abstract record class AppData() : Serializable(), IAppComponent
{
    private enum Group { Button, Dialog, Footer, General, Group, Header, Limit, Log, Menu, Model, Size, Window }

    /// <see cref="Region.Property.Protected"/>

    protected virtual WindowState DefaultWindowState => WindowState.Normal;

    /// <see cref="Region.Property.Public"/>
    #region

    [Name("Auto save"), Style(Template.CheckSwitch, Pin = Sides.LeftOrTop)]
    public bool AutoSave { get => Get(false); set => Set(value); }

    #endregion

    private readonly Dictionary<object, object> mix = [];

    /// <summary>Get and set miscellaneous data like a dictionary for stuff that can't be defined explicitly!</summary>
    ///<remarks>Don't worry about absent keys!</remarks>
    public object this[object key]
    {
        get => mix.ContainsKey(key) ? mix[key] : null;
        set
        {
            if (mix.ContainsKey(key))
                mix[key] = value;

            else mix.Add(key, value);
        }
    }

    /// <see cref="Tab"/>
    #region

    [TabView(View = View.Main)]
    private enum Tab
    {
        [TabStyle(Group = false,
            Image = Images.Analyze)]
        Analyze,
        [TabStyle(Group = false,
            Image = Images.Dialog)]
        Dialog,
        [TabStyle(Group = false,
            Image = Images.Font)]
        Font,
        [TabStyle(Group = false,
            Image = Images.General)]
        General,
        [TabStyle(Image = Images.Globe)]
        Language,
        [TabStyle(Image = Images.Log)]
        Log,
        [TabStyle(Image = Images.Bell)]
        Notification,
        [TabStyle(Group = false,
            Image = Images.Lock)]
        Password,
        [TabStyle(Image = Images.Palette)]
        Theme,
        [TabStyle(Image = Images.Warning)]
        Warn,
        [TabStyle(Image = Images.Window)]
        Window
    }

    /// <see cref="Tab.Analyze"/>
    #region

    [Name("Enable")]
    [Style(Template.CheckSwitch, Tab = Tab.Analyze,
        Pin = Sides.LeftOrTop)]
    public bool AnalysisEnable { get => Get(false); set => Set(value); }

    #endregion

    /// <see cref="Tab.Dialog"/>
    #region

    [Group(Group.General)]
    [Styles.Color(Template.ColorText,
        Alpha = true,
        Tab = Tab.Dialog,
        ValueConvert = typeof(ConvertColorToString)), Name("Background")]
    public ByteVector4 DialogBackground { get => Get(new ByteVector4("AAFFFFFF")); set => Set(value); }

    [Group(Group.Button)]
    [Name("Style")]
    [Style(Tab = Tab.Dialog)]
    public Controls.ButtonStyle DialogButtonStyle { get => Get(Controls.ButtonStyle.Circle); set => Set(value); }

    [Group(Group.Size)]
    [Name("Maximum height")]
    [Styles.Number(1.0, double.MaxValue, 1.0, CanUpDown = true,
        Tab = Tab.Dialog)]
    public double DialogMaximumHeight { get => Get(720.0); set => Set(value); }

    [Group(Group.Size)]
    [Name("Maximum width")]
    [Styles.Number(1.0, double.MaxValue, 1.0, CanUpDown = true,
        Tab = Tab.Dialog)]
    public double DialogMaximumWidth { get => Get(540.0); set => Set(value); } //900

    [Group(Group.Size)]
    [Name("Minimum width")]
    [Styles.Number(1.0, double.MaxValue, 1.0, CanUpDown = true,
        Tab = Tab.Dialog)]
    public double DialogMinimumWidth { get => Get(540.0); set => Set(value); } //360

    [Group(Group.Log)]
    [Name("Show on level")]
    [Style(Template.EnumFlag,
        Tab = Tab.Dialog)]
    public EntryLevel DialogOnLogLevel { get => Get(EntryLevel.High); set => Set(value); }

    [Group(Group.Log)]
    [Name("Show on type")]
    [Style(Template.EnumFlag,
        Tab = Tab.Dialog)]
    public ResultType DialogOnLogType { get => Get(ResultType.Error); set => Set(value); }

    #endregion

    /// <see cref="Tab.Font"/>
    #region

    [Name("Family")]
    [Style(Tab = Tab.Font)]
    public FontFamily FontFamily { get => Get(new FontFamily("Calibri")); set => Set(value); }
    //ValueConverter.Instance.Get<ConvertFontFamilyToString>()

    [Name("Scale")]
    [Styles.Number(Tab = Tab.Font,
        Step = 0.01,
        Maximum = 3,
        Minimum = 0.5,
        RightText = "%")]
    public double FontScale { get => Get(1.0); set => Set(value); }

    [Name("Size")]
    [Styles.Number(Tab = Tab.Font,
        Step = 1.0,
        Maximum = 72.0,
        Minimum = 1.0,
        RightText = "px")]
    public double FontSize { get => Get(12.0); set => Set(value); }

    [Group(Group.Group)]
    [Name("Family")]
    [Style(Tab = Tab.Font)]
    public FontFamily GroupFontFamily { get => Get(new FontFamily("Segoe UI")); set => Set(value); }
    //ValueConverter.Instance.Get<ConvertFontFamilyToString>()

    [Group(Group.Group)]
    [Name("Scale")]
    [Styles.Number(Tab = Tab.Font,
        Step = 0.01,
        Maximum = 3,
        Minimum = 0.5,
        RightText = "%")]
    public double GroupFontScale { get => Get(1.0); set => Set(value); }

    [Group(Group.Group)]
    [Name("Size")]
    [Styles.Number(Tab = Tab.Font,
        Step = 1.0,
        Maximum = 72.0,
        Minimum = 1.0,
        RightText = "px")]
    public double GroupFontSize { get => Get(15.0); set => Set(value); }

    [Group(Group.Menu)]
    [Name("Family")]
    [Style(Tab = Tab.Font)]
    public FontFamily MenuFontFamily { get => Get(new FontFamily("Segoe UI")); set => Set(value); }
    //ValueConverter.Instance.Get<ConvertFontFamilyToString>()

    [Group(Group.Menu)]
    [Name("Scale")]
    [Styles.Number(Tab = Tab.Font,
        Step = 0.01,
        Maximum = 3,
        Minimum = 0.5,
        RightText = "%")]
    public double MenuFontScale { get => Get(1.0); set => Set(value); }

    [Group(Group.Menu)]
    [Name("Size")]
    [Styles.Number(Tab = Tab.Font,
        Step = 1.0,
        Maximum = 72.0,
        Minimum = 1.0,
        RightText = "px")]
    public double MenuFontSize { get => Get(13.0); set => Set(value); }

    #endregion

    /// <see cref="Tab.General"/>
    #region

    [Style(Tab = Tab.General,
        Name = "Show in taskbar",
        Description = "Show in taskbar.")]
    public virtual bool TaskbarIconVisibility { get => Get(true); set => Set(value); }

    [Style(Tab = Tab.General,
        Name = "Hide instead of close",
        Description = "Closing the window hides it instead.")]
    public virtual bool TaskbarIconHidesInsteadOfCloses { get => Get(true); set => Set(value); }

    #endregion

    /// <see cref="Tab.Language"/>
    #region

    [Style(Tab = Tab.Language,
        Name = "Language")]
    public Language Language { get => Get(Language.English); set => Set(value); }

    #endregion

    /// <see cref="Tab.Log"/>
    #region

    [Style(Tab = Tab.Log,
        Name = "Clear on exit")]
    public bool LogClearOnExit { get => Get(true); set => Set(value); }

    [Group(Group.Limit)]
    [Name("Limit")]
    [Styles.Number(0, 5000, 1,
        Tab = Tab.Log)]
    public int LogLimit { get => Get(5000); set => Set(value); }

    [Group(Group.Limit)]
    [Style(Tab = Tab.Log,
        Name = "Limit action")]
    public ListWritableLimitAction LogLimitAction { get => Get(ListWritableLimitAction.RemoveFirst); set => Set(value); }

    [Name("Options")]
    [Style(Tab = Tab.Log)]
    [StyleOverride(nameof(StyleAttribute.NameHide), true)]
    [StyleOverride(nameof(StyleAttribute.IsLockable), true)]
    public virtual LogOptions LogOptions { get => Get(new LogOptions()); set => Set(value); }

    #endregion

    /// <see cref="Tab.Notification"/>
    #region

    [Group(Group.Log)]
    [Name("Notify on level")]
    [Style(Template.EnumFlag,
        Tab = Tab.Notification)]
    public EntryLevel NotifyOnLogLevel { get => Get(EntryLevel.All); set => Set(value); }

    [Group(Group.Log)]
    [Name("Notify on type")]
    [Style(Template.EnumFlag,
        Tab = Tab.Notification)]
    public ResultType NotifyOnLogType { get => Get(ResultType.All); set => Set(value); }

    [Group(Group.Limit)]
    [Name("Limit")]
    [Styles.Number(0, 5000, 1,
        Tab = Tab.Notification)]
    public int NotificationLimit { get => Get(5000); set => Set(value); }

    [Group(Group.Limit)]
    [Name("Limit action")]
    [Style(Tab = Tab.Notification)]
    public ListWritableLimitAction NotificationLimitAction { get => Get(ListWritableLimitAction.ClearAndArchive); set => Set(value); }

    #endregion

    /// <see cref="Tab.Password"/>
    #region

    public bool PasswordErrorNeverShow { get => Get(false); set => Set(value); }

    [Name("Dialog title")]
    [Styles.Text(Tab = Tab.Password)]
    public string PasswordDialogTitle { get => Get("Unlock"); set => Set(value); }

    [Name("Enable")]
    [Style(Template.CheckSwitch, Tab = Tab.Password)]
    public bool PasswordEnable { get => Get(false); set => Set(value); }

    [Name("Password")]
    [Styles.Text(Template.Password, Tab = Tab.Password)]
    [VisibilityTrigger(nameof(PasswordType), Comparison.Equal, PasswordType.Default)]
    public string PasswordDefault { get => Get(""); set => Set(value); }

    [Name("Interval")]
    [Styles.Number(0.0, double.MaxValue, 1.0, CanUpDown = true, Tab = Tab.Password,
        RightText = "seconds")]
    public double PasswordInterval { get => Get(5 * 60); set => Set(value); }

    [Name("Password")]
    [Style(Tab = Tab.Password)]
    [VisibilityTrigger(nameof(PasswordType), Comparison.Equal, PasswordType.Pattern)]
    public Pattern PasswordPattern { get => Get<Pattern>(); set => Set(value); }

    [Name("Password")]
    [Styles.Number(0, 9999, 1, Tab = Tab.Password)]
    [VisibilityTrigger(nameof(PasswordType), Comparison.Equal, PasswordType.Pin)]
    public int PasswordPin { get => Get(0); set => Set(value); }

    [Name("Type")]
    [Style(Tab = Tab.Password,
        Pin = Sides.LeftOrTop)]
    public PasswordType PasswordType { get => Get(PasswordType.Default); set => Set(value); }

    #endregion

    /// <see cref="Tab.Theme"/>
    #region

    [Name("Theme"), Style(Tab = Tab.Theme,
        Index = 1)]
    public static AppResources Themes => Appp.Model.Theme;

    public string Theme { get => Get($"{DefaultThemes.Light}"); set => Set(value); }

    [Style(Tab = Tab.Theme,
        Name = "Auto save",
        Index = 0)]
    public bool AutoSaveTheme { get => Get(true); set => Set(value); }

    #endregion

    /// <see cref="Tab.Warn"/>
    #region

    [Group(Group.Window)]
    [Name("On closing with errors")]
    [Style(Tab = Tab.Warn)]
    public bool WarnOnCloseWithErrors { get => Get(true); set => Set(value); }

    [Group(Group.Window)]
    [Name("On closing with unsaved documents")]
    [Style(Tab = Tab.Warn)]
    public bool WarnOnCloseWithUnsavedDocuments { get => Get(true); set => Set(value); }

    #endregion

    /// <see cref="Tab.Window"/>
    #region

    [Group(Group.Button)]
    [Name("Style")]
    [Style(Tab = Tab.Window)]
    public Controls.ButtonStyle WindowButtonStyle { get => Get(Controls.ButtonStyle.Circle); set => Set(value); }

    [Group(Group.Footer)]
    [Name("Show")]
    [Style(Tab = Tab.Window)]
    public bool WindowFooterVisibility { get => Get(true); set => Set(value); }

    [Group(Group.Header)]
    [Name("Show")]
    [Style(Tab = Tab.Window)]
    public bool WindowHeaderVisibility { get => Get(true); set => Set(value); }

    [Group(Group.Header)]
    [Name("Placement")]
    [Style(Tab = Tab.Window)]
    public SideY WindowHeaderPlacement { get => Get(SideY.Top); set => Set(value); }

    public virtual double WindowHeight { get => Get(900.0); set => Set(value); }

    public virtual double WindowWidth { get => Get(1200.0); set => Set(value); }

    public virtual WindowState WindowState { get => Get(DefaultWindowState, false); set => Set(value, false); }

    [Style(Tab = Tab.Window,
        Name = "Show top level icons")]
    [Group(Group.Menu)]
    public bool WindowMenuShowTopLevelIcons { get => Get(true); set => Set(value); }

    [Group(Group.Menu)]
    [Style(Tab = Tab.Window,
        Name = "Orientation")]
    public Orient WindowMenuOrientation { get => Get(Orient.Horizontal); set => Set(value); }

    #endregion

    [Group(Group.Model)]
    [Styles.Object(Tab = Tab.Window,
        Name = "Data",
        NameHide = true)]
    public static object ViewModelData => Appp.Model.ViewModel;

    #endregion

    /// <see cref="Region.Method"/>

    [NotStable]
    protected override void OnDeserialized()
    {
        base.OnDeserialized();
        ///Instance.GetPropertyValuesThatImplement<IItemList>(this).ForEach(i => i.Refresh());
        ///Instance.GetPropertyValuesThatImplement<IListWritable>(this).ForEach(i => i.Load());
    }

    [NotStable]
    protected override void OnSerialized()
    {
        base.OnSerialized();
        ///Instance.GetPropertyValuesThatImplement<IListWritable>(this).ForEach(i => i.Save());
    }

    /// <see cref="ICommand"/>
    #region

    [Image(Images.Reset), Name("Reset")]
    [Style(View = View.HeaderItem)]
    public virtual ICommand ResetCommand => Commands[nameof(ResetCommand)] ??= new RelayCommand(() => { });

    [Image(Images.Save), Name("Save")]
    [Style(View = View.HeaderItem)]
    public virtual ICommand SaveCommand => Commands[nameof(SaveCommand)] ??= new RelayCommand(() => Serialize());

    #endregion
}