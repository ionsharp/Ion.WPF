using Hardcodet.Wpf.TaskbarNotification;
using Ion.Analysis;
using Ion.Collect;
using Ion.Controls;
using Ion.Imaging;
using Ion.Input;
using Ion.Local;
using Ion.Numeral;
using Ion.Reflect;
using Ion.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WPFLocalizeExtension.Engine;

namespace Ion.Core;

/// <see cref="AppModel"/>
#region

/// <inheritdoc/>
[Styles.Object(Filter = Filter.None,
    MemberViewType = MemberViewType.All,
    MemberView = Ion.View.Footer | Ion.View.Header | Ion.View.HeaderItem | Ion.View.HeaderOption)]
public abstract record class AppModel : AppModelBase, IAppModel, IWindow, IFrameworkElementReference
{
    /// <see cref="Region.Key"/>

    public static readonly IFrameworkElementKey TaskbarIconKey = new ReferenceKey<TaskbarIcon>();

    /// <see cref="Region.Event"/>
    #region

    public event EventHandler<EventArgs> SetLanguage;

    public event EventHandler<EventArgs> ThemeChanged;

    #endregion

    /// <see cref="Region.Field"/>
    #region

    private bool firstLoad;

    private PropertyBinding logOptionsBinding;

    ///

    private SplashWindow splashWindow;

    private StartupEventArgs startEvent;

    #endregion

    /// <see cref="Region.Property.Protected"/>
    #region

    protected ListObservableWritable<Entry> Log
    { get => Get<ListObservableWritable<Entry>>(); private set => Set(value); }

    protected ListObservableWritable<Notification> Notifications
    { get => Get<ListObservableWritable<Notification>>(); private set => Set(value); }

    protected TaskbarIcon
        TaskbarIcon;

    #endregion

    /// <see cref="Region.Property.Public"/>
    #region

    public IAnalyze<Assembly> Analyzer { get => Get<IAnalyze<Assembly>>(); private set => Set(value); }

    public virtual Type AnalyzerType => typeof(AppAnalyzer);

    public virtual CacheByType Clipboard { get => Get<CacheByType>(); private set => Set(value); }

    public AppData Data { get => Get<AppData>(); private set => Set(value); }

    public virtual string DataFilePath => $@"{DataFolderPath}\Option.data";

    public virtual AppDataFolder DataFolder => AppDataFolder.Documents;

    /// <summary>The folder where data used by the app is saved.</summary>
    public virtual string DataFolderPath => $@"{DataFolderPathShared}\{nameof(AppFull)}\{XAssembly.GetInfo(AssemblySource.Entry).Title}";

    /// <summary>The folder where data shared by all apps is saved.</summary>
    public virtual string DataFolderPathShared
    {
        get
        {
            var result = GetFolderPath(DataFolder);
            return DataFolder switch
            {
                AppDataFolder.Documents => $@"{result}\{XAssembly.GetInfo(AssemblyData.Name).Company}",
                AppDataFolder.Execution => $@"{result}\Data",
                _ => throw new NotSupportedException(),
            };
        }
    }

    protected virtual Type DataType { get; }

    public AppLinkList Links { get => Get<AppLinkList>(); private set => Set(value); }

    public virtual string LinkFilePath => Appp.Model.DataFolderPath + $@"\Links";

    public AppMenu Menu { get => Get<AppMenu>(); private set => Set(value); }

    protected virtual Type MenuType { get; }

    public AppView View { get => Get<AppView>(); private set => Set(value); }

    protected virtual Type ViewType { get; }

    public IViewModel ViewModel { get => Get<IViewModel>(); protected set => Set(value); }

    protected virtual Type ViewModelType => typeof(ViewModel);

    ///

    private bool saveData = true;

    private bool saveLog;

    protected bool exitCheck = true;

    protected bool exitHide = true;

    public virtual double ExitDelay => 1.5; //Seconds

    public virtual string ExitTitle => "Exiting...";

    ///

    public virtual AppResources Theme { get => Get<AppResources>(); private set => Set(value); }

    public virtual PathList Themes { get; private set; } = new PathList(new Storage.Filter(ItemType.File, "theme"));

    public virtual string ThemePath => Appp.Model.DataFolderPathShared + $@"\Themes";

    ///

    public virtual string Title => XAssembly.GetInfo(AssemblySource.Entry).Title;

    #endregion

    /// <see cref="AppModel.ExitTasks"/>
    #region

    public virtual IEnumerable<AppTask> ExitTasks { get; }

    #endregion

    /// <see cref="AppModel.StartTasks"/>
    #region

    public IEnumerable<AppTask> DefaultStartTasks
    {
        get
        {
            yield return new(0, "Log",
                () =>
                {
                    Log = new();
                    Log.SetFile(DataFolderPath, "Log", "dat");
                    Log.Limit = ListWritableLimit.Default;
                    Log.Load();

                    Analysis.Log.Added += OnLogAdded;
                });
            yield return new(0, "Notifications",
                () =>
                {
                    Notifications = new();
                    Notifications.SetFile(DataFolderPath, "Notifications", "dat");
                    Notifications.Limit = ListWritableLimit.Default;
                    Notifications.Load();

                    Notifications.Changed += OnNotificationsChanged;
                });
            yield return new(1, "Data",
                () =>
                {
                    FileSerializer.Deserialize(DataFilePath, out AppData oldData);
                    Data = oldData ?? DataType.Create<AppData>();
                });
            yield return new(0, "Analysis",
                () => Data.AnalysisEnable.If(() =>
                {
                    Analyzer = new AppAnalyzer(new());
                    Analyzer.Analyze([.. AssemblyData.ProjectNames.Select(i => XAssembly.Get(i)), XAssembly.Entry]);
                }));
            yield return new(1, "Language",
                () =>
                {
                    OnSetLanguage(Data.Language);
                });
            yield return new(1, "Links",
                () =>
                {
                    Links = new(LinkFilePath);
                });
            yield return new(1, "Menu",
                () =>
                {
                    Menu = MenuType.Create<AppMenu>(this);
                });
            yield return new(1, "Theme",
                () =>
                {
                    Theme.LoadTheme(Data.Theme);
                    Folder.Exists(ThemePath).If(() => Folder.Create(ThemePath));

                    Themes.Subscribe();
                    _ = Themes.RefreshAsync(ThemePath);

                    OnSetTheme();
                });
            yield return new(1, "View",
                () =>
                {
                    ViewModel = ViewModelType.Create<IViewModel>();
                });
        }
    }

    public virtual IEnumerable<AppTask> StartTasks { get; }

    #endregion

    /// <see cref="Region.Constructor"/>

    protected AppModel() : base()
    {
        Appp.AddElement(this);

        /// <see cref="AppFull"/>

        AppFull.Current.ExceptionUnhandled += OnExceptionUnhandled;
        AppFull.Current.If<IAppSingle>(i => i.Reloaded += OnAppReloaded);

        /// <see cref="AppResources"/>

        Theme = [];
        Theme.LoadTheme(DefaultThemes.Light);
    }

    /// <see cref="EventHandler"/>
    #region

    private void OnAppLoaded(IAppFull input, IList<string> arguments)
        => OnAppLoaded(arguments);

    private void OnAppReloaded(IAppSingle input, AppReloadedEventArgs e)
        => OnAppReloaded(e.Arguments);

    private void OnDataChanged(IPropertySet sender, PropertySetEventArgs e)
        => OnDataChanged(e);

    private void OnDataSaved(object sender, EventArgs e)
        => OnDataSaved(sender as AppData);

    private void OnDataSaving(object sender, EventArgs e)
        => OnDataSaving(sender as AppData);

    private void OnExceptionUnhandled(object sender, Core.UnhandledExceptionEventArgs e)
        => OnExceptionUnhandled(e);

    private void OnLinkDisabled(object sender, EventArgs<IAppLink> e)
        => OnLinkDisabled(e.A);

    private void OnLinkEnabled(object sender, EventArgs<IAppLink> e)
        => OnLinkEnabled(e.A);

    [NotComplete]
    private void OnLogAdded(LogEventArgs e)
    {
        if (e.Entry is Notification)
            Notifications.Add(e.Entry as Notification);

        ///else if (e.Entry is Entry)
            ///Log.Add(e.Entry as Entry);
    }

    private void OnLogChanged(IListChanged<Entry> sender, Collect.ListChangedEventArgs e)
        => e.Change.If(i => i == ListChange.Add, i => e.NewItems.ForEach(j => OnLogAdded(new LogEventArgs((Entry)j))));

    private void OnViewClosed(object sender, EventArgs e)
        => OnViewClosed();

    private void OnViewClosing(object sender, System.ComponentModel.CancelEventArgs e)
        => OnViewClosing(e);

    private void OnViewLoaded(object sender, RoutedEventArgs e)
        => OnViewLoaded(e);

    #endregion

    /// <see cref="Region.Method.Private"/>
    #region

    private void OnNotificationExpired(object sender, EventArgs e)
        => Notifications.Remove(sender as Notification);

    private void OnNotificationsChanged(IListChanged<Notification> sender, Collect.ListChangedEventArgs e)
    {
        switch (e.Change)
        {
            case ListChange.Add:
                e.NewItems.FirstOrDefault().If<Notification>(i =>
                {
                    i.Expired += OnNotificationExpired;
                    OnNotificationAdded(i);
                });
                break;
            case ListChange.Remove:
                e.OldItems.FirstOrDefault().If<Notification>(i =>
                {
                    i.Expired -= OnNotificationExpired;
                    OnNotificationExpired(i);
                });
                break;
        }
    }

    ///

    protected void HideView()
    {
        View.Hide();
        View.ShowInTaskbar = false;
    }

    protected void ShowView()
    {
        View.Show();
        View.ShowInTaskbar = true;
    }

    protected void ShowPasswordDialog(bool error = false)
    {
        var form = new PasswordForm(Data.PasswordType);
        Dialog.ShowObject(Data.PasswordDialogTitle, form, Resource.GetImageUri(Images.Lock), i =>
        {
            if (i == 0 && form.Password == Data.PasswordDefault)
            {
                //Correct!
            }
            else
            {
                //Incorrect!
                var neverShow = new Accessor<bool>(() => Data.PasswordErrorNeverShow, i => Data.PasswordErrorNeverShow = i);
                if (neverShow.Get())
                    ShowPasswordDialog(true);

                else Dialog.ShowResult("Wrong password", new PasswordNotValid(), i => ShowPasswordDialog(true), neverShow, Buttons.Ok);
            }
        },
        Buttons.Continue);
    }

    #endregion

    /// <see cref="Region.Method.Protected"/>
    #region

    /// <summary>Start (1)</summary>
    void IAppModel.Start(StartupEventArgs e)
    {
        startEvent = e;

        splashWindow = new SplashWindow();
        splashWindow.SetResourceReference(SplashWindow.StyleProperty, SplashWindow.DefaultStyle);
        splashWindow.Shown += OnAppStarted;
        splashWindow.Show();
    }

    /// <summary>Start (2)</summary>
    private void OnAppStarted(object sender, EventArgs e)
    {
        splashWindow.Shown -= OnAppStarted;
        OnAppStarted(startEvent);
    }

    /// <summary>Start (3)</summary>
    protected virtual async void OnAppStarted(StartupEventArgs e)
    {
        await OnAppStartedTasks(splashWindow.Delay);

        firstLoad = true;
        OnAppLoaded(e.Args);

        View =
        (
            Application.Current.MainWindow
                = Appp.Model.ViewType.Create<Window>()
        )
        as AppView;

        await splashWindow.FadeOut();
        View.Show();

        splashWindow.Close();
        splashWindow = null;

        startEvent = null;
    }

    /// <summary>Start (4)</summary>
    protected virtual async Task OnAppStartedTasks(int delay)
    {
        await Task.Run(async () =>
        {
            var allTasks = StartTasks?.Any() == true
                ? DefaultStartTasks.Concat(StartTasks)
                : DefaultStartTasks;

            double a = 0, b = allTasks.Count(), progress = 0;
            foreach (var i in allTasks)
            {
                System.Threading.Thread.Sleep(delay);

                a++;
                progress = a / b;

                await Dispatch.BeginInvoke(() => { splashWindow.Message = i.Message; splashWindow.Progress = progress; });
                Try.Do(() =>
                {
                    if (!i.Dispatch)
                    {
                        i.Action();
                        return;
                    }
                    Dispatch.Do(i.Action);
                },
                e => Analysis.Log.Write(e));
            }
        });
    }

    /// <summary>Start (5a)</summary>
    protected virtual void OnAppLoaded(IList<string> arguments)
    {
        XToolTip.Initialize();
    }

    /// <summary>Start (5b)</summary>
    protected virtual void OnAppReloaded(IList<string> arguments)
    {
        View.Activate();
        //Do something if view is just hidden...
    }

    ///

    protected virtual void OnDataChanged(PropertySetEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(AppData.NotificationLimit):
            case nameof(AppData.NotificationLimitAction):
                Notifications.Limit
                    = new(Data.NotificationLimit, Data.NotificationLimitAction);

                break;

            case nameof(AppData.Language):
                OnSetLanguage(Appp.Model.Data.Language);
                break;

            case nameof(AppData.LogLimit):
            case nameof(AppData.LogLimitAction):
                Log.Limit = new(Data.LogLimit, Data.LogLimitAction);
                break;

            case nameof(AppData.PasswordEnable):
                break;

            case nameof(AppData.TaskbarIconVisibility):
                if (!Data.TaskbarIconVisibility)
                    ShowView();

                break;

            case nameof(AppData.Theme):
                Appp.Model.Theme.LoadTheme(Appp.Model.Data.Theme);
                OnSetTheme();
                break;
        }
        //if (AutoSave) Save();
    }

    protected virtual void OnDataLoaded(AppData data) { }

    protected virtual void OnDataSaved(AppData data)
    {
        if (data.AutoSaveTheme)
        {
            if (File.Exists(data.Theme))
                SaveTheme(System.IO.Path.GetFileNameWithoutExtension(data.Theme));
        }
    }

    protected virtual void OnDataSaving(AppData data) { }

    protected virtual void OnExceptionUnhandled(UnhandledExceptionEventArgs e) => Analysis.Log.Write(e.Error, EntryLevel.High);

    protected virtual void OnExitCancelled(Result result) { }

    protected virtual void OnLinkDisabled(IAppLink extension) { }

    protected virtual void OnLinkEnabled(IAppLink extension) { }

    protected virtual void OnLogAdded(IList input)
    {
        if (Data is null)
            return;

        foreach (Entry i in input)
        {
            if (Data.DialogOnLogLevel.HasFlag(i.Level))
            {
                if (Data.DialogOnLogType.HasFlag(i.Result.Type))
                    Dialog.ShowResult($"{i.Result.Type}", i.Result, Buttons.Ok);
            }
            if (Data.NotifyOnLogLevel.HasFlag(i.Level))
            {
                if (Data.NotifyOnLogType.HasFlag(i.Result.Type))
                    Analysis.Log.Notify(i.Sender, i.Result, Notification.DefaultExpiration);
            }
        }
    }

    protected virtual void OnNotificationAdded(Notification i) { }

    protected virtual void OnNotificationExpired(Notification i) { }

    protected virtual void OnSetData(Value<AppData> e)
    {
        Appp.AddElement(Data);
        e.OldValue.IfNotNull(i =>
        {
            i.PropertySet
                -= OnDataChanged;
            i.Serialized
                -= OnDataSaved;
            i.Serializing
                -= OnDataSaving;

            BindingManager.Unset(logOptionsBinding);
            logOptionsBinding = null;
        });
        e.NewValue.IfNotNull(i =>
        {
            i.PropertySet
                += OnDataChanged;
            i.Serialized
                += OnDataSaved;
            i.Serializing
                += OnDataSaving;

            //logOptionsBinding = new(BindMode.TwoWay, nameof(AppData.LogOptions), Data, nameof(ILog.Options), Log);
            //BindingManager.Set(logOptionsBinding);

            OnDataLoaded(i);
        });
    }

    protected virtual void OnSetLanguage(Language i)
    {
        Try.Do(() =>
        {
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = new CultureInfo(i.GetAttribute<CultureAttribute>().Code);
        },
        e => Analysis.Log.Write(e));
        SetLanguage?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnSetLog(Value<IListWritable<Entry>> e)
    {
        e.OldValue.IfNotNull(i => i.Changed -= OnLogChanged);
        e.NewValue.IfNotNull(i => i.Changed += OnLogChanged);
    }

    protected virtual void OnSetLinks(Value<AppLinkList> e)
    {
        e.OldValue.IfNotNull(i =>
        {
            i.Disabled
                -= OnLinkDisabled;
            i.Enabled
                -= OnLinkEnabled;

            i.Unsubscribe();
            i.Clear();
        });
        e.NewValue.IfNotNull(i =>
        {
            Try.Do(() =>
            {
                Folder.Create(LinkFilePath);
                if (!Folder.GetFiles(LinkFilePath).Where(j => System.IO.Path.GetExtension(j).ToLower() == ".ext").Any())
                {
                    var defaultLinks = XAssembly.Get(AssemblyData.Name)
                        .GetDerivedTypes<IAppLink>()
                        .Select(j => Try.Get(() => j.Create<IAppLink>()))
                        .ToList();

                    foreach (var link in defaultLinks)
                    {
                        if (link is not null)
                        {
                            var defaultExtensionPath = $@"{LinkFilePath}\{link.Name}.ext";
                            if (!File.Exists(defaultExtensionPath))
                                FileSerializer.Serialize(defaultExtensionPath, link);
                        }
                    }
                }
            },
            e => Analysis.Log.Write(e));

            i.Disabled
                += OnLinkDisabled;
            i.Enabled
                += OnLinkEnabled;

            //i.Subscribe();
            i.Refresh();
        });
    }

    protected virtual void OnSetMenu(Value<AppView> e)
    {
        Appp.AddElement(Menu);
    }

    protected virtual void OnSetTheme()
    {
        //This doesn't change automatically when the theme does...
        TaskbarIcon?.ContextMenu?.UpdateDefaultStyle();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnSetView(Value<AppView> e)
    {
        Appp.AddElement(View);
        e.OldValue.IfNotNull(i =>
        {
            i.DataContext = null;
            i.Closed
                -= OnViewClosed;
            i.Closing
                -= OnViewClosing;
            i.Loaded
                -= OnViewLoaded;
        });
        e.NewValue.IfNotNull(i =>
        {
            i.DataContext = this;
            i.Closed
                += OnViewClosed;
            i.Closing
                += OnViewClosing;
            i.Loaded
                += OnViewLoaded;

            i.WindowStartupLocation
                = WindowStartupLocation.CenterScreen;
        });
    }

    protected virtual void OnSetViewModel(Value<IViewModel> e) { }

    protected virtual void OnViewClosed() { }

    protected virtual async void OnViewClosing(System.ComponentModel.CancelEventArgs e)
    {
        //0) Hide instead?
        if (exitHide)
        {
            if (Data.TaskbarIconVisibility)
            {
                if (Data.TaskbarIconHidesInsteadOfCloses)
                {
                    e.Cancel = true;
                    HideView(); return;
                }
            }
        }

        //1) Check if anything needs done first
        if (exitCheck)
        {
            //a) Determine exit action
            Action exitAction = null;

            //  i) Save data
            void aData()
            {
                if (Appp.Model.Notifications.Save() is Error a)
                    throw a;

                if (Data.Serialize() is Error b)
                    throw b;
            }
            // ii) Save log
            void aLog()
            {
                if (Data.LogClearOnExit)
                {
                    Appp.Model.Log.Clear();
                    While.Do(() => Appp.Model.Log.Count > 0);
                }
                var r = Appp.Model.Log.Save();
                if (r is Error s)
                    throw s;
            }

            //iii) If warning, do both consecutively
            if (Data.WarnOnCloseWithErrors)
            {
                if (saveData)
                    exitAction = aData;

                if (saveLog)
                    exitAction = aLog;
            }

            // iv) If NOT warning, do both concurrently
            else exitAction = () => { aData(); aLog(); };

            //b) Do exit action
            if (exitAction is not null)
            {
                e.Cancel = true;

                var result = await Dialog.ShowProgress(ExitTitle, i => exitAction(), TimeSpan.FromSeconds(ExitDelay), true);
                Analysis.Log.Write(result);

                //No error
                if (result)
                {
                    if (Data.WarnOnCloseWithErrors)
                    {
                        //Data was saved (without error)
                        if (saveData)
                        {
                            saveData = false; saveLog = true;
                            exitCheck = true;
                        }

                        //Log was saved (without error)
                        else if (saveLog)
                        {
                            saveData = false; saveLog = false;
                            exitCheck = false;
                        }
                    }
                    //Data AND log was saved (without error)
                    else exitCheck = false;
                }

                //Error
                else
                {
                    //Warn!
                    if (Data.WarnOnCloseWithErrors)
                    {
                        var never = new Accessor<bool>(() => !Data.WarnOnCloseWithErrors, i => Data.WarnOnCloseWithErrors = !i);

                        //Data was saved (with error)
                        if (saveData)
                        {
                            Dialog.ShowResult(XAssembly.GetInfo(AssemblySource.Entry).Title, new Error($"An error occurred while saving data. Close anyway?") { Inner = result as Error }, i =>
                            {
                                if (i == 0)
                                {
                                    //Next checks
                                    saveData = false; saveLog = true;
                                    exitCheck = true;

                                    XWindow.SetDisableCancel(View, true);
                                    View.Close();
                                }
                                if (i == 1)
                                {
                                    //Reset checks
                                    saveData = true; saveLog = false;
                                    exitCheck = true;

                                    OnExitCancelled(result);
                                }
                            },
                            never,
                            Buttons.YesCancel);
                        }

                        //Log was saved (with error)
                        else if (saveLog)
                        {
                            Dialog.ShowResult(XAssembly.GetInfo(AssemblySource.Entry).Title, new Error($"An error occurred while saving data. Close anyway?") { Inner = result as Error }, i =>
                            {
                                if (i == 0)
                                {
                                    //Next checks
                                    saveData = false; saveLog = false;
                                    exitCheck = false;

                                    XWindow.SetDisableCancel(View, true);
                                    View.Close();
                                }
                                if (i == 1)
                                {
                                    //Reset checks
                                    saveData = true; saveLog = false;
                                    exitCheck = true;

                                    OnExitCancelled(result);
                                }
                            },
                            never,
                            Buttons.YesCancel);
                        }
                        return;
                    }
                    //Don't warn!
                    else
                    {
                        //Data AND log was saved (with error)
                        exitCheck = false;
                        View.Close(); return;
                    }
                }
            }
        }
    }

    protected virtual void OnViewLoaded(RoutedEventArgs e)
    {
        if (Data?.PasswordEnable == true)
            ShowPasswordDialog();
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public string GetDataFolderPath(string relativePath) => DataFolderPath + $@"\" + relativePath;

    public string GetDataFolderPathRoot(string relativePath) => DataFolderPathShared + $@"\" + relativePath;

    public Result SaveTheme(string nameWithoutExtension)
        => Appp.Model.Theme.ActiveTheme.TrySerialize($@"{ThemePath}\{nameWithoutExtension}.theme");

    public virtual void SetReference(IFrameworkElementKey a, FrameworkElement b)
    {
        if (a == TaskbarIconKey)
            TaskbarIcon = (TaskbarIcon)b;
    }

    #endregion

    /// <see cref="Region.Method.Static"/>
    #region

    public static string GetFolderPath(AppDataFolder folder)
    {
        return folder switch
        {
            AppDataFolder.Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            AppDataFolder.Execution => System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            AppDataFolder.Local => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppDataFolder.Roaming => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _ => throw new NotSupportedException(),
        };
    }

    #endregion

    /// <see cref="ICommand"/>
    #region

    public ICommand ExitCommand
        => Commands[nameof(ExitCommand)]
        ??= new RelayCommand(View.Close, () => true);

    public ICommand KillCommand
        => Commands[nameof(KillCommand)]
        ??= new RelayCommand(() => { exitHide = false; View.Close(); }, () => true);

    public ICommand HideCommand
        => Commands[nameof(HideCommand)]
        ??= new RelayCommand(HideView, () => View.IsVisible);

    public ICommand ShowCommand
        => Commands[nameof(ShowCommand)]
        ??= new RelayCommand(ShowView, () => !View.IsVisible);

    public ICommand ThemeCommand
        => Commands[nameof(ThemeCommand)]
        ??= new RelayCommand<string>(i => Data.Theme = i, i => true);

    #endregion

    /// <see cref="IAppModel"/>
    #region

    AppData IAppModel.Data => Data;

    Type IAppModel.DataType => DataType;

    IListWritable<Entry> IAppModel.Log => Log;

    AppLinkList IAppModel.Links => Links;

    AppMenu IAppModel.Menu => Menu;

    Type IAppModel.MenuType => MenuType;

    IListWritable<Notification> IAppModel.Notifications => Notifications;

    IAppView IAppModel.View { get => View; set => View = (AppView)value; }

    IViewModel IAppModel.ViewModel => ViewModel;

    Type IAppModel.ViewType => ViewType;

    Window IAppModel.Window => View as Window;

    #endregion

    /// <see cref="IPropertySet"/>

    public override void OnSetProperty(PropertySetEventArgs e)
    {
        base.OnSetProperty(e);
        switch (e.PropertyName)
        {
            case nameof(Data):
                OnSetData(e);
                break;

            case nameof(Log):
                OnSetLog(e);
                break;

            case nameof(Links):
                OnSetLinks(e);
                break;

            case nameof(Menu):
                OnSetMenu(e);
                break;

            case nameof(View):
                OnSetView(e);
                break;

            case nameof(ViewModel):
                OnSetViewModel(e);
                break;
        }
    }

    /// <see cref="IWindow"/>
    #region

    public virtual IEnumerable<ButtonModel> FooterButtons => default;

    public virtual IEnumerable<ButtonModel> HeaderButtons
    {
        get
        {
            yield return new ButtonModel()
            {
                Color = Brushes.Gold,
                Command = HelpCommand,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Help)),
                Tip = "Help"
            };
            yield return new ButtonModel()
            {
                Color = Brushes.DarkGray,
                Command = HideCommand,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Hide)),
                Tip = "Hide"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("FFAA44"))),
                Command = XWindow.MinimizeCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Minimize)),
                Tip = "Minimize"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("44AA44"))),
                Command = XWindow.MaximizeCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Maximize)),
                Tip = "Maximize"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("007ACC"))),
                Command = XWindow.RestoreCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Restore)),
                Tip = "Restore"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("CC3344"))),
                Command = XWindow.CloseCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.X)),
                Tip = "Close"
            };
        }
    }

    #endregion
}

#endregion

/// <see cref="AppModel{data,menu,view}"/>
#region

/// <inheritdoc/>
public abstract record class AppModel<data, menu, view>() : AppModel() where data : AppData where menu : AppMenu where view : AppView
{
    /// <see cref="Region.Property"/>

    new public data Data => (data)base.Data;

    protected override Type DataType => typeof(data);

    new public menu Menu => (menu)base.Menu;

    protected override Type MenuType => typeof(menu);

    new public view View => (view)base.View;

    protected override Type ViewType => typeof(view);
}

#endregion

/// <see cref="AppModel{data,menu,view,viewModel}"/>
#region

/// <inheritdoc/>
public abstract record class AppModel<data, menu, view, viewModel>() : AppModel<data, menu, view>() where data : AppData where menu : AppMenu where view : AppView where viewModel : IDataViewModel
{
    /// <see cref="Region.Property"/>

    new public viewModel ViewModel => base.ViewModel.As<viewModel>();

    protected override Type ViewModelType => typeof(viewModel);

    protected override void OnSetViewModel(Value<IViewModel> e)
    {
        base.OnSetViewModel(e);
        if (e.NewValue is IDataViewModel i)
        {
            i.DataFolderPath
                = Appp.Model.DataFolderPath;
            i.DataFileName
                = nameof(View);
            i.DataFileExtension
                = "data";

            i.Load();
            i.Subscribe();
        }
    }
}

#endregion