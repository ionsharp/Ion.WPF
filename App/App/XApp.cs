using Ion.Core;
using System.Runtime.InteropServices;
using System.Windows;

namespace Ion.Appliance;

[Extend<Application>]
public static class XApp
{
    [DllImport("user32")]
    public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

    [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

    [DllImport("user32")]
    public static extern void LockWorkStation();

    private static void Hibernate()
        => SetSuspendState(true, true, true);

    private static void Lock()
        => LockWorkStation();

    private static void LogOff()
        => ExitWindowsEx(0, 0);

    private static void Restart()
        => System.Diagnostics.Process.Start("shutdown", "/r /t 0");

    private static void Shutdown()
        => System.Diagnostics.Process.Start("shutdown", "/s /t 0");

    private static void Sleep()
        => SetSuspendState(false, true, true);

    public static void Exit(this Application input, AppExitMethod method)
    {
        switch (method)
        {
            case AppExitMethod.None: break;
            case AppExitMethod.Exit:
                input.Shutdown(0);
                break;

            case AppExitMethod.Hibernate:
                Hibernate();
                break;

            case AppExitMethod.Lock:
                Lock();
                break;

            case AppExitMethod.LogOff:
                LogOff();
                break;

            case AppExitMethod.Restart:
                Restart();
                break;

            case AppExitMethod.Shutdown:
                Shutdown();
                break;

            case AppExitMethod.Sleep:
                Sleep();
                break;
        }
    }
}