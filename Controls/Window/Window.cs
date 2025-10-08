using Ion;
using Ion.Analysis;
using Ion.Collect;
using Ion.Data;
using Ion.Numeral;
using Ion.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Ion.Controls;

[Extend<Window>]
public static class XWindow
{
    /// <see cref="Region.Field"/>
    #region

    public static readonly ReferenceKey<Border> BorderKey = new();

    public static readonly ReferenceKey<DisplayDialog> DisplayDialog = new();

    public static readonly ResourceKey GripTemplateKey = new();

    public static readonly ResourceKey HeaderPatternTemplate = new();

    public static readonly ResourceKey Template = new();

    private const int SC_MOVE = 0xF010;

    ///

    private const uint SWP_NOSIZE = 0x0001;

    private const uint SWP_NOMOVE = 0x0002;

    private const uint SWP_NOACTIVATE = 0x0010;

    private const uint SWP_NOZORDER = 0x0004;

    ///

    private const int WM_ACTIVATEAPP = 0x001C;

    private const int WM_ACTIVATE = 0x0006;

    private const int WM_SETFOCUS = 0x0007;

    private const int WM_SYSCOMMAND = 0x0112;

    private const int WM_WINDOWPOSCHANGING = 0x0046;

    #endregion

    /// <see cref="Region.Property"/>
    #region

    #region AutoCenter

    public static readonly DependencyProperty AutoCenterProperty = DependencyProperty.RegisterAttached("AutoCenter", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(false, OnAutoCenterChanged));
    public static bool GetAutoCenter(Window i) => (bool)i.GetValue(AutoCenterProperty);
    public static void SetAutoCenter(Window i, bool input) => i.SetValue(AutoCenterProperty, input);

    private static void OnAutoCenterChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
            window.AddHandlerAttached((bool)e.NewValue, AutoCenterProperty, i => i.SizeChanged += AutoCenter_SizeChanged, i => i.SizeChanged -= AutoCenter_SizeChanged);
    }

    private static void AutoCenter_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is Window i)
        {
            if (e.HeightChanged)
                i.Top += (e.PreviousSize.Height - e.NewSize.Height) / 2;

            if (e.WidthChanged)
                i.Left += (e.PreviousSize.Width - e.NewSize.Width) / 2;
        }
    }

    #endregion

    #region CanMaximize

    public static readonly DependencyProperty CanMaximizeProperty = DependencyProperty.RegisterAttached("CanMaximize", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(true));
    public static bool GetCanMaximize(Window i) => (bool)i.GetValue(CanMaximizeProperty);
    public static void SetCanMaximize(Window i, bool input) => i.SetValue(CanMaximizeProperty, input);

    #endregion

    #region CanMove

    public static readonly DependencyProperty CanMoveProperty = DependencyProperty.RegisterAttached("CanMove", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(true, OnCanMoveChanged));
    public static bool GetCanMove(Window i) => (bool)i.GetValue(CanMoveProperty);
    public static void SetCanMove(Window i, bool input) => i.SetValue(CanMoveProperty, input);

    private static void OnCanMoveChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
        {
            window.SourceInitialized
                -= CanMove_SourceInitialized;

            if ((bool)e.OldValue)
            {
                window.SourceInitialized
                    += CanMove_SourceInitialized;
            }
        }
    }

    private static void CanMove_SourceInitialized(object sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.SourceInitialized
                -= CanMove_SourceInitialized;

            HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).AddHook(CanMove_WndProc);
        }
    }

    private static nint CanMove_WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_SYSCOMMAND:
                var command = wParam.ToInt32() & 0xfff0;
                if (command == SC_MOVE)
                    handled = true;

                break;

            default: break;
        }
        return nint.Zero;
    }

    #endregion

    #region CloseCommand

    public static readonly RoutedUICommand CloseCommand = new(nameof(CloseCommand), nameof(CloseCommand), typeof(XWindow));

    private static void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        SetResult(sender as Window, Convert.ToInt32(e.Parameter ?? -1));
        SystemCommands.CloseWindow(sender as Window);
    }

    private static void CanClose(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    #endregion

    #region MarkNotificationCommand

    public static readonly RoutedUICommand MarkNotificationCommand = new(nameof(MarkNotificationCommand), nameof(MarkNotificationCommand), typeof(XWindow));

    private static void OnMarkNotificationCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window)
            (e.Parameter as Notification).IsRead = true;
    }

    private static void OnCanExecuteMarkNotificationCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window)
            e.CanExecute = e.Parameter is Notification;
    }

    #endregion

    #region ContentStyle

    public static readonly DependencyProperty ContentStyleProperty = DependencyProperty.RegisterAttached("ContentStyle", typeof(Style), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static Style GetContentStyle(Window i) => (Style)i.GetValue(ContentStyleProperty);
    public static void SetContentStyle(Window i, Style input) => i.SetValue(ContentStyleProperty, input);

    #endregion

    #region (readonly) Dialogs

    private static readonly DependencyPropertyKey DialogsKey = DependencyProperty.RegisterAttachedReadOnly("Dialogs", typeof(Stack<DialogModel>), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty DialogsProperty = DialogsKey.DependencyProperty;
    public static Stack<DialogModel> GetDialogs(Window i) => i.GetValueOrSetDefault(DialogsKey, () => new Stack<DialogModel>());

    #endregion

    #region DialogBlur

    public static readonly DependencyProperty DialogBlurProperty = DependencyProperty.RegisterAttached("DialogBlur", typeof(double), typeof(XWindow), new FrameworkPropertyMetadata(1000.0));
    public static double GetDialogBlur(Window i) => (double)i.GetValue(DialogBlurProperty);
    public static void SetDialogBlur(Window i, double input) => i.SetValue(DialogBlurProperty, input);

    #endregion

    #region DialogTemplate

    public static readonly DependencyProperty DialogTemplateProperty = DependencyProperty.RegisterAttached("DialogTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetDialogTemplate(Window i) => (DataTemplate)i.GetValue(DialogTemplateProperty);
    public static void SetDialogTemplate(Window i, DataTemplate input) => i.SetValue(DialogTemplateProperty, input);

    #endregion

    #region DialogTransition

    public static readonly DependencyProperty DialogTransitionProperty = DependencyProperty.RegisterAttached("DialogTransition", typeof(Transitions), typeof(XWindow), new FrameworkPropertyMetadata(Transitions.Default));
    public static Transitions GetDialogTransition(Window i) => (Transitions)i.GetValue(DialogTransitionProperty);
    public static void SetDialogTransition(Window i, Transitions input) => i.SetValue(DialogTransitionProperty, input);

    #endregion

    #region DisableCancel

    public static readonly DependencyProperty DisableCancelProperty = DependencyProperty.RegisterAttached("DisableCancel", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(false));
    public static bool GetDisableCancel(Window i) => (bool)i.GetValue(DisableCancelProperty);
    public static void SetDisableCancel(Window i, bool input) => i.SetValue(DisableCancelProperty, input);

    #endregion

    #region Extend

    public static readonly DependencyProperty ExtendProperty = DependencyProperty.RegisterAttached("Extend", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(false, OnExtendChanged));
    public static bool GetExtend(Window i) => (bool)i.GetValue(ExtendProperty);
    public static void SetExtend(Window i, bool input) => i.SetValue(ExtendProperty, input);

    private static void OnExtendChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
        {
            window.SourceInitialized -= OnSourceInitialized;
            if ((bool)e.NewValue)
                window.SourceInitialized += OnSourceInitialized;
        }
    }

    #endregion

    #region Footer

    public static readonly DependencyProperty FooterProperty = DependencyProperty.RegisterAttached("Footer", typeof(object), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static object GetFooter(Window i) => i.GetValue(FooterProperty);
    public static void SetFooter(Window i, object input) => i.SetValue(FooterProperty, input);

    #endregion

    #region FooterButtons

    public static readonly DependencyProperty FooterButtonsProperty = DependencyProperty.RegisterAttached("FooterButtons", typeof(ButtonList), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static ButtonList GetFooterButtons(Window i) => (ButtonList)i.GetValue(FooterButtonsProperty);
    public static void SetFooterButtons(Window i, ButtonList input) => i.SetValue(FooterButtonsProperty, input);

    #endregion

    #region FooterTemplate

    public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.RegisterAttached("FooterTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetFooterTemplate(Window i) => (DataTemplate)i.GetValue(FooterTemplateProperty);
    public static void SetFooterTemplate(Window i, DataTemplate input) => i.SetValue(FooterTemplateProperty, input);

    #endregion

    #region FooterTemplateSelector

    public static readonly DependencyProperty FooterTemplateSelectorProperty = DependencyProperty.RegisterAttached("FooterTemplateSelector", typeof(DataTemplateSelector), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplateSelector GetFooterTemplateSelector(Window i) => (DataTemplateSelector)i.GetValue(FooterTemplateSelectorProperty);
    public static void SetFooterTemplateSelector(Window i, DataTemplateSelector input) => i.SetValue(FooterTemplateSelectorProperty, input);

    #endregion

    #region FooterVisibility

    public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.RegisterAttached("FooterVisibility", typeof(Visibility), typeof(XWindow), new FrameworkPropertyMetadata(Visibility.Collapsed));
    public static Visibility GetFooterVisibility(Window i) => (Visibility)i.GetValue(FooterVisibilityProperty);
    public static void SetFooterVisibility(Window i, Visibility input) => i.SetValue(FooterVisibilityProperty, input);

    #endregion

    #region HeaderBackground

    public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.RegisterAttached("HeaderBackground", typeof(Brush), typeof(XWindow), new FrameworkPropertyMetadata(Brushes.Transparent));
    public static Brush GetHeaderBackground(Window i) => (Brush)i.GetValue(HeaderBackgroundProperty);
    public static void SetHeaderBackground(Window i, Brush input) => i.SetValue(HeaderBackgroundProperty, input);

    #endregion

    #region HeaderButtons

    public static readonly DependencyProperty HeaderButtonsProperty = DependencyProperty.RegisterAttached("HeaderButtons", typeof(object), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static object GetHeaderButtons(Window i) => i.GetValue(HeaderButtonsProperty);
    public static void SetHeaderButtons(Window i, object input) => i.SetValue(HeaderButtonsProperty, input);

    #endregion

    #region HeaderButtonStyle

    public static readonly DependencyProperty HeaderButtonStyleProperty = DependencyProperty.RegisterAttached("HeaderButtonStyle", typeof(ButtonStyle), typeof(XWindow), new FrameworkPropertyMetadata(ButtonStyle.Square));
    public static ButtonStyle GetHeaderButtonStyle(Window i) => (ButtonStyle)i.GetValue(HeaderButtonStyleProperty);
    public static void SetHeaderButtonStyle(Window i, ButtonStyle input) => i.SetValue(HeaderButtonStyleProperty, input);

    #endregion

    #region HeaderHeight

    public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.RegisterAttached("HeaderHeight", typeof(double), typeof(XWindow), new FrameworkPropertyMetadata(double.NaN));
    public static double GetHeaderHeight(Window i) => (double)i.GetValue(HeaderButtonStyleProperty);
    public static void SetHeaderHeight(Window i, double input) => i.SetValue(HeaderButtonStyleProperty, input);

    #endregion

    #region Icon

    public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(ImageSource), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static ImageSource GetIcon(Window i) => (ImageSource)i.GetValue(IconProperty);
    public static void SetIcon(Window i, ImageSource input) => i.SetValue(IconProperty, input);

    #endregion

    #region IconMenu

    public static readonly DependencyProperty IconMenuProperty = DependencyProperty.RegisterAttached("IconMenu", typeof(object), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static object GetIconMenu(Window i) => i.GetValue(IconMenuProperty);
    public static void SetIconMenu(Window i, object input) => i.SetValue(IconMenuProperty, input);

    #endregion

    #region IconSize

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.RegisterAttached("IconSize", typeof(MSize<double>), typeof(XWindow), new FrameworkPropertyMetadata(null));
    [TypeConverter(typeof(DoubleSizeTypeConverter))]
    public static MSize<double> GetIconSize(Window i) => (MSize<double>)i.GetValue(IconSizeProperty);
    public static void SetIconSize(Window i, MSize<double> input) => i.SetValue(IconSizeProperty, input);

    #endregion

    #region IconTemplate

    public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.RegisterAttached("IconTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetIconTemplate(Window i) => (DataTemplate)i.GetValue(IconTemplateProperty);
    public static void SetIconTemplate(Window i, DataTemplate input) => i.SetValue(IconTemplateProperty, input);

    #endregion

    #region IconVisibility

    public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.RegisterAttached("IconVisibility", typeof(Visibility), typeof(XWindow), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetIconVisibility(Window i) => (Visibility)i.GetValue(IconVisibilityProperty);
    public static void SetIconVisibility(Window i, Visibility input) => i.SetValue(IconVisibilityProperty, input);

    #endregion

    #region HeaderPlacement

    public static readonly DependencyProperty HeaderPlacementProperty = DependencyProperty.RegisterAttached("HeaderPlacement", typeof(SideY), typeof(XWindow), new FrameworkPropertyMetadata(SideY.Top));
    public static SideY GetHeaderPlacement(Window i) => (SideY)i.GetValue(HeaderPlacementProperty);
    public static void SetHeaderPlacement(Window i, SideY input) => i.SetValue(HeaderPlacementProperty, input);

    #endregion

    #region HeaderVisibility

    public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.RegisterAttached("HeaderVisibility", typeof(Visibility), typeof(XWindow), new FrameworkPropertyMetadata(Visibility.Visible));
    public static Visibility GetHeaderVisibility(Window i) => (Visibility)i.GetValue(HeaderVisibilityProperty);
    public static void SetHeaderVisibility(Window i, Visibility input) => i.SetValue(HeaderVisibilityProperty, input);

    #endregion

    #region IsAlwaysMaximized

    public static readonly DependencyProperty IsAlwaysMaximizedProperty = DependencyProperty.RegisterAttached("IsAlwaysMaximized", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(false, OnIsAlwaysMaximizedChanged));
    public static bool GetIsAlwaysMaximized(Window i) => (bool)i.GetValue(IsAlwaysMaximizedProperty);
    public static void SetIsAlwaysMaximized(Window i, bool input) => i.SetValue(IsAlwaysMaximizedProperty, input);

    private static void OnIsAlwaysMaximizedChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
            window.AddHandlerAttached((bool)e.NewValue, IsAlwaysMaximizedProperty, i => i.StateChanged += IsAlwaysMaximized_StateChanged, i => i.StateChanged -= IsAlwaysMaximized_StateChanged);
    }

    private static void IsAlwaysMaximized_StateChanged(object sender, EventArgs e)
    {
        if (sender is Window window)
        {
            if (window.WindowState != WindowState.Maximized)
                window.WindowState = WindowState.Maximized;
        }
    }

    #endregion

    #region IsChild

    public static readonly DependencyProperty IsChildProperty = DependencyProperty.RegisterAttached("IsChild", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(false, OnIsChildChanged));
    public static bool GetIsChild(Window i) => (bool)i.GetValue(IsChildProperty);
    public static void SetIsChild(Window i, bool input) => i.SetValue(IsChildProperty, input);

    private static void OnIsChildChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window i)
        {
            i.Owner = null;
            if ((bool)e.NewValue)
                i.Owner = Application.Current.MainWindow;
        }
    }

    #endregion

    #region IsDialogActive

    public static readonly DependencyProperty IsDialogActiveProperty = DependencyProperty.RegisterAttached("IsDialogActive", typeof(bool), typeof(XWindow), new FrameworkPropertyMetadata(false));
    public static bool GetIsDialogActive(Window i) => (bool)i.GetValue(IsDialogActiveProperty);
    public static void SetIsDialogActive(Window i, bool input) => i.SetValue(IsDialogActiveProperty, input);

    #endregion

    #region MaximizeCommand

    public static readonly RoutedUICommand MaximizeCommand = new(nameof(MaximizeCommand), nameof(MaximizeCommand), typeof(XWindow));

    private static void OnMaximize(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MaximizeWindow(sender as Window);
    private static void OnCanMaximize(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (sender as Window).CanMaximize();

    #endregion

    #region Menu

    public static readonly DependencyProperty MenuProperty = DependencyProperty.RegisterAttached("Menu", typeof(object), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static object GetMenu(Window i) => i.GetValue(MenuProperty);
    public static void SetMenu(Window i, object input) => i.SetValue(MenuProperty, input);

    #endregion

    #region MenuOrientation

    public static readonly DependencyProperty MenuOrientationProperty = DependencyProperty.RegisterAttached("MenuOrientation", typeof(Orient), typeof(XWindow), new FrameworkPropertyMetadata(Orient.Horizontal));
    public static Orient GetMenuOrientation(Window i) => (Orient)i.GetValue(MenuOrientationProperty);
    public static void SetMenuOrientation(Window i, Orient input) => i.SetValue(MenuOrientationProperty, input);

    #endregion

    #region MenuTemplate

    public static readonly DependencyProperty MenuTemplateProperty = DependencyProperty.RegisterAttached("MenuTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetMenuTemplate(Window i) => (DataTemplate)i.GetValue(MenuTemplateProperty);
    public static void SetMenuTemplate(Window i, DataTemplate input) => i.SetValue(MenuTemplateProperty, input);

    #endregion

    #region MinimizeCommand

    public static readonly RoutedUICommand MinimizeCommand = new(nameof(MinimizeCommand), nameof(MinimizeCommand), typeof(XWindow));

    private static void OnMinimize(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(sender as Window);
    private static void OnCanMinimize(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

    #endregion

    #region Notifications

    private static readonly DependencyPropertyKey NotificationsKey = DependencyProperty.RegisterAttachedReadOnly("Notifications", typeof(IList), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty NotificationsProperty = NotificationsKey.DependencyProperty;
    public static IList GetNotifications(Window i) => i.GetValueOrSetDefault(NotificationsKey, () => new ListObservable<Notification>());

    #endregion

    #region NotificationTemplate

    public static readonly DependencyProperty NotificationTemplateProperty = DependencyProperty.RegisterAttached("NotificationTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetNotificationTemplate(Window i) => (DataTemplate)i.GetValue(NotificationTemplateProperty);
    public static void SetNotificationTemplate(Window i, DataTemplate input) => i.SetValue(NotificationTemplateProperty, input);

    #endregion

    #region Placement

    public static readonly DependencyProperty PlacementProperty = DependencyProperty.RegisterAttached("Placement", typeof(WindowPlacement), typeof(XWindow), new FrameworkPropertyMetadata(WindowPlacement.None, OnPlacementChanged));
    public static WindowPlacement GetPlacement(Window i) => (WindowPlacement)i.GetValue(PlacementProperty);
    public static void SetPlacement(Window i, WindowPlacement input) => i.SetValue(PlacementProperty, input);

    private static void OnPlacementChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
        {
            switch ((WindowPlacement)e.OldValue)
            {
                case WindowPlacement.Bottom:
                    Bottom_Unsubscribe(window);
                    break;

                case WindowPlacement.Top:
                    window.Topmost = false;
                    break;
            }
            switch ((WindowPlacement)e.NewValue)
            {
                case WindowPlacement.Bottom:
                    Bottom_Subscribe(window);
                    break;

                case WindowPlacement.Top:
                    window.Topmost = true;
                    break;
            }
        }
    }

    ///

    private static void Bottom_Subscribe(Window window)
    {
        SetBottomHandler(window, new(window));

        window.Closing
            += Bottom_Closing;
        window.Loaded
            += Bottom_Loaded;
    }

    private static void Bottom_Unsubscribe(Window window)
    {
        window.Closing
            -= Bottom_Closing;
        window.Loaded
            -= Bottom_Loaded;

        if (GetBottomHandler(window) is InternalBottomHandler i)
        {
            HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).RemoveHook(new HwndSourceHook(i.WndProc));
            SetBottomHandler(window, null);
        }
    }

    ///

    private static void Bottom_Closing(object sender, CancelEventArgs e)
    {
        if (sender is Window window)
            Bottom_Unsubscribe(window);
    }

    private static void Bottom_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Window window)
        {
            window.Loaded
                -= Bottom_Loaded;

            GetBottomHandler(window).Update();
            HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).AddHook(new HwndSourceHook(GetBottomHandler(window).WndProc));
        }
    }

    #region (internal) BottomHandler

    internal static readonly DependencyProperty BottomHandlerProperty = DependencyProperty.RegisterAttached("BottomHandler", typeof(InternalBottomHandler), typeof(XWindow), new FrameworkPropertyMetadata(null));
    internal static InternalBottomHandler GetBottomHandler(Window i) => (InternalBottomHandler)i.GetValue(BottomHandlerProperty);
    internal static void SetBottomHandler(Window i, InternalBottomHandler input) => i.SetValue(BottomHandlerProperty, input);

    #endregion

    #region (internal) class InternalBottomHandler

    internal class InternalBottomHandler(Window window)
    {
        private static readonly nint HWND_BOTTOM = new(1);

        public readonly Window Window = window;

        public void Update() => SetWindowPos(new WindowInteropHelper(Window).Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

        public nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == WM_SETFOCUS)
            {
                Update();
                handled = true;
            }
            return nint.Zero;
        }
    }

    #endregion

    #endregion

    #region RestoreCommand

    public static readonly RoutedUICommand RestoreCommand = new(nameof(RestoreCommand), nameof(RestoreCommand), typeof(XWindow));

    private static void OnRestore(object sender, ExecutedRoutedEventArgs e) => SystemCommands.RestoreWindow(sender as Window);
    private static void OnCanRestore(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (sender as Window).CanRestore();

    #endregion

    #region Result

    public static readonly DependencyProperty ResultProperty = DependencyProperty.RegisterAttached("Result", typeof(int), typeof(XWindow), new FrameworkPropertyMetadata(-1));
    public static int GetResult(Window i) => (int)i.GetValue(ResultProperty);
    public static void SetResult(Window i, int input) => i.SetValue(ResultProperty, input);

    #endregion

    #region StartupLocation

    public static readonly DependencyProperty StartupLocationProperty = DependencyProperty.RegisterAttached("StartupLocation", typeof(WindowStartupLocation), typeof(XWindow), new FrameworkPropertyMetadata(WindowStartupLocation.Manual, OnStartupLocationChanged));
    public static WindowStartupLocation GetStartupLocation(Window i) => (WindowStartupLocation)i.GetValue(StartupLocationProperty);
    public static void SetStartupLocation(Window i, WindowStartupLocation input) => i.SetValue(StartupLocationProperty, input);

    private static void OnStartupLocationChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
            window.WindowStartupLocation = (WindowStartupLocation)e.NewValue;
    }

    #endregion

    #region SystemMenuItems

    private const uint MF_SEPARATOR = 0x800;
    private const uint MF_BYCOMMAND = 0x0;
    private const uint MF_BYPOSITION = 0x400;
    private const uint MF_STRING = 0x0;
    private const uint MF_ENABLED = 0x0;
    private const uint MF_DISABLED = 0x2;

    ///

    [DllImport("user32.dll")]
    private static extern nint GetSystemMenu(nint hWnd, bool bRevert);

    [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool InsertMenu(nint hmenu, int position, uint flags, uint item_id, [MarshalAs(UnmanagedType.LPTStr)] string item_text);

    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(nint hMenu, uint uIDEnableItem, uint uEnable);

    private static readonly Dictionary<Window, nint> systemMenus;

    ///

    /// <summary>https://www.codeproject.com/Articles/70568/An-MVVM-friendly-approach-to-adding-system-menu-en</summary>
    public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.RegisterAttached("MenuItems", typeof(FreezableCollection<SystemMenuItem>), typeof(XWindow), new PropertyMetadata(OnMenuItemsChanged));
    public static FreezableCollection<SystemMenuItem> GetMenuItems(Window i) => i.GetValueOrSetDefault(MenuItemsProperty, () => new FreezableCollection<SystemMenuItem>());
    public static void SetMenuItems(Window i, FreezableCollection<SystemMenuItem> input) => i.SetValue(MenuItemsProperty, input);

    private static void OnMenuItemsChanged(DependencyObject i, DependencyPropertyChangedEventArgs e)
    {
        if (i is Window window)
        {
            if (e.NewValue is FreezableCollection<SystemMenuItem> items)
                SetMenuItems(window, items);
        }
    }

    #endregion

    #region TaskbarIconTemplate

    public static readonly DependencyProperty TaskbarIconTemplateProperty = DependencyProperty.RegisterAttached("TaskbarIconTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetTaskbarIconTemplate(Window i) => (DataTemplate)i.GetValue(TaskbarIconTemplateProperty);
    public static void SetTaskbarIconTemplate(Window i, DataTemplate input) => i.SetValue(TaskbarIconTemplateProperty, input);

    #endregion

    #region TitleTemplate

    public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.RegisterAttached("TitleTemplate", typeof(DataTemplate), typeof(XWindow), new FrameworkPropertyMetadata(null));
    public static DataTemplate GetTitleTemplate(Window i) => (DataTemplate)i.GetValue(TitleTemplateProperty);
    public static void SetTitleTemplate(Window i, DataTemplate input) => i.SetValue(TitleTemplateProperty, input);

    #endregion

    #endregion

    /// <see cref="Region.Method.Private"/>
    #region

    [DllImport("user32.dll")]
    private static extern nint BeginDeferWindowPos(int nNumWindows);

    [DllImport("user32.dll")]
    private static extern nint DeferWindowPos(nint hWinPosInfo, nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool EndDeferWindowPos(nint hWinPosInfo);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(nint hMonitor, MONITORINFO lpmi);

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint handle, int flags);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    ///

    private static void GetMinMaxInfo(nint hwnd, nint lParam)
    {
        var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

        // Adjust the maximized size and position to fit the work area of the correct monitor
        var MONITOR_DEFAULTTONEAREST = 0x00000002;
        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

        if (monitor != nint.Zero)
        {
            var monitorInfo = new MONITORINFO();
            GetMonitorInfo(monitor, monitorInfo);

            var rcWorkArea
                = monitorInfo.rcWork;
            var rcMonitorArea
                = monitorInfo.rcMonitor;

            mmi.ptMaxPosition.x
                = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
            mmi.ptMaxPosition.y
                = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
            mmi.ptMaxSize.x
                = Math.Abs(rcWorkArea.right - rcWorkArea.left);
            mmi.ptMaxSize.y
                = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
        }
        Marshal.StructureToPtr(mmi, lParam, true);
    }

    private static nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        Window window = null;
        switch (msg)
        {
            case (int)WindowMessages.WM_SYSCOMMAND:
                break;
                var item = GetMenuItems(window).FirstOrDefault(i => i.Id == wParam.ToInt32());
                if (item != null)
                {
                    item.Command.Execute(item.CommandParameter);
                    handled = true;
                }

            case (int)WindowMessages.WM_INITMENUPOPUP:
                break;
                if (systemMenus[window] == wParam)
                {
                    foreach (var i in GetMenuItems(window))
                        EnableMenuItem(systemMenus[window], (uint)i.Id, i.Command.CanExecute(i.CommandParameter) ? MF_ENABLED : MF_DISABLED);

                    handled = true;
                }

            case (int)WindowMessages.WM_GETMINMAXINFO:
                GetMinMaxInfo(hwnd, lParam);
                handled = true;
                break;
        }
        return nint.Zero;
    }

    ///

    private static void OnSourceInitialized(object sender, EventArgs e)
    {
        if (sender is Window window)
        {
            /*
            var helper = new WindowInteropHelper(window);

            var menu = GetSystemMenu(helper.Handle, false);
            systemMenus.Add(window, menu);

            if (GetMenuItems(window).Count > 0)
                InsertMenu(menu, -1, MF_BYPOSITION | MF_SEPARATOR, 0, String.Empty);

            foreach (SystemMenuItem item in GetMenuItems(window))
                InsertMenu(systemMenus[window], (int)item.Id, MF_BYCOMMAND | MF_STRING, (uint)item.Id, item.Header);

            var source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
            */

            ///

            window.SourceInitialized -= OnSourceInitialized;
            HwndSource.FromHwnd(new WindowInteropHelper(window).Handle).AddHook(new HwndSourceHook(WndProc));

            window.AddOnce
                (new CommandBinding(CloseCommand,
                OnClose, CanClose));
            window.AddOnce
                (new CommandBinding(MarkNotificationCommand,
                OnMarkNotificationCommandExecuted, OnCanExecuteMarkNotificationCommand));
            window.AddOnce
                (new CommandBinding(MaximizeCommand,
                OnMaximize, OnCanMaximize));
            window.AddOnce
                (new CommandBinding(MinimizeCommand,
                OnMinimize, OnCanMinimize));
            window.AddOnce
                (new CommandBinding(RestoreCommand,
                OnRestore, OnCanRestore));
        }
    }

    #endregion

    /// <see cref="Region.Method.Public"/>
    #region

    public static double ActualLeft(this Window input)
    {
        if (input.WindowState == WindowState.Maximized)
        {
            var field = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (double)field.GetValue(input);
        }
        else return input.Left;
    }

    public static double ActualTop(this Window input)
    {
        if (input.WindowState == WindowState.Maximized)
        {
            var field = typeof(Window).GetField("_actualTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (double)field.GetValue(input);
        }
        else return input.Top;
    }

    ///

    public static bool CanMaximize(this Window window) => window != null && window.WindowStyle != WindowStyle.ToolWindow && window.WindowState != WindowState.Maximized && window.ResizeMode != ResizeMode.NoResize;

    public static void Center(this Window input)
    {
        input.Left
            = SystemParameters.PrimaryScreenWidth / 2.0 - input.Width / 2.0;
        input.Top
            = SystemParameters.PrimaryScreenHeight / 2.0 - input.Height / 2.0;
    }

    public static bool CanRestore(this Window window) => window != null && window.WindowStyle != WindowStyle.ToolWindow && window.WindowState != WindowState.Normal && window.ResizeMode != ResizeMode.NoResize;

    #endregion
}